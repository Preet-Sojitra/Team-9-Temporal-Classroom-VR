using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class GroqLLMClient : MonoBehaviour
{
    [Header("API Settings")]
    [Tooltip("Get a free API key from console.groq.com")]
    public string groqApiKey = "";
    public string model = "llama-3.1-8b-instant";

    [TextArea(10, 20)]
    public string systemPrompt = @"You are a sentient, cynical antique clock trapped in a time-travel escape room. Players have 10 minutes to solve puzzles spanning the Past and Future. Give very short (1-2 sentence) snarky hints or torment them for being slow. Do NOT give away direct answers. Keep responses brief. 

HERE ARE THE RULES OF THIS SPECIFIC GAME TO BASE YOUR HINTS ON:
- Puzzle 1: (Replace this with a rule, e.g., 'To open the door, players must find the red key under the projector in the past.')
- Puzzle 2: (Replace this with another rule, e.g., 'The code for the keypad is 1984.')
- Remember, never tell them '1984' directly. Always give cryptic hints like 'Think about when big brother was watching.'";

    [System.Serializable]
    private class GroqMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class GroqRequest
    {
        public string model;
        public List<GroqMessage> messages;
    }

    // JSON response wrappers
    [System.Serializable]
    private class GroqResponse
    {
        public List<GroqChoice> choices;
    }

    [System.Serializable]
    private class GroqChoice
    {
        public GroqMessage message;
    }

    public void AskQuestion(string playerText, System.Action<string> onResponse)
    {
        StartCoroutine(SendRequest(playerText, onResponse));
    }

    private IEnumerator SendRequest(string playerText, System.Action<string> onResponse)
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";

        GroqRequest req = new GroqRequest
        {
            model = this.model,
            messages = new List<GroqMessage>
            {
                new GroqMessage { role = "system", content = systemPrompt },
                new GroqMessage { role = "user", content = playerText }
            }
        };

        string json = JsonUtility.ToJson(req);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + groqApiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Groq Error: " + request.error + " - " + request.downloadHandler.text);
            onResponse?.Invoke("Tick tock... my gears are jammed. I cannot speak right now.");
        }
        else
        {
            try
            {
                GroqResponse result = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                if (result != null && result.choices.Count > 0)
                {
                    onResponse?.Invoke(result.choices[0].message.content);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON Parse Error: " + e.Message);
            }
        }
    }
}
