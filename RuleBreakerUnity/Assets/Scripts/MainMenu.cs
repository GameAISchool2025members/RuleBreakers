using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private string sceneName;

    private bool isLoading;

    public void StartGame()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadScene());
            isLoading = true;
        }
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGame();
        }
    }
}
