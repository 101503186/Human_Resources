using UnityEngine;

public class PlayerSpawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.vipIsDead)
            {
                GameManager.Instance.GameVictory();
            }
        }
    }
}
