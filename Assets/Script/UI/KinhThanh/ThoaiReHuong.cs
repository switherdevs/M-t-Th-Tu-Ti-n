using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hội thoại phân nhánh (Branching Dialogue) - dùng cho lựa chọn mang tính đạo đức.
/// Player nhấn E để bắt đầu.
/// - Node không có lựa chọn: nhấn Enter để qua node tiếp theo.
/// - Node có lựa chọn: click vào Button UI được tạo tự động (không giới hạn số lượng).
/// </summary>
public class BranchingDialogueNPC : MonoBehaviour
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextNodeIndex = -1; // -1 = kết thúc hội thoại
    }

    [System.Serializable]
    public class DialogueNode
    {
        public string speakerName;
        [TextArea(2, 5)] public string content;
        [Tooltip("Để trống nếu node này chỉ cần nhấn Enter để đi tới node kế tiếp (index + 1)")]
        public DialogueChoice[] choices;
    }

    [Header("Dialogue Data")]
    [SerializeField] private DialogueNode[] dialogueNodes;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueContentText;
    [SerializeField] private GameObject interactPrompt;

    [Header("Choice UI")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private int currentNodeIndex;
    private bool isPlayerInRange;
    private bool isDialogueActive;

    private void Update()
    {
        if (isPlayerInRange && !isDialogueActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDialogue();
        }

        if (isDialogueActive && !HasChoices(currentNodeIndex) && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GoToNode(currentNodeIndex + 1);
        }
    }

    private bool HasChoices(int nodeIndex)
    {
        return dialogueNodes[nodeIndex].choices != null && dialogueNodes[nodeIndex].choices.Length > 0;
    }

    private void StartDialogue()
    {
        if (dialogueNodes == null || dialogueNodes.Length == 0) return;

        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);
        GoToNode(0);
    }

    private void GoToNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= dialogueNodes.Length)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = nodeIndex;
        DialogueNode node = dialogueNodes[nodeIndex];
        speakerNameText.text = node.speakerName;
        dialogueContentText.text = node.content;

        if (HasChoices(nodeIndex))
        {
            ShowChoices(node.choices);
        }
        else
        {
            ClearChoices();
        }
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        ClearChoices();
        choicesContainer.gameObject.SetActive(true);

        foreach (DialogueChoice choice in choices)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

            DialogueChoice capturedChoice = choice; // tránh lỗi closure trong foreach
            btn.onClick.AddListener(() => OnChoiceSelected(capturedChoice));

            spawnedButtons.Add(btn.gameObject);
        }
    }

    private void ClearChoices()
    {
        foreach (GameObject obj in spawnedButtons)
        {
            Destroy(obj);
        }
        spawnedButtons.Clear();
        choicesContainer.gameObject.SetActive(false);
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        GoToNode(choice.nextNodeIndex);
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        ClearChoices();
        if (isPlayerInRange && interactPrompt != null) interactPrompt.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (isDialogueActive) EndDialogue();
        }
    }
}
