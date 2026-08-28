using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestHUDTracker : MonoBehaviour
{
    public static QuestHUDTracker Instance;

    [Header("--- THÔNG BÁO MẶC ĐỊNH (KHI CHƯA CÓ QUEST DANG LÀM) ---")]
    [Tooltip("Text TMP hiển thị thông báo hướng dẫn khi không có Quest nào đang nhận")]
    public TextMeshProUGUI textThongBaoMacDinh;

    [Tooltip("Nội dung hiển thị khi chưa nhận Quest")]
    public string noiDungThongBaoMacDinh = "Hãy đến kinh thành nhận nhiệm vụ";

    [Tooltip("Nội dung hiển thị khi đã hoàn thành tất cả nhiệm vụ")]
    public string noiDungHoanThanhTatCa = "Cảnh giới bạn đã cao, hãy đập đá";

    [Header("--- MẢNG TEXT HIỂN THỊ QUEST ĐỘNG ---")]
    [Tooltip("Kéo các Text TMP dùng để hiển thị danh sách nhiệm vụ trên HUD vào đây.")]
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
        OnQuestProgressChanged += CapNhatGiaoDienHUD;
    }

    private void OnDisable()
    {
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
    }

    private IEnumerator Start()
    {
        yield return null;
        CapNhatGiaoDienHUD();
    }

    public static void ThongBaoCapNhatHUD()
    {
        OnQuestProgressChanged?.Invoke();
    }

    public void CapNhatGiaoDienHUD()
    {
        XoaRongToanBoText();

        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            return;
        }

        List<ProgressQuest> danhSachQuestDangActive = LayToanBoQuestDangKichHoat();

        // 🎯 KIỂM TRA: Nếu không có quest nào đang Active (Đang làm / Chờ trả)
        if (danhSachQuestDangActive.Count == 0)
        {
            if (textThongBaoMacDinh != null)
            {
                textThongBaoMacDinh.gameObject.SetActive(true);

                // Kiểm tra xem có phải đã làm xong TẤT CẢ các quest hay chưa
                if (KiemTraDaHoanThanhTatCaQuest())
                {
                    textThongBaoMacDinh.text = noiDungHoanThanhTatCa;
                }
                else
                {
                    textThongBaoMacDinh.text = noiDungThongBaoMacDinh;
                }
            }
            return;
        }

        // Nếu có quest đang làm thì ẩn text mặc định đi
        if (textThongBaoMacDinh != null)
        {
            textThongBaoMacDinh.gameObject.SetActive(false);
        }

        if (danhSachTextQuestUI == null || danhSachTextQuestUI.Length == 0) return;

        for (int i = 0; i < danhSachQuestDangActive.Count; i++)
        {
            if (i >= danhSachTextQuestUI.Length) break;
            if (danhSachTextQuestUI[i] == null) continue;

            ProgressQuest progress = danhSachQuestDangActive[i];
            QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(progress.idQuest);

            if (data != null)
            {
                danhSachTextQuestUI[i].gameObject.SetActive(true);

                if (progress.trangThai == TrangThaiQuest.DangLam)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: {progress.soBoXuongDaDiet}/{data.soLuongBoXuongCanDiet}";
                }
                else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: <color=green>[Hoàn thành] Trả nhiệm vụ!</color>";
                }
            }
        }
    }

    /// <summary>
    /// 🎯 Hàm phụ trợ: Kiểm tra xem toàn bộ Quest trong Save Data đã Hoàn Thành hay chưa
    /// </summary>
    private bool KiemTraDaHoanThanhTatCaQuest()
    {
        List<ProgressQuest> danhSachProgress = QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress;

        // Nếu trong Save File chưa có quest nào hoặc danh sách Data bị rỗng
        if (danhSachProgress == null || danhSachProgress.Count == 0)
        {
            return false;
        }

        // Đếm xem trong QuestSaveSystem có đúng bao nhiêu QuestData gốc
        int tongSoQuestTrongGame = QuestSaveSystem.Instance.danhSachQuestData.Count;

        // Nếu số quest trong file save chưa đủ bằng số quest tạo sẵn trong game -> Chưa xong hết
        if (danhSachProgress.Count < tongSoQuestTrongGame)
        {
            return false;
        }

        // Duyệt từng quest xem có quest nào CHƯA hoàn thành không
        foreach (ProgressQuest progress in danhSachProgress)
        {
            if (progress.trangThai != TrangThaiQuest.HoanThanh)
            {
                return false; // Còn ít nhất 1 quest chưa xong
            }
        }

        return true; // Tất cả quest đều đã HoanThanh
    }

    private List<ProgressQuest> LayToanBoQuestDangKichHoat()
    {
        List<ProgressQuest> ketQua = new List<ProgressQuest>();

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