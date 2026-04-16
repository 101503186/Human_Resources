using UnityEngine;

public class EnemyGunScript : MonoBehaviour
{
    [Header("Gun Details")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform barrelOpening;
    [SerializeField] private AudioSource audioSource;
    public AudioClip gunShotClip;

    [Header("Gun Settings")]
    [SerializeField] private int magSize;
    [SerializeField] private float reloadTime;
    [SerializeField] private float fireRate;
    [SerializeField] private float accuracy;

    private int currentAmmo;
    private float nextShootTime;
    private float timeReloading;
    private bool isReloading;

    private void Start()
    {
        currentAmmo = magSize;
    }

    private void Update()
    {
        if (isReloading)
        {
            UpdateReload();
        }
    }

    public void Shoot(Transform shootOrigin)
    {
        if (!CanShoot()) return;

        nextShootTime = Time.time + (1f / fireRate);
        Instantiate(muzzleFlash, barrelOpening);
        audioSource.PlayOneShot(gunShotClip);
        currentAmmo--;

        Vector3 direction = Quaternion.Euler(
            Random.Range(-accuracy, accuracy),
            Random.Range(-accuracy, accuracy),
            0
        ) * shootOrigin.forward;

        RaycastHit hit;
        if(Physics.Raycast(shootOrigin.position, direction, out hit, 100f))
        {
            Debug.DrawLine(shootOrigin.position, hit.point, Color.yellow, 0.1f);

            GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point, Quaternion.identity);

            // Align with surface
            bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0);

            // Offset slightly to prevent z-fighting
            bulletHole.transform.position += hit.normal * 0.001f;

            // Parent to object so it moves with it
            if (hit.rigidbody != null)
            {
                bulletHole.transform.SetParent(hit.rigidbody.transform);
            }
            else
            {
                bulletHole.transform.SetParent(hit.collider.transform);
            }
            Destroy(bulletHole, 30f);

            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth player = hit.collider.GetComponentInParent<PlayerHealth>();

                if (player != null)
                {
                    player.TakeDamage(1f); // adjust damage as needed
                }
            }
        }

        if(currentAmmo <= 0)
        {
            StartReload();
        }
    }

    private bool CanShoot()
    {
        if(isReloading) return false;
        if (Time.time < nextShootTime) return false;
        if(currentAmmo <= 0) return false;

        return true;
    }

    //Start reload (called once)
    private void StartReload()
    {
        if (isReloading) return;

        isReloading = true;
        timeReloading = 0f;
    }

    //Update reload every frame
    private void UpdateReload()
    {
        timeReloading += Time.deltaTime;

        if (timeReloading >= reloadTime)
        {
            currentAmmo = magSize;
            isReloading = false;
            timeReloading = 0f;
            nextShootTime = Time.time + 0.1f;
        }
    }
}
