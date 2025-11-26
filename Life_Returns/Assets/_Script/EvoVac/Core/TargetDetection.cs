using UnityEngine;
using System;

public class TargetDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform nozzleTransform;
    [SerializeField] private VacuumStats stats;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private TrashItem currentTarget;
    private TrashItem previousTarget;

    public event Action<TrashItem> OnTargetChanged;

    void Update()
    {
        DetectTarget();
        UpdateHighlight();
    }

    void DetectTarget()
    {
        previousTarget = currentTarget;
        currentTarget = null;

        Ray ray = new Ray(nozzleTransform.position, nozzleTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, stats.detectionRange, stats.trashLayer))
        {
            TrashItem trash = hit.collider.GetComponent<TrashItem>();
            if (trash != null && trash.itemData != null)
            {
                currentTarget = trash;
            }
        }

        // Debug visualization
        if (showDebugRay)
        {
            Color rayColor = currentTarget != null ? Color.green : Color.red;
            Debug.DrawRay(nozzleTransform.position, nozzleTransform.forward * stats.detectionRange, rayColor);
        }

        // Trigger event if target changed
        if (currentTarget != previousTarget)
        {
            OnTargetChanged?.Invoke(currentTarget);
        }
    }

    void UpdateHighlight()
    {
        // Remove highlight from previous target
        if (previousTarget != null && previousTarget != currentTarget)
        {
            previousTarget.SetHighlight(false);
        }

        // Add highlight to current target
        if (currentTarget != null)
        {
            currentTarget.SetHighlight(true);
        }
    }

    public TrashItem GetCurrentTarget() => currentTarget;
    public Transform GetNozzleTransform() => nozzleTransform;

    void OnDrawGizmosSelected()
    {
        if (nozzleTransform == null || stats == null) return;

        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(nozzleTransform.position, stats.detectionRange);

        // Draw suction cone
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Vector3 forward = nozzleTransform.forward * stats.detectionRange;
        Vector3 right = nozzleTransform.right * stats.coneRadius;
        Vector3 up = nozzleTransform.up * stats.coneRadius;

        Gizmos.DrawLine(nozzleTransform.position, nozzleTransform.position + forward + right);
        Gizmos.DrawLine(nozzleTransform.position, nozzleTransform.position + forward - right);
        Gizmos.DrawLine(nozzleTransform.position, nozzleTransform.position + forward + up);
        Gizmos.DrawLine(nozzleTransform.position, nozzleTransform.position + forward - up);
    }
}