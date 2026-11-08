using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DialogueTextManager : MonoBehaviour
{
    [Header("=== NỘI DUNG TIẾNG ANH ===")]
    [TextArea(3, 5)]
    [SerializeField] private string englishText;

    private TextMeshProUGUI targetTMP;
    private string originalText; // Lưu lại chữ tiếng Việt gốc

    private void Awake()
    {
        targetTMP = GetComponent<TextMeshProUGUI>();
        if (targetTMP != null)
        {
            originalText = targetTMP.text; // Lưu lại text gốc ban đầu
        }
    }

    private void OnEnable()
    {
        // Đăng ký nhận sự kiện đổi ngôn ngữ để đổi text ngay lập tức
        GameCore.Settings.SettingsManager.OnLanguageChanged += HandleLanguageChanged;
        ApplyText();
    }

    private void OnDisable()
    {
        GameCore.Settings.SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(string newLang)
    {
        ApplyText();
    }

    public void ApplyText()
    {
        if (targetTMP == null) return;

        bool isEnglishSelected = false;
        if (GameCore.Settings.SettingsManager.Instance != null)
        {
            isEnglishSelected = GameCore.Settings.SettingsManager.Instance.IsEnglish();
        }

        if (isEnglishSelected && !string.IsNullOrEmpty(englishText))
        {
            targetTMP.text = englishText;
        }
        else
        {
            targetTMP.text = originalText; // Trả về tiếng Việt gốc nếu chọn VN
        }
    }
}