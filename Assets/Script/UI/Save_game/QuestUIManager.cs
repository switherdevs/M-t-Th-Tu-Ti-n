using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class QuestUIElement
{
    [Header("--- THÔNG TIN DÒNG NHIỆM VỤ ---")]
    public QuestData questData;
    public TextMeshProUGUI textTenNhiemVu;
    public Button nutMoQuest;
}

// Cấu trúc đại diện cho 1 ô hiển thị Item thưởng trong Bảng Thông Tin Chung
[Serializable]
public class RewardSlotUI
{
    [Tooltip("GameObject chứa toàn bộ ô Item thưởng này (Dùng để Bật/Tắt)")]
    public GameObject slotGameObject;

    [Tooltip("Component Image hiển thị Icon của Item")]
    public Image imageIcon;

    [Tooltip("Text TMP hiển thị Số Lượng Item (VD: x5, x10)")]
    public TextMeshProUGUI textSoLuong;
}

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("--- DANH SÁCH DÒNG NHIỆM VỤ ---")]
    public List<QuestUIElement> danhSachQuestUI = new List<QuestUIElement>();

    [Header("--- KHU VỰC HIỂN THỊ PHẦN THƯỞNG DÙNG CHUNG (NGOÀI MẢNG) ---")]
    [Tooltip("Mảng/Danh sách các Ô UI hiển thị phần thưởng dùng chung cho bảng thông tin")]
    public List<RewardSlotUI> danhSachSlotThuongUI = new List<RewardSlotUI>();

    [Header("--- THÀNH PHẦN UI GIAO TIẾP CHUNG CẦN KÉO VÀO ---")]
    public TextMeshProUGUI textLoiThoaiNPC;
    public TextMeshProUGUI textTienTrinhQuest;

    [Header("--- CÁC NÚT BẤM XỬ LÝ TRONG BẢNG THOẠI (BUTTONS) ---")]
    public Button nutDongY;
    public Button nutTuChoi;
    public Button nutTraNhiemVu;
    public Button nutDongBang;

    [Header("--- TRẠNG THÁI HOÀN THÀNH TẤT CẢ QUEST ---")]
    public bool Complete = false;

    private QuestData questDangXem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DongBangThoai();
    }

    private void Start()
    {
        KhoiTaoDanhSachQuestUI();
    }

    public void KhoiTaoDanhSachQuestUI()
    {
        foreach (var element in danhSachQuestUI)
        {
            if (element != null && element.questData != null)
            {
                ProgressQuest progress = QuestSaveSystem.Instance != null
                    ? QuestSaveSystem.Instance.LayTienTrinhQuest(element.questData.idQuest)
                    : null;

                int soQuaiDaGiet = progress != null ? progress.soBoXuongDaDiet : 0;
                int soQuaiYeuCau = element.questData.soLuongBoXuongCanDiet;

                string chuoiTrangThai = LayChuoiTrangThai(progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan);

                if (element.textTenNhiemVu != null)
                {
                    element.textTenNhiemVu.text = $"{element.questData.tenNhiemVu} [{chuoiTrangThai}] ({soQuaiDaGiet}/{soQuaiYeuCau})";
                }

                if (element.nutMoQuest != null)
                {
                    element.nutMoQuest.onClick.RemoveAllListeners();

                    QuestData targetData = element.questData;

                    element.nutMoQuest.onClick.AddListener(() =>
                    {
                        MoBangThoaiQuest(targetData);
                    });
                }
            }
        }

        KiemTraToanBoQuestHoanThanh();
    }

    public void KiemTraToanBoQuestHoanThanh()
    {
        if (danhSachQuestUI == null || danhSachQuestUI.Count == 0)
        {
            Complete = false;
            return;
        }

        bool tatCaDaXong = true;

        foreach (var element in danhSachQuestUI)
        {
            if (element != null && element.questData != null)
            {
                ProgressQuest progress = QuestSaveSystem.Instance != null
                    ? QuestSaveSystem.Instance.LayTienTrinhQuest(element.questData.idQuest)
                    : null;

                if (progress == null || progress.trangThai != TrangThaiQuest.HoanThanh)
                {
                    tatCaDaXong = false;
                    break;
                }
            }
        }

        Complete = tatCaDaXong;
    }

    public QuestData LayQuestDataTheoID(int idQuest)
    {
        foreach (var element in danhSachQuestUI)
        {
            if (element != null && element.questData != null && element.questData.idQuest == idQuest)
            {
                return element.questData;
            }
        }
        return null;
    }

    /// <summary>
    /// Mở Bảng Thoại & Cập nhật mảng Phần Thưởng Dùng Chung theo Quest được click
    /// </summary>
    public void MoBangThoaiQuest(QuestData questData)
    {
        if (questData == null) return;

        questDangXem = questData;

        if (textLoiThoaiNPC != null) textLoiThoaiNPC.gameObject.SetActive(true);
        if (textTienTrinhQuest != null) textTienTrinhQuest.gameObject.SetActive(true);

        // 1. CẬP NHẬT HIỂN THỊ MẢNG PHẦN THƯỞNG DÙNG CHUNG
        CapNhatGiaoDienPhanThuongDungChung(questData);

        // 2. TÍNH TOÁN TRẠNG THÁI VÀ BẬT/TẮT CÁC NÚT BẤM
        ProgressQuest progress = QuestSaveSystem.Instance != null
            ? QuestSaveSystem.Instance.LayTienTrinhQuest(questData.idQuest)
            : null;

        TrangThaiQuest trangThaiHienTai = progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan;

        int soQuaiDaGiet = progress != null ? progress.soBoXuongDaDiet : 0;
        int soQuaiYeuCau = questData.soLuongBoXuongCanDiet;
        string chuoiTrangThai = LayChuoiTrangThai(trangThaiHienTai);

        if (textTienTrinhQuest != null)
        {
            textTienTrinhQuest.text = $"Tiến trình: {soQuaiDaGiet}/{soQuaiYeuCau} Quái | Trạng thái: {chuoiTrangThai}";
        }

        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        switch (trangThaiHienTai)
        {
            case TrangThaiQuest.ChuaNhan:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiNhanQuest;
                if (nutDongY != null) nutDongY.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DangLam:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiDangLam;
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DaXongChuaTra:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Tốt lắm! Ngươi đã hoàn thành nhiệm vụ. Đây là phần thưởng!";
                if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.HoanThanh:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Cảm ơn đại hiệp đã giúp đỡ dân lành!";
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Hàm phụ trách Bật/Tắt và gán dữ liệu vào mảng Ô Thưởng Dùng Chung
    /// </summary>
    private void CapNhatGiaoDienPhanThuongDungChung(QuestData questData)
    {
        if (danhSachSlotThuongUI == null || danhSachSlotThuongUI.Count == 0) return;

        // Ẩn tất cả các ô thưởng cũ
        foreach (var slot in danhSachSlotThuongUI)
        {
            if (slot != null && slot.slotGameObject != null)
            {
                slot.slotGameObject.SetActive(false);
            }
        }

        // Duyệt danh sách phần thưởng của Quest được chọn và hiển thị lên các Slot UI tương ứng
        if (questData.danhSachPhanThuong != null)
        {
            for (int i = 0; i < questData.danhSachPhanThuong.Count; i++)
            {
                // Tránh vượt quá số lượng ô UI được chuẩn bị sẵn trên Editor
                if (i >= danhSachSlotThuongUI.Count) break;

                ItemRewardData reward = questData.danhSachPhanThuong[i];
                RewardSlotUI slotUI = danhSachSlotThuongUI[i];

                if (reward != null && slotUI != null)
                {
                    // Bật ô UI lên
                    if (slotUI.slotGameObject != null) slotUI.slotGameObject.SetActive(true);

                    // Gán Sprite Icon
                    if (slotUI.imageIcon != null && reward.iconItem != null)
                    {
                        slotUI.imageIcon.sprite = reward.iconItem;
                    }

                    // Gán Text số lượng
                    if (slotUI.textSoLuong != null)
                    {
                        slotUI.textSoLuong.text = $"x{reward.soLuong}";
                    }
                }
            }
        }
    }

    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);

        KhoiTaoDanhSachQuestUI();
        MoBangThoaiQuest(questDangXem);

        QuestHUDTracker.ThongBaoCapNhatHUD();
    }

    public void OnClickHuyHoacTuChoiQuest()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.ChuaNhan);

        KhoiTaoDanhSachQuestUI();
        DongBangThoai();

        QuestHUDTracker.ThongBaoCapNhatHUD();
    }

    public void OnClickTraNhiemVu()
    {
        if (questDangXem == null) return;

        // 1. Cập nhật trạng thái Quest sang Hoàn Thành
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.HoanThanh);

        // 2. Tự động lưu toàn bộ mảng phần thưởng vào Save Game
        questDangXem.LuuPhanThuongVaoSaveGame();

        // 3. Refresh UI thoại
        KhoiTaoDanhSachQuestUI();
        DongBangThoai();

        QuestHUDTracker.ThongBaoCapNhatHUD();
    }

    public void DongBangThoai()
    {
        if (textLoiThoaiNPC != null) textLoiThoaiNPC.gameObject.SetActive(false);
        if (textTienTrinhQuest != null) textTienTrinhQuest.gameObject.SetActive(false);

        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        // Ẩn mảng ô thưởng dùng chung khi đóng bảng thoại
        if (danhSachSlotThuongUI != null)
        {
            foreach (var slot in danhSachSlotThuongUI)
            {
                if (slot != null && slot.slotGameObject != null)
                {
                    slot.slotGameObject.SetActive(false);
                }
            }
        }
    }

    private string LayChuoiTrangThai(TrangThaiQuest trangThai)
    {
        switch (trangThai)
        {
            case TrangThaiQuest.ChuaNhan:
                return "Chưa nhận";
            case TrangThaiQuest.DangLam:
                return "Đang làm";
            case TrangThaiQuest.DaXongChuaTra:
                return "Chờ trả thưởng";
            case TrangThaiQuest.HoanThanh:
                return "Hoàn thành";
            default:
                return "Chưa nhận";
        }
    }
}