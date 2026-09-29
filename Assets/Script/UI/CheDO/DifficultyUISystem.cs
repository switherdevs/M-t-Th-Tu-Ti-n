using UnityEngine;
using UnityEngine.UI;
using TMPro; // Bổ sung thư viện TextMeshPro

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
    [SerializeField] private TextMeshProUGUI txtDifficultyName; // Kéo thả UI Text (TMP) vào đây

    [Header("=== MÀU SẮC ĐỂ PHÂN BIỆT TRẠNG THÁI NÚT ===")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    private const string SAVED_DIFFICULTY_KEY = "GameDifficulty";
    private Button currentlySelectedButton;

    private void Start()
    {
        // Gán Sự Kiện Click Cho 4 Nút Bấm
        if (btnEasy != null) btnEasy.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Easy, btnEasy));
        if (btnNormal != null) btnNormal.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Normal, btnNormal));
        if (btnHard != null) btnHard.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Hard, btnHard));
        if (btnAsian != null) btnAsian.onClick.AddListener(() => OnSelectDifficulty(DifficultyMode.Asian, btnAsian));

        // Đọc Save Đã Lưu Trước Đó, Mặc Định Là Normal (1) Nếu Chưa Lưu
        int savedModeIndex = PlayerPrefs.GetInt(SAVED_DIFFICULTY_KEY, (int)DifficultyMode.Normal);
        DifficultyMode currentMode = (DifficultyMode)savedModeIndex;

        // Bật Nút Tương Ứng Với Save Hiện Tại
        HighlightSavedButton(currentMode);
    }

    private void OnSelectDifficulty(DifficultyMode mode, Button clickedButton)
    {
        // 1. Nếu Chọn Lại Đúng Nút Đang Được Giữ Thì Không Làm Gì
        if (currentlySelectedButton == clickedButton) return;

        // 2. Bật Nút Cũ Ra (Trả Về Trạng Thái Bình Thường)
        if (currentlySelectedButton != null)
        {
            SetButtonVisual(currentlySelectedButton, false);
        }

        // 3. Giữ Nút Mới Được Chọn
        currentlySelectedButton = clickedButton;
        SetButtonVisual(currentlySelectedButton, true);

        // 4. Cập nhật TextMeshPro hiển thị tên độ khó bằng tiếng Anh
        UpdateDifficultyText(mode);

        // 5. Lưu Vào Save File (PlayerPrefs) Ngay Lập Tức
        PlayerPrefs.SetInt(SAVED_DIFFICULTY_KEY, (int)mode);
        PlayerPrefs.Save();

        Debug.Log($"<color=yellow>[DifficultyUI]</color> Đã lưu độ khó mới: {mode}");
    }

    private void HighlightSavedButton(DifficultyMode mode)
    {
        Button targetButton = mode switch
        {
            DifficultyMode.Easy => btnEasy,
            DifficultyMode.Normal => btnNormal,
            DifficultyMode.Hard => btnHard,
            DifficultyMode.Asian => btnAsian,
            _ => btnNormal
        };

        if (targetButton != null)
        {
            currentlySelectedButton = targetButton;
            SetButtonVisual(targetButton, true);
        }

        // Cập nhật Text hiển thị đúng chế độ đã save bằng tiếng Anh khi vừa vào game
        UpdateDifficultyText(mode);
    }

    private void SetButtonVisual(Button btn, bool isSelected)
    {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null)
        {
            btnImage.color = isSelected ? selectedColor : normalColor;
        }

        // Làm Cho Nút Đang Giữ Bị Vô Hiệu Hóa Click Tạm Thời Để Rõ Ràng Hơn
        btn.interactable = !isSelected;
    }

    // Hàm cập nhật chữ hiển thị trên UI TextMeshPro bằng Tiếng Anh
    private void UpdateDifficultyText(DifficultyMode mode)
    {
        if (txtDifficultyName == null) return;

        switch (mode)
        {
            case DifficultyMode.Easy:
                txtDifficultyName.text = "Difficulty: EASY";
                break;
            case DifficultyMode.Normal:
                txtDifficultyName.text = "Difficulty: NORMAL";
                break;
            case DifficultyMode.Hard:
                txtDifficultyName.text = "Difficulty: HARD";
                break;
            case DifficultyMode.Asian:
                txtDifficultyName.text = "Difficulty: ASIAN";
                break;
        }
    }
}