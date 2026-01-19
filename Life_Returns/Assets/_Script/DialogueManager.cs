using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueUI;
    public TextMeshProUGUI NPC_name;
    public TextMeshProUGUI dialogueText; // Renamed to avoid confusion with the class
    public Queue<string> sentences;
    public float typeSpeed = 0.05f;

    private DialogueTrigger currentDialogueTrigger;

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogueData, DialogueTrigger trigger)
    {
        currentDialogueTrigger = trigger;
        NPC_name.text = dialogueData.name;

        sentences.Clear();
        foreach (string sentence in dialogueData.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void EndDialogue()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);

        // This is the critical missing link:
        if (currentDialogueTrigger != null)
        {
            currentDialogueTrigger.EndDialogue();
        }

        currentDialogueTrigger = null;
    }
}