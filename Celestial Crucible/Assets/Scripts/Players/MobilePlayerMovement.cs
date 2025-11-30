using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MobilePlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public MobileJoystick joystick;

    [Header("Movement")]
    public float speed = 7f;                 
    public float acceleration = 14f;        
    public float deceleration = 16f;       
    public float airControl = 0.55f;       

    [Header("Jumping")]
    public float jumpHeight = 2.2f;
    public float gravity = -35f;
    public float coyoteTime = 0.15f;        
    public float jumpBufferTime = 0.12f;    

    [Header("Rotation")]
    public float rotateSpeed = 14f;  // Smoother & responsive rotation

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
        ApplyRotation();   // 🔥 added rotation like advanced controller
    }

    // -----------------------------------------------------------
    // GROUND CHECK (coyote time handling)
    // -----------------------------------------------------------
    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            if (velocity.y < 0) velocity.y = -2f;
        }
        else coyoteCounter -= Time.deltaTime;
    }

    // -----------------------------------------------------------
    // JOYSTICK INPUT (camera-relative)
    // -----------------------------------------------------------
    void HandleInput()
    {
        float x = joystick.Horizontal();
        float z = joystick.Vertical();

        Vector3 input = (Camera.main.transform.right * x) + (Camera.main.transform.forward * z);
        input.y = 0f;
        input.Normalize();

        float targetSpeed = speed * input.magnitude;
        float currentSpeed = new Vector3(moveVector.x, 0, moveVector.z).magnitude;

        // Smooth acceleration + deceleration
        moveVector = Vector3.Lerp(
            moveVector,
            input * targetSpeed,
            (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
        );
    }

    // -----------------------------------------------------------
    // APPLY MOVEMENT
    // -----------------------------------------------------------
    void ApplyMovement()
    {
        float control = isGrounded ? 1f : airControl;
        controller.Move(moveVector * control * Time.deltaTime);
    }

    // -----------------------------------------------------------
    // ROTATION — faces movement direction like AAA mobile titles
    // -----------------------------------------------------------
    void ApplyRotation()
    {
        Vector3 flatVel = new Vector3(moveVector.x, 0, moveVector.z);

        if (flatVel.sqrMagnitude > 0.001f)   // prevents snapping & jitter
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    // -----------------------------------------------------------
    // JUMP + Buffer + Coyote Time
    // -----------------------------------------------------------
    public void JumpButton()
    {
        jumpBufferCounter = jumpBufferTime;
    }

    void ApplyJump()
    {
        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;
        }
    }

    // -----------------------------------------------------------
    // GRAVITY
    // -----------------------------------------------------------
    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // OPTIONAL — hook for Animator later
    public float GetSpeed() => moveVector.magnitude;
    public Vector3 GetMoveVector() => moveVector;
    public bool Grounded() => isGrounded;
}



