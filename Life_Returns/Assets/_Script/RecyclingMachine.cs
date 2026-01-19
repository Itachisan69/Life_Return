using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecyclingMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VacuumSuction vacuumSuction;
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private GameObject sellingUICanvas;
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemRowPrefab;
    [SerializeField] private TMP_Text totalValueText;
    [SerializeField] private Button sellAllButton;
    [SerializeField] private Button closeButton;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sellSFX;

    private Transform player;
    private bool isUIOpen = false;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (sellingUICanvas != null)
        {
            sellingUICanvas.SetActive(false);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(SellAllItems);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
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

        if (interactPrompt != null && !isUIOpen)
        {
            interactPrompt.SetActive(playerInRange);
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

        if (isUIOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
        }
    }

    void OpenUI()
    {
        if (vacuumSuction == null) return;

        isUIOpen = true;

        if (sellingUICanvas != null)
        {
            sellingUICanvas.SetActive(true);
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        DisablePlayerControls();
        PopulateInventoryList();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseUI()
    {
        isUIOpen = false;

        if (sellingUICanvas != null)
        {
            sellingUICanvas.SetActive(false);
        }

        EnablePlayerControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void PopulateInventoryList()
    {
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }

        var inventory = vacuumSuction.GetInventory();
        var itemGroups = new System.Collections.Generic.Dictionary<SuckableObjectData, int>();

        foreach (var item in inventory)
        {
            if (itemGroups.ContainsKey(item))
            {
                itemGroups[item]++;
            }
            else
            {
                itemGroups[item] = 1;
            }
        }

        float totalValue = 0f;

        foreach (var group in itemGroups)
        {
            SuckableObjectData itemData = group.Key;
            int quantity = group.Value;
            float itemTotalPrice = itemData.sellPrice * quantity;
            totalValue += itemTotalPrice;

            GameObject row = Instantiate(itemRowPrefab, itemListContainer);
            ItemRow itemRow = row.GetComponent<ItemRow>();

            if (itemRow != null)
            {
                itemRow.Setup(itemData, quantity);
            }
        }

        if (totalValueText != null)
        {
            totalValueText.text = $"Total Value: ${totalValue:F2}";
        }

        if (sellAllButton != null)
        {
            sellAllButton.interactable = inventory.Count > 0;
        }
    }

    void SellAllItems()
    {
        if (vacuumSuction == null || playerMoney == null) return;

        var inventory = vacuumSuction.GetInventory();

        if (inventory.Count == 0)
        {
            Debug.Log("Inventory is empty!");
            return;
        }

        float totalMoney = 0f;
        foreach (var item in inventory)
        {
            totalMoney += item.sellPrice;
        }

        playerMoney.AddMoney(totalMoney);
        vacuumSuction.ClearInventory();

        if (audioSource != null && sellSFX != null)
        {
            audioSource.PlayOneShot(sellSFX);
        }

        Debug.Log($"Sold all items for ${totalMoney:F2}");
        PopulateInventoryList();
    }

    void DisablePlayerControls()
    {
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Freeze camera rotation
        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        if (vacuumSuction != null)
        {
            vacuumSuction.enabled = false;
        }
    }

    void EnablePlayerControls()
    {
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Unfreeze camera rotation
        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }

        if (vacuumSuction != null)
        {
            vacuumSuction.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}