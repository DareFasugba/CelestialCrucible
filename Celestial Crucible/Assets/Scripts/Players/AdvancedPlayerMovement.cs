using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float airControl = 0.5f;
    public float rotationSmooth = 10f; // how fast the player rotates to face movement

    [Header("Jumping")]
    public float jumpHeight = 2.2f;
    public float gravity = -35f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.12f;

    [Header("Ground")]
    public float groundSnapForce = 5f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private Vector3 moveDirection;
    private Animator animator;

    float coyoteCounter;
    float jumpBufferCounter;
    bool isGrounded;

    public Vector3 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;
    public Vector3 GetMoveDirection() => moveDirection;

//sprint features
    public float sprintSpeed = 9f;
    public bool allowSprint = true;

    [Header("Camera FOV")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float sprintFOV = 72f;
    public float fovLerpSpeed = 8f;

    bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); //fixed
    }

    void Update()
    {
        HandleGroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJumping();
        ApplyGravity();
        ApplyRotation();
        animator.SetFloat("Speed", moveDirection.magnitude);
        HandleCameraFOV();
    }

void OnGUI()
{
    GUI.Label(new Rect(10, 10, 300, 30), "MoveDir: " + moveDirection);
}

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;

            if (velocity.y < 0)
                velocity.y = -2f; // snap to ground (prevents floatiness)
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
        animator.SetBool("isGrounded", isGrounded);
    }

    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Move relative to camera
        Vector3 inputDir = (Camera.main.transform.right * x + Camera.main.transform.forward * z);
        inputDir.y = 0f;
        inputDir.Normalize();

        // Smooth acceleration
        isSprinting = allowSprint
                   && isGrounded
                   && inputDir.magnitude > 0.1f
                   && Input.GetKey(KeyCode.LeftShift);

        if (animator.GetBool("IsAttacking"))
        {
            isSprinting = false;
        }

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        float targetSpeed = speed * inputDir.magnitude;

        // Animator update
        animator.SetBool("IsSprinting", isSprinting);
        float currentXZSpeed = new Vector3(moveDirection.x, 0, moveDirection.z).magnitude;

        if (targetSpeed > currentXZSpeed)
            moveDirection = Vector3.Lerp(moveDirection, inputDir * targetSpeed, acceleration * Time.deltaTime);
        else
            moveDirection = Vector3.Lerp(moveDirection, inputDir * targetSpeed, deceleration * Time.deltaTime);
    }

    void ApplyMovement()
    {
        // Full control on ground, partial control in air
        float control = isGrounded ? 1f : airControl;
        Vector3 horizontalMove = moveDirection * control;
        controller.Move(horizontalMove * Time.deltaTime);
    }

    void ApplyJumping()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;

            animator.SetTrigger("Jump");
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ApplyRotation()
{
    Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z);

    if (lookDir.sqrMagnitude > 0.001f)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
    }
}

void HandleCameraFOV()
{
    if (playerCamera == null)
        return;

    // Optional: no FOV boost in air
    if (!isGrounded)
        return;

    float targetFOV = isSprinting ? sprintFOV : normalFOV;

    playerCamera.fieldOfView = Mathf.Lerp(
        playerCamera.fieldOfView,
        targetFOV,
        fovLerpSpeed * Time.deltaTime
    );
}


}


