using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoney : MonoBehaviour
{
    [Header("Money Settings")]
    [SerializeField] private float currentMoney = 0f;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyDisplayText;

    public float CurrentMoney => currentMoney;

    void Start()
    {
        UpdateMoneyDisplay();
    }

    /// <summary>
    /// Adds money to the player's balance.
    /// </summary>
    public void AddMoney(float amount)
    {
        currentMoney += amount;
        UpdateMoneyDisplay();
        Debug.Log($"Money added: ${amount:F2}. Total: ${currentMoney:F2}");
    }

    /// <summary>
    /// Spends money from the player's balance.
    /// </summary>
    public bool SpendMoney(float amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyDisplay();
            Debug.Log($"Money spent: ${amount:F2}. Remaining: ${currentMoney:F2}");
            return true;
        }
        else
        {
            Debug.Log("Not enough money!");
            return false;
        }
    }

    void UpdateMoneyDisplay()
    {
        if (moneyDisplayText != null)
        {
            moneyDisplayText.text = $"${currentMoney:F2}";
        }
    }
}