using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyUISystem : MonoBehaviour
{
    public enum DifficultyMode
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Asian = 3
    }

    [Header("=== DANH SÁCH 4 BUTTON (Easy, Normal, Hard, Asian) ===")]
    [SerializeField] private Button btnEasy;
    [SerializeField] private Button btnNormal;
    [SerializeField] private Button btnHard;
    [SerializeField] private Button btnAsian;

    [Header("=== HIỂN THỊ TÊN ĐỘ KHÓ (TEXT MESH PRO) ===")]
    [SerializeField] private TextMeshProUGUI txtDifficultyName;

    [Header("=== MÀU SẮC ĐỂ PHÂN BIỆT TRẠNG THÁI NÚT ===")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    private const string SAVED_DIFFICULTY_KEY = "GameDifficulty";
    private Button currentlySelectedButton;
    private bool daDangKySuKien = false;

    private void Awake()
    {
        DangKySuKienNut();
    }

    private void OnEnable()
    {
        DangKySuKienNut();
        DongBoTrangThaiTuSave();
    }

    private void Start()
    {
        if (txtDifficultyName == null)
        {
            Debug.LogWarning("[DifficultyUI] Chưa gán 'txtDifficultyName' (TextMeshProUGUI) trên Inspector!");
        }

        DongBoTrangThaiTuSave();
    }

    // Đăng ký sự kiện click 1 lần duy nhất, tránh gắn trùng listener khi bật/tắt UI
    private void DangKySuKienNut()
    {
        if (daDangKySuKien) return;

        if (btnEasy != null) btnEasy.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Easy, btnEasy));
        if (btnNormal != null) btnNormal.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Normal, btnNormal));
        if (btnHard != null) btnHard.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Hard, btnHard));
        if (btnAsian != null) btnAsian.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Asian, btnAsian));

        daDangKySuKien = true;
    }

    // Đọc độ khó từ PlayerPrefs và cập nhật cả nút lẫn TextMeshPro
    public void DongBoTrangThaiTuSave()
    {
        int savedModeIndex = PlayerPrefs.GetInt(SAVED_DIFFICULTY_KEY, (int)DifficultyMode.Normal);
        DifficultyMode currentMode = (DifficultyMode)Mathf.Clamp(savedModeIndex, 0, 3);

        ApDungHienThiDoKho(currentMode, LayButtonTheoMode(currentMode));
    }

    private void OnSelectDifficulty(DifficultyMode mode, Button clickedButton)
    {
        if (currentlySelectedButton == clickedButton) return;

        ApDungHienThiDoKho(mode, clickedButton);

        PlayerPrefs.SetInt(SAVED_DIFFICULTY_KEY, (int)mode);
        PlayerPrefs.Save();

        Debug.Log($"<color=yellow>[DifficultyUI]</color> Đã lưu độ khó mới: {mode}");
    }

    private void ApDungHienThiDoKho(DifficultyMode mode, Button targetButton)
    {
        // Reset màu các nút về bình thường trước khi highlight nút được chọn
        ResetTatCaNut();

        currentlySelectedButton = targetButton;
        if (currentlySelectedButton != null)
        {
            SetButtonVisual(currentlySelectedButton, true);
        }

        UpdateDifficultyText(mode);
    }

    private void ResetTatCaNut()
    {
        Button[] allButtons = { btnEasy, btnNormal, btnHard, btnAsian };
        foreach (Button btn in allButtons)
        {
            if (btn != null)
            {
                SetButtonVisual(btn, false);
            }
        }
    }

    private Button LayButtonTheoMode(DifficultyMode mode)
    {
        return mode switch
        {
            DifficultyMode.Easy => btnEasy,
            DifficultyMode.Normal => btnNormal,
            DifficultyMode.Hard => btnHard,
            DifficultyMode.Asian => btnAsian,
            _ => btnNormal
        };
    }

    private void SetButtonVisual(Button btn, bool isSelected)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = isSelected ? selectedColor : normalColor;
        }

        btn.interactable = !isSelected;
    }

    private void UpdateDifficultyText(DifficultyMode mode)
    {
        if (txtDifficultyName == null) return;

        txtDifficultyName.text = mode switch
        {
            DifficultyMode.Easy => "Difficulty: EASY",
            DifficultyMode.Normal => "Difficulty: NORMAL",
            DifficultyMode.Hard => "Difficulty: HARD",
            DifficultyMode.Asian => "Difficulty: ASIAN",
            _ => "Difficulty: NORMAL"
        };
    }
}