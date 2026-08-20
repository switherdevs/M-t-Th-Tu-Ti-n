using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class InventorySaveData
{
    public string idItem;
    public int soLuong;
}

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
            panelKhoDo.SetActive(!trangThaiHienTai);

            if (trangThaiHienTai)
            {
                AnThongTinBangCoDinh();
            }
        }
    }

    /// <summary>
    /// Hiện chữ thông tin item lên Bảng Cố Định khi rê chuột vào ô
    /// </summary>
    public void HienThongTinBangCoDinh(ItemData data)
    {
        if (data == null) return;

        // Nếu có kéo Panel khung viền thì mới bật Panel lên
        if (panelBangCoDinh != null) panelBangCoDinh.SetActive(true);

        if (textTenItemCoDinh != null)
            textTenItemCoDinh.text = $"<b>{data.tenItem}</b>";

        if (textCanhGioiCoDinh != null)
            textCanhGioiCoDinh.text = $"Cảnh giới: <color=yellow>{data.canhGioiYeuCau}</color>";

        if (textMoTaCoDinh != null)
            textMoTaCoDinh.text = data.moTaItem;
    }

    /// <summary>
    /// Xóa chữ khi rời chuột khỏi ô (AN TOÀN: Không dùng SetActive false để tránh tắt nhầm Kho đồ)
    /// </summary>
    public void AnThongTinBangCoDinh()
    {
        // CHỈ XÓA CHỮ, KHÔNG ẨN PANEL ĐỂ TRÁNH BỊ ẨN KHO ĐỒ KHI KÉO NHẦM INSPECTOR
        if (textTenItemCoDinh != null) textTenItemCoDinh.text = "";
        if (textCanhGioiCoDinh != null) textCanhGioiCoDinh.text = "";
        if (textMoTaCoDinh != null) textMoTaCoDinh.text = "";

        // Nếu bạn tạo riêng 1 Panel khung nhỏ cho bảng cố định thì mới ẩn nó
        if (panelBangCoDinh != null && panelBangCoDinh != panelKhoDo)
        {
            panelBangCoDinh.SetActive(false);
        }
    }

    public void SuDungItem(InventorySlotUI slot)
    {
        ItemData item = slot.itemDataHienTai;
        if (item == null) return;

        CanhGioiYeuCau canhGioiPlayerHienTai = CanhGioiYeuCau.PhamNhan;

        if ((int)canhGioiPlayerHienTai < (int)item.canhGioiYeuCau)
        {
            Debug.LogWarning($"[Kho Đồ] Chưa đủ cảnh giới! Yêu cầu: {item.canhGioiYeuCau}");
            return;
        }

        slot.soLuongHienTai--;

        if (slot.soLuongHienTai <= 0)
        {
            slot.XoaRongO();
            AnThongTinBangCoDinh();
        }
        else
        {
            slot.CapNhatGiaoDienSoLuong();
        }

        Debug.Log($"[Kho Đồ] Đã dùng 1 món: {item.tenItem}");
    }
}