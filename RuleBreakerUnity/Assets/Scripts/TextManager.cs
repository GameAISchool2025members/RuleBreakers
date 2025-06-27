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
    public string gameContext = "a board game where players move pieces on a grid";

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
        return $@"You are a game rule validator for {gameContext}. 

Please analyze the following rule and determine if it is valid:
""{rule}""

A valid rule should be:
1. Clear and unambiguous
2. Implementable in the game context
3. Fair and balanced
4. Not contradictory to basic game mechanics
5. Specific enough to be actionable

Please respond with:
- VALID or INVALID
- A brief explanation (1-2 sentences)

Format your response as:
STATUS: [VALID/INVALID]
REASON: [Your explanation]";
    }
    
    private void ProcessValidationResponse(string originalRule, string llmResponse)
    {
        Debug.Log($"LLM Response: {llmResponse}");
        
        // Parse the response
        bool isValid = false;
        string reason = "Unable to parse response";
        
        if (llmResponse.Contains("STATUS:") && llmResponse.Contains("REASON:"))
        {
            try
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
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing LLM response: {e.Message}");
                reason = "Error parsing validation response";
            }
        }
        else
        {
            // Fallback parsing if format is not followed
            isValid = llmResponse.ToUpper().Contains("VALID") && !llmResponse.ToUpper().Contains("INVALID");
            reason = llmResponse;
        }
        
        // Print the result
        string result = $"Rule: \"{originalRule}\" is {(isValid ? "VALID" : "INVALID")}";
        Debug.Log(result);
        Debug.Log($"Reason: {reason}");
        
        // Send result to ReadLLMOutput
        ReadLLMOutput($"{(isValid ? "VALID" : "INVALID")}: {reason}");
    }

    public void ReadLLMOutput(string output)
    {
        llmOutput = output;
        Debug.Log(llmOutput);
        canSend = true;
    }
}
