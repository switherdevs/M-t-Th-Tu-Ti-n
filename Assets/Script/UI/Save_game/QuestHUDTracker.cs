using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using GameCore.Quests;

public class QuestHUDTracker : MonoBehaviour
{
    public static QuestHUDTracker Instance;

    // =========================================================
    // THÔNG BÁO MẶC ĐỊNH
    // =========================================================
    [Header("--- THÔNG BÁO MẶC ĐỊNH ---")]
    [Tooltip("Text hiển thị dòng thông báo khi không có nhiệm vụ nào đang làm")]
    public TextMeshProUGUI textThongBaoMacDinh;

    [Tooltip("Nội dung hiện khi chưa nhận nhiệm vụ")]
    public string noiDungThongBaoMacDinh = "Hãy đến kinh thành nhận nhiệm vụ";

    [Tooltip("Nội dung hiện khi đã hoàn thành toàn bộ nhiệm vụ trong game")]
    public string noiDungHoanThanhTatCa = "Cảnh giới bạn đã cao, hãy đập đá";

    // =========================================================
    // MẢNG TEXT HIỂN THỊ QUEST ĐỘNG
    // =========================================================
    [Header("--- MẢNG TEXT HIỂN THỊ QUEST ĐỘNG ---")]
    [Tooltip("Kéo danh sách các TextMeshProUGUI dòng hiển thị quest vào đây")]
    public TextMeshProUGUI[] danhSachTextQuestUI;

    public static event Action OnQuestProgressChanged;

    // Bộ nhớ tạm Cache lưu QuestData để tối ưu hiệu năng
    private static Dictionary<int, QuestData> cacheQuestData = new Dictionary<int, QuestData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        OnQuestProgressChanged += CapNhatGiaoDienHUD;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ThucHienCapNhatTre());
    }

    private IEnumerator Start()
    {
        yield return null;
        CapNhatGiaoDienHUD();
    }

    private IEnumerator ThucHienCapNhatTre()
    {
        yield return null;
        CapNhatGiaoDienHUD();
    }

    /// <summary>
    /// Kích hoạt sự kiện thông báo cho toàn bộ hệ thống làm mới HUD
    /// </summary>
    public static void ThongBaoCapNhatHUD()
    {
        OnQuestProgressChanged?.Invoke();
    }

    /// <summary>
    /// Hàm chính xử lý làm sạch và vẽ lại thông tin nhiệm vụ lên giao diện HUD
    /// </summary>
    public void CapNhatGiaoDienHUD()
    {
        XoaRongToanBoText();

        // 1. Kiểm tra singleton SaveSystem
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            return;
        }

        // 2. Lấy danh sách nhiệm vụ đang làm
        List<ProgressQuest> danhSachQuestDangActive = LayToanBoQuestDangKichHoat();

        // 3. Nếu không có nhiệm vụ nào đang kích hoạt -> Hiện text mặc định
        if (danhSachQuestDangActive.Count == 0)
        {
            if (textThongBaoMacDinh != null)
            {
                textThongBaoMacDinh.gameObject.SetActive(true);
                textThongBaoMacDinh.text = KiemTraDaHoanThanhTatCaQuest() ? noiDungHoanThanhTatCa : noiDungThongBaoMacDinh;
            }
            return;
        }

        // 4. Nếu có nhiệm vụ -> Ẩn text mặc định và vẽ danh sách
        if (textThongBaoMacDinh != null)
        {
            textThongBaoMacDinh.gameObject.SetActive(false);
        }

        if (danhSachTextQuestUI == null || danhSachTextQuestUI.Length == 0)
        {
            return;
        }

        // 5. Duyệt qua mảng và gán dữ liệu hiển thị
        for (int i = 0; i < danhSachQuestDangActive.Count; i++)
        {
            if (i >= danhSachTextQuestUI.Length) break;
            if (danhSachTextQuestUI[i] == null) continue;

            ProgressQuest progress = danhSachQuestDangActive[i];

            // 🎯 Lấy QuestData từ hệ thống lưu trữ đồng bộ
            QuestData data = LayQuestDataDongBo(progress.idQuest);

            if (data != null)
            {
                danhSachTextQuestUI[i].gameObject.SetActive(true);

                int soYeuCau = data.loaiQuest == LoaiQuest.DietQuai ? data.soLuongBoXuongCanDiet : data.soLuongCanGiaiCuu;

                if (progress.trangThai == TrangThaiQuest.DangLam)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: {progress.soBoXuongDaDiet}/{soYeuCau}";
                }
                else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: <color=green>[Hoàn thành] Trả nhiệm vụ!</color>";
                }
            }
            else
            {
                Debug.LogWarning($"[QuestHUDTracker] Không tìm thấy QuestData cho Quest ID: {progress.idQuest}");
            }
        }
    }

    /// <summary>
    /// 🎯 HÀM LẤY QUEST DATA ĐỒNG BỘ NĂNG ĐỘNG
    /// </summary>
    private QuestData LayQuestDataDongBo(int idQuest)
    {
        // Lớp 1: Đọc từ Cache RAM
        if (cacheQuestData.TryGetValue(idQuest, out QuestData cached) && cached != null)
        {
            return cached;
        }

        // Lớp 2: Tìm từ QuestSaveSystem.Instance
        if (QuestSaveSystem.Instance != null)
        {
            QuestData dataFromSave = QuestSaveSystem.Instance.LayQuestDataTheoID(idQuest);
            if (dataFromSave != null)
            {
                cacheQuestData[idQuest] = dataFromSave;
                return dataFromSave;
            }
        }

        // Lớp 3: Tìm trực tiếp trong thư mục Resources (Quét tất cả thư mục con)
        QuestData[] allQuests = Resources.LoadAll<QuestData>("");
        foreach (QuestData q in allQuests)
        {
            if (q != null && q.idQuest == idQuest)
            {
                cacheQuestData[idQuest] = q;
                return q;
            }
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra người chơi đã làm xong toàn bộ quest có trong game hay chưa
    /// </summary>
    private bool KiemTraDaHoanThanhTatCaQuest()
    {
        if (QuestSaveSystem.Instance?.duLieuSaveHienTai?.danhSachProgress == null) return false;

        List<ProgressQuest> danhSachProgress = QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress;

        if (danhSachProgress == null || danhSachProgress.Count == 0) return false;

        int tongSoQuestTrongGame = QuestSaveSystem.Instance.danhSachQuestData != null ? QuestSaveSystem.Instance.danhSachQuestData.Count : 0;
        if (tongSoQuestTrongGame == 0 || danhSachProgress.Count < tongSoQuestTrongGame) return false;

        foreach (ProgressQuest progress in danhSachProgress)
        {
            if (progress.trangThai != TrangThaiQuest.HoanThanh) return false;
        }

        return true;
    }

    /// <summary>
    /// Lọc lấy các nhiệm vụ đang làm (DangLam) hoặc đã làm xong nhưng chưa trả (DaXongChuaTra)
    /// </summary>
    private List<ProgressQuest> LayToanBoQuestDangKichHoat()
    {
        List<ProgressQuest> ketQua = new List<ProgressQuest>();

        if (QuestSaveSystem.Instance?.duLieuSaveHienTai?.danhSachProgress == null) return ketQua;

        foreach (ProgressQuest progress in QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress)
        {
            if (progress.trangThai == TrangThaiQuest.DangLam || progress.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                ketQua.Add(progress);
            }
        }

        return ketQua;
    }

    /// <summary>
    /// Xóa toàn bộ nội dung dòng text và ẩn các ô text dư thừa trên UI
    /// </summary>
    private void XoaRongToanBoText()
    {
        if (danhSachTextQuestUI == null) return;

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