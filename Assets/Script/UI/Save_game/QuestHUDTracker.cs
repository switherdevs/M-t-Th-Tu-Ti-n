using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class QuestHUDTracker : MonoBehaviour
{
    public static QuestHUDTracker Instance;

    [Header("--- CẤU HÌNH TRUY VẤN MẢNG ---")]
    [Tooltip("Tích chọn nếu ID Quest trong Game của bạn bắt đầu từ 1 (ID: 1, 2, 3...)\nBỏ tích nếu ID Quest bắt đầu từ 0 (ID: 0, 1, 2...)")]
    public bool idQuestBatDauTu1 = true;

    [Header("--- MẢNG TEXT HIỂN THỊ QUEST THEO ID ---")]
    [Tooltip("Kéo các Text TMP vào đây theo đúng thứ tự mảng:\n- Element 0: Text cho Quest 1 (nếu idQuestBatDauTu1 = true)\n- Element 1: Text cho Quest 2...")]
    public TextMeshProUGUI[] danhSachTextQuestUI;

    // Sự kiện Cập nhật HUD real-time
    public static event Action OnQuestProgressChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện khi tiến trình Quest thay đổi
        OnQuestProgressChanged += CapNhatGiaoDienHUD;
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
    }

    private IEnumerator Start()
    {
        // Trì hoãn 1 khung hình để đảm bảo QuestSaveSystem đã LoadSaveFile() hoàn tất
        yield return null;

        // Khởi tạo và hiển thị UI ngay lập tức khi vào Game
        CapNhatGiaoDienHUD();
    }

    /// <summary>
    /// Hàm gọi sự kiện cập nhật UI thời gian thực từ bên ngoài
    /// </summary>
    public static void ThongBaoCapNhatHUD()
    {
        OnQuestProgressChanged?.Invoke();
    }

    /// <summary>
    /// Hàm duyệt mảng và hiển thị nội dung trực tiếp lên Text TMP dựa theo Index và ID Quest
    /// </summary>
    public void CapNhatGiaoDienHUD()
    {
        if (danhSachTextQuestUI == null || danhSachTextQuestUI.Length == 0)
        {
            Debug.LogWarning("[QuestHUDTracker] Mảng danhSachTextQuestUI đang trống! Hãy kéo Text TMP vào Inspector.");
            return;
        }

        // 1. Mặc định ẩn toàn bộ Text trong mảng trước khi quét dữ liệu
        XoaRongToanBoText();

        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogError("[QuestHUDTracker] Chưa đọc được dữ liệu QuestSaveSystem!");
            return;
        }

        // 2. Duyệt qua mảng Text theo chỉ số index 'i'
        for (int i = 0; i < danhSachTextQuestUI.Length; i++)
        {
            if (danhSachTextQuestUI[i] == null) continue;

            // 🎯 TÍNH TOÁN ID QUEST THỰC TẾ ĐỂ TRÁNH BỊ LỆCH 1 GIÁ TRỊ:
            // Nếu idQuestBatDauTu1 = true  -> index 0 tương ứng idQuest = 1 (i + 1)
            // Nếu idQuestBatDauTu1 = false -> index 0 tương ứng idQuest = 0 (i)
            int idQuestThucTe = idQuestBatDauTu1 ? (i + 1) : i;

            // Lấy tiến trình Quest từ hệ thống Save dựa theo ID Quest thực tế
            ProgressQuest progress = QuestSaveSystem.Instance.LayTienTrinhQuest(idQuestThucTe);

            if (progress == null)
            {
                continue;
            }

            Debug.Log($"[QuestHUDTracker] Mảng Index [{i}] -> Tìm Quest ID [{idQuestThucTe}] -> Trạng thái: {progress.trangThai}");

            // Chỉ hiển thị khi Quest đang ở trạng thái DangLam hoặc DaXongChuaTra
            if (progress.trangThai == TrangThaiQuest.DangLam || progress.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                // Lấy ScriptableObject dữ liệu cố định của Quest theo ID thực tế
                QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(idQuestThucTe);

                if (data != null)
                {
                    // Bật GameObject chứa Text lên
                    danhSachTextQuestUI[i].gameObject.SetActive(true);

                    // Gán nội dung hiển thị dựa vào trạng thái
                    if (progress.trangThai == TrangThaiQuest.DangLam)
                    {
                        danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: {progress.soBoXuongDaDiet}/{data.soLuongBoXuongCanDiet}";
                    }
                    else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                    {
                        danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: <color=green>[Hoàn thành] quay về thành trả nhiệm vụ!</color>";
                    }
                }
                else
                {
                    Debug.LogWarning($"[QuestHUDTracker] Quest ID {idQuestThucTe} đang làm nhưng KHÔNG tìm thấy QuestData!");
                }
            }
        }
    }

    /// <summary>
    /// Hàm ẩn tất cả các GameObject Text có trong mảng
    /// </summary>
    private void XoaRongToanBoText()
    {
        for (int i = 0; i < danhSachTextQuestUI.Length; i++)
        {
            if (danhSachTextQuestUI[i] != null)
            {
                danhSachTextQuestUI[i].text = "";
                danhSachTextQuestUI[i].gameObject.SetActive(false);
            }
        }
    }
}