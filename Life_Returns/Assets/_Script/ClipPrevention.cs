using UnityEngine;

public class WeaponClipPrevention : MonoBehaviour
{
    [Header("Clip Prevention Settings")]
    [SerializeField] private float checkDistance = 1f;
    [SerializeField] private Vector3 retractedRotation = new Vector3(45f, 0f, 0f); // Weapon rotates up when near wall
    [SerializeField] private Vector3 retractedPosition = new Vector3(0f, -0.2f, -0.3f); // Weapon moves back and down
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float positionSpeed = 10f;
    [SerializeField] private LayerMask collisionLayers; // Set to everything except player

    private Transform playerCam;
    private float targetLerp = 0f;
    private Vector3 originalPosition;

    void Start()
    {
        // Find the camera automatically
        playerCam = Camera.main.transform;

        // Store the original local position
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        CheckForClipping();
        ApplyRotationAndPosition();
    }

    void CheckForClipping()
    {
        RaycastHit hit;

        // Raycast from camera forward to check for walls
        if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, checkDistance, collisionLayers))
        {
            // Calculate how close we are to the wall (0 = far, 1 = very close)
            float proximityToWall = 1f - (hit.distance / checkDistance);
            targetLerp = Mathf.Clamp01(proximityToWall);
        }
        else
        {
            targetLerp = 0f;
        }

        // Optional: Draw debug ray in Scene view
        Debug.DrawRay(playerCam.position, playerCam.forward * checkDistance,
            targetLerp > 0 ? Color.red : Color.green);
    }

    void ApplyRotationAndPosition()
    {
        // Smoothly interpolate rotation between normal and retracted
        Quaternion normalRotation = Quaternion.identity;
        Quaternion retractedRot = Quaternion.Euler(retractedRotation);
        Quaternion targetRotation = Quaternion.Lerp(normalRotation, retractedRot, targetLerp);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Smoothly interpolate position between normal and retracted
        Vector3 targetPosition = Vector3.Lerp(originalPosition, originalPosition + retractedPosition, targetLerp);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, positionSpeed * Time.deltaTime);
    }
}