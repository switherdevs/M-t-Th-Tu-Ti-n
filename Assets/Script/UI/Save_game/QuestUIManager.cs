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

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("--- DANH SÁCH / MẢNG CÁC PHẦN TỬ UI NHIỆM VỤ ---")]
    public List<QuestUIElement> danhSachQuestUI = new List<QuestUIElement>();

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

    public void MoBangThoaiQuest(QuestData questData)
    {
        if (questData == null) return;

        questDangXem = questData;

        if (textLoiThoaiNPC != null) textLoiThoaiNPC.gameObject.SetActive(true);
        if (textTienTrinhQuest != null) textTienTrinhQuest.gameObject.SetActive(true);

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

    // 🎯 HÀM ĐỒNG Ý NHẬN QUEST
    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        // 1. Cập nhật Save
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);

        // 2. Refresh UI bảng thoại
        KhoiTaoDanhSachQuestUI();
        MoBangThoaiQuest(questDangXem);

        // 3. 🎯 PHÁT TÍN HIỆU CẬP NHẬT HUD THỜI GIAN THỰC
        QuestHUDTracker.ThongBaoCapNhatHUD();

        Debug.Log("<color=yellow>[QuestUI]</color> Đã nhận nhiệm vụ ID: " + questDangXem.idQuest);
    }

    // 🎯 HÀM HỦY NHIỆM VỤ / TỪ CHỐI
    public void OnClickHuyHoacTuChoiQuest()
    {
        if (questDangXem == null) return;

        // 1. Cập nhật Save về ChưaNhan
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.ChuaNhan);

        // 2. Reset bảng thoại
        KhoiTaoDanhSachQuestUI();
        DongBangThoai();

        // 3. 🎯 PHÁT TÍN HIỆU CẬP NHẬT HUD THỜI GIAN THỰC (XÓA DÒNG QUEST HỦY KHỎI HUD NGAY)
        QuestHUDTracker.ThongBaoCapNhatHUD();

        Debug.Log("<color=red>[QuestUI]</color> Đã hủy/từ chối nhiệm vụ ID: " + questDangXem.idQuest);
    }

    // 🎯 HÀM TRẢ NHIỆM VỤ
    public void OnClickTraNhiemVu()
    {
        if (questDangXem == null) return;

        // 1. Cập nhật Save
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.HoanThanh);

        // 2. Trao thưởng
        if (questDangXem.prefabItemPhanThuong != null)
        {
            for (int i = 0; i < questDangXem.soLuongItemThuong; i++)
            {
                Instantiate(questDangXem.prefabItemPhanThuong, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
            }
            Debug.Log($"<color=green>[Reward]</color> Đã trao {questDangXem.soLuongItemThuong}x {questDangXem.prefabItemPhanThuong.name}");
        }

        // 3. Refresh UI thoại
        KhoiTaoDanhSachQuestUI();
        DongBangThoai();

        // 4. 🎯 PHÁT TÍN HIỆU CẬP NHẬT HUD THỜI GIAN THỰC
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