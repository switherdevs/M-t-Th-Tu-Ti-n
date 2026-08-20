using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("--- QUẢN LÝ BẬT / TẮT KHO ĐỒ ---")]
    [Tooltip("Kéo Panel UI chứa toàn bộ Kho đồ vào đây")]
    public GameObject panelKhoDo;
    public KeyCode phimBatTat = KeyCode.Tab;

    [Header("--- QUẢN LÝ BẢNG CỐ ĐỊNH ---")]
    [Tooltip("Panel khung viền của bảng thông tin (Nếu không dùng thì để trống)")]
    public GameObject panelBangCoDinh;
    public TextMeshProUGUI textTenItemCoDinh;
    public TextMeshProUGUI textCanhGioiCoDinh;
    public TextMeshProUGUI textMoTaCoDinh;

    [Header("--- DANH SÁCH 36 Ô UI ---")]
    public InventorySlotUI[] danhSach36SlotUI;

    [Header("--- CƠ SỞ DỮ LIỆU ITEM ---")]
    public List<ItemData> cacItemDataTrongGame;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ban đầu xóa sạch chữ trên bảng cố định
        AnThongTinBangCoDinh();

        // Mặc định ẩn kho đồ khi vừa vào game
        if (panelKhoDo != null)
        {
            panelKhoDo.SetActive(false);
        }

        // Tải dữ liệu từ File Save ngay khi vào Game
        TaiDuLieuTuSaveGame();
    }

    private void Update()
    {
        // Bắt sự kiện nhấn phím TAB để Đóng/Mở Kho đồ
        if (Input.GetKeyDown(phimBatTat))
        {
            ToggleKhoDo();
        }
    }

    public void ToggleKhoDo()
    {
        if (panelKhoDo != null)
        {
            bool trangThaiHienTai = panelKhoDo.activeSelf;
            bool trangThaiMoi = !trangThaiHienTai;
            panelKhoDo.SetActive(trangThaiMoi);

            if (trangThaiMoi)
            {
                // Khi mở kho đồ lên thì tải/cập nhật lại dữ liệu mới nhất từ File Save
                TaiDuLieuTuSaveGame();
            }
            else
            {
                AnThongTinBangCoDinh();
            }
        }
    }

    /// <summary>
    /// Hàm đọc danh sách Item từ QuestSaveSystem và hiển thị lên các ô UI
    /// </summary>
    public void TaiDuLieuTuSaveGame()
    {
        // 1. Xóa sạch dữ liệu hiển thị cũ trên 36 ô UI
        if (danhSach36SlotUI != null)
        {
            for (int i = 0; i < danhSach36SlotUI.Length; i++)
            {
                if (danhSach36SlotUI[i] != null)
                {
                    danhSach36SlotUI[i].XoaRongO();
                }
            }
        }

        // 2. Kiểm tra Save System
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogWarning("[InventoryManager] Chưa tìm thấy dữ liệu QuestSaveSystem!");
            return;
        }

        List<SaveItemData> danhSachSave = QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachItemSave;
        if (danhSachSave == null || danhSachSave.Count == 0) return;

        // 3. Lặp qua danh sách item trong Save File và nạp vào từng ô UI
        int slotIndex = 0;
        for (int i = 0; i < danhSachSave.Count; i++)
        {
            if (slotIndex >= danhSach36SlotUI.Length) break;

            SaveItemData saveData = danhSachSave[i];

            ItemData dataMatch = TimItemDataTheoID(saveData.idItem);

            if (dataMatch != null && saveData.soLuong > 0)
            {
                if (danhSach36SlotUI[slotIndex] != null)
                {
                    danhSach36SlotUI[slotIndex].CapNhatOUI(dataMatch, saveData.soLuong);
                    slotIndex++;
                }
            }
        }
    }

    private ItemData TimItemDataTheoID(string id)
    {
        if (cacItemDataTrongGame == null) return null;

        for (int i = 0; i < cacItemDataTrongGame.Count; i++)
        {
            if (cacItemDataTrongGame[i] != null && cacItemDataTrongGame[i].idItem == id)
            {
                return cacItemDataTrongGame[i];
            }
        }
        return null;
    }

    public void HienThongTinBangCoDinh(ItemData data)
    {
        if (data == null) return;

        if (panelBangCoDinh != null) panelBangCoDinh.SetActive(true);

        if (textTenItemCoDinh != null)
            textTenItemCoDinh.text = $"<b>{data.tenItem}</b>";

        if (textCanhGioiCoDinh != null)
            textCanhGioiCoDinh.text = $"Cảnh giới: <color=yellow>{data.canhGioiYeuCau}</color>";

        if (textMoTaCoDinh != null)
            textMoTaCoDinh.text = data.moTaItem;
    }

    public void AnThongTinBangCoDinh()
    {
        if (textTenItemCoDinh != null) textTenItemCoDinh.text = "";
        if (textCanhGioiCoDinh != null) textCanhGioiCoDinh.text = "";
        if (textMoTaCoDinh != null) textMoTaCoDinh.text = "";

        if (panelBangCoDinh != null && panelBangCoDinh != panelKhoDo)
        {
            panelBangCoDinh.SetActive(false);
        }
    }

    /// <summary>
    /// HÀM ĐÃ SỬA TRIỆT ĐỂ: KHÔNG TRỪ SỐ LƯỢNG KHI CLICK
    /// </summary>
    public void SuDungItem(InventorySlotUI slot)
    {
        ItemData item = slot.itemDataHienTai;
        if (item == null) return;

        // CẤM TRỪ SỐ LƯỢNG: Đã xóa hoàn toàn đoạn code "slot.soLuongHienTai--"
        Debug.Log($"[Kho Đồ] Đã click vào vật phẩm: {item.tenItem} (Số lượng hiện tại giữ nguyên: {slot.soLuongHienTai})");
    }
}