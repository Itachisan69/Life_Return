using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class VacuumSuction : MonoBehaviour
{
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

    private Dictionary<GameObject, Rigidbody> objectsInTrigger = new Dictionary<GameObject, Rigidbody>();
    private HashSet<GameObject> beingConsumed = new HashSet<GameObject>();
    private bool isSucking = false;
    private ParticleSystem suctionVFXInstance;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSuction();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopSuction();
        }

        if (isSucking)
        {
            ApplySuctionForce();
        }
    }

    void StartSuction()
    {
        isSucking = true;

        if (suctionAudioSource != null)
        {
            suctionAudioSource.loop = true;
            suctionAudioSource.Play();
        }

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

        if (suctionAudioSource != null)
        {
            suctionAudioSource.Stop();
        }

        if (suctionVFXInstance != null)
        {
            suctionVFXInstance.Stop();
        }

        // Re-enable gravity for all objects not being consumed
        foreach (var kvp in objectsInTrigger)
        {
            if (kvp.Key != null && kvp.Value != null && !beingConsumed.Contains(kvp.Key))
            {
                kvp.Value.useGravity = true;
                kvp.Value.drag = 0f;
            }
        }
    }

    void ApplySuctionForce()
    {
        // Create a copy to avoid collection modified exception
        List<KeyValuePair<GameObject, Rigidbody>> objectsList = new List<KeyValuePair<GameObject, Rigidbody>>(objectsInTrigger);

        foreach (var kvp in objectsList)
        {
            GameObject obj = kvp.Key;
            Rigidbody rb = kvp.Value;

            if (obj == null || rb == null || beingConsumed.Contains(obj)) continue;

            // Disable gravity
            rb.useGravity = false;
            rb.drag = 1f;

            Vector3 toNozzle = nozzle.position - obj.transform.position;
            float distance = toNozzle.magnitude;

            // Check for consumption
            if (distance <= consumeDistance)
            {
                ConsumeObject(obj, rb, normalShrinkDuration);
                continue;
            }

            Vector3 direction = toNozzle / distance;

            // Tornado spiral
            float angle = Time.time * tornadoSpeed + obj.GetInstanceID();
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 spiralOffset = (right * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)) * tornadoRadius / (distance + 1f);

            // Target velocity with acceleration based on distance
            float speedBoost = 1f + (3f / (distance + 1f));
            Vector3 targetVelocity = (direction + spiralOffset.normalized * 0.3f) * pullForce * speedBoost;

            // Clamp to max speed
            if (targetVelocity.magnitude > maxSpeed)
            {
                targetVelocity = targetVelocity.normalized * maxSpeed;
            }

            // Apply force
            Vector3 force = (targetVelocity - rb.velocity) * rb.mass * 10f;
            rb.AddForce(force, ForceMode.Force);
        }
    }

    void ConsumeObject(GameObject obj, Rigidbody rb, float shrinkDuration)
    {
        beingConsumed.Add(obj);
        objectsInTrigger.Remove(obj);

        // Completely stop the object
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Disable collider immediately to prevent clipping
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Snap to nozzle position immediately, then shrink
        obj.transform.position = nozzle.position;
        obj.transform.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                // TODO: Add to player inventory here
                if (obj != null) Destroy(obj);
            });
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Suckable")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null || objectsInTrigger.ContainsKey(other.gameObject)) return;

        objectsInTrigger[other.gameObject] = rb;

        // If already sucking and in consume range, consume immediately
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

        // Make sure it's tracked
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

    void OnDrawGizmosSelected()
    {
        if (nozzle != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nozzle.position, consumeDistance);
        }
    }
}