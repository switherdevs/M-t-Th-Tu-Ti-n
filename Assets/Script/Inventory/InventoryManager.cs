using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Cấu trúc dữ liệu lưu trữ 1 ô trong Save Game
[System.Serializable]
public class InventorySaveData
{
    public string idItem;
    public int soLuong;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("--- QUẢN LÝ BẢNG CỐ ĐỊNH (RÊ CHUỘT HẠN CHẾ KHÔNG KHÍ) ---")]
    public GameObject panelBangCoDinh;          // Panel chứa bảng thông tin
    public TextMeshProUGUI textTenItemCoDinh;   // Text hiển thị Tên
    public TextMeshProUGUI textCanhGioiCoDinh; // Text hiển thị Cảnh Giới yêu cầu
    public TextMeshProUGUI textMoTaCoDinh;     // Text hiển thị Mô Tả

    [Header("--- DANH SÁCH 36 Ô UI ---")]
    public InventorySlotUI[] danhSach36SlotUI;  // Mảng chứa đúng 36 ô UI trong Canvas

    [Header("--- CƠ SỞ DỮ LIỆU ITEM ---")]
    public List<ItemData> cacItemDataTrongGame; // Kéo toàn bộ file ItemData trong project vào đây

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ban đầu ẩn hoàn toàn chữ/bảng cố định
        AnThongTinBangCoDinh();
    }

    /// <summary>
    /// Hàm hiển thị thông tin lên Bảng Cố Định khi rê chuột vào ô
    /// </summary>
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

    /// <summary>
    /// Hàm ẩn Bảng Cố Định khi rời chuột khỏi ô
    /// </summary>
    public void AnThongTinBangCoDinh()
    {
        if (panelBangCoDinh != null) panelBangCoDinh.SetActive(false);
        if (textTenItemCoDinh != null) textTenItemCoDinh.text = "";
        if (textCanhGioiCoDinh != null) textCanhGioiCoDinh.text = "";
        if (textMoTaCoDinh != null) textMoTaCoDinh.text = "";
    }

    /// <summary>
    /// Xử lý dùng Item + Kiểm tra Cảnh Giới của Player
    /// </summary>
    public void SuDungItem(InventorySlotUI slot)
    {
        ItemData item = slot.itemDataHienTai;
        if (item == null) return;

        // Giả lập lấy cảnh giới người chơi (Ví dụ: lấy từ Player/CharacterStats)
        CanhGioiYeuCau canhGioiPlayerHienTai = CanhGioiYeuCau.PhamNhan; 

        // KIỂM TRA ĐIỀU KIỆN CẢNH GIỚI:
        if ((int)canhGioiPlayerHienTai < (int)item.canhGioiYeuCau)
        {
            Debug.LogWarning($"[Kho Đồ] Chưa đủ cảnh giới! Yêu cầu: {item.canhGioiYeuCau}");
            return;
        }

        // ĐỦ CẢNH GIỚI: Tiến hành trừ số lượng (xếp chồng tối đa 64)
        slot.soLuongHienTai--;

        if (slot.soLuongHienTai <= 0)
        {
            slot.XoaRongO();
            AnThongTinBangCoDinh(); // Xóa sạch bảng nếu dùng hết item cuối cùng
        }
        else
        {
            slot.CapNhatGiaoDienUI_SoLuong(slot.soLuongHienTai);
        }

        Debug.Log($"[Kho Đồ] Đã dùng 1 món: {item.tenItem}");
    }
}

// Hàm phụ bổ sung cho slot UI
public static class SlotUIExtension
{
    public static void CapNhatGiaoDienUI_SoLuong(this InventorySlotUI slot, int soLuongMoi)
    {
        slot.soLuongHienTai = soLuongMoi;
        if (slot.textSoLuong != null)
            slot.textSoLuong.text = soLuongMoi > 1 ? soLuongMoi.ToString() : "";
    }
}