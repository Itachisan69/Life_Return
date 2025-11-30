using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class VacuumSuction : MonoBehaviour
{
    #region Serialized Fields

    [Header("References")]
    [SerializeField] private Transform nozzle;
    [SerializeField] private AudioSource suctionAudioSource;
    [SerializeField] private ParticleSystem suctionVFXPrefab;

    [Header("Suction Settings")]
    [SerializeField] private float pullForce = 8f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float tornadoRadius = 0.8f;
    [SerializeField] private float tornadoSpeed = 4f;
    [SerializeField] private float consumeDistance = 0.4f;
    [SerializeField] private float normalShrinkDuration = 0.3f;
    [SerializeField] private float quickShrinkDuration = 0.15f;

    [Header("Inventory System")]
    [SerializeField] private int maxInventoryCapacity = 10;

    [Header("Battery System")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 5f; // Units per second while sucking

    [Header("Audio Clips")]
    [SerializeField] private AudioClip consumeSuccessSFX;
    [SerializeField] private AudioClip consumeFailSFX;
    [SerializeField] private AudioClip batteryEmptySFX;

    #endregion

    #region Private Fields

    private Dictionary<GameObject, Rigidbody> objectsInTrigger = new Dictionary<GameObject, Rigidbody>();
    private HashSet<GameObject> beingConsumed = new HashSet<GameObject>();
    private List<SuckableObjectData> inventory = new List<SuckableObjectData>();

    private ParticleSystem suctionVFXInstance;
    private float currentBattery;
    private bool isSucking = false;
    private bool batteryDepleted = false;

    #endregion

    #region Properties

    public int CurrentInventorySize
    {
        get
        {
            int size = 0;
            foreach (var data in inventory)
            {
                size += data.size;
            }
            return size;
        }
    }

    public float CurrentBattery => currentBattery;
    public float MaxBattery => maxBattery;
    public bool IsBatteryDepleted => batteryDepleted;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        currentBattery = maxBattery;
    }

    void Update()
    {
        HandleBatteryDrain();
        HandleInput();
    }

    void FixedUpdate()
    {
        if (isSucking && !batteryDepleted)
        {
            ApplySuctionForce();
        }
    }

    #endregion

    #region Input & Battery Management

    void HandleInput()
    {
        if (batteryDepleted)
        {
            if (isSucking)
            {
                StopSuction();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartSuction();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopSuction();
        }
    }

    void HandleBatteryDrain()
    {
        if (isSucking && !batteryDepleted)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                batteryDepleted = true;

                if (suctionAudioSource != null)
                {
                    suctionAudioSource.Stop();
                    if (batteryEmptySFX != null)
                    {
                        suctionAudioSource.PlayOneShot(batteryEmptySFX);
                    }
                }

                StopSuction();
                Debug.Log("Battery depleted!");
            }
        }
    }

    /// <summary>
    /// Fully recharges the battery. Call this from external scripts.
    /// </summary>
    public void RechargeBattery()
    {
        currentBattery = maxBattery;
        batteryDepleted = false;
        Debug.Log("Battery fully recharged!");
    }

    /// <summary>
    /// Partially recharges the battery by a specific amount.
    /// </summary>
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);

        if (currentBattery > 0f)
        {
            batteryDepleted = false;
        }

        Debug.Log($"Battery recharged by {amount}. Current: {currentBattery}/{maxBattery}");
    }

    #endregion

    #region Suction Control

    void StartSuction()
    {
        if (batteryDepleted) return;

        isSucking = true;

        // Start audio
        if (suctionAudioSource != null)
        {
            suctionAudioSource.loop = true;
            suctionAudioSource.Play();
        }

        // Start VFX
        if (suctionVFXPrefab != null && suctionVFXInstance == null)
        {
            suctionVFXInstance = Instantiate(suctionVFXPrefab, nozzle.position, nozzle.rotation, nozzle);
        }

        if (suctionVFXInstance != null)
        {
            suctionVFXInstance.Play();
        }
    }

    void StopSuction()
    {
        isSucking = false;

        // Stop audio
        if (suctionAudioSource != null)
        {
            suctionAudioSource.Stop();
        }

        // Stop VFX
        if (suctionVFXInstance != null)
        {
            suctionVFXInstance.Stop();
        }

        // Re-enable gravity for all tracked objects not being consumed
        foreach (var kvp in objectsInTrigger)
        {
            if (kvp.Key != null && kvp.Value != null && !beingConsumed.Contains(kvp.Key))
            {
                kvp.Value.useGravity = true;
                kvp.Value.drag = 0f;
            }
        }
    }

    #endregion

    #region Suction Physics

    void ApplySuctionForce()
    {
        // Create a copy to avoid collection modified exception
        List<KeyValuePair<GameObject, Rigidbody>> objectsList = new List<KeyValuePair<GameObject, Rigidbody>>(objectsInTrigger);

        foreach (var kvp in objectsList)
        {
            GameObject obj = kvp.Key;
            Rigidbody rb = kvp.Value;

            if (obj == null || rb == null || beingConsumed.Contains(obj)) continue;

            // Disable gravity while being sucked
            rb.useGravity = false;
            rb.drag = 2f; // Increased drag for smoother motion

            Vector3 toNozzle = nozzle.position - obj.transform.position;
            float distance = toNozzle.magnitude;

            // Check if close enough to consume
            if (distance <= consumeDistance)
            {
                ConsumeObject(obj, rb, normalShrinkDuration);
                continue;
            }

            Vector3 direction = toNozzle / distance;

            // Create tornado spiral effect
            float angle = Time.time * tornadoSpeed + obj.GetInstanceID();
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 spiralOffset = (right * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * tornadoRadius / (distance + 1f);

            // Calculate target velocity with distance-based acceleration
            float speedBoost = 1f + (3f / (distance + 1f));
            Vector3 targetVelocity = (direction + spiralOffset.normalized * 0.3f) * pullForce * speedBoost;

            // Clamp to max speed
            if (targetVelocity.magnitude > maxSpeed)
            {
                targetVelocity = targetVelocity.normalized * maxSpeed;
            }

            // Smooth force application using Lerp for less jitter
            Vector3 desiredVelocityChange = targetVelocity - rb.velocity;
            Vector3 force = desiredVelocityChange * rb.mass * 5f; // Reduced multiplier for smoother movement
            rb.AddForce(force, ForceMode.Force);

            // Reduce angular velocity to prevent spinning
            rb.angularVelocity *= 0.9f;
        }
    }

    #endregion

    #region Object Consumption

    void ConsumeObject(GameObject obj, Rigidbody rb, float shrinkDuration)
    {
        SuckableObject suckable = obj.GetComponent<SuckableObject>();
        if (suckable == null || suckable.data == null)
        {
            return;
        }

        SuckableObjectData objectData = suckable.data;
        int objectSize = objectData.size;

        // Check if there's enough inventory space
        if (CurrentInventorySize + objectSize >= maxInventoryCapacity)
        {
            HandleInventoryFull(obj, rb, objectData, objectSize);
            return;
        }

        // Proceed with consumption
        beingConsumed.Add(obj);
        objectsInTrigger.Remove(obj);

        // Stop the object completely
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Disable collider
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Play success sound
        if (suctionAudioSource != null && consumeSuccessSFX != null)
        {
            suctionAudioSource.PlayOneShot(consumeSuccessSFX);
        }

        // Animate shrinking and destruction
        obj.transform.position = nozzle.position;
        obj.transform.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => FinalizeConsumption(obj, objectData));
    }

    void HandleInventoryFull(GameObject obj, Rigidbody rb, SuckableObjectData objectData, int objectSize)
    {
        // Remove from tracking
        objectsInTrigger.Remove(obj);
        beingConsumed.Remove(obj);

        // Restore physics and push back slightly
        if (rb != null)
        {
            rb.useGravity = true;
            rb.drag = 0f;
            rb.AddForce(-rb.velocity.normalized * 5f, ForceMode.Impulse);
        }

        // Play fail sound
        if (suctionAudioSource != null && consumeFailSFX != null)
        {
            suctionAudioSource.PlayOneShot(consumeFailSFX);
        }

        Debug.Log($"Inventory full! Cannot suck up {objectData.objectName} (Size: {objectSize}).");
    }

    void FinalizeConsumption(GameObject obj, SuckableObjectData objectData)
    {
        // Add to inventory
        inventory.Add(objectData);

        Debug.Log($"Consumed {objectData.objectName}. Inventory: {CurrentInventorySize}/{maxInventoryCapacity}");

        // Destroy the game object
        if (obj != null) Destroy(obj);

        beingConsumed.Remove(obj);
    }

    #endregion

    #region Trigger Events

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Suckable")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null || objectsInTrigger.ContainsKey(other.gameObject)) return;

        objectsInTrigger[other.gameObject] = rb;

        // If already sucking and object enters within consume range, consume immediately
        if (isSucking)
        {
            float distance = Vector3.Distance(other.transform.position, nozzle.position);
            if (distance <= consumeDistance)
            {
                ConsumeObject(other.gameObject, rb, quickShrinkDuration);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Suckable")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Ensure object is tracked
        if (!objectsInTrigger.ContainsKey(other.gameObject))
        {
            objectsInTrigger[other.gameObject] = rb;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Suckable")) return;

        if (objectsInTrigger.ContainsKey(other.gameObject))
        {
            // Restore physics if not being consumed
            if (!beingConsumed.Contains(other.gameObject))
            {
                Rigidbody rb = objectsInTrigger[other.gameObject];
                if (rb != null)
                {
                    rb.useGravity = true;
                    rb.drag = 0f;
                }
            }
            objectsInTrigger.Remove(other.gameObject);
        }
    }

    #endregion

    #region Upgrade Methods

    /// <summary>
    /// Increases the maximum inventory capacity.
    /// </summary>
    public void UpgradeInventoryCapacity(int amount)
    {
        maxInventoryCapacity += amount;
        Debug.Log($"Inventory upgraded! New Capacity: {maxInventoryCapacity}");
    }

    /// <summary>
    /// Increases the maximum battery capacity and fully recharges it.
    /// </summary>
    public void UpgradeMaxBattery(float amount)
    {
        maxBattery += amount;
        currentBattery = maxBattery;
        batteryDepleted = false;
        Debug.Log($"Battery upgraded! New Max Battery: {maxBattery}");
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        if (nozzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nozzle.position, consumeDistance);
        }
    }

    #endregion
}