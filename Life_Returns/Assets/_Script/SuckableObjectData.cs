// SuckableObjectData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewSuckableData", menuName = "Vacuum Game/Suckable Object Data", order = 1)]
public class SuckableObjectData : ScriptableObject
{
    public string objectName = "Mystery Object";
    public int size = 1; // Inventory space it takes up
    public int sellPrice = 10;
    public Rarity rarity = Rarity.Common;

    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
}