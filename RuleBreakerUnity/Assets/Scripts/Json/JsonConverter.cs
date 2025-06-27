using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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

    [Header(" ")]
    public bool testParseValidPlacementJSONToDictionary = false;
    public string placementJSONToParse = "Please add JSON.";

    [Header(" ")]
    public bool testParseRuleResponseJSONToObject = false;
    public string ruleJSONToParse = "Please add JSON.";

    [Header(" ")]
    public bool testParseNewRuleResponseJSONToObject = false;
    public string newRuleJSONToParse = "Please add JSON.";

    [Header("Response")]
    [TextArea(10, 100)]
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

        if (testParseValidPlacementJSONToDictionary)
        {
            testParseValidPlacementJSONToDictionary = false;
            Dictionary<string, PlacementEntry> result = ParseValidPlacementJSONToDictionary(placementJSONToParse, out bool valid);
            response = valid.ToString();
        }

        if (testParseRuleResponseJSONToObject)
        {
            testParseRuleResponseJSONToObject = false;
            RuleResponse result = ParseRuleResponseJSONToObject(ruleJSONToParse, out bool valid);
            response = $"valid parse: {valid.ToString()}\nvalid: {result.status}\nreason: {result.reason}";
        }

        if (testParseNewRuleResponseJSONToObject)
        {
            testParseNewRuleResponseJSONToObject=false;
            NewRuleResponse result = ParseNewRuleResponseJSONToObject(newRuleJSONToParse, out bool valid);
            response = $"valid parse: {valid.ToString()}\nrule: {result.rule}";
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

    public class PlacementEntry
    {
        public bool P1 { get; set; }
        public bool P2 { get; set; }
    }

    public static Dictionary<string, PlacementEntry> ParseValidPlacementJSONToDictionary(string json, out bool valid)
    {
        Dictionary<string, PlacementEntry>  placementDict = new Dictionary<string, PlacementEntry>();
        valid = false;

        //First check if string is valid json
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("String was null or empty.");
            return placementDict;
        }

        //Can we parse it into a JObject?
        try
        {
            var testObject = JObject.Parse(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into JObject: " + e.Message);
            return placementDict;
        }

        //Can we deserialize into string bool dict?
        try
        {
            placementDict = JsonConvert.DeserializeObject<Dictionary<string, PlacementEntry>>(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into Dictionary<string, bool>: " + e.Message);
            return placementDict;
        }

        //Do we have all required coordinates?
        for (char file = 'A'; file <= 'H'; file++)
        {
            for (int rank = 1; rank <= 8; rank++)
            {
                string coord = $"{file}{rank}";

                if (!placementDict.ContainsKey(coord))
                {
                    //Return early
                    Debug.LogError($"Key {coord} is missing!\nNote that other keys may be missing as well.");
                    return placementDict;
                }
            }
        }

        //All tests successful!
        valid = true;
        return placementDict;
    }

    public class RuleResponse
    {
        public bool status;
        public string reason;
    }

    public static RuleResponse ParseRuleResponseJSONToObject(string json, out bool valid)
    {
        RuleResponse response = new RuleResponse();
        valid = false;

        //First check if string is valid json
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("String was null or empty.");
            return response;
        }

        JObject testObject;
        //Can we parse it into a JObject?
        try
        {
            testObject = JObject.Parse(json);
            Debug.Log(testObject.ToString());
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into JObject: " + e.Message);
            return response;
        }

        //Check if contents are correct
        if (!testObject.ContainsKey("status"))
        {
            Debug.LogError("Status is missing.");
            return response;
        }
        if (!testObject.ContainsKey("reason"))
        {
            Debug.LogError("Reason is missing.");
            return response;
        }


        //Can we deserialize into string bool dict?
        try
        {
            response = JsonConvert.DeserializeObject<RuleResponse>(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into RuleResponse: " + e.Message);
            return response;
        }



        //All tests successful!
        valid = true;
        return response;
    }

    public class NewRuleResponse
    {
        public string rule;
    }

    public static NewRuleResponse ParseNewRuleResponseJSONToObject(string json, out bool valid)
    {
        NewRuleResponse response = new NewRuleResponse();
        valid = false;

        //First check if string is valid json
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("String was null or empty.");
            return response;
        }

        JObject testObject;
        //Can we parse it into a JObject?
        try
        {
            testObject = JObject.Parse(json);
            Debug.Log(testObject.ToString());
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into JObject: " + e.Message);
            return response;
        }

        //Check if contents are correct
        if (!testObject.ContainsKey("rule"))
        {
            Debug.LogError("Rule is missing.");
            return response;
        }

        //Can we deserialize into string bool dict?
        try
        {
            response = JsonConvert.DeserializeObject<NewRuleResponse>(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Could not parse into NewRuleResponse: " + e.Message);
            return response;
        }

        //All tests successful!
        valid = true;
        return response;
    }
}
