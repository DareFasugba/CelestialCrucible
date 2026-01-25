using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 9f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float airControl = 0.5f;

    [Header("Ground Snap")]
    public float groundSnapForce = 6f;


    [Header("Jumping")]
    public float jumpHeight = 2.2f;
    public float gravity = -35f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.12f;

    [Header("Camera FOV")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float sprintFOV = 72f;
    public float fovLerpSpeed = 8f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float sprintStaminaDrain = 20f;
    public float jumpStaminaCost = 15f;
    public float staminaRegenRate = 25f;
    public float regenDelay = 0.8f;

    [Header("UI")]
    public Image staminaFillImage;

    Vector3 velocity;
    Vector3 moveDirection;

    Animator animator;

    float coyoteCounter;
    float jumpBufferCounter;
    float currentStamina;
    float regenTimer;

    bool isGrounded;
    bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    void Update()
    {
        HandleGroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJumping();
        ApplyGravity();
        ApplyRotation();
        HandleCameraFOV();
        HandleStamina();

        animator.SetFloat("Speed", moveDirection.magnitude);
    }

    // --------------------------------------------------
    // GROUND CHECK
    // --------------------------------------------------
    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            if (velocity.y < 0)
                velocity.y += 0;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        animator.SetBool("isGrounded", isGrounded);
    }

    // --------------------------------------------------
    // INPUT + SPRINT
    // --------------------------------------------------
    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir =
            Camera.main.transform.right * x +
            Camera.main.transform.forward * z;

        inputDir.y = 0f;
        inputDir.Normalize();

        isSprinting =
            isGrounded &&
            inputDir.magnitude > 0.1f &&
            Input.GetKey(KeyCode.LeftShift) &&
            currentStamina > 0f &&
            !animator.GetBool("IsAttacking");

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        float targetSpeed = speed * inputDir.magnitude;

        float currentSpeed = new Vector3(moveDirection.x, 0, moveDirection.z).magnitude;

        moveDirection = Vector3.Lerp(
            moveDirection,
            inputDir * targetSpeed,
            (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
        );

        animator.SetBool("IsSprinting", isSprinting);
    }

    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------
    void ApplyMovement()
    {
        float control = isGrounded ? 1f : airControl;
        controller.Move(moveDirection * control * Time.deltaTime);
    }

    // --------------------------------------------------
    // JUMPING (STAMINA COST)
    // --------------------------------------------------
    void ApplyJumping()
    {
        if (Input.GetButtonDown("Jump") && currentStamina >= jumpStaminaCost)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;

            currentStamina -= jumpStaminaCost;
            regenTimer = regenDelay;

            animator.SetTrigger("Jump");
        }
    }

    // --------------------------------------------------
    // GRAVITY
    // --------------------------------------------------
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


    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------
    void ApplyRotation()
    {
        Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);

        if (lookDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
    }

    // --------------------------------------------------
    // CAMERA FOV
    // --------------------------------------------------
    void HandleCameraFOV()
    {
        if (!playerCamera || !isGrounded)
            return;

        float targetFOV = isSprinting ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovLerpSpeed * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // STAMINA SYSTEM
    // --------------------------------------------------
    void HandleStamina()
    {
        if (isSprinting)
        {
            currentStamina -= sprintStaminaDrain * Time.deltaTime;
            regenTimer = regenDelay;
        }
        else
        {
            if (regenTimer > 0)
                regenTimer -= Time.deltaTime;
            else
                currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (staminaFillImage)
            staminaFillImage.fillAmount = currentStamina / maxStamina;
    }
}



