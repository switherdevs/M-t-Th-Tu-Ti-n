using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using GameCore.Quests;

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
    public int soBoXuongDaDiet; // Biến đếm chung
}

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

// 🎯 DỮ LIỆU LƯU CẤP ĐỘ & CHỈ SỐ SKILL
[Serializable]
public class SaveSkillData
{
    public string skillName;
    public int skillLevel;
    public float currentDamage;   // Sát thương hiện tại đã nâng cấp
    public float currentCooldown; // Thời gian hồi hiện tại đã nâng cấp

    public SaveSkillData(string name, int level, float damage, float cooldown)
    {
        skillName = name;
        skillLevel = level;
        currentDamage = damage;
        currentCooldown = cooldown;
    }
}

[Serializable]
public class PlayerStatsSaveData
{
    public string tenCanhGioi = "Luyện Khí Tầng 1";
    public float level = 1f;
    public float currentExp = 0f;
    public float maxExp = 5f;
    public float maxHP = 100f;
    public float damage = 20f;
    public float armor = 0.1f;
    public float maxEnergy = 100f;

    public List<string> danhSachCanhGioiDaDotPha = new List<string>();
}

[Serializable]
public class DanhSachSaveQuest
{
    // 🎯 LƯU CẢ TÊN MAP CŨ VÀ TÊN MAP MỚI
    public string tenMapTruocDo = "ThanhTrucLam";
    public string tenMapMoiTiepTheo = "UMinhLam";

    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
    public List<SaveItemData> danhSachItemSave = new List<SaveItemData>();
    public List<SaveSkillData> danhSachSkillSave = new List<SaveSkillData>(); // 🎯 LƯU DANH SÁCH SKILL
    public PlayerStatsSaveData playerStats = new PlayerStatsSaveData();
}

public class QuestSaveSystem : MonoBehaviour
{
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HÌNH DỮ LIỆU QUEST ---")]
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

                if (duLieuSaveHienTai.danhSachProgress == null)
                    duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();

                if (duLieuSaveHienTai.danhSachItemSave == null)
                    duLieuSaveHienTai.danhSachItemSave = new List<SaveItemData>();

                if (duLieuSaveHienTai.danhSachSkillSave == null)
                    duLieuSaveHienTai.danhSachSkillSave = new List<SaveSkillData>();

                if (duLieuSaveHienTai.playerStats == null)
                    duLieuSaveHienTai.playerStats = new PlayerStatsSaveData();

                if (duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha == null)
                    duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha = new List<string>();

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

    // =========================================================
    // 🎯 HÀM ĐỒNG BỘ ĐẾM VÀ GIỚI HẠN SỐ LƯỢNG QUEST
    // =========================================================

    /// <summary>
    /// Đếm tổng số nhiệm vụ người chơi đang nhận (Đang làm + Đã xong chưa trả)
    /// </summary>
    public int DemSoQuestDangLam()
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachProgress == null) return 0;

        int count = 0;
        foreach (ProgressQuest q in duLieuSaveHienTai.danhSachProgress)
        {
            if (q.trangThai == TrangThaiQuest.DangLam || q.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Kiểm tra xem người chơi có đủ điều kiện nhận thêm Quest mới không (Mặc định tối đa 3)
    /// </summary>
    public bool KiemTraCoTheNhanQuestMoi(int maxQuest = 3)
    {
        return DemSoQuestDangLam() < maxQuest;
    }

    // 🎯 HÀM LƯU TOÀN BỘ THÔNG TIN SKILL VÀO FILE SAVE
    public void LuuSkillFullData(string nameSkill, int levelSkill, float damage, float cooldown)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.danhSachSkillSave == null) duLieuSaveHienTai.danhSachSkillSave = new List<SaveSkillData>();

        SaveSkillData skillSave = duLieuSaveHienTai.danhSachSkillSave.Find(s => s.skillName == nameSkill);
        if (skillSave != null)
        {
            skillSave.skillLevel = levelSkill;
            skillSave.currentDamage = damage;
            skillSave.currentCooldown = cooldown;
        }
        else
        {
            duLieuSaveHienTai.danhSachSkillSave.Add(new SaveSkillData(nameSkill, levelSkill, damage, cooldown));
        }

        SaveDuLieuQuestToTxt();
    }

    // 🎯 HÀM LƯU CẤP ĐỘ SKILL (TƯƠNG THÍCH MỞ RỘNG)
    public void LuuCapDoSkill(string nameSkill, int levelSkill)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.danhSachSkillSave == null) duLieuSaveHienTai.danhSachSkillSave = new List<SaveSkillData>();

        SaveSkillData skillSave = duLieuSaveHienTai.danhSachSkillSave.Find(s => s.skillName == nameSkill);
        if (skillSave != null)
        {
            skillSave.skillLevel = levelSkill;
        }
        else
        {
            duLieuSaveHienTai.danhSachSkillSave.Add(new SaveSkillData(nameSkill, levelSkill, 0f, 0f));
        }

        SaveDuLieuQuestToTxt();
    }

    // 🎯 HÀM ĐỌC CẤP ĐỘ SKILL TỪ FILE SAVE
    public int LayCapDoSkill(string nameSkill)
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachSkillSave == null) return 1;

        SaveSkillData skillSave = duLieuSaveHienTai.danhSachSkillSave.Find(s => s.skillName == nameSkill);
        return skillSave != null ? skillSave.skillLevel : 1;
    }

    // 🎯 HÀM LẤY TOÀN BỘ DATA DÃ LƯU CỦA 1 SKILL
    public SaveSkillData LayDuLieuSkill(string nameSkill)
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachSkillSave == null) return null;
        return duLieuSaveHienTai.danhSachSkillSave.Find(s => s.skillName == nameSkill);
    }

    public void LuuMapTruocDo(string tenMap)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        duLieuSaveHienTai.tenMapTruocDo = tenMap;
        SaveDuLieuQuestToTxt();
        Debug.Log("<color=cyan>[Save System]</color> Đã ghi nhận Map trước đó: " + tenMap);
    }

    public void LuuMapMoiTiepTheo(string tenMapMoi)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        duLieuSaveHienTai.tenMapMoiTiepTheo = tenMapMoi;
        SaveDuLieuQuestToTxt();
        Debug.Log("<color=cyan>[Save System]</color> Đã ghi nhận Map mới tiếp theo: " + tenMapMoi);
    }

    public string LayMapTruocDo()
    {
        if (duLieuSaveHienTai == null || string.IsNullOrEmpty(duLieuSaveHienTai.tenMapTruocDo))
        {
            return "ThanhTrucLam";
        }
        return duLieuSaveHienTai.tenMapTruocDo;
    }

    public string LayMapMoiTiepTheo()
    {
        if (duLieuSaveHienTai == null || string.IsNullOrEmpty(duLieuSaveHienTai.tenMapMoiTiepTheo))
        {
            return "UMinhLam";
        }
        return duLieuSaveHienTai.tenMapMoiTiepTheo;
    }

    public void LuuItemVaoSaveGame(string idItem, int soLuong = 1)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        if (duLieuSaveHienTai.danhSachItemSave == null)
            duLieuSaveHienTai.danhSachItemSave = new List<SaveItemData>();

        SaveItemData itemDaCo = duLieuSaveHienTai.danhSachItemSave.Find(x => x.idItem == idItem);

        if (itemDaCo != null)
        {
            itemDaCo.soLuong += soLuong;
            if (itemDaCo.soLuong <= 0)
            {
                duLieuSaveHienTai.danhSachItemSave.Remove(itemDaCo);
            }
        }
        else if (soLuong > 0)
        {
            duLieuSaveHienTai.danhSachItemSave.Add(new SaveItemData(idItem, soLuong));
        }

        SaveDuLieuQuestToTxt();
    }

    public int LaySoLuongItemTrongKho(string idItem)
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachItemSave == null) return 0;

        SaveItemData item = duLieuSaveHienTai.danhSachItemSave.Find(x => x.idItem == idItem);
        return item != null ? item.soLuong : 0;
    }

    public QuestData LayQuestDataTheoID(int idQuest)
    {
        foreach (QuestData q in danhSachQuestData)
        {
            if (q != null && q.idQuest == idQuest) return q;
        }
        return null;
    }

    public ProgressQuest LayTienTrinhQuest(int idQuest)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        if (duLieuSaveHienTai.danhSachProgress == null)
            duLieuSaveHienTai.danhSachProgress = new List<ProgressQuest>();

        foreach (ProgressQuest quest in duLieuSaveHienTai.danhSachProgress)
        {
            if (quest.idQuest == idQuest) return quest;
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

        if (trangThaiMoi == TrangThaiQuest.ChuaNhan)
        {
            quest.soBoXuongDaDiet = 0;
        }

        SaveDuLieuQuestToTxt();
    }

    public void GhiNhanDietQuai(int idQuai, int soLuong = 1)
    {
        if (soLuong <= 0) return;
        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            if (questProgress.trangThai != TrangThaiQuest.DangLam) continue;

            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);
            if (questData == null || questData.loaiQuest != LoaiQuest.DietQuai || questData.idQuaiCanDiet != idQuai) continue;

            questProgress.soBoXuongDaDiet += soLuong;

            if (questProgress.soBoXuongDaDiet >= questData.soLuongBoXuongCanDiet)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongBoXuongCanDiet;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra;
            }

            coThayDoi = true;
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();
            QuestHUDTracker.ThongBaoCapNhatHUD();
        }
    }

    public void GhiNhanGiaiCuu(int idDoiTuong, int soLuong = 1)
    {
        if (soLuong <= 0) return;
        bool coThayDoi = false;

        foreach (ProgressQuest questProgress in duLieuSaveHienTai.danhSachProgress)
        {
            if (questProgress.trangThai != TrangThaiQuest.DangLam) continue;

            QuestData questData = LayQuestDataTheoID(questProgress.idQuest);
            if (questData == null || questData.loaiQuest != LoaiQuest.GiaiCuu || questData.idDoiTuongCanGiaiCuu != idDoiTuong) continue;

            questProgress.soBoXuongDaDiet += soLuong;

            if (questProgress.soBoXuongDaDiet >= questData.soLuongCanGiaiCuu)
            {
                questProgress.soBoXuongDaDiet = questData.soLuongCanGiaiCuu;
                questProgress.trangThai = TrangThaiQuest.DaXongChuaTra;
            }

            coThayDoi = true;
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();
            QuestHUDTracker.ThongBaoCapNhatHUD();
        }
    }

    public bool KiemTraNPCGiaiCuuCoDuocPhepXuatHien(int idDoiTuong)
    {
        if (danhSachQuestData == null || danhSachQuestData.Count == 0) return false;

        QuestData questData = danhSachQuestData.Find(q =>
            q != null &&
            q.loaiQuest == LoaiQuest.GiaiCuu &&
            q.idDoiTuongCanGiaiCuu == idDoiTuong
        );

        if (questData == null) return false;

        ProgressQuest progress = LayTienTrinhQuest(questData.idQuest);
        return progress != null && progress.trangThai == TrangThaiQuest.DangLam;
    }

    public bool KiemTraDaDatCanhGioi(string idCanhGioi)
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.playerStats == null) return false;
        if (duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha == null) return false;

        return duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha.Contains(idCanhGioi);
    }
}