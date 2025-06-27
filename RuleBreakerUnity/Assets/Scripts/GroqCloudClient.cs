using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GroqMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class GroqRequest
{
    public string model;
    public GroqMessage[] messages;
    public float temperature = 0.7f;
    public int max_tokens = 150;
}

[System.Serializable]
public class GroqChoice
{
    public GroqMessage message;
}

[System.Serializable]
public class GroqResponse
{
    public GroqChoice[] choices;
    public string error;
}

public class GroqCloudClient : MonoBehaviour
{
    [Header("GroqCloud Settings")]
    [SerializeField] private string apiKey = "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"; // Set your API key in inspector or through code
    [SerializeField] private string model = "llama-3.3-70b-versatile";
    
    private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
    
    public delegate void OnResponseReceived(string response, bool success);
    
    private void Start()
    {
        // Try to get API key from environment if not set in inspector
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = System.Environment.GetEnvironmentVariable("GROQ_API_KEY");
        }
    }
    
    /// <summary>
    /// Send a simple text message to GroqCloud API
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="callback">Callback function to handle the response</param>
    public void SendMessage(string message, OnResponseReceived callback)
    {
        StartCoroutine(SendMessageCoroutine(message, callback));
    }
    
    /// <summary>
    /// Send a chat completion request with custom messages
    /// </summary>
    /// <param name="messages">Array of messages to send</param>
    /// <param name="callback">Callback function to handle the response</param>
    public void SendChatCompletion(GroqMessage[] messages, OnResponseReceived callback)
    {
        StartCoroutine(SendChatCompletionCoroutine(messages, callback));
    }
    
    private IEnumerator SendMessageCoroutine(string message, OnResponseReceived callback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("GroqCloud API key is not set!");
            callback?.Invoke("Error: API key not set", false);
            yield break;
        }
        
        // Create the request object
        GroqMessage[] messages = new GroqMessage[]
        {
            new GroqMessage { role = "user", content = message }
        };
        
        yield return StartCoroutine(SendChatCompletionCoroutine(messages, callback));
    }
    
    private IEnumerator SendChatCompletionCoroutine(GroqMessage[] messages, OnResponseReceived callback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("GroqCloud API key is not set!");
            callback?.Invoke("Error: API key not set", false);
            yield break;
        }
        
        // Create the request object
        GroqRequest request = new GroqRequest
        {
            model = model,
            messages = messages,
            temperature = 1.0f,
            max_tokens = 4096
        };
        
        // Convert to JSON
        string jsonData = JsonUtility.ToJson(request);
        Debug.Log($"Sending request: {jsonData}");
        
        // Create Unity web request
        UnityWebRequest webRequest = new UnityWebRequest(GROQ_API_URL, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        
        // Set headers
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        
        // Send the request
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string responseText = webRequest.downloadHandler.text;
            Debug.Log($"Response received: {responseText}");
            
            try
            {
                GroqResponse response = JsonUtility.FromJson<GroqResponse>(responseText);
                if (response.choices != null && response.choices.Length > 0)
                {
                    string content = response.choices[0].message.content;
                    callback?.Invoke(content, true);
                }
                else
                {
                    callback?.Invoke("No response content received", false);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse response: {e.Message}");
                callback?.Invoke($"Error parsing response: {e.Message}", false);
            }
        }
        else
        {
            Debug.LogError($"Request failed: {webRequest.error}");
            Debug.LogError($"Response Code: {webRequest.responseCode}");
            Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            callback?.Invoke($"Request failed: {webRequest.error}", false);
        }
        
        webRequest.Dispose();
    }
    
    /// <summary>
    /// Set the API key programmatically
    /// </summary>
    /// <param name="key">Your GroqCloud API key</param>
    public void SetApiKey(string key)
    {
        apiKey = key;
    }
    
    /// <summary>
    /// Set the model to use for requests
    /// </summary>
    /// <param name="modelName">Model name (e.g., "mixtral-8x7b-32768", "llama3-70b-8192")</param>
    public void SetModel(string modelName)
    {
        model = modelName;
    }
} 