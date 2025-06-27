using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public RulesManager rulesManager;

    [SerializeField]
    private string sceneName;

    private bool isLoading;

    public GameObject playScreen;
    public GameObject controlScreen;
    public GameObject conditionsScreen;

    public Toggle toggleRandom;
    public Toggle toggle3Win;
    public Toggle toggle4Win;
    public Toggle toggle5Win;

    public bool choosingLoseCon;
    public List<Button> conditionButtons = new List<Button>();

    public void Start()
    {
        if (rulesManager == null)
        {
            rulesManager = GameObject.Find("RulesManager").GetComponent<RulesManager>();
        }

        playScreen.SetActive(false);
        controlScreen.SetActive(false);
        conditionsScreen.SetActive(false);
        choosingLoseCon = false;
        conditionButtons.Clear();
    }

    public void StartGame()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadScene());
            isLoading = true;
            Debug.Log("Game starting...");
        }
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(sceneName);
    }

    public void PlayScreen()
    {
        if (!isLoading)
        {
            playScreen.SetActive(true);
        }
    }

    public void OpenControls()
    {
        if (!isLoading)
        {
            controlScreen.SetActive(true);
        }
    }

    public void CloseControls()
    {
        if (!isLoading)
        {
            controlScreen.SetActive(false);
        }
    }

    public void QuitGame()
    {
        if (!isLoading)
        {
            isLoading = true;
            Debug.Log("Game has now closed.");
            Application.Quit();
        }
    }

    public void SetRandom()
    {
        rulesManager.SetRandomConditions(toggleRandom.isOn);
    }

    public void Set3()
    {
        rulesManager.Set3Wins(toggle3Win.isOn);
    }

    public void Set4()
    {
        rulesManager.Set4Wins(toggle4Win.isOn);
    }

    public void Set5()
    {
        rulesManager.Set5Wins(toggle5Win.isOn);
    }

    public void OpenConditions()
    {
        // Reset the conditions every time we open the scene to avoid overlaps
        rulesManager.SetWinCon("");
        rulesManager.SetLoseCon("");
        choosingLoseCon = false;
        conditionsScreen.SetActive(true);
    }

    public void ConditionSelected(string condition)
    {
        Debug.Log(condition);

        bool valid = true;

        if (choosingLoseCon)
        {
            // Can only choose if not the same
            if (condition == rulesManager.GetWinCon())
            {
                valid = false;
            }
        }

        if (!valid)

        {
            Debug.Log("Lose Condition cannot be the same as Win Condition.");
        }

        else
        {
            if(!choosingLoseCon)
            {
                rulesManager.SetWinCon(condition);
                choosingLoseCon = true;
            }

            else
            {
                rulesManager.SetLoseCon(condition);
                conditionsScreen.SetActive(false);
            }
        }
    }
}
