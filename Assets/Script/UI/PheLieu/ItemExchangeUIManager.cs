using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Cấu trúc khai báo 1 dòng trao đổi trong danh sách mảng
[Serializable]
public class ExchangeUIElement
{
    [Header("--- THÔNG TIN NGUYÊN LIỆU (CẦN TRAO ĐỔI) ---")]
    public ItemData itemCanDoi;
    public int soLuongCanDoi = 1;

    [Header("--- THÔNG TIN THÀNH PHẨM (ITEM FINAL) ---")]
    public ItemData itemFinal;
    public int soLuongFinal = 1;

    [Header("--- UI DÒNG TRAO ĐỔI (LIST BUTTON) ---")]
    public TextMeshProUGUI textTenTraoDoi;
    public Image imageIconMinhHoa;
    public Button nutChonTraoDoi;
}

// Cấu trúc ô hiển thị Slot Item trong khu vực dùng chung
[Serializable]
public class ItemSlotUI
{
    public Image imageIcon;
    public TextMeshProUGUI textTenItem;
    public TextMeshProUGUI textSoLuong;
}

public class ItemExchangeUIManager : MonoBehaviour
{
    public static ItemExchangeUIManager Instance;

    [Header("--- DANH SÁCH MẢNG CÁC DÒNG TRAO ĐỔI ---")]
    public List<ExchangeUIElement> danhSachTraoDoiUI = new List<ExchangeUIElement>();

    [Header("--- KHU VỰC HIỂN THỊ DÙNG CHUNG (CHI TIẾT TRAO ĐỔI) ---")]
    [Tooltip("Ô hiển thị Item nguyên liệu cần trao đổi")]
    public ItemSlotUI slotItemCanDoi;

    [Tooltip("Ô hiển thị Item thành phẩm (Item Final)")]
    public ItemSlotUI slotItemFinal;

    [Header("--- NÚT BẤM THỰC HIỆN TRAO ĐỔI DÙNG CHUNG ---")]
    public Button nutXacNhanDoi;
    public TextMeshProUGUI textThongBaoNut;

    private ExchangeUIElement elementDangXem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        KhoiTaoDanhSachTraoDoiUI();

        // Mặc định nạp dữ liệu phần tử đầu tiên (Index 0) lên khung dùng chung khi bắt đầu
        if (danhSachTraoDoiUI != null && danhSachTraoDoiUI.Count > 0)
        {
            HienThiChiTietTraoDoi(danhSachTraoDoiUI[0]);
        }
    }

    /// <summary>
    /// Khởi tạo và đăng ký sự kiện Click cho từng nutChonTraoDoi trên mỗi dòng
    /// </summary>
    public void KhoiTaoDanhSachTraoDoiUI()
    {
        for (int i = 0; i < danhSachTraoDoiUI.Count; i++)
        {
            ExchangeUIElement element = danhSachTraoDoiUI[i];

            if (element != null)
            {
                // Hiển thị tên dòng
                if (element.textTenTraoDoi != null && element.itemFinal != null)
                {
                    element.textTenTraoDoi.text = $"Đổi {element.itemFinal.tenItem}";
                }

                // Hiển thị Icon minh họa trên dòng nút bấm
                if (element.imageIconMinhHoa != null && element.itemFinal != null)
                {
                    element.imageIconMinhHoa.sprite = element.itemFinal.iconItem;
                }

                // Gán sự kiện OnClick cho nutChonTraoDoi của dòng
                if (element.nutChonTraoDoi != null)
                {
                    element.nutChonTraoDoi.onClick.RemoveAllListeners();

                    ExchangeUIElement targetElement = element;
                    element.nutChonTraoDoi.onClick.AddListener(() =>
                    {
                        HienThiChiTietTraoDoi(targetElement);
                    });
                }
            }
        }
    }

    /// <summary>
    /// Thay đổi toàn bộ hình ảnh, text tên và số lượng trên khu vực dùng chung theo dòng được chọn
    /// </summary>
    public void HienThiChiTietTraoDoi(ExchangeUIElement element)
    {
        if (element == null || element.itemCanDoi == null || element.itemFinal == null) return;

        elementDangXem = element;

        // 1. GÁN THÔNG TIN ITEM CẦN TRAO ĐỔI VÀO KHUNG CHUNG
        if (slotItemCanDoi != null)
        {
            if (slotItemCanDoi.imageIcon != null) slotItemCanDoi.imageIcon.sprite = element.itemCanDoi.iconItem;
            if (slotItemCanDoi.textTenItem != null) slotItemCanDoi.textTenItem.text = element.itemCanDoi.tenItem;

            // Đọc số lượng item hiện có từ QuestSaveSystem
            int soLuongDangCo = QuestSaveSystem.Instance != null
                ? QuestSaveSystem.Instance.LaySoLuongItemTrongKho(element.itemCanDoi.idItem)
                : 0;

            bool duItem = soLuongDangCo >= element.soLuongCanDoi;
            string mauText = duItem ? "<color=green>" : "<color=red>";

            if (slotItemCanDoi.textSoLuong != null)
            {
                slotItemCanDoi.textSoLuong.text = $"{mauText}{soLuongDangCo}/{element.soLuongCanDoi}</color>";
            }
        }

        // 2. GÁN THÔNG TIN ITEM FINAL (SẢN PHẨM) VÀO KHUNG CHUNG
        if (slotItemFinal != null)
        {
            if (slotItemFinal.imageIcon != null) slotItemFinal.imageIcon.sprite = element.itemFinal.iconItem;
            if (slotItemFinal.textTenItem != null) slotItemFinal.textTenItem.text = element.itemFinal.tenItem;
            if (slotItemFinal.textSoLuong != null) slotItemFinal.textSoLuong.text = $"x{element.soLuongFinal}";
        }

        // 3. KIỂM TRA ĐIỀU KIỆN ĐỂ KÍCH HOẠT NÚT ĐỔI DÙNG CHUNG
        bool duDieuKien = ItemExchangeManager.Instance != null
            && ItemExchangeManager.Instance.KiemTraDuItemDoi(element.itemCanDoi, element.soLuongCanDoi);

        if (nutXacNhanDoi != null)
        {
            nutXacNhanDoi.interactable = duDieuKien;
        }

        if (textThongBaoNut != null)
        {
            textThongBaoNut.text = duDieuKien ? "XÁC NHẬN ĐỔI" : "THIẾU ITEM";
        }
    }

    /// <summary>
    /// Sự kiện bấm nút "Xác Nhận Đổi" dùng chung
    /// </summary>
    public void OnClickXacNhanTraoDoi()
    {
        if (elementDangXem == null || ItemExchangeManager.Instance == null) return;

        bool thanhCong = ItemExchangeManager.Instance.ThucHienTraoDoi(
            elementDangXem.itemCanDoi,
            elementDangXem.soLuongCanDoi,
            elementDangXem.itemFinal,
            elementDangXem.soLuongFinal
        );

        if (thanhCong)
        {
            // Cập nhật lại UI thông tin số lượng ngay sau khi đổi thành công
            HienThiChiTietTraoDoi(elementDangXem);
        }
    }
}