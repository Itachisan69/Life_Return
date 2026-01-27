using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VacuumHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VacuumSuction vacuum;

    [Header("Battery UI")]
    [SerializeField] private TextMeshProUGUI batteryText; // Drag your battery TMP text here
 
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowBatteryColor = Color.red;

    [Header("Inventory UI")]
    [SerializeField] private TextMeshProUGUI inventoryText; // Drag your inventory TMP text here

    void Update()
    {
        if (vacuum == null) return;

        UpdateBatteryUI();
        UpdateInventoryUI();
    }

    void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            // Rounds the float (e.g., 99.4 becomes 99)
            int current = Mathf.RoundToInt(vacuum.CurrentBattery);
            int max = Mathf.RoundToInt(vacuum.MaxBattery);

            batteryText.text = $"Battery: {current} / {max}";

            // Visual feedback: Text turns red when battery is depleted
            batteryText.color = vacuum.IsBatteryDepleted ? lowBatteryColor : normalColor;
        }

        
    }

    void UpdateInventoryUI()
    {
        if (inventoryText != null)
        {
            // Shows "Inventory: 5 / 10"
            inventoryText.text = $"Inventory: {vacuum.CurrentInventorySize} / {vacuum.maxInventoryCapacity}";

            // Change text color to red if the bag is full
            inventoryText.color = (vacuum.CurrentInventorySize >= vacuum.maxInventoryCapacity)
                ? lowBatteryColor
                : normalColor;
        }
    }
}