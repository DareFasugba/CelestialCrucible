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

    [Header("Sprint")]
    public float sprintSpeed = 11f;
    private bool sprintHeld;

    [Header("Camera FOV")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float fovLerpSpeed = 8f;



    private Animator animator;
    private bool isSprinting;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GroundCheck();
        HandleInput();
        ApplyMovement();
        ApplyJump();
        ApplyGravity();
        ApplyRotation();   // 🔥 added rotation like advanced controller
        animator.SetFloat("Speed", moveVector.magnitude);
        HandleCameraFOV();
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

        animator.SetBool("isGrounded", isGrounded);
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

        isSprinting =
        sprintHeld &&
        isGrounded &&
        input.magnitude > 0.1f &&
        !animator.GetBool("IsAttacking");
        
    float moveSpeed = isSprinting ? sprintSpeed : speed;
    float targetSpeed = moveSpeed * input.magnitude;
    
    // Animator
    animator.SetBool("IsSprinting", isSprinting);

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
    Vector3 flatMove = new Vector3(moveVector.x, 0, moveVector.z);

    // Only rotate if we’re actually moving
    if (flatMove.magnitude < 0.1f)
        return;

    Quaternion targetRot = Quaternion.LookRotation(flatMove.normalized);

    // Smooth + clamp rotation speed
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRot,
        rotateSpeed * Time.deltaTime
    );
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
            animator.SetTrigger("Jump");
        }
    }

    // Called by UI Button (OnPointerDown)
    public void SprintDown()
    {
        sprintHeld = true;
    }

    // Called by UI Button (OnPointerUp)
    public void SprintUp()
    {
        sprintHeld = false;
    }

    void HandleCameraFOV()
{
    if (playerCamera == null) return;
    if (!isGrounded)
    return;


    float targetFOV = isSprinting ? sprintFOV : normalFOV;

    playerCamera.fieldOfView = Mathf.Lerp(
        playerCamera.fieldOfView,
        targetFOV,
        fovLerpSpeed * Time.deltaTime
    );
    playerCamera.fieldOfView =
    Mathf.Clamp(playerCamera.fieldOfView, normalFOV, sprintFOV);

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



