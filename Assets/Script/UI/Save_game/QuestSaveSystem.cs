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
    public int soBoXuongDaDiet; // Hoặc soQuaiDaGiet
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
    [Tooltip("Danh sách các ScriptableObject QuestData trong game.")]
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
            DontDestroyOnLoad(gameObject); // Giữ QuestSaveSystem không bị xóa khi chuyển Scene
        }
        else
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

    // =========================================================
    // SAVE
    // =========================================================
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

    // =========================================================
    // LOAD
    // =========================================================
    public void LoadDuLieuQuestFromTxt()
    {
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                string chuoiJson = File.ReadAllText(duongDanTuyetDoi);
                duLieuSaveHienTai = JsonUtility.FromJson<DanhSachSaveQuest>(chuoiJson);

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
            TaoFileSaveMoi();
        }
    }

    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai = new DanhSachSaveQuest();
        SaveDuLieuQuestToTxt();
    }

    // =========================================================
    // HÀM BỔ SUNG: TÌM QUEST DATA TRONG DANH SÁCH KÉO THẢ INSPECTOR
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
    // LẤY TIẾN TRÌNH QUEST
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

        foreach (ProgressQuest quest in duLieuSaveHienTai.danhSachProgress)
        {
            if (quest.idQuest == idQuest)
            {
                return quest;
            }
        }

        // Nếu Quest chưa từng xuất hiện trong Save
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
    // CẬP NHẬT TRẠNG THÁI QUEST
    // =========================================================
    public void CapNhatTrangThaiQuest(int idQuest, TrangThaiQuest trangThaiMoi)
    {
        ProgressQuest quest = LayTienTrinhQuest(idQuest);
        quest.trangThai = trangThaiMoi;

        SaveDuLieuQuestToTxt();

        Debug.Log("<color=yellow>[Quest]</color> Quest ID " + idQuest + " → " + trangThaiMoi);
    }

    // =========================================================
    // GHI NHẬN QUÁI BỊ TIÊU DIỆT (LẤY DỮ LIỆU TRỰC TIẾP TỪ INSPECTOR)
    // =========================================================
    public void GhiNhanDietQuai(int idQuai, int soLuong = 1)
    {
        if (soLuong <= 0) return;

        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            // Chỉ xử lý Quest đang làm
            if (questProgress.trangThai != TrangThaiQuest.DangLam)
            {
                continue;
            }

            // 🎯 LẤY DỮ LIỆU TRỰC TIẾP TỪ MẢNG KÉO THẢ TRONG INSPECTOR
            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);

            // Nếu không tìm thấy dữ liệu QuestData trong Inspector thì bỏ qua an toàn
            if (questData == null)
            {
                continue;
            }

            // Kiểm tra đúng loại quái cần diệt
            if (questData.idQuaiCanDiet != idQuai)
            {
                continue;
            }

            // Cộng số lượng quái
            questProgress.soBoXuongDaDiet += soLuong;

            // Kiểm tra nếu đạt đủ mục tiêu
            if (questProgress.soBoXuongDaDiet >= questData.soLuongBoXuongCanDiet)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongBoXuongCanDiet;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra;

                Debug.Log("<color=green>[Quest]</color> " + questData.tenNhiemVu + " đã đạt đủ điều kiện trả nhiệm vụ.");
            }

            coThayDoi = true;
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();

            // Nếu có UI ở scene này thì cập nhật lại UI luôn
            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
            }
        }
    }
}