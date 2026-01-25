using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SwimmingController : MonoBehaviour
{
    CharacterController controller;
    Animator animator;

    [Header("Swimming Movement")]
    public float swimSpeed = 4f;
    public float swimAcceleration = 8f;
    public float rotationSpeed = 10f;

    [Header("Water Surface Heights")]
public float treadOffset = 2.3f;   // lower in water (idle)
public float swimOffset = 1.9f;    // higher in water (moving)
public float surfaceSnapSpeed = 6f;

    [Header("Disable While Swimming")]
    public MonoBehaviour keyboardMovement; // AdvancedPlayerMovement
    public MonoBehaviour mobileMovement;   // MobilePlayerMovement
    public MonoBehaviour combatScript;
    public MobileJoystick joystick;        // optional

    Vector3 swimVelocity;
    bool isSwimming;
    float waterSurfaceY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        isSwimming = false;
        animator.SetBool("IsSwimming", false);
        animator.SetFloat("SwimSpeed", 0f);
    }

    void Update()
    {
        if (!isSwimming) return;

        HandleSwimInput();
        ApplySwimMovement();
        ApplyRotation();

        float flatSpeed = new Vector3(swimVelocity.x, 0, swimVelocity.z).magnitude;
        animator.SetFloat("SwimSpeed", flatSpeed);
    }

    // --------------------------------------------------
    // INPUT (KEYBOARD + MOBILE)
    // --------------------------------------------------
    void HandleSwimInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (joystick != null)
        {
            x += joystick.Horizontal();
            z += joystick.Vertical();
        }

        Vector3 inputDir =
            Camera.main.transform.right * x +
            Camera.main.transform.forward * z;

        inputDir.y = 0f;

        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        swimVelocity = Vector3.Lerp(
            swimVelocity,
            inputDir * swimSpeed,
            swimAcceleration * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // MOVEMENT (SURFACE-LOCKED)
    // --------------------------------------------------
    void ApplySwimMovement()
{
    // Horizontal movement
    controller.Move(
        new Vector3(swimVelocity.x, 0, swimVelocity.z) * Time.deltaTime
    );

    // Determine swim vs tread
    bool isMoving =
        new Vector3(swimVelocity.x, 0, swimVelocity.z).magnitude > 0.1f;

    float targetOffset = isMoving ? swimOffset : treadOffset;
    float targetY = waterSurfaceY - targetOffset;

    // Vertical input
    float desiredY = transform.position.y + swimVelocity.y * Time.deltaTime;

    // Clamp around target surface height
    desiredY = Mathf.Clamp(
        desiredY,
        targetY - 0.3f,
        targetY + 0.2f
    );

    float smoothY = Mathf.Lerp(
        transform.position.y,
        desiredY,
        surfaceSnapSpeed * Time.deltaTime
    );

    controller.Move(Vector3.up * (smoothY - transform.position.y));
}


    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------
    void ApplyRotation()
    {
        Vector3 flatDir = new Vector3(swimVelocity.x, 0f, swimVelocity.z);

        if (flatDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(flatDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // WATER DETECTION
    // --------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (isSwimming) return;

        EnterWater(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Water")) return;
        if (!isSwimming) return;

        ExitWater();
    }

    void EnterWater(Collider water)
    {
        isSwimming = true;
        swimVelocity = Vector3.zero;

        waterSurfaceY = water.bounds.max.y;

        if (keyboardMovement) keyboardMovement.enabled = false;
        if (mobileMovement) mobileMovement.enabled = false;
        if (combatScript) combatScript.enabled = false;

        animator.SetBool("IsSwimming", true);
        animator.SetBool("IsAttacking", false);
        animator.SetFloat("SwimSpeed", 0f);
    }

    void ExitWater()
    {
        isSwimming = false;

        if (keyboardMovement) keyboardMovement.enabled = true;
        if (mobileMovement) mobileMovement.enabled = true;
        if (combatScript) combatScript.enabled = true;

        animator.SetBool("IsSwimming", false);
        animator.SetFloat("SwimSpeed", 0f);
    }
}


