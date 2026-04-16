using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject[] mainMenuElements;
    public GameObject[] instructionsElements;

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Instructions()
    {
        foreach(var element in mainMenuElements)
        {
            element.gameObject.SetActive(false);
        }

        foreach (var element in instructionsElements)
        {
            element.gameObject.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Return()
    {
        foreach (var element in instructionsElements)
        {
            element.gameObject.SetActive(false);
        }

        foreach (var element in mainMenuElements)
        {
            element.gameObject.SetActive(true);
        }
    }
}
