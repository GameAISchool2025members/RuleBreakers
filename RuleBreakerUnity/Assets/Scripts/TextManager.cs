using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextManager : MonoBehaviour
{
    public TMP_Text inputField;

    public string llmInput;
    public string llmOutput;

    public bool canSend = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ReadStringInput()
    {
        string input = inputField.text.ToString();
        llmInput = input.Substring(0, input.Length - 1);    // For some reason, Input Field has an non-whitespace end character, we delete this here.
        Debug.Log(llmInput);
    }

    public void SendInputToLLM()
    {
        if (String.IsNullOrWhiteSpace(llmInput))
        {
            Debug.Log("You are trying to send an empty message to the LLM!");
            return;
        }
        if (canSend)
        {
            Debug.Log("Sending " + llmInput + " to LLM model...");
            // Do all the code for LLM interactions in here.
            canSend = false;
        }
    }

    public void ReadLLMOutput(string output)
    {
        llmOutput = output;
        Debug.Log(llmOutput);
        canSend = true;
    }
}
