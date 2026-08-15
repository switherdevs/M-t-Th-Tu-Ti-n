using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// Enum quản lý 4 trạng thái của một nhiệm vụ
public enum TrangThaiQuest
{
    ChuaNhan = 0,
    DangLam = 1,
    DaXongChuaTra = 2,
    HoanThanh = 3
}

// Lớp lưu tiến trình cá nhân của từng nhiệm vụ
[Serializable]
public class ProgressQuest
{
    public int idQuest;
    public TrangThaiQuest trangThai;
    public int soBoXuongDaDiet; // Số lượng quái mục tiêu đã diệt được
}

// Lớp bao gói danh sách tiến trình để JsonUtility có thể Serialize thành file TXT/JSON
[Serializable]
public class DanhSachSaveQuest
{
    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
}

public class QuestSaveSystem : MonoBehaviour
{
    // Biến static lưu Instance dùng cho Design Pattern Singleton trong Scene
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HÌNH DỮ LIỆU QUEST (KÉO SCRIPTABLE OBJECT VÀO ĐÂY) ---")]
    [Tooltip("Danh sách các ScriptableObject QuestData trong game.")]
    public List<QuestData> danhSachQuestData = new List<QuestData>();

    [Header("--- CẤU HÌNH SAVE ---")]
    [Tooltip("Tên file lưu tiến trình Quest.")]
    public string tenFileSave = "QuestProgressData.txt";

    private string duongDanTuyetDoi;

    // Object chứa dữ liệu save đang hoạt động trên RAM
    public DanhSachSaveQuest duLieuSaveHienTai = new DanhSachSaveQuest();

    private void Awake()
    {
        // 🎯 KIỂM TRA Singleton THEO TỪNG SCENE (KHÔNG DÙNG DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            // Nếu trong cùng 1 Scene lỡ có 2 QuestSaveSystem thì hủy bớt 1 cái
            Destroy(gameObject);
            return;
        }

        // Tạo đường dẫn tuyệt đối đến file Save trong ổ cứng
        duongDanTuyetDoi = Path.Combine(
            Application.persistentDataPath,
            tenFileSave
        );

        // Nạp dữ liệu Save ngay khi vào Scene mới
        LoadDuLieuQuestFromTxt();
    }

    // =========================================================
    // 1. LƯU DỮ LIỆU XUỐNG FILE TXT / JSON
    // =========================================================
    public void SaveDuLieuQuestToTxt()
    {
        try
        {
            // Chuyển C# Object thành chuỗi định dạng JSON
            string chuoiJson = JsonUtility.ToJson(duLieuSaveHienTai, true);

            // Ghi file xuống đĩa cứng
            File.WriteAllText(duongDanTuyetDoi, chuoiJson);

            Debug.Log("<color=green>[Quest Save]</color> Đã lưu Quest: " + duongDanTuyetDoi);
        }
        catch (Exception e)
        {
            Debug.LogError("[Quest Save] Lỗi ghi file: " + e.Message);
        }
    }

    // =========================================================
    // 2. NẠP DỮ LIỆU TỪ Ổ CỨNG LÊN DỰ ÁN
    // =========================================================
    public void LoadDuLieuQuestFromTxt()
    {
        // Kiểm tra xem file TXT đã tồn tại hay chưa
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                // Đọc chuỗi văn bản từ file TXT
                string chuoiJson = File.ReadAllText(duongDanTuyetDoi);

                // Chuyển lại thành C# Object
                duLieuSaveHienTai = JsonUtility.FromJson<DanhSachSaveQuest>(chuoiJson);

                // Bảo vệ chống Null Reference
                if (duLieuSaveHienTai == null)
                {
                    duLieuSaveHienTai = new DanhSachSaveQuest();
                }

                if (duLieuSaveHienTai.danhSachProgress == null)
                {
                    duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();
                }

                Debug.Log("<color=cyan>[Quest Load]</color> Đã load dữ liệu Quest.");
            }
            catch (Exception e)
            {
                Debug.LogError("[Quest Load] Lỗi đọc file: " + e.Message);
                TaoFileSaveMoi();
            }
        }
        else
        {
            // Nếu lần đầu chơi (chưa có file) thì tạo file save mới
            TaoFileSaveMoi();
        }
    }

    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai = new DanhSachSaveQuest();
        SaveDuLieuQuestToTxt();
    }

    // =========================================================
    // 3. TÌM DỮ LIỆU QUESTDATA TRONG INSPECTOR TỰ ĐỘNG
    // =========================================================
    public QuestData LayQuestDataTheoID(int idQuest)
    {
        foreach (QuestData q in danhSachQuestData)
        {
            if (q != null && q.idQuest == idQuest)
            {
                return q;
            }
        }
        return null;
    }

    // =========================================================
    // 4. LẤY HOẶC TẠO MỚI TIẾN TRÌNH QUEST CỦA PLAYER
    // =========================================================
    public ProgressQuest LayTienTrinhQuest(int idQuest)
    {
        if (duLieuSaveHienTai == null)
        {
            duLieuSaveHienTai = new DanhSachSaveQuest();
        }

        if (duLieuSaveHienTai.danhSachProgress == null)
        {
            duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();
        }

        // Tìm tiến trình nhiệm vụ đã lưu
        foreach (ProgressQuest quest in duLieuSaveHienTai.danhSachProgress)
        {
            if (quest.idQuest == idQuest)
            {
                return quest;
            }
        }

        // Nếu Quest này chưa từng lưu trong file -> Tạo bản ghi mới ở trạng thái Chưa Nhận
        ProgressQuest questMoi = new ProgressQuest
        {
            idQuest = idQuest,
            trangThai = TrangThaiQuest.ChuaNhan,
            soBoXuongDaDiet = 0
        };

        duLieuSaveHienTai.danhSachProgress.Add(questMoi);
        SaveDuLieuQuestToTxt();

        return questMoi;
    }

    // =========================================================
    // 5. CẬP NHẬT TRẠNG THÁI QUEST (NHẬN / HỦY / TRẢ QUEST)
    // =========================================================
    public void CapNhatTrangThaiQuest(int idQuest, TrangThaiQuest trangThaiMoi)
    {
        ProgressQuest quest = LayTienTrinhQuest(idQuest);
        quest.trangThai = trangThaiMoi;

        SaveDuLieuQuestToTxt();

        Debug.Log("<color=yellow>[Quest]</color> Quest ID " + idQuest + " → " + trangThaiMoi);
    }

    // =========================================================
    // 6. GHI NHẬN QUÁI BỊ TIÊU DIỆT TỪ GAMEPLAY
    // =========================================================
    public void GhiNhanDietQuai(int idQuai, int soLuong = 1)
    {
        if (soLuong <= 0) return;

        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            // Chỉ cập nhật nếu nhiệm vụ đó đang ở trạng thái 'Đang Làm'
            if (questProgress.trangThai != TrangThaiQuest.DangLam)
            {
                continue;
            }

            // Lấy thông tin thiết lập của Quest này
            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);

            if (questData == null)
            {
                continue;
            }

            // Đúng loại quái cần giết mới tính điểm
            if (questData.idQuaiCanDiet != idQuai)
            {
                continue;
            }

            // Cộng dồn số quái đã tiêu diệt
            questProgress.soBoXuongDaDiet += soLuong;

            // Kiểm tra xem đã đạt số lượng tối đa yêu cầu chưa
            if (questProgress.soBoXuongDaDiet >= questData.soLuongBoXuongCanDiet)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongBoXuongCanDiet;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra; // Đổi sang Chờ Trả Thưởng

                Debug.Log("<color=green>[Quest]</color> " + questData.tenNhiemVu + " đã đạt đủ điều kiện trả nhiệm vụ.");
            }

            coThayDoi = true;
        }

        // Lưu lại dữ liệu ngay nếu có thay đổi tiến trình
        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();

            // Cập nhật lại UI trong Scene nếu đang mở
            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
            }
        }
    }
}