using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class DeepSeekDialogueManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiKey = "替换为API密钥:D"; // 
    [SerializeField] private string modelName = "deepseek-chat";
    [SerializeField] private string apiUrl = "https://api.deepseek.com/v1/chat/completions";

    [Header("Dialogue Settings")]
    [Range(0, 2)] public float temperature = 0.7f; // 控制回复的随机性
    [Range(1, 1000)] public int maxTokens = 150;  // 控制回复长度

    [System.Serializable]
    public class NPCCharacter
    {
        public string name = "Unity-Chan";
        [TextArea(3, 10)]
        public string personalityPrompt = "你是虚拟人物Unity-Chan，是个性格活泼，聪明可爱的女生。擅长Unity和C#编程知识。";
    }

    [SerializeField] public NPCCharacter npcCharacter;

    public delegate void DialogueCallback(string response, bool isSuccess);
    public void SendDialogueRequest(string userMessage, DialogueCallback callback)
    {
        StartCoroutine(ProcessDialogueRequest(userMessage, callback));
    }

    private IEnumerator ProcessDialogueRequest(string userInput, DialogueCallback callback)
    {
        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = npcCharacter.personalityPrompt },
            new Message { role = "user", content = userInput }
        };

        ChatRequest requestBody = new ChatRequest
        {
            model = modelName,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        UnityWebRequest request = CreateWebRequest(jsonBody);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
            callback?.Invoke(null, false);
            yield break;
        }

        DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(request.downloadHandler.text);
        if (response != null && response.choices.Length > 0)
        {
            string npcReply = response.choices[0].message.content;
            callback?.Invoke(npcReply, true);
        }
        else
        {
            callback?.Invoke(npcCharacter.name + "（陷入沉默）", false);
        }
    }

    private UnityWebRequest CreateWebRequest(string jsonBody)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        var request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Accept", "application/json");
        return request;
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class DeepSeekResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }
}