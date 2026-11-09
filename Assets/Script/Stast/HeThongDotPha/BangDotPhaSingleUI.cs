using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Settings; // Thêm namespace kết nối với SettingsManager

[Serializable]
public class ItemYeuCauSingle
{
    public ItemData itemData;
    public int soLuongYeuCau;
}

[Serializable]
public class SlotUIItemSingle
{
    public Image imageIcon;
    public TextMeshProUGUI textSoLuong;
}

public class BangDotPhaSingleUI : MonoBehaviour
{
    [Header("--- THÔNG TIN CẢNH GIỚI BẢNG NÀY (TIẾNG VIỆT) ---")]
    [Tooltip("ID duy nhất của cảnh giới này (dùng để lưu file save, VD: DotPha_LuyenKhi_1, DotPha_TrucCo)")]
    public string idCanhGioi = "DotPha_LuyenKhi_1";

    public string tenCanhGioiMoi = "Luyện Khí Kỳ";
    public float levelYeuCau = 20f;

    [Header("--- THÔNG TIN CẢNH GIỚI BẢNG NÀY (TIẾNG ANH) ---")]
    [Tooltip("Tên cảnh giới bằng Tiếng Anh (Để trống sẽ tự dùng Tiếng Việt)")]
    public string tenCanhGioiMoiEN = "Qi Condensation";

    [Header("--- CHỈ SỐ CỘNG THÊM KHI ĐỘT PHÁ ---")]
    public float congDamage = 20f;
    public float congMaxHP = 100f;
    public float congArmor = 10f;
    [Tooltip("Mỗi cấp cảnh giới cộng thêm Energy")]
    public float congEnergy = 10f;

    [Header("--- THÀNH PHẦN UI CỦA BẢNG ---")]
    public TextMeshProUGUI textTenCanhGioi;
    public TextMeshProUGUI textLevelYeuCau;

    public TextMeshProUGUI textSoSanhDamage;
    public TextMeshProUGUI textSoSanhMaxHP;
    public TextMeshProUGUI textSoSanhArmor;
    public TextMeshProUGUI textSoSanhEnergy;

    public Button nutDotPha;
    public TextMeshProUGUI textThongBaoNut;

    [Header("--- HIỆU ỨNG THÔNG BÁO ĐỘT PHÁ ---")]
    [Tooltip("Text TMP dùng để hiển thị chữ Đột Phá Thành Công")]
    public TextMeshProUGUI textThongBaoDotPha;
    [Tooltip("Thời gian hiển thị trước khi mờ dần (giây)")]
    public float thoiGianChoMo = 1.0f;
    [Tooltip("Tốc độ mờ dần")]
    public float tocDoMo = 1.5f;

    [Header("--- YÊU CẦU ITEM ---")]
    public List<ItemYeuCauSingle> danhSachItemYeuCau = new List<ItemYeuCauSingle>();
    public List<SlotUIItemSingle> danhSachSlotUI = new List<SlotUIItemSingle>();

    private Coroutine hieuUngThongBaoCoroutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (!TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (textThongBaoDotPha != null)
        {
            textThongBaoDotPha.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 1. Lắng nghe sự kiện đổi ngôn ngữ toàn cục
        SettingsManager.OnLanguageChanged += HandleLanguageChanged;

        // 2. Ép bảng UI này nhảy xuống cuối cùng trong Hierarchy để đè lên toàn bộ UI khác
        transform.SetAsLastSibling();

        // 3. Kích hoạt chặn raycast của CanvasGroup
        ThietLapChanTuongTacChuyenXuyen(true);

        // 4. Cập nhật dữ liệu
        CapNhatGiaoDienBang();
    }

    private void OnDisable()
    {
        SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
        ThietLapChanTuongTacChuyenXuyen(false);
    }

    private void HandleLanguageChanged(string newLang)
    {
        CapNhatGiaoDienBang();
    }

    private void ThietLapChanTuongTacChuyenXuyen(bool kichHoat)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = kichHoat;
            canvasGroup.interactable = kichHoat;
        }
    }

    public void CapNhatGiaoDienBang()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
        string hienThiTenCanhGioi = (isEN && !string.IsNullOrEmpty(tenCanhGioiMoiEN)) ? tenCanhGioiMoiEN : tenCanhGioiMoi;

        if (textTenCanhGioi != null)
        {
            textTenCanhGioi.text = hienThiTenCanhGioi;
        }

        float playerLevel = 0f;
        float curDmg = 20f, curHP = 100f, curArmor = 0.1f, curEnergy = 100f;
        bool daDotPhaRot = false;

        bool hasSaveSystem = (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null);

        if (hasSaveSystem)
        {
            PlayerStatsSaveData stats = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;
            playerLevel = stats.level;
            curDmg = stats.damage;
            curHP = stats.maxHP;
            curArmor = stats.armor;
            curEnergy = stats.maxEnergy;

            if (QuestSaveSystem.Instance.KiemTraDaDatCanhGioi(idCanhGioi))
            {
                daDotPhaRot = true;
            }
        }

        if (textSoSanhDamage != null)
        {
            string labelDmg = isEN ? "Damage: " : "Sát thương: ";
            textSoSanhDamage.text = $"{labelDmg}{curDmg} <color=green>➔ {curDmg + congDamage} (+{congDamage})</color>";
        }

        if (textSoSanhMaxHP != null)
        {
            string labelHP = isEN ? "Max HP: " : "Máu tối đa: ";
            textSoSanhMaxHP.text = $"{labelHP}{curHP} <color=green>➔ {curHP + congMaxHP} (+{congMaxHP})</color>";
        }

        if (textSoSanhArmor != null)
        {
            string labelArmor = isEN ? "Armor: " : "Phòng thủ: ";
            textSoSanhArmor.text = $"{labelArmor}{curArmor} <color=green>➔ {curArmor + congArmor} (+{congArmor})</color>";
        }

        if (textSoSanhEnergy != null)
        {
            string labelEnergy = isEN ? "Energy: " : "Năng lượng: ";
            textSoSanhEnergy.text = $"{labelEnergy}{curEnergy} <color=green>➔ {curEnergy + congEnergy} (+{congEnergy})</color>";
        }

        bool duLevel = playerLevel >= levelYeuCau;
        if (textLevelYeuCau != null)
        {
            string mauLevel = duLevel ? "<color=green>" : "<color=red>";
            string labelReqLevel = isEN ? "Required Level: " : "Level Yêu Cầu: ";
            textLevelYeuCau.text = $"{labelReqLevel}{mauLevel}{playerLevel:F1}/{levelYeuCau:F1}</color>";
        }

        bool duToanBoItem = true;

        for (int i = 0; i < danhSachSlotUI.Count; i++)
        {
            if (i < danhSachItemYeuCau.Count)
            {
                ItemYeuCauSingle yeuCau = danhSachItemYeuCau[i];
                danhSachSlotUI[i].imageIcon.gameObject.SetActive(true);

                if (yeuCau.itemData != null)
                {
                    danhSachSlotUI[i].imageIcon.sprite = yeuCau.itemData.iconItem;

                    int soLuongDangCo = 0;
                    if (hasSaveSystem)
                    {
                        soLuongDangCo = QuestSaveSystem.Instance.LaySoLuongItemTrongKho(yeuCau.itemData.idItem);
                    }

                    bool duItem = soLuongDangCo >= yeuCau.soLuongYeuCau;
                    if (!duItem) duToanBoItem = false;

                    string mauItem = duItem ? "<color=green>" : "<color=red>";
                    danhSachSlotUI[i].textSoLuong.text = $"{mauItem}{soLuongDangCo}/{yeuCau.soLuongYeuCau}</color>";
                }
            }
            else
            {
                if (danhSachSlotUI[i].imageIcon != null)
                    danhSachSlotUI[i].imageIcon.gameObject.SetActive(false);

                if (danhSachSlotUI[i].textSoLuong != null)
                    danhSachSlotUI[i].textSoLuong.text = "";
            }
        }

        if (daDotPhaRot)
        {
            if (nutDotPha != null) nutDotPha.interactable = false;
            if (textThongBaoNut != null)
            {
                textThongBaoNut.text = isEN ? "<color=yellow>BREAKTHROUGH ACHIEVED</color>" : "<color=yellow>ĐÃ ĐỘT PHÁ</color>";
            }
        }
        else
        {
            bool duDieuKien = duLevel && duToanBoItem;

            if (nutDotPha != null)
            {
                nutDotPha.interactable = duDieuKien;
            }

            if (textThongBaoNut != null)
            {
                if (isEN)
                {
                    textThongBaoNut.text = duDieuKien ? "BREAKTHROUGH" : "NOT ENOUGH REQUIREMENTS";
                }
                else
                {
                    textThongBaoNut.text = duDieuKien ? "ĐỘT PHÁ" : "CHƯA ĐỦ ĐIỀU KIỆN";
                }
            }
        }
    }

    public void OnClickThucHienDotPha()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogWarning("[Đột Phá] Chưa có QuestSaveSystem trong Scene!");
            return;
        }

        if (QuestSaveSystem.Instance.KiemTraDaDatCanhGioi(idCanhGioi))
        {
            Debug.LogWarning("[Đột Phá] Cảnh giới này đã được đột phá rồi!");
            return;
        }

        // 1. Trừ số lượng Item yêu cầu
        foreach (var yeuCau in danhSachItemYeuCau)
        {
            if (yeuCau.itemData != null)
            {
                QuestSaveSystem.Instance.LuuItemVaoSaveGame(yeuCau.itemData.idItem, -yeuCau.soLuongYeuCau);
            }
        }

        // 2. Thay đổi trực tiếp vào biến gốc trong Save Manager
        QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.tenCanhGioi = tenCanhGioiMoi;
        QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.damage += congDamage;
        QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.maxHP += congMaxHP;
        QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.armor += congArmor;
        QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.maxEnergy += congEnergy;

        // 3. Cập nhật danh sách các cảnh giới đã đột phá thành công
        if (QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha == null)
        {
            QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha = new List<string>();
        }
        if (!QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha.Contains(idCanhGioi))
        {
            QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.danhSachCanhGioiDaDotPha.Add(idCanhGioi);
        }

        // 4. Lưu dữ liệu mới xuống file Save .txt
        QuestSaveSystem.Instance.SaveDuLieuQuestToTxt();

        // 5. Cập nhật chỉ số thực tế cho CharacterStats trong Scene
        CharacterStats[] allStats = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var stat in allStats)
        {
            stat.TaiThongSoTuSaveFile();
        }

        // 6. Cập nhật Energy cho Skill Manager
        PlayerSkillManager skillManager = FindFirstObjectByType<PlayerSkillManager>();
        if (skillManager != null)
        {
            skillManager.CapNhatMaxEnergyTuSave();
        }

        CapNhatGiaoDienBang();
        HienThongBaoDotPhaThanhCong();

        Debug.Log($"<color=green>[Đột Phá Success]</color> Đã nâng cảnh giới lên {tenCanhGioiMoi} (ID: {idCanhGioi}).");
    }

    private void HienThongBaoDotPhaThanhCong()
    {
        if (textThongBaoDotPha == null) return;

        if (hieuUngThongBaoCoroutine != null)
        {
            StopCoroutine(hieuUngThongBaoCoroutine);
        }

        hieuUngThongBaoCoroutine = StartCoroutine(Routine_HieuUngMoDanText());
    }

    private IEnumerator Routine_HieuUngMoDanText()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
        string hienThiTenCanhGioi = (isEN && !string.IsNullOrEmpty(tenCanhGioiMoiEN)) ? tenCanhGioiMoiEN : tenCanhGioiMoi;

        textThongBaoDotPha.gameObject.SetActive(true);

        if (isEN)
        {
            textThongBaoDotPha.text = $"<color=yellow>BREAKTHROUGH SUCCESSFUL!\nREACHED: {hienThiTenCanhGioi}</color>";
        }
        else
        {
            textThongBaoDotPha.text = $"<color=yellow>ĐỘT PHÁ THÀNH CÔNG!\nĐẠT: {hienThiTenCanhGioi}</color>";
        }

        Color mauGoc = textThongBaoDotPha.color;
        mauGoc.a = 1f;
        textThongBaoDotPha.color = mauGoc;

        yield return new WaitForSeconds(thoiGianChoMo);

        while (textThongBaoDotPha.color.a > 0f)
        {
            Color mauHienTai = textThongBaoDotPha.color;
            mauHienTai.a -= Time.deltaTime * tocDoMo;
            textThongBaoDotPha.color = mauHienTai;
            yield return null;
        }

        mauGoc.a = 1f;
        textThongBaoDotPha.color = mauGoc;
        textThongBaoDotPha.gameObject.SetActive(false);
        hieuUngThongBaoCoroutine = null;
    }
}