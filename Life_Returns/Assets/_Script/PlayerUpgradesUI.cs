using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradesUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button closeButton;

    [Header("Battery Upgrade")]
    [SerializeField] private TMP_Text batteryInfoText;
    [SerializeField] private Button batteryUpgradeButton;

    [Header("Inventory Upgrade")]
    [SerializeField] private TMP_Text inventoryInfoText;
    [SerializeField] private Button inventoryUpgradeButton;

    [Header("Recharge")]
    [SerializeField] private TMP_Text rechargeInfoText;
    [SerializeField] private Button rechargeButton;

    private UpgradeMachine currentMachine;

    public bool IsUIOpen => upgradePanel.activeInHierarchy;

    void Awake()
    {
        // Hide the UI at the start
        upgradePanel.SetActive(false);

        closeButton.onClick.AddListener(CloseUIViaButton);
    }

    public void CloseUIViaButton()
    {
        if (currentMachine == null) return;

        // 1. Close the UI Panel
        CloseUI();

        // 2. Restore Cursor State for Gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 3. Resume Game Time
        Time.timeScale = 1f;

        Debug.Log("Upgrade UI closed by button.");
    }
    // --- Public Control Functions ---

    public void OpenUI(UpgradeMachine machine)
    {
        currentMachine = machine;

        // Link the button actions to the UpgradeMachine's public functions
        batteryUpgradeButton.onClick.RemoveAllListeners();
        inventoryUpgradeButton.onClick.RemoveAllListeners();
        rechargeButton.onClick.RemoveAllListeners();

        batteryUpgradeButton.onClick.AddListener(currentMachine.AttemptUpgradeBattery);
        inventoryUpgradeButton.onClick.AddListener(currentMachine.AttemptUpgradeInventory);
        rechargeButton.onClick.AddListener(currentMachine.AttemptRechargeBattery);

        UpdateUI(machine);
        upgradePanel.SetActive(true);
    }

    public void CloseUI()
    {
        upgradePanel.SetActive(false);
        currentMachine = null;
    }

    // --- Update Logic ---

    public void UpdateUI(UpgradeMachine machine)
    {
        UpdateBatteryUI(machine);
        UpdateInventoryUI(machine);
        UpdateRechargeUI(machine);
    }

    private void UpdateBatteryUI(UpgradeMachine machine)
    {
        int currentLevel = machine.CurrentBatteryLevel;
        UpgradeTier[] tiers = machine.BatteryUpgrades;

        if (currentLevel >= tiers.Length)
        {
            batteryInfoText.text = $"**Battery Capacity**\nLevel: MAX\n**MAXED**";
            batteryUpgradeButton.interactable = false;
        }
        else
        {
            UpgradeTier nextTier = tiers[currentLevel];
            batteryInfoText.text =
                $"**Battery Capacity**\nLevel: {currentLevel} -> {currentLevel + 1}\n" +
                $"Increase: +{nextTier.ValueIncrease:F0} Max Battery\n" +
                $"Cost: **${nextTier.Cost:F2}**";

            // Assuming PlayerMoney.CurrentMoney is accessible (which it is via the property)
            // You might need a direct reference to PlayerMoney if it's not on the machine
            // For now, this is sufficient as the machine handles the check:
            batteryUpgradeButton.interactable = true;
        }
    }

    private void UpdateInventoryUI(UpgradeMachine machine)
    {
        int currentLevel = machine.CurrentInventoryLevel;
        UpgradeTier[] tiers = machine.InventoryUpgrades;

        if (currentLevel >= tiers.Length)
        {
            inventoryInfoText.text = $"**Inventory Capacity**\nLevel: MAX\n**MAXED**";
            inventoryUpgradeButton.interactable = false;
        }
        else
        {
            UpgradeTier nextTier = tiers[currentLevel];
            int increase = Mathf.RoundToInt(nextTier.ValueIncrease);
            inventoryInfoText.text =
                $"**Inventory Capacity**\nLevel: {currentLevel} -> {currentLevel + 1}\n" +
                $"Increase: +{increase} Storage Size\n" +
                $"Cost: **${nextTier.Cost:F2}**";

            inventoryUpgradeButton.interactable = true;
        }
    }

    private void UpdateRechargeUI(UpgradeMachine machine)
    {
        float rechargeCost = machine.CurrentRechargeCost;

        rechargeInfoText.text =
            $"**Recharge Battery**\n" +
            $"Refills battery to 100%\n" +
            $"Cost: **${rechargeCost:F2}**";

        // Simple check to disable button if battery is full
        if (machine.vacuumSuction.CurrentBattery >= machine.vacuumSuction.MaxBattery)
        {
            rechargeButton.interactable = false;
            rechargeInfoText.text += "\n*(Battery is Full)*";
        }
        else
        {
            rechargeButton.interactable = true;
        }
    }
}