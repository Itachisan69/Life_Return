using UnityEngine;

public class EnhancedWeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.06f;
    [SerializeField] private float swaySmooth = 4f;

    [Header("Sway Rotation")]
    [SerializeField] private float rotationSwayMultiplier = 4f;
    [SerializeField] private float maxRotationSway = 5f;
    [SerializeField] private float rotationSwaySmooth = 12f;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 10f;
    [SerializeField] private float tiltSmooth = 5f;
    [SerializeField] private float maxTilt = 15f;

    [Header("Movement Bob")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float bobSmoothness = 10f;
    [SerializeField] private float runBobMultiplier = 1.5f;

    [Header("Jump/Land Effects")]
    [SerializeField] private float jumpAmount = 0.3f;
    [SerializeField] private float landAmount = 0.4f;
    [SerializeField] private float jumpRotation = -15f;
    [SerializeField] private float landRotation = 10f;
    [SerializeField] private float jumpLandSmooth = 8f;

    [Header("References")]
    [SerializeField] private Rigidbody playerRb;

    private Vector3 swayPos;
    private Vector3 swayEulerRot;
    private float bobTimer;
    private float speedBobMultiplier = 1f;

    // Jump/Land tracking
    private bool wasGrounded;
    private float jumpLandOffset;
    private float jumpLandRotOffset;
    private float velocityBeforeLand;

    // Original position
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;

        if (playerRb == null)
        {
            playerRb = GetComponentInParent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogWarning("No Rigidbody found! Jump/Land effects and movement bob won't work properly.");
            }
        }

        wasGrounded = IsGrounded();
    }

    void Update()
    {
        GetInput();
        HandleJumpAndLand();
        Sway();
        SwayRotation();
        Tilt();
        MovementBob();
        ApplyPositionAndRotation();
    }

    void GetInput()
    {
        // Empty for now, input is handled in individual functions
    }

    void Sway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayAmount;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayAmount;

        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetPosition = new Vector3(-mouseX, -mouseY, 0);
        swayPos = Vector3.Lerp(swayPos, targetPosition, Time.deltaTime * swaySmooth);
    }

    void SwayRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * rotationSwayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * rotationSwayMultiplier;

        mouseX = Mathf.Clamp(mouseX, -maxRotationSway, maxRotationSway);
        mouseY = Mathf.Clamp(mouseY, -maxRotationSway, maxRotationSway);

        Vector3 targetRotation = new Vector3(-mouseY, mouseX, 0);
        swayEulerRot = Vector3.Lerp(swayEulerRot, targetRotation, Time.deltaTime * rotationSwaySmooth);
    }

    void Tilt()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float tiltZ = Mathf.Clamp(mouseX * tiltAmount, -maxTilt, maxTilt);

        Vector3 currentRot = swayEulerRot;
        currentRot.z = Mathf.Lerp(currentRot.z, -tiltZ, Time.deltaTime * tiltSmooth);
        swayEulerRot = currentRot;
    }

    void MovementBob()
    {
        if (!enableBob || playerRb == null) return;

        Vector3 velocity = playerRb.velocity;
        float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        if (horizontalSpeed > 0.1f && IsGrounded())
        {
            // Determine if sprinting based on speed (adjust threshold as needed)
            speedBobMultiplier = horizontalSpeed > 6f ? runBobMultiplier : 1f;

            bobTimer += Time.deltaTime * bobSpeed * speedBobMultiplier;

            float bobX = Mathf.Cos(bobTimer) * bobAmount * speedBobMultiplier * 0.5f;
            float bobY = Mathf.Sin(bobTimer * 2) * bobAmount * speedBobMultiplier;

            swayPos += new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobTimer = 0;
        }
    }

    void HandleJumpAndLand()
    {
        if (playerRb == null) return;

        bool isGrounded = IsGrounded();

        // Detect jump (was grounded, now not grounded, and moving upward)
        if (wasGrounded && !isGrounded && playerRb.velocity.y > 0.1f)
        {
            jumpLandOffset = jumpAmount;
            jumpLandRotOffset = jumpRotation;
        }

        // Track velocity before landing for impact calculation
        if (!isGrounded)
        {
            velocityBeforeLand = playerRb.velocity.y;
        }

        // Detect landing (wasn't grounded, now grounded)
        if (!wasGrounded && isGrounded)
        {
            // Impact based on fall velocity
            float impactForce = Mathf.Clamp01(Mathf.Abs(velocityBeforeLand) / 10f);
            jumpLandOffset = -landAmount * impactForce;
            jumpLandRotOffset = landRotation * impactForce;
        }

        // Smooth return to normal
        jumpLandOffset = Mathf.Lerp(jumpLandOffset, 0, Time.deltaTime * jumpLandSmooth);
        jumpLandRotOffset = Mathf.Lerp(jumpLandRotOffset, 0, Time.deltaTime * jumpLandSmooth);

        wasGrounded = isGrounded;
    }

    void ApplyPositionAndRotation()
    {
        // Combine all position effects
        Vector3 targetPos = originalPosition + swayPos + new Vector3(0, jumpLandOffset, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * bobSmoothness);

        // Combine all rotation effects
        Vector3 targetRot = swayEulerRot + new Vector3(jumpLandRotOffset, 0, 0);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            Quaternion.Euler(targetRot),
            Time.deltaTime * rotationSwaySmooth
        );
    }

    bool IsGrounded()
    {
        if (playerRb == null) return true;

        // Simple ground check - raycast downward from player
        return Physics.Raycast(playerRb.position, Vector3.down, 1.2f);
    }
}