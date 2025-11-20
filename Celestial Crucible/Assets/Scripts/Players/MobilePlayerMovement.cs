using UnityEngine;

public class MobilePlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public MobileJoystick joystick;

    [Header("Movement")]
    public float speed = 7f;                 // Max running speed
    public float acceleration = 14f;         // Speed up rate
    public float deceleration = 16f;         // Slow down rate
    public float airControl = 0.55f;         // Control in air

    [Header("Jumping")]
    public float jumpHeight = 2.2f;
    public float gravity = -35f;
    public float coyoteTime = 0.15f;         // Jump forgiveness after leaving ground
    public float jumpBufferTime = 0.12f;     // Press jump early

    [Header("Ground")]
    public float groundSnapForce = 5f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private Vector3 moveVector;

    private bool isGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;

    void Update()
    {
        GroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJump();
        ApplyGravity();
    }

    // ------------------------------------------
    // GROUND CHECK
    // ------------------------------------------
    void GroundCheck()
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

    // ------------------------------------------
    // MOBILE INPUT (Joystick Only)
    // ------------------------------------------
    void HandleInput()
    {
        float x = joystick.Horizontal();
        float z = joystick.Vertical();

        Vector3 input = Camera.main.transform.right * x
                      + Camera.main.transform.forward * z;

        input.y = 0;
        input.Normalize();

        float targetMagnitude = input.magnitude * speed;
        float currentMagnitude = new Vector3(moveVector.x, 0, moveVector.z).magnitude;

        // Acceleration / deceleration smoothing
        if (targetMagnitude > currentMagnitude)
            moveVector = Vector3.Lerp(moveVector, input * targetMagnitude, acceleration * Time.deltaTime);
        else
            moveVector = Vector3.Lerp(moveVector, input * targetMagnitude, deceleration * Time.deltaTime);
    }

    // ------------------------------------------
    // APPLY MOVEMENT (Ground + Air Control)
    // ------------------------------------------
    void ApplyMovement()
    {
        float control = isGrounded ? 1f : airControl;

        Vector3 finalMove = moveVector * control;
        controller.Move(finalMove * Time.deltaTime);
    }

    // ------------------------------------------
    // JUMP + Coyote Time + Jump Buffer
    // ------------------------------------------
    public void JumpButton()
    {
        jumpBufferCounter = jumpBufferTime;  // Mobile button adds buffer
    }

    void ApplyJump()
    {
        jumpBufferCounter -= Time.deltaTime;

        // Buffered jump + coyote time
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;
        }
    }

    // ------------------------------------------
    // GRAVITY + Vertical Move
    // ------------------------------------------
    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}


