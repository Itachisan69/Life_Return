using UnityEngine;

[RequireComponent(typeof(TargetDetection))]
[RequireComponent(typeof(SuctionSystem))]
public class VacuumController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VacuumStats stats;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource vacuumAudioSource;
    [SerializeField] private GameObject suctionVFX;

    [Header("Polish")]
    [SerializeField] private bool enableFOVEffect = true;
    [SerializeField] private bool enableCameraShake = false;

    private TargetDetection targetDetection;
    private SuctionSystem suctionSystem;
    private TrashInfoUI trashUI;

    private float originalFOV;
    private float targetFOV;

    void Awake()
    {
        targetDetection = GetComponent<TargetDetection>();
        suctionSystem = GetComponent<SuctionSystem>();
        trashUI = FindObjectOfType<TrashInfoUI>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
            targetFOV = originalFOV;
        }

        // Disable VFX initially
        if (suctionVFX != null)
        {
            suctionVFX.SetActive(false);
        }
    }

    void Start()
    {
        // Subscribe to events
        targetDetection.OnTargetChanged += OnTargetChanged;
        suctionSystem.OnItemCollected += OnItemCollected;
    }

    void Update()
    {
        HandleInput();
        UpdateFOV();
    }

    void HandleInput()
    {
        // Start/Continue sucking
        if (Input.GetMouseButton(0)) // Left click held
        {
            TrashItem target = targetDetection.GetCurrentTarget();

            if (target != null && !suctionSystem.IsSucking)
            {
                StartVacuum(target);
            }
        }

        // Stop sucking
        if (Input.GetMouseButtonUp(0)) // Left click released
        {
            StopVacuum();
        }
    }

    void StartVacuum(TrashItem target)
    {
        suctionSystem.StartSucking(target);

        // Audio
        if (vacuumAudioSource != null && !vacuumAudioSource.isPlaying)
        {
            vacuumAudioSource.loop = true;
            vacuumAudioSource.Play();
        }

        // VFX
        if (suctionVFX != null)
        {
            suctionVFX.SetActive(true);
        }

        // FOV effect
        if (enableFOVEffect && playerCamera != null)
        {
            targetFOV = originalFOV + stats.fovIncreaseAmount;
        }
    }

    void StopVacuum()
    {
        suctionSystem.StopSucking();

        // Audio
        if (vacuumAudioSource != null)
        {
            vacuumAudioSource.Stop();
        }

        // VFX
        if (suctionVFX != null)
        {
            suctionVFX.SetActive(false);
        }

        // FOV effect
        if (enableFOVEffect && playerCamera != null)
        {
            targetFOV = originalFOV;
        }
    }

    void UpdateFOV()
    {
        if (!enableFOVEffect || playerCamera == null) return;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            stats.fovChangeSpeed * Time.deltaTime
        );
    }

    void OnTargetChanged(TrashItem newTarget)
    {
        // Update UI
        if (trashUI != null)
        {
            if (newTarget != null)
            {
                trashUI.ShowItemInfo(newTarget.itemData);
            }
            else
            {
                trashUI.HideItemInfo();
            }
        }
    }

    void OnItemCollected(TrashItem item)
    {
        // Add to inventory
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(item.itemData);
        }

        // Optional: Camera shake for heavy items
        if (enableCameraShake && item.itemData.weight > 5f)
        {
            // TODO: Implement camera shake
            // CameraShake.Instance.Shake(0.2f, 0.3f);
        }
    }

    void OnDestroy()
    {
        if (targetDetection != null)
        {
            targetDetection.OnTargetChanged -= OnTargetChanged;
        }
        if (suctionSystem != null)
        {
            suctionSystem.OnItemCollected -= OnItemCollected;
        }
    }
}