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

    [Tooltip("Nội dung hiện khi chưa nhận nhiệm vụ (Tiếng Việt)")]
    public string noiDungThongBaoMacDinh = "Hãy đến kinh thành nhận nhiệm vụ";
    [Tooltip("Nội dung hiện khi chưa nhận nhiệm vụ (Tiếng Anh)")]
    public string noiDungThongBaoMacDinhEN = "Go to the capital to accept quests";

    [Tooltip("Nội dung hiện khi đã hoàn thành toàn bộ nhiệm vụ trong game (Tiếng Việt)")]
    public string noiDungHoanThanhTatCa = "Cảnh giới bạn đã cao, hãy đập đá";
    [Tooltip("Nội dung hiện khi đã hoàn thành toàn bộ nhiệm vụ trong game (Tiếng Anh)")]
    public string noiDungHoanThanhTatCaEN = "Your realm is high, go mine stones";

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

        // 🎯 Lắng nghe sự kiện đổi ngôn ngữ toàn cục để cập nhật HUD ngay lập tức
        GameCore.Settings.SettingsManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Hủy đăng ký sự kiện
        GameCore.Settings.SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(string newLang)
    {
        CapNhatGiaoDienHUD();
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

    // Kiểm tra ngôn ngữ từ hệ thống save / settings
    private bool KiemTraNgonNguTiengAnh()
    {
        if (QuestSaveSystem.Instance != null)
        {
            return QuestSaveSystem.Instance.IsEnglish();
        }
        return false;
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

        bool isEnglish = KiemTraNgonNguTiengAnh();

        // 2. Lấy danh sách nhiệm vụ đang làm
        List<ProgressQuest> danhSachQuestDangActive = LayToanBoQuestDangKichHoat();

        // 3. Nếu không có nhiệm vụ nào đang kích hoạt -> Hiện text mặc định
        if (danhSachQuestDangActive.Count == 0)
        {
            if (textThongBaoMacDinh != null)
            {
                textThongBaoMacDinh.gameObject.SetActive(true);

                bool daHoanThanhTatCa = KiemTraDaHoanThanhTatCaQuest();
                if (daHoanThanhTatCa)
                {
                    textThongBaoMacDinh.text = isEnglish && !string.IsNullOrEmpty(noiDungHoanThanhTatCaEN) ? noiDungHoanThanhTatCaEN : noiDungHoanThanhTatCa;
                }
                else
                {
                    textThongBaoMacDinh.text = isEnglish && !string.IsNullOrEmpty(noiDungThongBaoMacDinhEN) ? noiDungThongBaoMacDinhEN : noiDungThongBaoMacDinh;
                }
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
                string tenQuestHienThi = data.tenNhiemVu;

                if (progress.trangThai == TrangThaiQuest.DangLam)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{tenQuestHienThi}</b>: {progress.soBoXuongDaDiet}/{soYeuCau}";
                }
                else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    string trangThaiHoanThanh = isEnglish ? "<color=green>[Completed] Turn in quest!</color>" : "<color=green>[Hoàn thành] Trả nhiệm vụ!</color>";
                    danhSachTextQuestUI[i].text = $"• <b>{tenQuestHienThi}</b>: {trangThaiHoanThanh}";
                }
            }
        }
    }

    private QuestData LayQuestDataDongBo(int idQuest)
    {
        if (cacheQuestData.TryGetValue(idQuest, out QuestData cached) && cached != null)
        {
            return cached;
        }

        if (QuestSaveSystem.Instance != null)
        {
            QuestData dataFromSave = QuestSaveSystem.Instance.LayQuestDataTheoID(idQuest);
            if (dataFromSave != null)
            {
                cacheQuestData[idQuest] = dataFromSave;
                return dataFromSave;
            }
        }

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