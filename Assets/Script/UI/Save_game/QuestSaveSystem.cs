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

    [Header("--- CẤU HÌNH DỮ LIỆU QUEST (KÉO SCRIPTABLE OBJECT VÀO ĐÂY) ---")]
    [Tooltip("Danh sách TẤT CẢ các ScriptableObject QuestData trong toàn bộ Game")]
    public List<QuestData> danhSachQuestData = new List<QuestData>();

    [Header("--- CẤU HÌNH SAVE ---")]
    [Tooltip("Tên file lưu tiến trình Quest.")]
    public string tenFileSave = "QuestProgressData.txt";

    private string duongDanTuyetDoi;
    public DanhSachSaveQuest duLieuSaveHienTai = new DanhSachSaveQuest();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        duongDanTuyetDoi = Path.Combine(
            Application.persistentDataPath,
            tenFileSave
        );

        LoadDuLieuQuestFromTxt();
    }

    public void SaveDuLieuQuestToTxt()
    {
        try
        {
            string chuoiJson = JsonUtility.ToJson(duLieuSaveHienTai, true);
            File.WriteAllText(duongDanTuyetDoi, chuoiJson);
            Debug.Log("<color=green>[Quest Save]</color> Đã lưu Quest: " + duongDanTuyetDoi);
        }
        catch (Exception e)
        {
            Debug.LogError("[Quest Save] Lỗi ghi file: " + e.Message);
        }
    }

    public void LoadDuLieuQuestFromTxt()
    {
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                string chuoiJson = File.ReadAllText(duongDanTuyetDoi);
                duLieuSaveHienTai = JsonUtility.FromJson<DanhSachSaveQuest>(chuoiJson);

                if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
                if (duLieuSaveHienTai.danhSachProgress == null) duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();

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
            TaoFileSaveMoi();
        }
    }

    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai = new DanhSachSaveQuest();
        SaveDuLieuQuestToTxt();
    }

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

    public ProgressQuest LayTienTrinhQuest(int idQuest)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.danhSachProgress == null) duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();

        foreach (ProgressQuest quest in duLieuSaveHienTai.danhSachProgress)
        {
            if (quest.idQuest == idQuest)
            {
                return quest;
            }
        }

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
        ProgressQuest quest = LayTienTrinhQuest(idQuest);
        quest.trangThai = trangThaiMoi;

        SaveDuLieuQuestToTxt();

        Debug.Log("<color=yellow>[Quest]</color> Quest ID " + idQuest + " → " + trangThaiMoi);
    }

    // 🎯 HÀM GHI NHẬN TIÊU DIỆT QUÁI (ĐÃ TỐI ƯU ĐỂ KHÔNG BỊ BỎ SÓT NHIỆM VỤ DÙ CHƠI Ở MAP NÀO)
    public void GhiNhanDietQuai(int idQuai, int soLuong = 1)
    {
        if (soLuong <= 0) return;

        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            // 1. Chỉ tính điểm khi Quest đang ở trạng thái 'Đang Làm' (TrangThai = 1)
            if (questProgress.trangThai != TrangThaiQuest.DangLam)
            {
                continue;
            }

            // 2. Tra cứu dữ liệu QuestData tương ứng
            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);

            // Nếu không tìm thấy QuestData trong danhSachQuestData -> Bỏ qua
            if (questData == null)
            {
                Debug.LogWarning($"[QuestSaveSystem] Không tìm thấy QuestData cho Quest ID: {questProgress.idQuest}. Hãy kiểm tra lại Inspector!");
                continue;
            }

            // 3. Kiểm tra xem loại quái bị diệt có đúng với yêu cầu của Quest không
            if (questData.idQuaiCanDiet != idQuai)
            {
                continue;
            }

            // 4. Cộng dồn số lượng quái tiêu diệt
            questProgress.soBoXuongDaDiet += soLuong;

            // 5. Kiểm tra nếu đủ số lượng yêu cầu -> Đổi sang trạng thái 'Đã Xong Chưa Trả' (TrangThai = 2)
            if (questProgress.soBoXuongDaDiet >= questData.soLuongBoXuongCanDiet)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongBoXuongCanDiet;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra;

                Debug.Log("<color=green>[Quest]</color> " + questData.tenNhiemVu + " đã hoàn thành mục tiêu diệt quái!");
            }

            coThayDoi = true;
        }

        // Lưu dữ liệu và làm mới UI nếu có tiến trình thay đổi
        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();

            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
            }

            // 🎯 TỰ ĐỘNG THÔNG BÁO CHO HUD TRACKER CẬP NHẬT THEO THỜI GIAN THỰC
            QuestHUDTracker.ThongBaoCapNhatHUD();
        }
    }
}