using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float slideSpeed = 5f;       // Tighter slide speed
    public float rotationSpeed = 100f;

    [Header("Durations")]
    public float slideDuration = 0.3f;  // Shorter, snappier slide
    public float jumpDelay = 0.2f;      // Delay for standing jump (wind-up)

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
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Safety check
        if (Keyboard.current == null) return;

        isGrounded = controller.isGrounded;

        // --- 0. SLIDING LOGIC (Blocks everything else) ---
        if (isSliding)
        {
            // While sliding, we FORCE forward movement based on where the character is facing
            Vector3 slideMotion = transform.forward * slideSpeed;
            slideMotion.y += gravity; // Keep gravity so we don't float
            controller.Move(slideMotion * Time.deltaTime);
            return; // Stop here! Do not execute the rest of the Update loop.
        }

        // --- 1. RUN CANCEL (Crouch -> Run) ---
        // If holding Shift while crouching, stand up immediately
        bool shiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        if (isCrouching && shiftPressed)
        {
            isCrouching = false;
            StandUp();
        }

        // --- 2. CROUCH & SLIDE INPUT ---
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            // Logic: Are we running forward?
            bool isMovingForward = Keyboard.current.wKey.isPressed;

            // If Running (Shift+W) AND not already crouching -> SLIDE
            if (shiftPressed && isMovingForward && !isCrouching)
            {
                StartCoroutine(SlideSequence());
            }
            else
            {
                // Otherwise -> Toggle Crouch
                isCrouching = !isCrouching;
                if (isCrouching) CrouchDown(); else StandUp();
            }
        }

        // --- 3. MOVEMENT CALCULATION ---
        float currentSpeed = 0f;

        // Priority: Crouch Speed > Run Speed > Walk Speed
        float targetSpeed = isCrouching ? crouchSpeed : (shiftPressed ? runSpeed : walkSpeed);

        // Move Forward / Backward
        if (Keyboard.current.wKey.isPressed) currentSpeed = targetSpeed;
        else if (Keyboard.current.sKey.isPressed) currentSpeed = -walkSpeed;

        Vector3 moveVelocity = transform.forward * currentSpeed;
        controller.Move(moveVelocity * Time.deltaTime);

        // Rotate Left / Right
        float turnDirection = 0f;
        if (Keyboard.current.aKey.isPressed) turnDirection = -1f;
        if (Keyboard.current.dKey.isPressed) turnDirection = 1f;

        transform.Rotate(0, turnDirection * rotationSpeed * Time.deltaTime, 0);

        // --- 4. JUMPING ---
        // We only jump if grounded, not already jumping, and NOT crouching
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isJumping && !isCrouching)
        {
            StartCoroutine(JumpSequence());
        }

        // --- 5. GRAVITY ---
        // If on ground, keep velocity slightly negative to stick to floor
        if (isGrounded && velocity.y < 0 && !isJumping) velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- 6. ANIMATOR UPDATE ---
        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));
    }

    // --- HELPER FUNCTIONS ---

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

        // Trigger Animation
        animator.SetTrigger("Slide");

        // Shrink Collider immediately
        CrouchDown();

        // Wait for the slide duration
        yield return new WaitForSeconds(slideDuration);

        // Stop sliding state (but stay crouched)
        isSliding = false;
    }

    IEnumerator JumpSequence()
    {
        isJumping = true;

        // Dynamic Delay: No delay if we are running!
        float currentSpeed = animator.GetFloat("Speed");
        float actualDelay = (currentSpeed > 0.1f) ? 0f : jumpDelay;

        animator.SetTrigger("Jump");

        if (actualDelay > 0) yield return new WaitForSeconds(actualDelay);

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        yield return new WaitForSeconds(0.1f); // Short cooldown

        isJumping = false;
    }
}