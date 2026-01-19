using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VacuumSuction vacuumSuction; // Added reference like Recycling script
    public Dialogue dialogue;
    public GameObject dialogueUI;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt;

    private Transform player;
    private bool playerInRange = false;
    private bool dialogueActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (dialogueUI != null) dialogueUI.SetActive(false);
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

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(playerInRange && !dialogueActive);
        }
    }

    void HandleInput()
    {
        if (!playerInRange || dialogueActive) return;

        if (Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        dialogueActive = true;

        // Optional: Freeze time
        // Time.timeScale = 0; 

        if (dialogueUI != null) dialogueUI.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);

        DisablePlayerControls();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        FindObjectOfType<DialogueManager>().StartDialogue(dialogue, this);
    }

    public void EndDialogue()
    {
        dialogueActive = false;

        // Optional: Unfreeze time
        // Time.timeScale = 1;

        EnablePlayerControls();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisablePlayerControls()
    {
        // 1. Disable Movement
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null) playerController.enabled = false;

        // 2. Disable Camera
        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null) cameraController.enabled = false;

        // 3. Disable Vacuum (Logic from Recycling Script)
        if (vacuumSuction != null)
        {
            vacuumSuction.enabled = false;
        }
    }

    void EnablePlayerControls()
    {
        var playerController = player?.GetComponent<MonoBehaviour>();
        if (playerController != null) playerController.enabled = true;

        var cameraController = Camera.main?.GetComponent<MonoBehaviour>();
        if (cameraController != null) cameraController.enabled = true;

        // Re-enable Vacuum
        if (vacuumSuction != null)
        {
            vacuumSuction.enabled = true;
        }
    }
}