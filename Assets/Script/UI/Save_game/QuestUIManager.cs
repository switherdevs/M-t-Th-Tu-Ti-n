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
    [Tooltip("Text (TMP) hiển thị Lời thoại NPC - Mặc định sẽ ẩn, chỉ hiện khi chọn Quest")]
    public TextMeshProUGUI textLoiThoaiNPC;

    [Tooltip("Text (TMP) hiển thị Tiến trình của Quest đang chọn (Nằm ngoài mảng danhSachQuestUI)")]
    public TextMeshProUGUI textTienTrinhQuest;

    [Header("--- CÁC NÚT BẤM XỬ LÝ TRONG BẢNG THOẠI (BUTTONS) ---")]
    public Button nutDongY;
    public Button nutTuChoi;        // Dùng làm Nút Hủy Nhiệm Vụ luôn
    public Button nutTraNhiemVu;
    public Button nutDongBang;

    private QuestData questDangXem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Mặc định ẩn lời thoại, tiến trình và các nút bấm khi vừa vào game
        DongBangThoai();
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
                // Lấy tiến trình hiện tại từ QuestSaveSystem
                ProgressQuest progress = QuestSaveSystem.Instance != null
                    ? QuestSaveSystem.Instance.LayTienTrinhQuest(element.questData.idQuest)
                    : null;

                // 🎯 LẤY TRỰC TIẾP GIÁ TRỊ TỪ SAVE VÀ SCRIPTABLEOBJECT (KHÔNG DÙNG REFLECTION)
                int soQuaiDaGiet = progress != null ? progress.soBoXuongDaDiet : 0;
                int soQuaiYeuCau = element.questData.soLuongBoXuongCanDiet;

                string chuoiTrangThai = LayChuoiTrangThai(progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan);

                // 1. Tự động hiển thị tên Quest + Trạng thái + Tiến trình lên Text Pro của phần tử đó
                if (element.textTenNhiemVu != null)
                {
                    element.textTenNhiemVu.text = $"{element.questData.tenNhiemVu} [{chuoiTrangThai}] ({soQuaiDaGiet}/{soQuaiYeuCau})";
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

        // Bật hiển thị Text Lời thoại NPC và Text Tiến trình Quest
        if (textLoiThoaiNPC != null) textLoiThoaiNPC.gameObject.SetActive(true);
        if (textTienTrinhQuest != null) textTienTrinhQuest.gameObject.SetActive(true);

        // Lấy tiến trình từ hệ thống Save
        ProgressQuest progress = QuestSaveSystem.Instance != null
            ? QuestSaveSystem.Instance.LayTienTrinhQuest(questData.idQuest)
            : null;

        TrangThaiQuest trangThaiHienTai = progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan;

        // 🎯 LẤY TRỰC TIẾP TỪ PROGRESS VÀ QUESTDATA
        int soQuaiDaGiet = progress != null ? progress.soBoXuongDaDiet : 0;
        int soQuaiYeuCau = questData.soLuongBoXuongCanDiet;
        string chuoiTrangThai = LayChuoiTrangThai(trangThaiHienTai);

        // Hiển thị tiến trình dạng (Số quái đã giết hiện tại / Số quái cần bị tiêu diệt)
        if (textTienTrinhQuest != null)
        {
            textTienTrinhQuest.text = $"Tiến trình: {soQuaiDaGiet}/{soQuaiYeuCau} Quái | Trạng thái: {chuoiTrangThai}";
        }

        // Ẩn an toàn tất cả nút bấm thao tác thoại trước khi bật nút phù hợp
        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        // Xử lý lời thoại NPC và Nút bấm theo từng trạng thái
        switch (trangThaiHienTai)
        {
            case TrangThaiQuest.ChuaNhan:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiNhanQuest;
                if (nutDongY != null) nutDongY.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true); // Nút Hủy / Từ chối
                break;

            case TrangThaiQuest.DangLam:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiDangLam;
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                // 🎯 KHI ĐANG LÀM: NÚT HỦY NHIỆM VỤ VẪN HIỆN ĐỂ NGƯỜI CHƠI BẤM HỦY QUEST VỀ LẠI 'CHƯA NHẬN'
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DaXongChuaTra:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Tốt lắm! Ngươi đã hoàn thành nhiệm vụ. Đây là phần thưởng!";
                if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true); // Vẫn có thể hủy nếu muốn
                break;

            case TrangThaiQuest.HoanThanh:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Cảm ơn đại hiệp đã giúp đỡ dân lành!";
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;
        }
    }

    // 🎯 HÀM BẤM ACCEPT (ĐỒNG Ý NHẬN QUEST)
    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        // Đổi trạng thái thành Đang Làm
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);

        // Cập nhật lại UI hiển thị ngay lập tức
        KhoiTaoDanhSachQuestUI();
        MoBangThoaiQuest(questDangXem);

        Debug.Log("<color=yellow>[QuestUI]</color> Đã nhận nhiệm vụ ID: " + questDangXem.idQuest);
    }

    // 🎯 HÀM BẤM HỦY NHIỆM VỤ / TỪ CHỐI -> QUAY VỀ TRẠNG THÁI 'CHƯA NHẬN'
    public void OnClickHuyHoacTuChoiQuest()
    {
        if (questDangXem == null) return;

        // Đưa trạng thái nhiệm vụ quay về Chưa Nhận
        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.ChuaNhan);

        // Tắt thoại / Reset UI
        KhoiTaoDanhSachQuestUI();
        DongBangThoai();
        Debug.Log("<color=red>[QuestUI]</color> Đã hủy/từ chối nhiệm vụ ID: " + questDangXem.idQuest + ". Đã quay về trạng thái Chưa Nhận!");
    }

    // 🎯 HÀM TẮT TOÀN BỘ BẢNG THOẠI / TIẾN TRÌNH / NÚT BẤM
    public void DongBangThoai()
    {
        // Ẩn Text Lời Thoại & Text Tiến Trình
        if (textLoiThoaiNPC != null) textLoiThoaiNPC.gameObject.SetActive(false);
        if (textTienTrinhQuest != null) textTienTrinhQuest.gameObject.SetActive(false);

        // Ẩn toàn bộ các nút thao tác
        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);
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

        KhoiTaoDanhSachQuestUI();
        DongBangThoai();
    }

    // 🛠️ HÀM PHỤ TRỢ: CHUYỂN ENUM TRẠNG THÁI THÀNH CHUỖI HIỂN THỊ DỄ ĐỌC
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