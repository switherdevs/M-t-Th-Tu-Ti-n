using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 🎯 CẤU TRÚC MỖI DÒNG QUEST TRÊN UI (GỒM TÊN, DATA VÀ NÚT BẤM MỞ)
[Serializable]
public class QuestUIElement
{
    [Header("--- THÔNG TIN DÒNG NHIỆM VỤ ---")]
    [Tooltip("ScriptableObject dữ liệu của Quest này")]
    public QuestData questData;

    [Tooltip("Text Mesh Pro hiển thị tên nhiệm vụ trên nút bấm này")]
    public TextMeshProUGUI textTenNhiemVu;

    [Tooltip("Nút bấm (Quest 1, Quest 2...) dùng để mở bảng thoại chi tiết")]
    public Button nutMoQuest;
}

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("--- DANH SÁCH / MẢNG CÁC PHẦN TỬ UI NHIỆM VỤ ---")]
    [Tooltip("Mảng chứa thông tin từng dòng Quest trên giao diện danh sách")]
    public List<QuestUIElement> danhSachQuestUI = new List<QuestUIElement>();

    [Header("--- THÀNH PHẦN UI GIAO TIẾP CHUNG CẦN KÉO VÀO ---")]
    public GameObject bangThoaiUI;                      // Panel chứa toàn bộ UI giao tiếp Quest
    public TextMeshProUGUI textLoiThoaiNPC;             // Text (TMP) hiển thị Lời thoại NPC chung

    [Header("--- CÁC NÚT BẤM XỬ LÝ TRONG BẢNG THOẠI (BUTTONS) ---")]
    public Button nutDongY;
    public Button nutTuChoi;
    public Button nutTraNhiemVu;
    public Button nutDongBang;

    private QuestData questDangXem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (bangThoaiUI != null) bangThoaiUI.SetActive(false);
    }

    private void Start()
    {
        // Tự động thiết lập tên và lắng nghe sự kiện Click cho từng Button trong mảng
        KhoiTaoDanhSachQuestUI();
    }

    // 🎯 HÀM KHỞI TẠO DỮ LIỆU VÀ ĐĂNG KÝ SỰ KIỆN NÚT BẤM MỞ QUEST
    public void KhoiTaoDanhSachQuestUI()
    {
        foreach (var element in danhSachQuestUI)
        {
            if (element != null && element.questData != null)
            {
                // 1. Tự động hiển thị tên Quest lên Text Pro của phần tử đó
                if (element.textTenNhiemVu != null)
                {
                    element.textTenNhiemVu.text = element.questData.tenNhiemVu;
                }

                // 2. Tự động gắn sự kiện Click vào Button Mở Quest tương ứng
                if (element.nutMoQuest != null)
                {
                    element.nutMoQuest.onClick.RemoveAllListeners(); // Xóa listener cũ tránh trùng lặp

                    // Tạo biến tạm lưu data để tránh lỗi tham chiếu closure trong vòng lặp
                    QuestData targetData = element.questData;

                    element.nutMoQuest.onClick.AddListener(() =>
                    {
                        MoBangThoaiQuest(targetData);
                    });
                }
            }
        }
    }

    // 🎯 HÀM TÌM DỮ LIỆU QUEST THEO ID (DÙNG CHO QUESTSAVESYSTEM)
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

    // 🎯 HÀM MỞ BẢNG UI THOẠI KHI BẤM NÚT MỞ QUEST
    public void MoBangThoaiQuest(QuestData questData)
    {
        if (questData == null)
        {
            Debug.LogWarning("<color=yellow>[QuestUI Warning]</color> Dữ liệu QuestData bị Null, không thể mở bảng thoại.");
            return;
        }

        questDangXem = questData;

        if (bangThoaiUI != null) bangThoaiUI.SetActive(true);

        // Lấy tiến trình từ hệ thống Save TXT
        ProgressQuest progress = QuestSaveSystem.Instance.LayTienTrinhQuest(questData.idQuest);

        // Ẩn an toàn tất cả nút bấm thao tác thoại
        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        // Xử lý lời thoại NPC và Nút bấm theo từng trạng thái
        switch (progress.trangThai)
        {
            case TrangThaiQuest.ChuaNhan:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiNhanQuest;
                if (nutDongY != null) nutDongY.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DangLam:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiDangLam;
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DaXongChuaTra:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Tốt lắm! Ngươi đã hoàn thành nhiệm vụ. Đây là phần thưởng!";
                if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.HoanThanh:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Cảm ơn đại hiệp đã giúp đỡ dân lành!";
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;
        }
    }

    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);
        DongBangThoai();
        Debug.Log("<color=yellow>[QuestUI]</color> Đã nhận nhiệm vụ ID: " + questDangXem.idQuest);
    }

    public void DongBangThoai()
    {
        if (bangThoaiUI != null) bangThoaiUI.SetActive(false);
    }

    public void OnClickTraNhiemVu()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.HoanThanh);

        if (questDangXem.prefabItemPhanThuong != null)
        {
            for (int i = 0; i < questDangXem.soLuongItemThuong; i++)
            {
                Instantiate(questDangXem.prefabItemPhanThuong, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
            }
            Debug.Log($"<color=green>[Reward]</color> Đã trao {questDangXem.soLuongItemThuong}x {questDangXem.prefabItemPhanThuong.name}");
        }

        DongBangThoai();
    }
}