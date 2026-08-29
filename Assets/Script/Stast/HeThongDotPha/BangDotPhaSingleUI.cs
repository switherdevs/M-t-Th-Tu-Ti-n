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
    [Tooltip("ID duy nhất của cảnh giới này (dùng để kiểm tra save file, VD: DotPha_LuyenKhi, DotPha_TrucCo)")]
    public string idCanhGioi = "DotPha_LuyenKhi";

    public string tenCanhGioiMoi = "Luyện Khí Kỳ";
    public float levelYeuCau = 20f;

    [Header("--- CHỈ SỐ CỘNG THÊM KHI ĐỘT PHÁ ---")]
    public float congDamage = 20f;
    public float congMaxHP = 100f;
    public float congArmor = 10f;
    [Tooltip("Mỗi cấp cảnh giới cộng thêm 10 Energy")]
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

    [Header("--- YÊU CẦU ITEM ---")]
    public List<ItemYeuCauSingle> danhSachItemYeuCau = new List<ItemYeuCauSingle>();
    public List<SlotUIItemSingle> danhSachSlotUI = new List<SlotUIItemSingle>();

    private void OnEnable()
    {
        CapNhatGiaoDienBang();
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

            // KIỂM TRA XEM ID CẢNH GIỚI NÀY ĐÃ ĐƯỢC LƯU TRONG DANH SÁCH ĐÃ ĐỘT PHÁ CHƯA
            if (stats.danhSachCanhGioiDaDotPha != null && stats.danhSachCanhGioiDaDotPha.Contains(idCanhGioi))
            {
                daDotPhaRot = true;
            }
        }

        // HIỂN THỊ SO SÁNH CHỈ SỐ
        if (textSoSanhDamage != null)
            textSoSanhDamage.text = $"Sát thương: {curDmg} <color=green>➔ {curDmg + congDamage} (+{congDamage})</color>";

        if (textSoSanhMaxHP != null)
            textSoSanhMaxHP.text = $"Máu tối đa: {curHP} <color=green>➔ {curHP + congMaxHP} (+{congMaxHP})</color>";

        if (textSoSanhArmor != null)
            textSoSanhArmor.text = $"Phòng thủ: {curArmor} <color=green>➔ {curArmor + congArmor} (+{congArmor})</color>";

        if (textSoSanhEnergy != null)
            textSoSanhEnergy.text = $"Năng lượng: {curEnergy} <color=green>➔ {curEnergy + congEnergy} (+{congEnergy})</color>";

        // HIỂN THỊ LEVEL YÊU CẦU
        bool duLevel = playerLevel >= levelYeuCau;
        if (textLevelYeuCau != null)
        {
            string mauLevel = duLevel ? "<color=green>" : "<color=red>";
            textLevelYeuCau.text = $"Level Yêu Cầu: {mauLevel}{playerLevel:F1}/{levelYeuCau:F1}</color>";
        }

        // KIỂM TRA ITEM YÊU CẦU
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

        // XỬ LÝ KHÓA NÚT BẤM VÀ THÔNG BÁO VĨNH VIỄN NẾU ĐÃ ĐỘT PHÁ
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

        // KIỂM TRA LẠI MỘT LẦN NỮA ĐỂ TRÁNH SPAM CLICK NÚT
        if (statsPlayer.danhSachCanhGioiDaDotPha != null && statsPlayer.danhSachCanhGioiDaDotPha.Contains(idCanhGioi))
        {
            Debug.LogWarning("[Đột Phá] Cảnh giới này đã được đột phá rồi!");
            return;
        }

        // 1. Trừ Item
        foreach (var yeuCau in danhSachItemYeuCau)
        {
            if (yeuCau.itemData != null)
            {
                QuestSaveSystem.Instance.LuuItemVaoSaveGame(yeuCau.itemData.idItem, -yeuCau.soLuongYeuCau);
            }
        }

        // 2. Tăng Chỉ Số Save File
        statsPlayer.tenCanhGioi = tenCanhGioiMoi;
        statsPlayer.damage += congDamage;
        statsPlayer.maxHP += congMaxHP;
        statsPlayer.armor += congArmor;
        statsPlayer.maxEnergy += congEnergy;

        // 🎯 GHI NHẬN ID CẢNH GIỚI NÀY VÀO DANH SÁCH ĐÃ ĐỘT PHÁ
        if (statsPlayer.danhSachCanhGioiDaDotPha == null)
        {
            statsPlayer.danhSachCanhGioiDaDotPha = new List<string>();
        }
        if (!statsPlayer.danhSachCanhGioiDaDotPha.Contains(idCanhGioi))
        {
            statsPlayer.danhSachCanhGioiDaDotPha.Add(idCanhGioi);
        }

        // 3. Lưu File Save JSON
        QuestSaveSystem.Instance.SaveDuLieuQuestToTxt();

        // 4. Cập nhật trực tiếp vào Nhân Vật Player
        CharacterStats[] allStats = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var stat in allStats)
        {
            stat.TaiThongSoTuSaveFile();
        }

        // 5. Đồng bộ lại Năng lượng tối đa sang PlayerSkillManager
        PlayerSkillManager skillManager = FindFirstObjectByType<PlayerSkillManager>();
        if (skillManager != null)
        {
            skillManager.CapNhatMaxEnergyTuSave();
        }

        // 6. Refresh lại Bảng UI Đột phá (Sẽ tự động khóa nút vĩnh viễn)
        CapNhatGiaoDienBang();

        Debug.Log($"<color=green>[Đột Phá Success]</color> Đã nâng cảnh giới lên {tenCanhGioiMoi} và lưu trạng thái khóa bảng.");
    }
}