using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class TrashItem : MonoBehaviour
{
    [Header("Item Data")]
    public TrashItemData itemData;

    [Header("Highlight")]
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer itemRenderer;

    [Header("State")]
    private bool isBeingSucked = false;
    private bool isHighlighted = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        itemRenderer = GetComponentInChildren<Renderer>();

        if (itemRenderer != null)
        {
            originalMaterial = itemRenderer.material;
        }

        originalScale = transform.localScale;

        // Set rigidbody properties for stable physics
        if (itemData != null)
        {
            rb.mass = Mathf.Max(itemData.weight, 0.1f); // Minimum mass to prevent issues
        }

        // Add physics constraints to prevent wild spinning
        rb.maxAngularVelocity = 10f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted || itemRenderer == null) return;

        isHighlighted = highlighted;

        if (highlighted && highlightMaterial != null)
        {
            itemRenderer.material = highlightMaterial;
        }
        else
        {
            itemRenderer.material = originalMaterial;
        }
    }

    public void StartSuction()
    {
        if (isBeingSucked) return;

        isBeingSucked = true;
        rb.useGravity = false;
        rb.drag = 2f; // Add some air resistance
    }

    public void StopSuction()
    {
        if (!isBeingSucked) return;

        isBeingSucked = false;
        rb.useGravity = true;
        rb.drag = 0.5f;
    }

    public void ApplySuctionForce(Vector3 force)
    {
        if (!isBeingSucked || rb == null) return;

        // Validate force is not infinite or NaN
        if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z) ||
            float.IsInfinity(force.x) || float.IsInfinity(force.y) || float.IsInfinity(force.z))
        {
            Debug.LogWarning("Invalid force detected, skipping application");
            return;
        }

        // Clamp velocity before applying force to prevent runaway physics
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, 50f);

        rb.AddForce(force, ForceMode.Force);
    }

    public void RotateToward(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    public void ShrinkToward(Vector3 target, float shrinkSpeed)
    {
        float shrinkFactor = Mathf.Lerp(transform.localScale.x, 0, shrinkSpeed * Time.deltaTime);
        transform.localScale = originalScale * shrinkFactor;
    }

    public bool IsBeingSucked => isBeingSucked;
    public Rigidbody Rigidbody => rb;
    public Vector3 OriginalScale => originalScale;
}