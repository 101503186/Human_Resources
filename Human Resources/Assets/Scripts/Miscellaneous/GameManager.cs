using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool vipIsDead = false;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject defeatScreen;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        Instance = this;
    }

    public void GameVictory()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        victoryScreen.SetActive(true);
    }

    public void GameLost()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        defeatScreen.SetActive(true);
    }
}
