using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth playerHealthScript;

    [SerializeField] private float playerHealth = 10f;
    public bool playerIsAlive;

    PlayerMovement playerMovement;

    private void Awake()
    {
        playerHealthScript = this;
    }

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerIsAlive = true;
    }

    public void TakeDamage(float damage)
    {
        playerHealth -= damage;

        if(playerHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        playerIsAlive = false;
        playerMovement.enabled = false;

        GameManager.Instance.GameLost();
    }
}
