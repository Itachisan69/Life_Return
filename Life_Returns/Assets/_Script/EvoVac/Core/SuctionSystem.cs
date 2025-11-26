using UnityEngine;
using System;

public class SuctionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform nozzleTransform;
    [SerializeField] private VacuumStats stats;

    private TrashItem currentlySuckedItem;
    private bool isSucking = false;

    // State tracking
    private enum SuctionPhase { Pulling, Rotating, Shrinking, Collecting }
    private SuctionPhase currentPhase = SuctionPhase.Pulling;

    public event Action<TrashItem> OnItemCollected;

    public void StartSucking(TrashItem target)
    {
        if (target == null || currentlySuckedItem != null) return;

        currentlySuckedItem = target;
        currentlySuckedItem.StartSuction();
        isSucking = true;
        currentPhase = SuctionPhase.Pulling;
    }

    public void StopSucking()
    {
        if (currentlySuckedItem != null)
        {
            currentlySuckedItem.StopSuction();
            currentlySuckedItem = null;
        }
        isSucking = false;
    }

    void Update()
    {
        if (!isSucking || currentlySuckedItem == null) return;

        // Handle rotation and shrinking in Update (visual)
        Vector3 toNozzle = nozzleTransform.position - currentlySuckedItem.transform.position;
        float distance = toNozzle.magnitude;

        if (currentPhase == SuctionPhase.Rotating || currentPhase == SuctionPhase.Shrinking)
        {
            RotateItem();
        }

        if (currentPhase == SuctionPhase.Shrinking)
        {
            ShrinkItem();

            // Check for collection
            if (distance < stats.collectDistance || currentlySuckedItem.transform.localScale.x < 0.1f)
            {
                currentPhase = SuctionPhase.Collecting;
                CollectItem();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isSucking || currentlySuckedItem == null) return;

        // Handle physics forces in FixedUpdate
        ProcessSuctionPhysics();
    }

    void ProcessSuctionPhysics()
    {
        Vector3 toNozzle = nozzleTransform.position - currentlySuckedItem.transform.position;
        float distance = toNozzle.magnitude;

        switch (currentPhase)
        {
            case SuctionPhase.Pulling:
                PullItem(toNozzle, distance);

                // Transition to rotating phase
                if (distance < stats.rotateDistance)
                {
                    currentPhase = SuctionPhase.Rotating;
                }
                break;

            case SuctionPhase.Rotating:
                PullItem(toNozzle, distance);

                // Transition to shrinking phase
                if (distance < stats.shrinkDistance)
                {
                    currentPhase = SuctionPhase.Shrinking;
                }
                break;

            case SuctionPhase.Shrinking:
                PullItemFast(toNozzle); // Magnet snap
                break;
        }
    }

    void PullItem(Vector3 direction, float distance)
    {
        // Prevent division issues
        if (direction.magnitude < 0.01f || currentlySuckedItem.itemData.weight <= 0)
            return;

        // Calculate suction speed based on weight (clamp to prevent extreme values)
        float weight = Mathf.Max(currentlySuckedItem.itemData.weight, 0.1f);
        float finalSuckSpeed = Mathf.Clamp(stats.suctionPower / weight, 1f, 100f);

        // Apply acceleration curve for more satisfying movement
        float normalizedDistance = 1f - Mathf.Clamp01(distance / stats.detectionRange);
        float curveMultiplier = stats.suctionAccelerationCurve.Evaluate(normalizedDistance);

        // Clamp final force to prevent physics explosion
        Vector3 force = direction.normalized * finalSuckSpeed * curveMultiplier;
        force = Vector3.ClampMagnitude(force, 500f); // Hard limit on force

        currentlySuckedItem.ApplySuctionForce(force);
    }

    void PullItemFast(Vector3 direction)
    {
        // Safety check
        if (direction.magnitude < 0.01f) return;

        // Magnet snap - fast final pull using MovePosition for stability
        Vector3 currentPos = currentlySuckedItem.transform.position;
        Vector3 targetPosition = Vector3.MoveTowards(
            currentPos,
            nozzleTransform.position,
            stats.magnetSnapSpeed * Time.deltaTime
        );

        // Use MovePosition instead of direct assignment for better physics
        currentlySuckedItem.Rigidbody.MovePosition(targetPosition);

        // Dampen velocity to prevent overshoot
        currentlySuckedItem.Rigidbody.velocity = Vector3.ClampMagnitude(
            currentlySuckedItem.Rigidbody.velocity,
            10f
        );
    }

    void RotateItem()
    {
        currentlySuckedItem.RotateToward(nozzleTransform.position, 5f);
    }

    void ShrinkItem()
    {
        currentlySuckedItem.ShrinkToward(nozzleTransform.position, 3f);
    }

    void CollectItem()
    {
        // Spawn collection VFX if available
        if (currentlySuckedItem.itemData.shrinkParticlePrefab != null)
        {
            Instantiate(
                currentlySuckedItem.itemData.shrinkParticlePrefab,
                currentlySuckedItem.transform.position,
                Quaternion.identity
            );
        }

        // Notify listeners (inventory will handle this)
        OnItemCollected?.Invoke(currentlySuckedItem);

        // Destroy the physical object
        Destroy(currentlySuckedItem.gameObject);
        currentlySuckedItem = null;
        isSucking = false;
    }

    public bool IsSucking => isSucking;
    public TrashItem CurrentItem => currentlySuckedItem;
}