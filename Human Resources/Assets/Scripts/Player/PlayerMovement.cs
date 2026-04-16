using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float walkSpeed = 6.0f;
    private float sprintSpeed = 10.0f;
    private float crouchSpeed = 2.0f;
    private float jumpHeight = 1.5f;
    private float gravityValue = -14.7f;

    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    public CharacterController controller;
    public Transform cam;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private GameObject playerObject;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    public InputActionReference crouchAction;
    public InputActionReference aimAction;
    public InputActionReference shootAction;

    public bool isAiming;

    [Header("Gun")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform barrelOpening;
    [SerializeField] private AudioSource audioSource;
    public AudioClip gunShotClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        crouchAction.action.Enable();
        aimAction.action.Enable();
        shootAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
        crouchAction.action.Disable();
        aimAction.action.Disable();
        shootAction.action.Disable();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        
        Vector3 basePosition = playerObject.transform.position;
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        if (groundedPlayer)
        {
            if (playerVelocity.y < -2f)
            {
                playerVelocity.y = -2f;
            }
        }

        // Read input
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;

        bool isSprinting = sprintAction.action.IsPressed();
        bool isCrouching = crouchAction.action.IsPressed();
        isAiming = aimAction.action.IsPressed();
        bool isShooting = shootAction.action.WasPressedThisFrame();
        Vector3 aimOffset = new Vector3(0.75f,-0.1f,2f);

        float currentSpeed;

        if(isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        if (isAiming)
        {
            transform.rotation = Quaternion.LookRotation(camForward);

            if (isShooting)
            {
                Instantiate(muzzleFlash, barrelOpening);
                audioSource.PlayOneShot(gunShotClip);

                Vector3 forward = cam.forward;
                RaycastHit hit;

                if (Physics.Raycast(cam.position, forward, out hit, 100))
                {
                    Debug.DrawLine(cam.position, hit.point, Color.red, 0.1f);

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

                    Rigidbody rb = hit.rigidbody;

                    if (hit.collider.CompareTag("EnemyBody"))
                    {
                        EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(1);
                        }
                    }
                    if (hit.collider.CompareTag("EnemyHead"))
                    {
                        EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(2);
                        }
                    }

                    if (rb != null)
                    {
                        Vector3 forceDirection = (hit.point - cam.position).normalized;

                        float forceAmount = 60f;

                        //Applies force away from where bullet landed
                        rb.AddForceAtPosition(forceDirection * forceAmount, hit.point, ForceMode.Impulse);
                    }
                }

                isShooting = false;
            }
        }

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

            if (!isAiming)
            {
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        if (groundedPlayer && jumpAction.action.WasPressedThisFrame())
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    void FixedUpdate()
    {
        //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //Debug.DrawRay(cam.position, forward, Color.green);
    }

    void Shooting()
    {
        Debug.Log("Shot");
    }
}
