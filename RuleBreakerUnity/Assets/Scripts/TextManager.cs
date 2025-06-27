using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text inputField;
    
    [Header("LLM Settings")]
    public GroqCloudClient groqClient;

    public string llmInput;
    public string llmOutput;

    public bool canSend = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find GroqCloudClient if not assigned
        if (groqClient == null)
        {
            groqClient = FindObjectOfType<GroqCloudClient>();
            if (groqClient == null)
            {
                Debug.LogError("GroqCloudClient not found! Please assign it in the inspector or add it to the scene.");
            }
        }
    }

    public void ReadStringInput()
    {
        string input = inputField.text.ToString();
        llmInput = input.Substring(0, input.Length - 1);    // For some reason, Input Field has an non-whitespace end character, we delete this here.
        Debug.Log(llmInput);
    }

    public void SendInputToLLM()
    {
        if (llmInput == "")
        {
            Debug.Log("You are trying to send an empty message to the LLM!");
            return;
        }
        if (canSend)
        {
            Debug.Log("Sending " + llmInput + " to LLM model...");
            // Do all the code for LLM interactions in here.
            StartCoroutine(ValidateRuleCoroutine(llmInput));
            canSend = false;
        }
    }
    
    private IEnumerator ValidateRuleCoroutine(string rule)
    {
        if (groqClient == null)
        {
            Debug.LogError("GroqCloudClient is not assigned!");
            ReadLLMOutput("Error: LLM client not available");
            yield break;
        }
        
        Debug.Log($"Validating rule: {rule}");
        
        // Create the validation prompt
        string prompt = CreateValidationPrompt(rule);
        
        // Send to Groq
        bool responseReceived = false;
        string llmResponse = "";
        bool success = false;
        
        groqClient.SendMessage(prompt, (response, isSuccess) =>
        {
            llmResponse = response;
            success = isSuccess;
            responseReceived = true;
        });
        
        // Wait for response
        while (!responseReceived)
        {
            yield return null;
        }
        
        // Process the response
        if (success)
        {
            ProcessValidationResponse(rule, llmResponse);
        }
        else
        {
            Debug.LogError($"Validation failed: {llmResponse}");
            ReadLLMOutput($"Validation failed: {llmResponse}");
        }
    }
    
    private string CreateValidationPrompt(string rule)
    {
        // Get current board state as JSON
        string boardStateJson = JsonConverter.BoardStateToJSON();

        Debug.Log("Board state: " + boardStateJson);
        
        return $@"You are a game rule validator for a board game where players move pieces on a 8x8 grid. The board is labeled A1 to H8, with A1 being the bottom left and H8 being the top right. There is a dynamic winning and losing condition which can change at the start of the game but stays static throughtout the game.

Each player can either place a piece on the board or create a rule that the next player needs to follow in the next turn. These rules are called Joker Rules. 

The rule to analyze is the following:
""{rule}""

Validate the rule by checking the following criteria, also known as Super Rules:
1. When referring to rows, horizontal, vertical or diagonal are all valid.
2. A piece on the board always stays in the same place and never leaves the board.
3. A piece can be flipped to the opposing color if a rule allows it.
4. Joker rule cannot change win or lose condition.
5. Rules cannot prevent players from taking their turn.
6. You cannot make a rule that completely blocks a player from placing a game piece; there must always be at least one viable location.
7. Rules can describe patterns, conditions, and game piece attributes, but not refer to individual turns or past moves.
8. A rule cannot make the board state invalid, unsolvable, or fully locked.
9. When referring to adjacency, the surrounding 8 tiles are valid.
10. A Joker Rule cannot change super rules.


Current board state:
{boardStateJson}

Return your response strictly as a JSON dictionary in exactly this format:
{{
    ""status"": true or false,
    ""reason"": ""Explanation in 1-2 line""
}}

Example response:
{{
    ""status"": true,
    ""reason"": ""The rule follows all super rules and creates valid game constraints.""
}}";
    }
    
    private void ProcessValidationResponse(string originalRule, string llmResponse)
    {
        Debug.Log($"LLM Response: {llmResponse}");
        
        // Parse the response
        bool isValid = false;
        string reason = "Unable to parse response";
        
        try
        {
            // Try to parse as JSON first
            if (llmResponse.Contains("\"status\"") && llmResponse.Contains("\"reason\""))
            {
                // Extract JSON from response (in case there's extra text)
                int startIndex = llmResponse.IndexOf('{');
                int endIndex = llmResponse.LastIndexOf('}');
                
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string jsonPart = llmResponse.Substring(startIndex, endIndex - startIndex + 1);
                    
                    // Parse JSON manually (simple approach)
                    string statusValue = ExtractJsonValue(jsonPart, "status");
                    string reasonValue = ExtractJsonValue(jsonPart, "reason");
                    
                    isValid = statusValue == "true";
                    reason = string.IsNullOrEmpty(reasonValue) ? "No reason provided" : reasonValue;
                }
                else
                {
                    throw new System.Exception("Invalid JSON structure");
                }
            }
            else
            {
                // Fallback: try old format or simple text parsing
                if (llmResponse.Contains("STATUS:") && llmResponse.Contains("REASON:"))
                {
                    string[] lines = llmResponse.Split('\n');
                    string statusLine = "";
                    string reasonLine = "";
                    
                    foreach (string line in lines)
                    {
                        if (line.Contains("STATUS:"))
                        {
                            statusLine = line.Substring(line.IndexOf("STATUS:") + 7).Trim();
                        }
                        else if (line.Contains("REASON:"))
                        {
                            reasonLine = line.Substring(line.IndexOf("REASON:") + 7).Trim();
                        }
                    }
                    
                    isValid = statusLine.ToUpper().Contains("VALID") && !statusLine.ToUpper().Contains("INVALID");
                    reason = string.IsNullOrEmpty(reasonLine) ? "No reason provided" : reasonLine;
                }
                else
                {
                    // Final fallback
                    isValid = llmResponse.ToUpper().Contains("TRUE") || (llmResponse.ToUpper().Contains("VALID") && !llmResponse.ToUpper().Contains("INVALID"));
                    reason = llmResponse;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing LLM response: {e.Message}");
            reason = "Error parsing validation response";
            isValid = false;
        }
        
        // Print the result
        string result = $"Rule: \"{originalRule}\" is {(isValid ? "VALID" : "INVALID")}";
        Debug.Log(result);
        Debug.Log($"Reason: {reason}");
        
        // Send result to ReadLLMOutput
        ReadLLMOutput($"{(isValid ? "VALID" : "INVALID")}: {reason}");
    }
    
    private string ExtractJsonValue(string json, string key)
    {
        string searchPattern = $"\"{key}\"";
        int keyIndex = json.IndexOf(searchPattern);
        if (keyIndex < 0) return "";
        
        int colonIndex = json.IndexOf(':', keyIndex);
        if (colonIndex < 0) return "";
        
        int startQuote = json.IndexOf('"', colonIndex);
        if (startQuote < 0) return "";
        
        int endQuote = json.IndexOf('"', startQuote + 1);
        if (endQuote < 0) return "";
        
        return json.Substring(startQuote + 1, endQuote - startQuote - 1);
    }

    public void ReadLLMOutput(string output)
    {
        llmOutput = output;
        Debug.Log(llmOutput);
        canSend = true;
    }
}
