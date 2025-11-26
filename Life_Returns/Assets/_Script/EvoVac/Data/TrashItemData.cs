using UnityEngine;

[CreateAssetMenu(fileName = "New Trash Item", menuName = "EcoVac/Trash Item Data")]
public class TrashItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite itemIcon;

    [Header("Physical Properties")]
    [Tooltip("Weight affects suction speed (heavier = slower)")]
    public float weight = 1f;

    [Tooltip("How much inventory space this takes")]
    public int storageSize = 1;

    [Header("Rarity & Value")]
    public ItemRarity rarity;
    public int sellValue;

    [Header("Visual")]
    public Color rarityColor = Color.white;
    public GameObject shrinkParticlePrefab;
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}