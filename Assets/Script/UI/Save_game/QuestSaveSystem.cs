using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public enum TrangThaiQuest
{
    ChuaNhan = 0,
    DangLam = 1,
    DaXongChuaTra = 2,
    HoanThanh = 3
}

[Serializable]
public class ProgressQuest
{
    public int idQuest;
    public TrangThaiQuest trangThai;
    public int soBoXuongDaDiet;
}

[Serializable]
public class DanhSachSaveQuest
{
    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
}

public class QuestSaveSystem : MonoBehaviour
{
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HÌNH ĐƯỜNG DẪN SAVE ---")]
    [Tooltip("Tên file txt sẽ lưu trực tiếp ở ổ đĩa/thư mục game")]
    public string tenFileSave = "QuestProgressData.txt";
    private string duongDanTuyetDoi;

    public DanhSachSaveQuest duLieuSaveHienTai = new DanhSachSaveQuest();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tạo đường dẫn tuyệt đối đến thư mục lưu trữ ứng dụng
        duongDanTuyetDoi = Path.Combine(Application.persistentDataPath, tenFileSave);

        // Vừa vào Scene là Tự động Load
        LoadDuLieuQuestFromTxt();
    }

    // 🎯 HÀM LƯU DỮ LIỆU RA FILE TXT (ĐƯỜNG DẪN TUYỆT ĐỐI)
    public void SaveDuLieuQuestToTxt()
    {
        try
        {
            string chuoiJson = JsonUtility.ToJson(duLieuSaveHienTai, true);
            File.WriteAllText(duongDanTuyetDoi, chuoiJson);
            Debug.Log("<color=green>[SaveQuest]</color> Đã lưu file TXT thành công tại: " + duongDanTuyetDoi);
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveQuest] Lỗi khi ghi file TXT: " + e.Message);
        }
    }

    // 🎯 HÀM LOAD DỮ LIỆU TỪ FILE TXT (TỰ TẠO MỚI NẾU KHÔNG THẤY)
    public void LoadDuLieuQuestFromTxt()
    {
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                string chuoiJson = File.ReadAllText(duongDanTuyetDoi);
                duLieuSaveHienTai = JsonUtility.FromJson<DanhSachSaveQuest>(chuoiJson);
                Debug.Log("<color=cyan>[LoadQuest]</color> Đã đọc dữ liệu từ File TXT!");
            }
            catch (Exception e)
            {
                Debug.LogError("[LoadQuest] Lỗi đọc file TXT, đang tạo dữ liệu mới. Chi tiết: " + e.Message);
                TaoFileSaveMoi();
            }
        }
        else
        {
            Debug.LogWarning("[LoadQuest] Không tìm thấy file TXT! Đang khởi tạo file Save mới...");
            TaoFileSaveMoi();
        }
    }

    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai = new DanhSachSaveQuest();
        SaveDuLieuQuestToTxt(); // Tự động ghi file mới
    }

    // --- HÀM Bổ TRỢ LẤY / CẬP NHẬT TRẠNG THÁI QUEST TRONG MẢNG ---
    public ProgressQuest LayTiencTrinhQuest(int idQuest)
    {
        foreach (var q in duLieuSaveHienTai.danhSachProgress)
        {
            if (q.idQuest == idQuest) return q;
        }

        // Nếu chưa có trong mảng Save -> Tạo mốc mới mặc định
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

    public void CapNhatTrangThaiQuest(int idQuest, TrangThaiQuest trangThaiMoi)
    {
        ProgressQuest q = LayTiencTrinhQuest(idQuest);
        q.trangThai = trangThaiMoi;
        SaveDuLieuQuestToTxt();
    }
}