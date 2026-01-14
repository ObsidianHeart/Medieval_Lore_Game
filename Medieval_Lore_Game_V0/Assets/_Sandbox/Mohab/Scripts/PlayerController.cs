using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 8f;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.2f; // Increased slightly for reliability
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isCrouching = false;

    private float originalHeight;
    private Vector3 originalCenter;
    private CapsuleCollider capsule;

    private PlayerControls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        if (capsule == null)
        {
            Debug.LogError("CapsuleCollider required on player!");
            enabled = false;
            return;
        }

        originalHeight = capsule.height;
        originalCenter = capsule.center;

        // Freeze rotation so player doesn't tip over
        rb.freezeRotation = true;

        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Jump.performed += _ => Jump();
        controls.Player.Crouch.performed += _ => ToggleCrouch(true);
        controls.Player.Crouch.canceled += _ => ToggleCrouch(false);
    }

    void OnDisable()
    {
        controls?.Player.Disable();
    }

    void FixedUpdate()
    {
        // 1. Read Input
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // 2. Run logic
        CheckGrounded(); // Best to check ground right before moving/jumping
        Move();
    }

    void CheckGrounded()
    {
        // Origin should be slightly above the bottom of the capsule
        Vector3 spherePosition = transform.position + originalCenter + Vector3.down * (originalHeight / 2.0f);

        // Check a small sphere at the feet
        isGrounded = Physics.CheckSphere(spherePosition, capsule.radius * 0.9f, groundLayer);
    }

    void Move()
    {
        float currentSpeed = moveSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        else if (controls.Player.Sprint.IsPressed()) currentSpeed = sprintSpeed;

        // Calculate target velocity (Only X and Z)
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * currentSpeed;

        // Apply velocity to RB, but PRESERVE the current Y velocity (Gravity/Jump)
        // Note: Use 'rb.velocity' for Unity 2022/2021. Use 'rb.linearVelocity' only for Unity 6+.
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    void Jump()
    {
        if (isGrounded && !isCrouching)
        {
            // Reset vertical velocity before jumping ensures consistent jump height
            // even if we were slightly falling or moving down a slope.
            Vector3 currentVel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(currentVel.x, 0, currentVel.z);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void ToggleCrouch(bool crouch)
    {
        // If we are trying to crouch...
        if (crouch)
        {
            if (!isCrouching)
            {
                isCrouching = true;

                // Shrink height
                capsule.height = originalHeight * 0.5f;

                // Move Center DOWN by half of the height difference
                // This keeps the bottom of the capsule at the same execution point (the feet)
                float heightDifference = originalHeight - capsule.height;
                capsule.center = originalCenter - new Vector3(0, heightDifference / 2, 0);
            }
        }
        // If we are trying to STAND UP...
        else
        {
            if (isCrouching)
            {
                // Check for ceiling before standing up
                // We cast a ray or sphere UP from the current position
                float castDistance = originalHeight - capsule.height + 0.1f;
                if (!Physics.SphereCast(transform.position + capsule.center, capsule.radius * 0.5f, Vector3.up, out _, castDistance, groundLayer))
                {
                    isCrouching = false;
                    capsule.height = originalHeight;
                    capsule.center = originalCenter;
                }
            }
        }
    }
}