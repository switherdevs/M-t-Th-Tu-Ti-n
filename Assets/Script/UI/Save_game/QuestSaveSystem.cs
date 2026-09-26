using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public int soBoXuongDaDiet; // Biến ghi nhận tiến trình
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

[Serializable]
public class SaveSkillData
{
    public string skillName;
    public int skillLevel;
    public float currentDamage;
    public float currentCooldown;

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
public class MoralPointsSaveData
{
    public int diemThien = 0;
    public int diemAc = 0;
    public int diemDanhVong = 0;
}

[Serializable]
public class DanhSachSaveQuest
{
    public string tenMapTruocDo = "";

    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
    public List<SaveItemData> danhSachItemSave = new List<SaveItemData>();
    public List<SaveSkillData> danhSachSkillSave = new List<SaveSkillData>();
    public PlayerStatsSaveData playerStats = new PlayerStatsSaveData();
    public MoralPointsSaveData moralStats = new MoralPointsSaveData();
}

public class QuestSaveSystem : MonoBehaviour
{
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HÌNH DỮ LIỆU QUEST ---")]
    [Tooltip("Danh sách QuestData tự động đồng bộ từ Inspector hoặc thư mục Resources")]
    public List<QuestData> danhSachQuestData = new List<QuestData>();

    [Header("--- CẤU HÌNH CÁC MAP CHÍNH (OVERWORLD) ---")]
    [Tooltip("Danh sách 3 Map Overworld chính trong game.")]
    public List<string> danhSachMapChinh = new List<string>()
    {
        "Map_1_Thanh Trúc Lâm",
        "Map_2U Minh Lâm",
        "Map_3 luyennguc"
    };

    [Header("--- CẤU HÌNH SAVE ---")]
    public string tenFileSave = "QuestProgressData.txt";

    private string duongDanTuyetDoi;
    public DanhSachSaveQuest duLieuSaveHienTai = new DanhSachSaveQuest();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Nếu đã có Instance cũ, hãy đồng bộ danh sách QuestData từ Scene mới sang Instance cũ trước khi hủy Object thừa
            if (danhSachQuestData != null && danhSachQuestData.Count > 0)
            {
                Instance.CapNhatDanhSachQuestData(danhSachQuestData);
            }
            Destroy(gameObject);
            return;
        }

        duongDanTuyetDoi = Path.Combine(
            Application.persistentDataPath,
            tenFileSave
        );

        NapToanBoQuestDataTuResources();
        LoadDuLieuQuestFromTxt();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        KiemTraVoiResetQuestKhiSangMapChinhMoi(scene.name);
    }

    /// <summary>
    /// 🎯 HÀM NẠP TẤT CẢ QUEST DATA TỪ THƯ MỤC RESOURCES ĐỂ TRÁNH LỖI THIẾU DATA KHI CHUYỂN MAP
    /// </summary>
    public void NapToanBoQuestDataTuResources()
    {
        QuestData[] loadedQuests = Resources.LoadAll<QuestData>("");
        if (loadedQuests != null && loadedQuests.Length > 0)
        {
            foreach (QuestData q in loadedQuests)
            {
                if (q != null && !danhSachQuestData.Contains(q))
                {
                    danhSachQuestData.Add(q);
                }
            }
            Debug.Log($"<color=cyan>[Save System]</color> Đã tự động nạp {loadedQuests.Length} QuestData từ Resources!");
        }
    }

    /// <summary>
    /// 🎯 HÀM CẬP NHẬT/BỔ SUNG CÁC QUEST DATA MỚI TỪ MAP HIỆN TẠI VÀO BỘ NHỚ CHUNG
    /// </summary>
    public void CapNhatDanhSachQuestData(List<QuestData> danhSachMoi)
    {
        if (danhSachMoi == null || danhSachMoi.Count == 0) return;

        foreach (QuestData q in danhSachMoi)
        {
            if (q != null && !danhSachQuestData.Exists(x => x.idQuest == q.idQuest))
            {
                danhSachQuestData.Add(q);
            }
        }
    }

    public void KiemTraVoiResetQuestKhiSangMapChinhMoi(string tenMapMoi)
    {
        if (!danhSachMapChinh.Contains(tenMapMoi)) return;

        string mapTruocDo = LayMapTruocDo();

        if (!string.IsNullOrEmpty(mapTruocDo) && mapTruocDo != tenMapMoi)
        {
            Debug.LogWarning($"<color=yellow>[QuestSystem]</color> Phát hiện chuyển từ Main Map {mapTruocDo} sang Main Map {tenMapMoi}. Tiến hành reset nhiệm vụ...");
            ResetToanBoQuestDangLam();
        }

        LuuMapTruocDo(tenMapMoi);
    }

    public void ResetToanBoQuestDangLam()
    {
        if (duLieuSaveHienTai == null || duLieuSaveHienTai.danhSachProgress == null) return;

        bool coThayDoi = false;

        foreach (ProgressQuest quest in duLieuSaveHienTai.danhSachProgress)
        {
            if (quest.trangThai == TrangThaiQuest.DangLam || quest.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                quest.trangThai = TrangThaiQuest.ChuaNhan;
                quest.soBoXuongDaDiet = 0;
                coThayDoi = true;
            }
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.DongBoActiveQuestsTuSaveSystem();
            }

            QuestHUDTracker.ThongBaoCapNhatHUD();
            Debug.Log("<color=red>[QuestSystem]</color> Đã reset tất cả nhiệm vụ đang làm!");
        }
    }

    public void SaveDuLieuQuestToTxt()
    {
        try
        {
            string chuoiJson = JsonUtility.ToJson(duLieuSaveHienTai, true);
            File.WriteAllText(duongDanTuyetDoi, chuoiJson);
            Debug.Log("<color=green>[Save System]</color> Đã lưu dữ liệu thành công!");

            Scene_load sceneLoader = UnityEngine.Object.FindAnyObjectByType<Scene_load>();
            if (sceneLoader != null)
            {
                sceneLoader.CapNhatTrangThaiButtonTiepTuc();
            }
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

                if (string.IsNullOrEmpty(duLieuSaveHienTai.tenMapTruocDo) || !danhSachMapChinh.Contains(duLieuSaveHienTai.tenMapTruocDo))
                {
                    if (danhSachMapChinh != null && danhSachMapChinh.Count > 0)
                    {
                        duLieuSaveHienTai.tenMapTruocDo = danhSachMapChinh[0];
                    }
                }

                DonDepNhiemVuLoiVacantData();

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

    public void XoaToanBoSaveData()
    {
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                File.Delete(duongDanTuyetDoi);
                Debug.Log("<color=red>[Save System]</color> Đã xóa file save cũ thành công!");
            }
            catch (Exception e)
            {
                Debug.LogError("[Save System] Không thể xóa file save: " + e.Message);
            }
        }

        TaoFileSaveMoi();
    }

    private void DonDepNhiemVuLoiVacantData()
    {
        if (danhSachQuestData == null || danhSachQuestData.Count == 0) return;
        if (duLieuSaveHienTai?.danhSachProgress == null) return;

        bool coThayDoi = false;
        for (int i = duLieuSaveHienTai.danhSachProgress.Count - 1; i >= 0; i--)
        {
            int id = duLieuSaveHienTai.danhSachProgress[i].idQuest;
            if (LayQuestDataTheoID(id) == null)
            {
                Debug.LogWarning($"<color=orange>[Save System]</color> Phát hiện Quest ID {id} không hợp lệ. Đang xóa...");
                duLieuSaveHienTai.danhSachProgress.RemoveAt(i);
                coThayDoi = true;
            }
        }

        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();
        }
    }

    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai = new DanhSachSaveQuest();

        if (danhSachMapChinh != null && danhSachMapChinh.Count > 0)
        {
            duLieuSaveHienTai.tenMapTruocDo = danhSachMapChinh[0];
        }

        Debug.Log($"<color=yellow>[Save System]</color> Khởi tạo dữ liệu save mới trong RAM thành công.");
    }

    public void ThayDoiDiemDaoDuc(int congDiemThien, int congDiemAc = 0, int congDiemDanhVong = 0)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();
        if (duLieuSaveHienTai.moralStats == null) duLieuSaveHienTai.moralStats = new MoralPointsSaveData();

        duLieuSaveHienTai.moralStats.diemThien = Mathf.Max(0, duLieuSaveHienTai.moralStats.diemThien + congDiemThien);
        duLieuSaveHienTai.moralStats.diemAc = Mathf.Max(0, duLieuSaveHienTai.moralStats.diemAc + congDiemAc);
        duLieuSaveHienTai.moralStats.diemDanhVong += congDiemDanhVong;

        SaveDuLieuQuestToTxt();
    }

    public int LayDiemThien() => duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemThien : 0;
    public int LayDiemAc() => duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemAc : 0;
    public int LayDiemDanhVong() => duLieuSaveHienTai?.moralStats != null ? duLieuSaveHienTai.moralStats.diemDanhVong : 0;

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

    public void LuuMapHienTaiLamMapTruocDo()
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        string tenMapHienTai = SceneManager.GetActiveScene().name;

        if (danhSachMapChinh.Contains(tenMapHienTai))
        {
            duLieuSaveHienTai.tenMapTruocDo = tenMapHienTai;
            SaveDuLieuQuestToTxt();
            Debug.Log("<color=cyan>[Save System]</color> Đã ghi nhận Main Map trước đó: " + tenMapHienTai);
        }
    }

    public void LuuMapTruocDo(string tenMap)
    {
        if (duLieuSaveHienTai == null) duLieuSaveHienTai = new DanhSachSaveQuest();

        if (danhSachMapChinh.Contains(tenMap))
        {
            duLieuSaveHienTai.tenMapTruocDo = tenMap;
            SaveDuLieuQuestToTxt();
            Debug.Log("<color=cyan>[Save System]</color> Đã ghi nhận Main Map trước đó: " + tenMap);
        }
    }

    public void LuuMapMoiTiepTheo(string tenMapMoi)
    {
        LuuMapTruocDo(tenMapMoi);
    }

    public string LayMapTruocDo()
    {
        if (duLieuSaveHienTai == null || string.IsNullOrEmpty(duLieuSaveHienTai.tenMapTruocDo))
        {
            if (danhSachMapChinh != null && danhSachMapChinh.Count > 0)
            {
                return danhSachMapChinh[0];
            }
            return "";
        }
        return duLieuSaveHienTai.tenMapTruocDo;
    }

    public string LayMapMoiTiepTheo()
    {
        return LayMapTruocDo();
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
        if (danhSachQuestData == null) return null;

        foreach (QuestData q in danhSachQuestData)
        {
            if (q != null && q.idQuest == idQuest) return q;
        }

        // Nếu chưa tìm thấy trong List, thử load trực tiếp từ Resources
        QuestData[] allQuests = Resources.LoadAll<QuestData>("");
        foreach (QuestData q in allQuests)
        {
            if (q != null && q.idQuest == idQuest)
            {
                if (!danhSachQuestData.Contains(q)) danhSachQuestData.Add(q);
                return q;
            }
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

    public void GhiNhanCapNhatTienTrinh(int idQuest, int soLuongMoi)
    {
        ProgressQuest quest = LayTienTrinhQuest(idQuest);
        if (quest == null) return;

        quest.soBoXuongDaDiet = soLuongMoi;

        QuestData qData = LayQuestDataTheoID(idQuest);
        if (qData != null)
        {
            int mucTieu = qData.loaiQuest == LoaiQuest.DietQuai ? qData.soLuongBoXuongCanDiet : qData.soLuongCanGiaiCuu;
            if (quest.soBoXuongDaDiet >= mucTieu)
            {
                quest.soBoXuongDaDiet = mucTieu;
                quest.trangThai = TrangThaiQuest.DaXongChuaTra;
            }
        }

        SaveDuLieuQuestToTxt();
        QuestHUDTracker.ThongBaoCapNhatHUD();
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