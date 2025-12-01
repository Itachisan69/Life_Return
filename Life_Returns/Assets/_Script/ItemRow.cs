using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SuckableObjectData;

public class ItemRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text pricePerUnitText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text totalPriceText;
    [SerializeField] private Image backgroundImage;

    public void Setup(SuckableObjectData itemData, int quantity)
    {
        if (itemData == null) return;

        if (itemNameText != null)
        {
            itemNameText.text = itemData.objectName;
        }

        if (rarityText != null)
        {
            rarityText.text = itemData.rarity.ToString();
            rarityText.color = GetRarityColor(itemData.rarity);
        }

        if (pricePerUnitText != null)
        {
            pricePerUnitText.text = $"${itemData.sellPrice:F2}";
        }

        if (quantityText != null)
        {
            quantityText.text = $"x{quantity}";
        }

        float totalPrice = itemData.sellPrice * quantity;
        if (totalPriceText != null)
        {
            totalPriceText.text = $"${totalPrice:F2}";
        }

        if (backgroundImage != null)
        {
            Color bgColor = GetRarityColor(itemData.rarity);
            bgColor.a = 0.1f;
            backgroundImage.color = bgColor;
        }
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case Rarity.Uncommon:
                return new Color(0.2f, 0.8f, 0.2f); // Green
            case Rarity.Rare:
                return new Color(0.2f, 0.5f, 1f); // Blue
            case Rarity.Epic:
                return new Color(0.6f, 0.2f, 0.8f); // Purple
            case Rarity.Legendary:
                return new Color(1f, 0.6f, 0f); // Orange
            default:
                return Color.white;
        }
    }
}
