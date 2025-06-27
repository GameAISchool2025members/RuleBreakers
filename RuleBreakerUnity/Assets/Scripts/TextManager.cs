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

        string rulePrompt = CreateRulePrompt(rule);
        
        // Send to Groq
        bool responseReceived = false;
        bool ruleResponseReceived = false;
        string llmResponse = "";
        string llmRuleResponse = "";
        bool success = false;
        bool successRule = false;

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
            //ProcessValidationResponse(rule, llmResponse);
            JsonConverter.RuleResponse response = JsonConverter.ParseRuleResponseJSONToObject(llmResponse, out bool valid);
            Debug.Log("Response: " + response.status + " " + response.reason);
            //ReadLLMOutput(response.reason);

            if (!valid)
            {
                Debug.LogError($"Validation failed: {llmResponse}");
                ReadLLMOutput($"Validation failed: {llmResponse}");
            }
            else
            {
                if (!response.status)
                {
                    groqClient.SendMessage(rulePrompt, (response, isSuccess) =>
                    {
                        llmRuleResponse = response;
                        successRule = isSuccess;
                        ruleResponseReceived = true;
                    });

                    if (successRule)
                    {
                        JsonConverter.RuleResponse ruleResponse = JsonConverter.ParseRuleResponseJSONToObject(llmRuleResponse, out bool validRule);
                        Debug.Log("Rule: " + ruleResponse.rule);

                        if (!validRule)
                        {
                            Debug.LogError($"Rule creation failed: {llmRuleResponse}");
                            ReadLLMOutput($"Rule creation failed: {llmRuleResponse}");
                        }
                    }
                    
                }
            }
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
11. A Joker Rule can not contain a specific coordinate. 

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

    private string CreateRulePrompt(string rule)
    {
        // Get current board state as JSON
        string boardStateJson = JsonConverter.BoardStateToJSON();

        
        return $@"You are a game rule validator and creator for a board game where players move pieces on a 8x8 grid. The board is labeled A1 to H8, with A1 being the bottom left and H8 being the top right. There is a dynamic winning and losing condition which can change at the start of the game but stays static throughtout the game.

Each player can either place a piece on the board or create a rule that the next player needs to follow in the next turn. These rules are called Joker Rules. 


You analyzed the following rule and found it to be invalid. You need to create a new rule that is valid but similar the rule you analyzed.

The rule you analyzed is the following:
""{rule}""

Validate the new rule by checking the following criteria, also known as Super Rules:
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
11. A Joker Rule can not contain a specific coordinate. 

Current board state:
{boardStateJson}

Return your response strictly as a JSON dictionary in exactly this format:
{{
    ""rule"": ""New rule""
}}

FOr example, you were provided a rule ""Player can only place on A1"" which you found invalid and you created the following new rule:
{{
    ""rule"": ""Player can only place on column A""
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

                    Debug.Log("Status: " + statusValue);

                    if (statusValue == "true")
                    {
                        isValid = true;
                    }
                    else if (statusValue == "false")
                    {
                        isValid = false;
                    }
                    //bool.TryParse(statusValue, out isValid);
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
        //ReadLLMOutput($"{(isValid ? "VALID" : "INVALID")}: {reason}");
    }
    
    private string ExtractJsonValue(string json, string key)
    {
        string searchPattern = $"\"{key}\"";
        int keyIndex = json.IndexOf(searchPattern);
        if (keyIndex < 0) return "";
        
        int colonIndex = json.IndexOf(':', keyIndex);
        if (colonIndex < 0) return "";
        
        // Skip whitespace after colon
        int valueStart = colonIndex + 1;
        while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
        {
            valueStart++;
        }
        
        if (valueStart >= json.Length) return "";
        
        // Check if value starts with a quote (string value)
        if (json[valueStart] == '"')
        {
            int endQuote = json.IndexOf('"', valueStart + 1);
            if (endQuote < 0) return "";
            return json.Substring(valueStart + 1, endQuote - valueStart - 1);
        }
        else
        {
            // Non-quoted value (boolean, number, etc.)
            int valueEnd = valueStart;
            while (valueEnd < json.Length && json[valueEnd] != ',' && json[valueEnd] != '}')
            {
                valueEnd++;
            }
            return json.Substring(valueStart, valueEnd - valueStart).Trim();
        }
    }

    public void ReadLLMOutput(string output)
    {
        llmOutput = output;
        Debug.Log(llmOutput);
        canSend = true;
    }
}
