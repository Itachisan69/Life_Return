using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class InventoryItem
{
    public TrashItemData itemData;
    public int quantity;

    public InventoryItem(TrashItemData data)
    {
        itemData = data;
        quantity = 1;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int maxStorageSize = 100;
    [SerializeField] private float maxWeight = 50f;

    [Header("Current Stats")]
    [SerializeField] private int currentStorageUsed = 0;
    [SerializeField] private float currentWeight = 0f;

    private List<InventoryItem> items = new List<InventoryItem>();

    public event Action<TrashItemData> OnItemAdded;
    public event Action OnInventoryChanged;

    public bool AddItem(TrashItemData itemData)
    {
        if (itemData == null) return false;

        // Check if there's space
        if (currentStorageUsed + itemData.storageSize > maxStorageSize)
        {
            Debug.Log("Inventory full! Not enough storage space.");
            return false;
        }

        if (currentWeight + itemData.weight > maxWeight)
        {
            Debug.Log("Inventory full! Too heavy.");
            return false;
        }

        // Check if item already exists in inventory (stack)
        InventoryItem existingItem = items.Find(i => i.itemData == itemData);

        if (existingItem != null)
        {
            existingItem.quantity++;
        }
        else
        {
            items.Add(new InventoryItem(itemData));
        }

        // Update stats
        currentStorageUsed += itemData.storageSize;
        currentWeight += itemData.weight;

        // Trigger events
        OnItemAdded?.Invoke(itemData);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Collected: {itemData.itemName} | Storage: {currentStorageUsed}/{maxStorageSize} | Weight: {currentWeight:F1}/{maxWeight}");

        return true;
    }

    public void RemoveItem(TrashItemData itemData, int quantity = 1)
    {
        InventoryItem item = items.Find(i => i.itemData == itemData);
        if (item == null) return;

        item.quantity -= quantity;

        if (item.quantity <= 0)
        {
            items.Remove(item);
        }

        currentStorageUsed -= itemData.storageSize * quantity;
        currentWeight -= itemData.weight * quantity;

        OnInventoryChanged?.Invoke();
    }

    public int GetTotalValue()
    {
        int total = 0;
        foreach (var item in items)
        {
            total += item.itemData.sellValue * item.quantity;
        }
        return total;
    }

    public List<InventoryItem> GetAllItems() => items;

    public int CurrentStorage => currentStorageUsed;
    public int MaxStorage => maxStorageSize;
    public float CurrentWeight => currentWeight;
    public float MaxWeight => maxWeight;
    public bool IsFull => currentStorageUsed >= maxStorageSize || currentWeight >= maxWeight;
}