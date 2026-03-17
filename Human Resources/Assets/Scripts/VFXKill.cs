using UnityEngine;

public class VFXKill : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
