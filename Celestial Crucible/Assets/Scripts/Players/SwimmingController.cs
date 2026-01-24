using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class SwimmingController : MonoBehaviour
{
    CharacterController controller;
    Animator animator;

    [Header("Swimming Movement")]
    public float swimSpeed = 4f;
    public float verticalSwimSpeed = 3f;
    public float waterDrag = 4f;

    [Header("Water Surface")]
    public float surfaceOffset = 2f;
    public float surfaceSnapSpeed = 6f;

    [Header("Disable While Swimming")]
    public MonoBehaviour keyboardMovement; // AdvancedPlayerMovement
    public MonoBehaviour mobileMovement;   // MobilePlayerMovement
    public MonoBehaviour combatScript;     // PlayerCombat (optional)
    public MobileJoystick joystick;        // optional

    Vector3 swimVelocity;
    bool isSwimming;
    float waterSurfaceY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // SAFETY RESET
        isSwimming = false;
        animator.SetBool("IsSwimming", false);
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
        inputDir.Normalize();

        swimVelocity = Vector3.Lerp(
            swimVelocity,
            inputDir * swimSpeed,
            Time.deltaTime * 8f
        );

        // Vertical swimming (jump disabled — reused for swim up)
        if (Input.GetButton("Jump"))
            swimVelocity.y = verticalSwimSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            swimVelocity.y = -verticalSwimSpeed;
        else
            swimVelocity.y = Mathf.Lerp(swimVelocity.y, 0f, waterDrag * Time.deltaTime);
    }

    // --------------------------------------------------
    // MOVEMENT + SURFACE SNAP
    // --------------------------------------------------
    void ApplySwimMovement()
    {
        // Horizontal
        controller.Move(
            new Vector3(swimVelocity.x, 0, swimVelocity.z) * Time.deltaTime
        );

        bool isTreading = animator.GetFloat("SwimSpeed") < 0.1f;

        if (isTreading)
        {
            float targetY = waterSurfaceY - surfaceOffset;
            float newY = Mathf.Lerp(
                transform.position.y,
                targetY,
                surfaceSnapSpeed * Time.deltaTime
            );

            controller.Move(Vector3.up * (newY - transform.position.y));
        }
        else
        {
            controller.Move(Vector3.up * swimVelocity.y * Time.deltaTime);
        }
    }

    // --------------------------------------------------
    // ROTATION
    // --------------------------------------------------
    void ApplyRotation()
    {
        Vector3 flatDir = new Vector3(swimVelocity.x, 0, swimVelocity.z);

        if (flatDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(flatDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            10f * Time.deltaTime
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

        // Disable movement & combat
        if (keyboardMovement) keyboardMovement.enabled = false;
        if (mobileMovement) mobileMovement.enabled = false;
        if (combatScript) combatScript.enabled = false;

        animator.SetBool("IsSwimming", true);
        animator.SetBool("IsAttacking", false); // safety
    }

    void ExitWater()
    {
        isSwimming = false;

        // Re-enable systems
        if (keyboardMovement) keyboardMovement.enabled = true;
        if (mobileMovement) mobileMovement.enabled = true;
        if (combatScript) combatScript.enabled = true;

        animator.SetBool("IsSwimming", false);
        animator.SetFloat("SwimSpeed", 0f);
    }
}

