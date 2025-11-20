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

    float coyoteCounter;
    float jumpBufferCounter;
    bool isGrounded;

    public Vector3 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleGroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJumping();
        ApplyGravity();
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
        float targetSpeed = walkSpeed * inputDir.magnitude;
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
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

