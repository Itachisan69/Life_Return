using UnityEngine;

public class UpgradeMachine : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] public VacuumSuction vacuumSuction;
    [SerializeField] private PlayerUpgradesUI upgradesUI;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt;

    [Header("Upgrade Tiers")]
    [SerializeField] private UpgradeTier[] batteryUpgrades;
    [SerializeField] private UpgradeTier[] inventoryUpgrades;

    [Header("Recharge Settings")]
    [SerializeField] private float rechargeCostMultiplier = 20f;

    private int currentBatteryLevel = 0;
    private int currentInventoryLevel = 0;

    private Transform player;
    private bool isUIOpen = false;
    private bool playerInRange = false;

    // Public Getters
    public UpgradeTier[] BatteryUpgrades => batteryUpgrades;
    public UpgradeTier[] InventoryUpgrades => inventoryUpgrades;
    public int CurrentBatteryLevel => currentBatteryLevel;
    public int CurrentInventoryLevel => currentInventoryLevel;
    public float CurrentRechargeCost => vacuumSuction != null ? vacuumSuction.MaxBattery / rechargeCostMultiplier : 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        CheckPlayerDistance();
        HandleInput();
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;

        // Hide prompt if UI is open or player is out of range
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(playerInRange && !isUIOpen);
        }

        // Auto-close UI if player walks away
        if (isUIOpen && !playerInRange)
        {
            CloseUI();
        }
    }

    void HandleInput()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (isUIOpen)
            {
                CloseUI();
            }
            else
            {
                OpenUI();
            }
        }

        // Allow closing with Escape key
        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    public void OpenUI()
    {
        if (upgradesUI == null) return;

        isUIOpen = true;
        upgradesUI.OpenUI(this);

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        DisablePlayerControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // IMPORTANT: Make sure your UI "Close" button calls THIS method, 
    // not just the one in the UpgradesUI script!
    public void CloseUI()
    {
        if (!isUIOpen) return; // Prevent double-firing

        isUIOpen = false;

        if (upgradesUI != null)
        {
            upgradesUI.CloseUI();
        }

        EnablePlayerControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisablePlayerControls()
    {
        // Disable Player Movement
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null) playerController.enabled = false;

        // Disable Camera Rotation
        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null) cameraController.enabled = false;

        // Disable Vacuum Tool
        if (vacuumSuction != null) vacuumSuction.enabled = false;
    }

    void EnablePlayerControls()
    {
        // Enable Player Movement
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null) playerController.enabled = true;

        // Enable Camera Rotation
        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null) cameraController.enabled = true;

        // Enable Vacuum Tool
        if (vacuumSuction != null) vacuumSuction.enabled = true;
    }

    // --- Upgrade Logic ---

    public void AttemptUpgradeBattery()
    {
        if (currentBatteryLevel >= batteryUpgrades.Length) return;

        UpgradeTier nextTier = batteryUpgrades[currentBatteryLevel];
        if (playerMoney.SpendMoney(nextTier.Cost))
        {
            vacuumSuction.UpgradeMaxBattery(nextTier.ValueIncrease);
            currentBatteryLevel++;
            upgradesUI.UpdateUI(this);
        }
    }

    public void AttemptUpgradeInventory()
    {
        if (currentInventoryLevel >= inventoryUpgrades.Length) return;

        UpgradeTier nextTier = inventoryUpgrades[currentInventoryLevel];
        if (playerMoney.SpendMoney(nextTier.Cost))
        {
            int increaseAmount = Mathf.RoundToInt(nextTier.ValueIncrease);
            vacuumSuction.UpgradeInventoryCapacity(increaseAmount);
            currentInventoryLevel++;
            upgradesUI.UpdateUI(this);
        }
    }

    public void AttemptRechargeBattery()
    {
        float cost = CurrentRechargeCost;
        if (vacuumSuction.CurrentBattery >= vacuumSuction.MaxBattery) return;

        if (playerMoney.SpendMoney(cost))
        {
            vacuumSuction.RechargeBattery();
            upgradesUI.UpdateUI(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}