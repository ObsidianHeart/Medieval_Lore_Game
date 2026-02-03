using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public TargetLock targetLockScript; // <--- NEW: Link to lock-on system

    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float slideSpeed = 5f;
    public float rotationSpeed = 100f;

    [Header("Durations")]
    public float slideDuration = 0.3f;
    public float jumpDelay = 0.2f;

    [Header("Physics")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Collider Settings")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float standCenterY = 0.9f;
    public float crouchCenterY = 0.5f;

    // --- Private Variables ---
    private Animator animator;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private bool isCrouching;
    private bool isSliding;

    void Start()
    {
        // <--- NEW: Hide Mouse Cursor for better aim
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        if (targetLockScript == null) targetLockScript = GetComponent<TargetLock>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        isGrounded = controller.isGrounded;

        // --- 0. SLIDING LOGIC ---
        if (isSliding)
        {
            Vector3 slideMotion = transform.forward * slideSpeed;
            slideMotion.y += gravity;
            controller.Move(slideMotion * Time.deltaTime);
            return;
        }

        // --- 1. RUN CANCEL ---
        bool shiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        if (isCrouching && shiftPressed)
        {
            isCrouching = false;
            StandUp();
        }

        // --- 2. CROUCH & SLIDE INPUT ---
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            bool isMovingForward = Keyboard.current.wKey.isPressed;
            if (shiftPressed && isMovingForward && !isCrouching)
            {
                StartCoroutine(SlideSequence());
            }
            else
            {
                isCrouching = !isCrouching;
                if (isCrouching) CrouchDown(); else StandUp();
            }
        }

        // --- 3. MOVEMENT CALCULATION ---
        float currentSpeed = 0f;
        float targetSpeed = isCrouching ? crouchSpeed : (shiftPressed ? runSpeed : walkSpeed);

        // Forward / Backward (W / S)
        if (Keyboard.current.wKey.isPressed) currentSpeed = targetSpeed;
        else if (Keyboard.current.sKey.isPressed) currentSpeed = -walkSpeed;

        Vector3 finalMove = transform.forward * currentSpeed;

        // --- 4. ROTATION & STRAFE LOGIC (THE BIG EDIT) ---
        // Check if we are Locked On
        if (targetLockScript != null && targetLockScript.isLocked)
        {
            // LOCKED MODE: 
            // 1. Don't rotate manually (TargetLock script forces us to face enemy).
            // 2. Use A/D to STRAFE (Move Left/Right) instead of turning.

            float strafeInput = 0f;
            if (Keyboard.current.aKey.isPressed) strafeInput = -1f;
            if (Keyboard.current.dKey.isPressed) strafeInput = 1f;

            // Add Strafe velocity to Forward velocity
            Vector3 strafeMove = transform.right * strafeInput * (isCrouching ? crouchSpeed : walkSpeed);
            finalMove += strafeMove;
        }
        else
        {
            // NORMAL MODE (Your Original Code):
            // Use A/D to ROTATE the character.

            float turnDirection = 0f;
            if (Keyboard.current.aKey.isPressed) turnDirection = -1f;
            if (Keyboard.current.dKey.isPressed) turnDirection = 1f;

            transform.Rotate(0, turnDirection * rotationSpeed * Time.deltaTime, 0);
        }

        // Apply Movement
        controller.Move(finalMove * Time.deltaTime);

        // --- 5. JUMPING ---
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping && !isCrouching)
        {
            StartCoroutine(JumpSequence());
        }

        // --- 6. GRAVITY ---
        if (isGrounded && velocity.y < 0 && !isJumping) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- 7. ANIMATOR UPDATE ---
        animator.SetFloat("Speed", currentSpeed);
    }

    // --- HELPER FUNCTIONS (UNCHANGED) ---

    void CrouchDown()
    {
        controller.height = crouchHeight;
        controller.center = new Vector3(0, crouchCenterY, 0);
        animator.SetBool("IsCrouching", true);
    }

    void StandUp()
    {
        controller.height = standHeight;
        controller.center = new Vector3(0, standCenterY, 0);
        animator.SetBool("IsCrouching", false);
    }

    IEnumerator SlideSequence()
    {
        isSliding = true;
        animator.SetTrigger("Slide");
        CrouchDown();
        yield return new WaitForSeconds(slideDuration);
        isSliding = false;
    }

    IEnumerator JumpSequence()
    {
        isJumping = true;
        float currentSpeed = animator.GetFloat("Speed");
        float actualDelay = (currentSpeed > 0.1f) ? 0f : jumpDelay;
        animator.SetTrigger("Jump");
        if (actualDelay > 0) yield return new WaitForSeconds(actualDelay);
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }
}