using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public enum SlideDirection { Left, Right }

/// <summary>
/// Hội thoại tuyến tính (Linear Dialogue) - Kích hoạt bằng Click chuột/Button UI.
/// Nâng cấp: Tích hợp Avatar hình ảnh trượt vào mượt mà (có tùy chỉnh hướng, vị trí dừng, tốc độ).
/// </summary>
public class LinearDialogueNPC : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string content;

        [Header("=== AVATAR SETTINGS ===")]
        [Tooltip("Ảnh chân dung nhân vật cho câu thoại này")]
        public Sprite speakerAvatar;

        [Tooltip("Hướng trượt của Avatar vào màn hình (Trái hoặc Phải)")]
        public SlideDirection slideDirection = SlideDirection.Left;
    }

    [Header("Dialogue Data")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueContentText;

    [Header("UI Avatar Slide Settings")]
    [Tooltip("Component Image hiển thị chân dung nhân vật")]
    [SerializeField] private Image avatarImage;

    [Tooltip("Vị trí dừng (Anchored Position) mong muốn của Avatar trên Canvas. Ví dụ: Left stop (X:-400, Y:-100), Right stop (X:400, Y:-100)")]
    [SerializeField] private Vector2 stopPositionLeft = new Vector2(-400f, -100f);
    [SerializeField] private Vector2 stopPositionRight = new Vector2(400f, -100f);

    [Tooltip("Khoảng cách đẩy xa ra ngoài màn hình để chuẩn bị trượt vào")]
    [SerializeField] private float slideOffsetDistance = 500f;

    [Tooltip("Thời gian hiệu ứng trượt hoàn thành (giây) - Càng nhỏ trượt càng nhanh")]
    [SerializeField] private float slideDuration = 0.35f;

    [Header("UI Fade Settings (Làm Mờ Màn Hình/UI)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeSpeed = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float targetFadeAlpha = 0f;

    [Header("Scene Transition Settings")]
    [SerializeField] private bool changeSceneOnEnd = false;
    [SerializeField] private string targetSceneName;

    private int currentLineIndex;
    private bool isDialogueActive;
    private Coroutine fadeCoroutine;
    private Coroutine avatarSlideCoroutine;
    private RectTransform avatarRectTransform;

    private void Awake()
    {
        if (avatarImage != null)
        {
            avatarRectTransform = avatarImage.GetComponent<RectTransform>();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }
    }

    private void Update()
    {
        if (isDialogueActive)
        {
            if ((Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                AdvanceDialogue();
            }
        }
    }

    private void OnMouseDown()
    {
        if (!isDialogueActive)
        {
            StartDialogue();
        }
    }

    public void StartDialogueFromButton()
    {
        if (!isDialogueActive)
        {
            StartDialogue();
        }
    }

    public void NextLineFromButton()
    {
        if (isDialogueActive)
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        isDialogueActive = true;
        currentLineIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        TriggerFade(targetFadeAlpha);

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex >= dialogueLines.Length)
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
        DialogueLine line = dialogueLines[currentLineIndex];

        if (speakerNameText != null) speakerNameText.text = line.speakerName;
        if (dialogueContentText != null) dialogueContentText.text = line.content;

        // Xử lý hiển thị và kích hoạt trượt Avatar
        HandleAvatarSlide(line);
    }

    /// <summary>
    /// Xử lý cập nhật Sprite và trượt Avatar từ mép màn hình vào vị trí dừng tùy chỉnh
    /// </summary>
    private void HandleAvatarSlide(DialogueLine line)
    {
        if (avatarImage == null || avatarRectTransform == null) return;

        if (line.speakerAvatar != null)
        {
            avatarImage.gameObject.SetActive(true);
            avatarImage.sprite = line.speakerAvatar;

            if (avatarSlideCoroutine != null)
            {
                StopCoroutine(avatarSlideCoroutine);
            }

            avatarSlideCoroutine = StartCoroutine(Routine_SlideAvatar(line.slideDirection));
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Coroutine tính toán hiệu ứng trượt mượt mà (SmoothStep / Lerp)
    /// </summary>
    private IEnumerator Routine_SlideAvatar(SlideDirection direction)
    {
        // 1. Xác định vị trí dừng (Target Position) theo tùy chỉnh trong Inspector
        Vector2 targetPos = (direction == SlideDirection.Left) ? stopPositionLeft : stopPositionRight;

        // 2. Tính vị trí bắt đầu (Start Position): Đẩy xa ra ngoài mép màn hình
        float startXOffset = (direction == SlideDirection.Left) ? -slideOffsetDistance : slideOffsetDistance;
        Vector2 startPos = targetPos + new Vector2(startXOffset, 0f);

        avatarRectTransform.anchoredPosition = startPos;

        float elapsedTime = 0f;

        // 3. Thực hiện trượt mượt mà
        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;

            // Dùng Mathf.SmoothStep để chuyển động giảm tốc khi gần tới điểm dừng (tạo độ mượt tự nhiên)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            avatarRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothProgress);
            yield return null;
        }

        avatarRectTransform.anchoredPosition = targetPos;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;

        if (avatarSlideCoroutine != null) StopCoroutine(avatarSlideCoroutine);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
        }

        TriggerFade(1f, () =>
        {
            if (changeSceneOnEnd && !string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"[LinearDialogueNPC] Đã hết thoại, đang chuyển sang Scene: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
        });
    }

    #region HỆ THỐNG LÀM MỜ VÀ ẨN UI (FADE & DISABLE SYSTEM)

    private void TriggerFade(float targetAlpha, System.Action onComplete = null)
    {
        if (fadeCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
    {
        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }

    #endregion
}