using System;
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
    public string tenCanhGioiMoi = "Kim Đan Kỳ";
    public int levelYeuCau = 20;

    [Header("--- CHỈ SỐ CỘNG THÊM KHI ĐỘT PHÁ ---")]
    public float congDamage = 20f;
    public float congMaxHP = 100f;
    public float congArmor = 10f;

    [Header("--- THÀNH PHẦN UI CỦA BẢNG ---")]
    public TextMeshProUGUI textTenCanhGioi;
    public TextMeshProUGUI textLevelYeuCau;
    public Button nutDotPha;
    public TextMeshProUGUI textThongBaoNut;

    [Header("--- YÊU CẦU ITEM ---")]
    public List<ItemYeuCauSingle> danhSachItemYeuCau = new List<ItemYeuCauSingle>();
    public List<SlotUIItemSingle> danhSachSlotUI = new List<SlotUIItemSingle>();

    private void OnEnable()
    {
        CapNhatGiaoDienBang();
    }

    /// <summary>
    /// Hàm kiểm tra Save Game và hiển thị đúng/sai lên Bảng
    /// </summary>
    public void CapNhatGiaoDienBang()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null) return;

        PlayerStatsSaveData statsPlayer = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

        // 1. Hiển thị Tên & Level
        if (textTenCanhGioi != null) textTenCanhGioi.text = tenCanhGioiMoi;

        bool duLevel = statsPlayer.level >= levelYeuCau;
        if (textLevelYeuCau != null)
        {
            string mauLevel = duLevel ? "<color=green>" : "<color=red>";
            textLevelYeuCau.text = $"Level Yêu Cầu: {mauLevel}{statsPlayer.level}/{levelYeuCau}</color>";
        }

        // 2. Kiểm tra Item trong Kho
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

                    int soLuongDangCo = QuestSaveSystem.Instance.LaySoLuongItemTrongKho(yeuCau.itemData.idItem);
                    bool duItem = soLuongDangCo >= yeuCau.soLuongYeuCau;
                    if (!duItem) duToanBoItem = false;

                    string mauItem = duItem ? "<color=green>" : "<color=red>";
                    danhSachSlotUI[i].textSoLuong.text = $"{mauItem}{soLuongDangCo}/{yeuCau.soLuongYeuCau}</color>";
                }
            }
            else
            {
                // Slot thừa thì ẩn đi
                danhSachSlotUI[i].imageIcon.gameObject.SetActive(false);
                danhSachSlotUI[i].textSoLuong.text = "";
            }
        }

        // 3. Khóa / Mở Nút Đột Phá
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

    /// <summary>
    /// Gán trực tiếp vào Sự kiện OnClick() của Nút Đột Phá trên Bảng này
    /// </summary>
    public void OnClickThucHienDotPha()
    {
        PlayerStatsSaveData statsPlayer = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

        // 1. Trừ Item
        foreach (var yeuCau in danhSachItemYeuCau)
        {
            if (yeuCau.itemData != null)
            {
                QuestSaveSystem.Instance.LuuItemVaoSaveGame(yeuCau.itemData.idItem, -yeuCau.soLuongYeuCau);
            }
        }

        // 2. Tăng Chỉ Số
        statsPlayer.tenCanhGioi = tenCanhGioiMoi;
        statsPlayer.damage += congDamage;
        statsPlayer.maxHP += congMaxHP;
        statsPlayer.armor += congArmor;

        // 3. Lưu Save File JSON
        QuestSaveSystem.Instance.SaveDuLieuQuestToTxt();

        // 4. Refresh UI
        CapNhatGiaoDienBang();

        PlayerStatsUI statsUI = FindFirstObjectByType<PlayerStatsUI>();
        if (statsUI != null) statsUI.CapNhatGiaoDienChiSo();

        Debug.Log($"<color=green>[Đột Phá Success]</color> Đã nâng cảnh giới lên {tenCanhGioiMoi}");
    }
}