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

// 🎯 BỔ SUNG: Cấu trúc lưu vật phẩm vào File
[Serializable]
public class SaveItemData
{
    public string idItem;
    public int soLuong;

    public SaveItemData(string id, int count)
    {
        idItem = id;
        soLuong = count;
    }
}

[Serializable]
public class DanhSachSaveQuest
{
    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
    // 🎯 BỔ SUNG: Danh sách lưu trữ vật phẩm
    public List<SaveItemData> danhSachItemSave = new List<SaveItemData>();
}

public class QuestSaveSystem : MonoBehaviour
{
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HINH DỮ LIỆU QUEST ---")]
    public List<QuestData> danhSachQuestData = new List<QuestData>();

    [Header("--- CẤU HÌNH SAVE ---")]
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
            Debug.Log("<color=green>[Save System]</color> Đã lưu dữ liệu: " + duongDanTuyetDoi);
        }
        catch (Exception e)
        {
            Debug.LogError("[Save System] Lỗi ghi file: " + e.Message);
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
                if (duLieuSaveHienTai.danhSachItemSave == null) duLieuSaveHienTai.danhSachItemSave = new List<SaveItemData>();

                Debug.Log("<color=cyan>[Save System]</color> Đã load dữ liệu thành công.");
            }
            catch (Exception e)
            {
                Debug.LogError("[Save System] Lỗi đọc file: " + e.Message);
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

    // 🎯 HÀM BỔ SUNG: LƯU ITEM KHI THẮNG MÀN HOẶC NHẶT ĐỒ
    public void LuuItemVaoSaveGame(string idItem, int soLuong = 1)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.danhSachItemSave == null) duLieuSaveHienTai.danhSachItemSave = new List<SaveItemData>();

        // Kiểm tra xem Item đã có trong Save chưa
        SaveItemData itemDaCo = duLieuSaveHienTai.danhSachItemSave.Find(x => x.idItem == idItem);

        if (itemDaCo != null)
        {
            itemDaCo.soLuong += soLuong; // Nếu có rồi thì cộng dồn số lượng
        }
        else
        {
            duLieuSaveHienTai.danhSachItemSave.Add(new SaveItemData(idItem, soLuong)); // Chưa có thì thêm mới
        }

        SaveDuLieuQuestToTxt();
        Debug.Log($"<color=yellow>[Save System]</color> Đã lưu Item ID: {idItem} (Số lượng: {soLuong}) vào File Save!");
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

    public void GhiNhanDietQuai(int idQuai, int soLuong = 1)
    {
        if (soLuong <= 0) return;

        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            if (questProgress.trangThai != TrangThaiQuest.DangLam)
            {
                continue;
            }

            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);

            if (questData == null)
            {
                Debug.LogWarning($"[QuestSaveSystem] Không tìm thấy QuestData cho Quest ID: {questProgress.idQuest}.");
                continue;
            }

            if (questData.idQuaiCanDiet != idQuai)
            {
                continue;
            }

            questProgress.soBoXuongDaDiet += soLuong;

            if (questProgress.soBoXuongDaDiet >= questData.soLuongBoXuongCanDiet)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongBoXuongCanDiet;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra;

                Debug.Log("<color=green>[Quest]</color> " + questData.tenNhiemVu + " đã hoàn thành mục tiêu diệt quái!");
            }

            coThayDoi = true;
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();

            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
            }

            QuestHUDTracker.ThongBaoCapNhatHUD();
        }
    }
}