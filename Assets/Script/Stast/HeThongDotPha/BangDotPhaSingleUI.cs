using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [Header("--- THÔNG TIN CẢNH GIỚI BẢNG NÀY ---")]
    [Tooltip("ID duy nhất của cảnh giới này (dùng để lưu file save, VD: DotPha_LuyenKhi_1, DotPha_TrucCo)")]
    public string idCanhGioi = "DotPha_LuyenKhi_1";

    public string tenCanhGioiMoi = "Luyện Khí Kỳ";
    public float levelYeuCau = 20f;

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
        // 1. Ép bảng UI này nhảy xuống cuối cùng trong Hierarchy để đè lên toàn bộ UI khác
        transform.SetAsLastSibling();

        // 2. Kích hoạt chặn raycast của CanvasGroup
        ThietLapChanTuongTacChuyenXuyen(true);

        // 3. Cập nhật dữ liệu
        CapNhatGiaoDienBang();
    }

    private void OnDisable()
    {
        ThietLapChanTuongTacChuyenXuyen(false);
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
        if (textTenCanhGioi != null)
        {
            textTenCanhGioi.text = tenCanhGioiMoi;
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
            textSoSanhDamage.text = $"Sát thương: {curDmg} <color=green>➔ {curDmg + congDamage} (+{congDamage})</color>";

        if (textSoSanhMaxHP != null)
            textSoSanhMaxHP.text = $"Máu tối đa: {curHP} <color=green>➔ {curHP + congMaxHP} (+{congMaxHP})</color>";

        if (textSoSanhArmor != null)
            textSoSanhArmor.text = $"Phòng thủ: {curArmor} <color=green>➔ {curArmor + congArmor} (+{congArmor})</color>";

        if (textSoSanhEnergy != null)
            textSoSanhEnergy.text = $"Năng lượng: {curEnergy} <color=green>➔ {curEnergy + congEnergy} (+{congEnergy})</color>";

        bool duLevel = playerLevel >= levelYeuCau;
        if (textLevelYeuCau != null)
        {
            string mauLevel = duLevel ? "<color=green>" : "<color=red>";
            textLevelYeuCau.text = $"Level Yêu Cầu: {mauLevel}{playerLevel:F1}/{levelYeuCau:F1}</color>";
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
            if (textThongBaoNut != null) textThongBaoNut.text = "<color=yellow>ĐÃ ĐỘT PHÁ</color>";
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
                textThongBaoNut.text = duDieuKien ? "ĐỘT PHÁ" : "CHƯA ĐỦ ĐIỀU KIỆN";
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

        PlayerStatsSaveData statsPlayer = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

        if (QuestSaveSystem.Instance.KiemTraDaDatCanhGioi(idCanhGioi))
        {
            Debug.LogWarning("[Đột Phá] Cảnh giới này đã được đột phá rồi!");
            return;
        }

        foreach (var yeuCau in danhSachItemYeuCau)
        {
            if (yeuCau.itemData != null)
            {
                QuestSaveSystem.Instance.LuuItemVaoSaveGame(yeuCau.itemData.idItem, -yeuCau.soLuongYeuCau);
            }
        }

        statsPlayer.tenCanhGioi = tenCanhGioiMoi;
        statsPlayer.damage += congDamage;
        statsPlayer.maxHP += congMaxHP;
        statsPlayer.armor += congArmor;
        statsPlayer.maxEnergy += congEnergy;

        if (statsPlayer.danhSachCanhGioiDaDotPha == null)
        {
            statsPlayer.danhSachCanhGioiDaDotPha = new List<string>();
        }
        if (!statsPlayer.danhSachCanhGioiDaDotPha.Contains(idCanhGioi))
        {
            statsPlayer.danhSachCanhGioiDaDotPha.Add(idCanhGioi);
        }

        QuestSaveSystem.Instance.SaveDuLieuQuestToTxt();

        CharacterStats[] allStats = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var stat in allStats)
        {
            stat.TaiThongSoTuSaveFile();
        }

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
        textThongBaoDotPha.gameObject.SetActive(true);
        textThongBaoDotPha.text = $"<color=yellow>ĐỘT PHÁ THÀNH CÔNG!\nĐẠT: {tenCanhGioiMoi}</color>";

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