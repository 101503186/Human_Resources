using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public float detectionRange = 10;
    public float fieldOfView = 45;

    bool isInFOV, isInRange, isNotHidden;

    public GameObject player;

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.green, Mathf.Infinity, true);

        RaycastHit hit;
        if(Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, 10))
        {
            Debug.DrawRay(transform.position, (player.transform.position - transform.position), Color.lavender);
            if (hit.transform == player.transform)
            {
                isNotHidden = true;
            }
        }

        Vector3 side1 = player.transform.position - transform.position;
        Vector3 side2 = transform.forward;
        float angle = Vector3.SignedAngle(side1, side2, Vector3.up);
        if(angle < fieldOfView && angle > -1*fieldOfView)
        {
            isInFOV = true;
        }
    }
}
