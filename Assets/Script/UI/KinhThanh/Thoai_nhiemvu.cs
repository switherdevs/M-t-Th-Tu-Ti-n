using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Hội thoại theo trạng thái Quest (Condition-based Dialogue).
/// Trạng thái quest lưu bằng enum ngay trong script này.
/// Player nhấn E để bắt đầu, Enter để qua câu thoại tiếp theo.
/// Gọi CompleteQuest() từ script khác (VD: script điều kiện hoàn thành nhiệm vụ) khi player đủ điều kiện.
/// </summary>
public class QuestDialogueNPC : MonoBehaviour
{
    public enum QuestState { NotStarted, InProgress, Completed }

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string content;
    }

    [Header("Quest State")]
    [SerializeField] private QuestState currentQuestState = QuestState.NotStarted;

    [Header("Dialogue theo trạng thái Quest")]
    [SerializeField] private DialogueLine[] notStartedDialogue;
    [SerializeField] private DialogueLine[] inProgressDialogue;
    [SerializeField] private DialogueLine[] completedDialogue;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueContentText;
    [SerializeField] private GameObject interactPrompt;

    private DialogueLine[] activeDialogueSet;
    private int currentLineIndex;
    private bool isPlayerInRange;
    private bool isDialogueActive;

    private void Update()
    {
        if (isPlayerInRange && !isDialogueActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDialogue();
        }

        if (isDialogueActive && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            AdvanceDialogue();
        }
    }

    private void StartDialogue()
    {
        activeDialogueSet = GetDialogueSetForCurrentState();
        if (activeDialogueSet == null || activeDialogueSet.Length == 0) return;

        isDialogueActive = true;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);
        ShowCurrentLine();

        // Khi player nói chuyện lần đầu (NotStarted) -> tự nhận nhiệm vụ, chuyển sang InProgress
        if (currentQuestState == QuestState.NotStarted)
        {
            currentQuestState = QuestState.InProgress;
        }
    }

    private DialogueLine[] GetDialogueSetForCurrentState()
    {
        switch (currentQuestState)
        {
            case QuestState.NotStarted: return notStartedDialogue;
            case QuestState.InProgress: return inProgressDialogue;
            case QuestState.Completed: return completedDialogue;
            default: return null;
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex >= activeDialogueSet.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = activeDialogueSet[currentLineIndex];
        speakerNameText.text = line.speakerName;
        dialogueContentText.text = line.content;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        if (isPlayerInRange && interactPrompt != null) interactPrompt.SetActive(true);
    }

    /// <summary>
    /// Gọi hàm này từ script khác khi player hoàn thành điều kiện nhiệm vụ
    /// (VD: nộp đủ item, giết đủ quái, đến đúng vị trí...)
    /// </summary>
    public void CompleteQuest()
    {
        currentQuestState = QuestState.Completed;
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
