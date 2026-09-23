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

// 🎯 BỔ SUNG: DỮ LIỆU ĐIỂM ĐẠO ĐỨC (MORAL POINTS DATA)
[Serializable]
public class MoralPointsSaveData
{
    public int diemThien = 0;      // Point 1: Điểm Cứu Người / Việc Thiện
    public int diemAc = 0;         // Point 2: Điểm Tà Đạo / Bỏ Mặc / Việc Ác
    public int diemDanhVong = 0;   // Point 3: Điểm Danh Vọng / Uy Tín Giang Hồ
}

[Serializable]
public class DanhSachSaveQuest
{
    // 🎯 LƯU CẢ TÊN MAP CỦ VÀ TÊN MAP MỚI (Đã sửa trùng khớp 100% với tên Scene trong Build Profiles)
    public string tenMapTruocDo = "Map_1_Thanh Trúc Lâm";
    public string tenMapMoiTiepTheo = "Map_2_U Minh Lâm";

    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
    public List<SaveItemData> danhSachItemSave = new List<SaveItemData>();
    public List<SaveSkillData> danhSachSkillSave = new List<SaveSkillData>();
    public PlayerStatsSaveData playerStats = new PlayerStatsSaveData();

    // 🎯 BỔ SUNG: Dữ liệu điểm đạo đức trong Struct Save Json
    public MoralPointsSaveData moralStats = new MoralPointsSaveData();
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

                if (duLieuSaveHienTai.moralStats == null)
                    duLieuSaveHienTai.moralStats = new MoralPointsSaveData();

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
    // 🎯 QUẢN LÝ 3 ĐIỂM ĐẠO ĐỨC (MORAL POINTS SYSTEM)
    // =========================================================

    /// <summary>
    /// Cộng/Trừ trực tiếp các điểm đạo đức và tự động ghi vào Save File.
    /// </summary>
    /// <param name="congDiemThien">Số điểm Thiện thay đổi</param>
    /// <param name="congDiemAc">Số điểm Ác thay đổi</param>
    /// <param name="congDiemDanhVong">Số điểm Danh Vọng thay đổi</param>
    public void ThayDoiDiemDaoDuc(int congDiemThien, int congDiemAc = 0, int congDiemDanhVong = 0)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.moralStats == null) duLieuSaveHienTai.moralStats = new MoralPointsSaveData();

        duLieuSaveHienTai.moralStats.diemThien = Mathf.Max(0, duLieuSaveHienTai.moralStats.diemThien + congDiemThien);
        duLieuSaveHienTai.moralStats.diemAc = Mathf.Max(0, duLieuSaveHienTai.moralStats.diemAc + congDiemAc);
        duLieuSaveHienTai.moralStats.diemDanhVong += congDiemDanhVong;

        SaveDuLieuQuestToTxt();
        Debug.Log($"<color=yellow>[Đạo Đức Update]</color> Thiện: {duLieuSaveHienTai.moralStats.diemThien} | Ác: {duLieuSaveHienTai.moralStats.diemAc} | Danh Vọng: {duLieuSaveHienTai.moralStats.diemDanhVong}");
    }

    /// <summary>
    /// Lấy điểm Thiện hiện tại
    /// </summary>
    public int LayDiemThien()
    {
        return duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemThien : 0;
    }

    /// <summary>
    /// Lấy điểm Ác hiện tại
    /// </summary>
    public int LayDiemAc()
    {
        return duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemAc : 0;
    }

    /// <summary>
    /// Lấy điểm Danh Vọng hiện tại
    /// </summary>
    public int LayDiemDanhVong()
    {
        return duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemDanhVong : 0;
    }

    // =========================================================
    // 🎯 HÀM ĐỒNG BỘ ĐẾM VÀ GIỚI HẠN SỐ LƯỢNG QUEST
    // =========================================================

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

    public bool KiemTraCoTheNhanQuestMoi(int maxQuest = 3)
    {
        return DemSoQuestDangLam() < maxQuest;
    }

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

    public int LayCapDoSkill(string nameSkill)
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachSkillSave == null) return 1;

        SaveSkillData skillSave = duLieuSaveHienTai.danhSachSkillSave.Find(s => s.skillName == nameSkill);
        return skillSave != null ? skillSave.skillLevel : 1;
    }

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
            return "Map_1_Thanh Trúc Lâm";
        }
        return duLieuSaveHienTai.tenMapTruocDo;
    }

    public string LayMapMoiTiepTheo()
    {
        if (duLieuSaveHienTai == null || string.IsNullOrEmpty(duLieuSaveHienTai.tenMapMoiTiepTheo))
        {
            return "Map_2_U Minh Lâm";
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

                // 🎯 CỘNG NGAY 10 ĐIỂM THIỆN & 5 ĐIỂM DANH VỌNG KHI GIẢI CỨU THÀNH CÔNG
                ThayDoiDiemDaoDuc(10, 0, 5);
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