using UnityEngine;

public class RulesManager : MonoBehaviour
{
    public static RulesManager rulesManager;

    public static string winCon = "4InARow";
    public static string loseCon = "3InARow";

    public static bool randomCons = false;
    public static bool threeWins = false;
    public static bool fourWins = false;
    public static bool fiveWins = false;

    void Awake()
    {
        if (rulesManager == null) // If there is no instance already
        {
            DontDestroyOnLoad(gameObject); // Keep the GameObject, this component is attached to, across different scenes
            rulesManager = this;
        }
        else if (rulesManager != this) // If there is already an instance and it's not `this` instance
        {
            Destroy(gameObject); // Destroy the GameObject, this component is attached to
        }
    }

    public void SetRandomConditions(bool b)
    {
        randomCons = b;
        //Debug.Log("Rand: " + randomCons);
    }

    public bool GetRandomConditions()
    {
        return randomCons;
    }

    public void Set3Wins(bool b)
    {
        threeWins = b;
        //Debug.Log("Set 3: " + threeWins);
    }

    public void Set4Wins(bool b)
    {
        fourWins = b;
        //Debug.Log("Set 4: " + fourWins);
    }

    public void Set5Wins(bool b)
    {
        fiveWins = b;
        //Debug.Log("Set 5: " + fiveWins);
    }

    public bool Get3Wins()
    {
        return threeWins;
    }

    public bool Get4Wins()
    {
        return fourWins;
    }

    public bool Get5Wins()
    {
        return fiveWins;
    }

    public void SetWinCon(string s)
    {
        winCon = s;
    }

    public void SetLoseCon(string s)
    {
        loseCon = s;
    }

    public string GetWinCon()
    {
        return winCon;
    }

    public string GetLoseCon()
    {
        return loseCon;
    }
}
