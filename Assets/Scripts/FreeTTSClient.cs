using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FreeTTSClient : MonoBehaviour
{
    [Header("Voice Settings")]
    [Tooltip("Lower = deeper/creepier (0.7-0.9 for eerie), 1.0 = normal")]
    [Range(0.5f, 1.5f)]
    public float voicePitch = 0.82f;

    [Header("Eerie Audio Effects")]
    [Tooltip("Enable reverb for a haunted/echoing effect")]
    public bool enableReverb = true;
    [Tooltip("Enable echo for ghostly trailing effect")]
    public bool enableEcho = true;
    [Tooltip("Enable subtle distortion for an unsettling edge")]
    public bool enableDistortion = true;

    private AudioSource audioSource;
    private AudioReverbFilter reverbFilter;
    private AudioEchoFilter echoFilter;
    private AudioDistortionFilter distortionFilter;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = voicePitch;

        SetupEerieEffects();
    }

    private void SetupEerieEffects()
    {
        // --- REVERB: Makes voice sound like it's echoing inside an old clock ---
        if (enableReverb)
        {
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            reverbFilter.reverbPreset = AudioReverbPreset.Cave;
            reverbFilter.dryLevel = 0f;
            reverbFilter.room = -400f;
            reverbFilter.roomHF = -200f;
            reverbFilter.decayTime = 3.5f;       // Long decay for that eerie trailing
            reverbFilter.reflectionsLevel = -100f;
            reverbFilter.reverbLevel = 200f;
        }

        // --- ECHO: Ghostly repeating whisper effect ---
        if (enableEcho)
        {
            echoFilter = gameObject.AddComponent<AudioEchoFilter>();
            echoFilter.delay = 180f;       // Short delay (ms) for a tight echo
            echoFilter.decayRatio = 0.3f;  // How much each echo fades
            echoFilter.dryMix = 1f;
            echoFilter.wetMix = 0.4f;      // Blend of echo effect
        }

        // --- DISTORTION: Subtle crackling, like the clock is ancient and broken ---
        if (enableDistortion)
        {
            distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
            distortionFilter.distortionLevel = 0.15f; // Very subtle -- just enough to feel "off"
        }
    }

    /// <summary>
    /// Speaks the given text with eerie audio effects applied.
    /// </summary>
    public void SpeakText(string text)
    {
        StartCoroutine(DownloadAndPlayAudio(text));
    }

    private IEnumerator DownloadAndPlayAudio(string text)
    {
        // Google TTS has a ~200 character limit per request, so split long text
        string[] chunks = SplitText(text, 180);

        for (int i = 0; i < chunks.Length; i++)
        {
            string chunk = chunks[i];
            string encodedText = UnityWebRequest.EscapeURL(chunk);
            string url = $"https://translate.google.com/translate_tts?ie=UTF-8&total={chunks.Length}&idx={i}&textlen={chunk.Length}&client=tw-ob&q={encodedText}&tl=en";

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("TTS Error: " + www.error);
                    yield break;
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.pitch = voicePitch; // Ensure pitch is set
                    audioSource.clip = clip;
                    audioSource.Play();

                    // Wait for this chunk to finish (account for pitch change)
                    yield return new WaitForSeconds((clip.length / voicePitch) + 0.1f);
                }
            }
        }
    }

    private string[] SplitText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return new string[] { text };

        var chunks = new System.Collections.Generic.List<string>();
        while (text.Length > 0)
        {
            if (text.Length <= maxLength)
            {
                chunks.Add(text);
                break;
            }

            // Try to split at a sentence boundary (. ! ?)
            int splitIndex = text.LastIndexOf('.', maxLength);
            if (splitIndex < maxLength / 2) splitIndex = text.LastIndexOf(' ', maxLength);
            if (splitIndex <= 0) splitIndex = maxLength;

            chunks.Add(text.Substring(0, splitIndex + 1).Trim());
            text = text.Substring(splitIndex + 1).Trim();
        }
        return chunks.ToArray();
    }
}
