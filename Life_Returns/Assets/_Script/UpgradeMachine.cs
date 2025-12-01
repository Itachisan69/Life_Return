using UnityEngine;

public class UpgradeMachine : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the player's money script.")]
    [SerializeField] private PlayerMoney playerMoney;

    [Tooltip("Reference to the player's vacuum script.")]
    [SerializeField] public VacuumSuction vacuumSuction;

    [Tooltip("Reference to the UI Manager script.")]
    [SerializeField] private PlayerUpgradesUI upgradesUI;

    [Tooltip("The key the player presses to interact (e.g., E).")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Upgrade Tiers")]
    [Tooltip("Define the cost and increase for each Battery Capacity upgrade.")]
    [SerializeField] private UpgradeTier[] batteryUpgrades;

    [Tooltip("Define the cost and increase for each Inventory Capacity upgrade.")]
    [SerializeField] private UpgradeTier[] inventoryUpgrades;

    [Header("Recharge Settings")]
    [Tooltip("The multiplier used to calculate the recharge cost based on max battery. Cost = MaxBattery / Multiplier")]
    [SerializeField] private float rechargeCostMultiplier = 20f;

    // Tracks the current upgrade level for each system
    private int currentBatteryLevel = 0;
    private int currentInventoryLevel = 0;

    private bool isPlayerNearby = false;

    // --- Public Getters for UI Initialization ---

    public UpgradeTier[] BatteryUpgrades => batteryUpgrades;
    public UpgradeTier[] InventoryUpgrades => inventoryUpgrades;
    public int CurrentBatteryLevel => currentBatteryLevel;
    public int CurrentInventoryLevel => currentInventoryLevel;
    public float CurrentRechargeCost => vacuumSuction.MaxBattery / rechargeCostMultiplier;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactionKey))
        {
            if (upgradesUI.IsUIOpen)
            {
                // UI is closing
                upgradesUI.CloseUI();

                // --- CURSOR FIX: RELEASE CURSOR ---
                Cursor.lockState = CursorLockMode.Locked; // Lock cursor for gameplay
                Cursor.visible = false;                   // Hide cursor
                                                          // ------------------------------------

                Time.timeScale = 1f; // Resume gameplay
            }
            else
            {
                // UI is opening
                upgradesUI.OpenUI(this);

                // --- CURSOR FIX: ENABLE CURSOR ---
                Cursor.lockState = CursorLockMode.None; // Unlock cursor
                Cursor.visible = true;                  // Show cursor
                                                        // ---------------------------------

                Time.timeScale = 0f; // Pause gameplay
            }
        }
    }

    // --- Interaction Triggers ---

    private void OnTriggerEnter(Collider other)
    {
        // Assuming the player has a "Player" tag
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log($"Press {interactionKey} to use Upgrade Machine.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (upgradesUI.IsUIOpen)
            {
                upgradesUI.CloseUI();
                Time.timeScale = 1f; // Resume game time
            }
        }
    }

    // --- Upgrade Functions (Called by UI Buttons) ---

    public void AttemptUpgradeBattery()
    {
        if (currentBatteryLevel >= batteryUpgrades.Length)
        {
            Debug.Log("Battery is already at Max Level!");
            return;
        }

        UpgradeTier nextTier = batteryUpgrades[currentBatteryLevel];

        if (playerMoney.SpendMoney(nextTier.Cost))
        {
            // Apply the upgrade effect to the VacuumSuction script
            vacuumSuction.UpgradeMaxBattery(nextTier.ValueIncrease);
            currentBatteryLevel++;
            Debug.Log($"Battery upgraded to Level {currentBatteryLevel}. New Max Battery: {vacuumSuction.MaxBattery}");
            upgradesUI.UpdateUI(this); // Refresh UI to show new level/cost
        }
        else
        {
            Debug.Log("Cannot afford Battery Upgrade!");
        }
    }

    public void AttemptUpgradeInventory()
    {
        if (currentInventoryLevel >= inventoryUpgrades.Length)
        {
            Debug.Log("Inventory is already at Max Level!");
            return;
        }

        UpgradeTier nextTier = inventoryUpgrades[currentInventoryLevel];

        if (playerMoney.SpendMoney(nextTier.Cost))
        {
            // Apply the upgrade effect to the VacuumSuction script
            // Note: Your VacuumSuction script uses 'int' for inventory capacity
            int increaseAmount = Mathf.RoundToInt(nextTier.ValueIncrease);
            vacuumSuction.UpgradeInventoryCapacity(increaseAmount);
            currentInventoryLevel++;
            Debug.Log($"Inventory upgraded to Level {currentInventoryLevel}. New Capacity: {vacuumSuction.maxInventoryCapacity}");
            upgradesUI.UpdateUI(this); // Refresh UI to show new level/cost
        }
        else
        {
            Debug.Log("Cannot afford Inventory Upgrade!");
        }
    }

    public void AttemptRechargeBattery()
    {
        float cost = CurrentRechargeCost;

        if (vacuumSuction.CurrentBattery >= vacuumSuction.MaxBattery)
        {
            Debug.Log("Battery is already full!");
            return;
        }

        if (playerMoney.SpendMoney(cost))
        {
            vacuumSuction.RechargeBattery(); // Recharge to full
            Debug.Log($"Battery fully recharged for ${cost:F2}.");
            upgradesUI.UpdateUI(this); // Refresh UI
        }
        else
        {
            Debug.Log("Cannot afford to recharge battery!");
        }
    }
}