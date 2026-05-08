using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class GroqWhisperClient : MonoBehaviour
{
    private string groqApiKey;
    private AudioClip recordingClip;
    private const int MaxRecordingTime = 10;

    public bool IsRecording { get; private set; }

    private void Start()
    {
        groqApiKey = GetComponent<GroqLLMClient>().groqApiKey;
    }

    public void StartRecording()
    {
        if (IsRecording) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            Debug.LogWarning("[GroqWhisper] Microphone permission not yet granted. Requesting...");
            return;
        }
#endif

        IsRecording = true;
        recordingClip = Microphone.Start(null, false, MaxRecordingTime, 44100);
        Debug.Log("Microphone Recording Started...");
    }

    public void StopRecordingAndTranscribe(Action<string> onTranscriptionDone)
    {
        if (!IsRecording) return;

        IsRecording = false;
        int position = Microphone.GetPosition(null);
        Microphone.End(null);
        Debug.Log("Microphone Recording Stopped.");

        if (position < 1000)
        {
            onTranscriptionDone?.Invoke("");
            return;
        }

        AudioClip trimmedClip = TrimClip(recordingClip, position);
        byte[] wavData = ConvertToWav(trimmedClip);

        StartCoroutine(UploadWavToGroq(wavData, onTranscriptionDone));
    }

    private IEnumerator UploadWavToGroq(byte[] wavData, Action<string> onTranscriptionDone)
    {
        string url = "https://api.groq.com/openai/v1/audio/transcriptions";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
        form.AddField("model", "whisper-large-v3");

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + groqApiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Groq Whisper Error: " + request.error + " - " + request.downloadHandler.text);
            onTranscriptionDone?.Invoke(""); // failed
        }
        else
        {
            try
            {
                TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(request.downloadHandler.text);
                onTranscriptionDone?.Invoke(response.text);
            }
            catch (Exception e)
            {
                Debug.LogError("Whisper JSON Parse error: " + e.Message);
            }
        }
    }

    private AudioClip TrimClip(AudioClip original, int samples)
    {
        float[] sampleData = new float[samples * original.channels];
        original.GetData(sampleData, 0);

        AudioClip newClip = AudioClip.Create(original.name, samples, original.channels, original.frequency, false);
        newClip.SetData(sampleData, 0);
        return newClip;
    }

    private byte[] ConvertToWav(AudioClip clip)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            int hz = clip.frequency;
            int channels = clip.channels;
            int samples = clip.samples;
            float[] data = new float[samples * channels];
            clip.GetData(data, 0);

            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + samples * channels * 2);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write((short)channels);
                writer.Write(hz);
                writer.Write(hz * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(samples * channels * 2);

                for (int i = 0; i < data.Length; i++)
                {
                    writer.Write((short)(data[i] * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    [Serializable]
    private class TranscriptionResponse
    {
        public string text;
    }
}
