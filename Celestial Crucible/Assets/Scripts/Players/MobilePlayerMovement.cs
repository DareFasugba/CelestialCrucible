using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MobilePlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public MobileJoystick joystick;

    [Header("Movement")]
    public float speed = 7f;
    public float sprintSpeed = 11f;
    public float acceleration = 14f;
    public float deceleration = 16f;
    public float airControl = 0.55f;

    [Header("Ground Snap")]
    public float groundSnapForce = 6f;


    [Header("Jumping")]
    public float jumpHeight = 2.2f;
    public float gravity = -35f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.12f;

    [Header("Rotation")]
    public float rotateSpeed = 14f;

    [Header("Sprint")]
    private bool sprintHeld;

    [Header("Camera FOV")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float fovLerpSpeed = 8f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float sprintDrain = 20f;     // per second
    public float jumpCost = 15f;
    public float regenRate = 25f;       // per second
    public float regenDelay = 0.8f;

    [Header("Stamina UI")]
    public RectTransform staminaFill;

    private float currentStamina;
    private float regenTimer;
    private float staminaBarWidth;

    private Vector3 velocity;
    private Vector3 moveVector;
    private bool isGrounded;
    private bool isSprinting;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        currentStamina = maxStamina;

        if (staminaFill != null)
            staminaBarWidth = staminaFill.sizeDelta.x;
    }

    void Update()
    {
        GroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJump();
        ApplyGravity();
        ApplyRotation();
        HandleStamina();
        HandleCameraFOV();

        animator.SetFloat("Speed", moveVector.magnitude);
    }

    // -----------------------------------------------------------
    // GROUND CHECK
    // -----------------------------------------------------------
    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else coyoteCounter -= Time.deltaTime;

        animator.SetBool("isGrounded", isGrounded);
    }

    // -----------------------------------------------------------
    // INPUT
    // -----------------------------------------------------------
    void HandleInput()
    {
        float x = joystick.Horizontal();
        float z = joystick.Vertical();

        Vector3 input =
            Camera.main.transform.right * x +
            Camera.main.transform.forward * z;

        input.y = 0f;
        input.Normalize();

        isSprinting =
            sprintHeld &&
            isGrounded &&
            input.magnitude > 0.1f &&
            currentStamina > 0f &&
            !animator.GetBool("IsAttacking");

        float moveSpeed = isSprinting ? sprintSpeed : speed;
        float targetSpeed = moveSpeed * input.magnitude;

        animator.SetBool("IsSprinting", isSprinting);

        float currentSpeed = new Vector3(moveVector.x, 0, moveVector.z).magnitude;

        moveVector = Vector3.Lerp(
            moveVector,
            input * targetSpeed,
            (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
        );
    }

    // -----------------------------------------------------------
    // MOVEMENT
    // -----------------------------------------------------------
    void ApplyMovement()
    {
        float control = isGrounded ? 1f : airControl;
        controller.Move(moveVector * control * Time.deltaTime);
    }

    // -----------------------------------------------------------
    // ROTATION
    // -----------------------------------------------------------
    void ApplyRotation()
    {
        Vector3 flatMove = new Vector3(moveVector.x, 0, moveVector.z);
        if (flatMove.magnitude < 0.1f) return;

        Quaternion targetRot = Quaternion.LookRotation(flatMove.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    // -----------------------------------------------------------
    // JUMP
    // -----------------------------------------------------------
    public void JumpButton()
    {
        if (currentStamina >= jumpCost)
            jumpBufferCounter = jumpBufferTime;
    }

    void ApplyJump()
    {
        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;

            currentStamina -= jumpCost;
            regenTimer = regenDelay;

            animator.SetTrigger("Jump");
        }
    }

    // -----------------------------------------------------------
    // GRAVITY
    // -----------------------------------------------------------
    void ApplyGravity()
{
    if (isGrounded && velocity.y <= 0f)
    {
        // Strong downward snap to keep feet planted
        velocity.y = -groundSnapForce;
    }
    else
    {
        velocity.y += gravity * Time.deltaTime;
    }

    controller.Move(Vector3.up * velocity.y * Time.deltaTime);
}


    // -----------------------------------------------------------
    // STAMINA SYSTEM (RECTTRANSFORM BASED)
    // -----------------------------------------------------------
    void HandleStamina()
    {
        if (isSprinting)
        {
            currentStamina -= sprintDrain * Time.deltaTime;
            regenTimer = regenDelay;
        }
        else
        {
            if (regenTimer > 0)
                regenTimer -= Time.deltaTime;
            else
                currentStamina += regenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (staminaFill != null)
        {
            float percent = currentStamina / maxStamina;
            staminaFill.sizeDelta = new Vector2(
                staminaBarWidth * percent,
                staminaFill.sizeDelta.y
            );
        }
    }

    // -----------------------------------------------------------
    // CAMERA FOV
    // -----------------------------------------------------------
    void HandleCameraFOV()
    {
        if (playerCamera == null || !isGrounded) return;

        float targetFOV = isSprinting ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovLerpSpeed * Time.deltaTime
        );
    }

    // -----------------------------------------------------------
    // UI BUTTON HOOKS
    // -----------------------------------------------------------
    public void SprintDown() => sprintHeld = true;
    public void SprintUp() => sprintHeld = false;

    // OPTIONAL ACCESSORS
    public float GetStamina() => currentStamina;
    public bool IsSprinting() => isSprinting;
}




