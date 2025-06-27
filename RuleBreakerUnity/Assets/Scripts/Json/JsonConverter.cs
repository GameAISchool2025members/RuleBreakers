using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonConverter : MonoBehaviour
{
    [Header("This component does not need to be added to a gameobject, but comes with simple test functions.")]
    [Header(" ")]
    public bool testBoardStateToJSON = false;

    [Header(" ")]
    public bool testRulePromptToJSON = false;
    public string newRule = "This is a new rule.";

    [Header("Response")]
    [TextArea(10,100)]
    public string response;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (testBoardStateToJSON)
        {
            testBoardStateToJSON = false;
            response = BoardStateToJSON();
        }

        if (testRulePromptToJSON)
        {
            testRulePromptToJSON = false;
            response = RulePromptToJSON(newRule);
        }
    }

    public static string BoardStateToJSON()
    {
        Dictionary<string, string> boardState = new Dictionary<string, string>();

        for (char file = 'A'; file <= 'H'; file++)
        {
            for (int rank = 1; rank <= 8; rank++)
            {
                string coord = $"{file}{rank}";

                if (GameManager.gameManager.p1Coords.Contains(coord))
                    boardState[coord] = "Player1";
                else if (GameManager.gameManager.p2Coords.Contains(coord))
                    boardState[coord] = "Player2";
                else
                    boardState[coord] = "Empty";
            }
        }

        string json = JsonConvert.SerializeObject(boardState, Formatting.Indented);
        Debug.Log(json);
        return json;
    }

    public static string RulePromptToJSON(string newPrompt)
    {
        Dictionary<string, string> prompt = new Dictionary<string, string>();

        prompt["NewRule"] = newPrompt;

        string json = JsonConvert.SerializeObject(prompt, Formatting.Indented);
        Debug.Log(json);
        return json;
    }
}
