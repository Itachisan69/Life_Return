using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashInfoUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Image rarityColorBar;
    [SerializeField] private Image itemIcon;

    [Header("Animation")]
    [SerializeField] private float fadeSpeed = 5f;

    private CanvasGroup canvasGroup;
    private bool shouldShow = false;

    void Awake()
    {
        canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = infoPanel.AddComponent<CanvasGroup>();
        }

        HideItemInfo();
    }

    void Update()
    {
        // Smooth fade in/out
        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    public void ShowItemInfo(TrashItemData itemData)
    {
        if (itemData == null) return;

        shouldShow = true;
        infoPanel.SetActive(true);

        // Set item name
        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        // Set rarity
        if (rarityText != null)
        {
            rarityText.text = itemData.rarity.ToString();
            rarityText.color = itemData.rarityColor;
        }

        // Set rarity color bar
        if (rarityColorBar != null)
        {
            rarityColorBar.color = itemData.rarityColor;
        }

        // Set icon
        if (itemIcon != null && itemData.itemIcon != null)
        {
            itemIcon.sprite = itemData.itemIcon;
            itemIcon.gameObject.SetActive(true);
        }
        else if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(false);
        }
    }

    public void HideItemInfo()
    {
        shouldShow = false;
        // Don't immediately disable - let it fade out
        Invoke(nameof(DisablePanel), 0.5f);
    }

    void DisablePanel()
    {
        if (!shouldShow)
        {
            infoPanel.SetActive(false);
        }
    }
}