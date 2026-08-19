using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Thư viện bắt sự kiện chuột
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("--- UI THÀNH PHẦN TRONG Ô ---")]
    public Image imageIcon;             // Ảnh icon vật phẩm
    public TextMeshProUGUI textSoLuong; // Text hiển thị số lượng

    [HideInInspector] public ItemData itemDataHienTai;
    [HideInInspector] public int soLuongHienTai;

    /// <summary>
    /// Hàm cập nhật giao diện của riêng ô này
    /// </summary>
    public void CapNhatOUI(ItemData data, int soLuong)
    {
        itemDataHienTai = data;
        soLuongHienTai = soLuong;

        if (itemDataHienTai != null && soLuongHienTai > 0)
        {
            imageIcon.gameObject.SetActive(true);
            imageIcon.sprite = itemDataHienTai.iconItem;

            // Nếu số lượng > 1 mới hiện số, ngược lại ẩn đi cho đẹp
            textSoLuong.text = soLuongHienTai > 1 ? soLuongHienTai.ToString() : "";
        }
        else
        {
            // Ô trống
            XoaRongO();
        }
    }

    public void XoaRongO()
    {
        itemDataHienTai = null;
        soLuongHienTai = 0;
        if (imageIcon != null) imageIcon.gameObject.SetActive(false);
        if (textSoLuong != null) textSoLuong.text = "";
    }

    // --- SỰ KIỆN RÊ CHUỘT VÀO Ô ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemDataHienTai != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.HienThongTinBangCoDinh(itemDataHienTai);
        }
    }

    // --- SỰ KIỆN RÊ CHUỘT RA KHỎI Ô ---
    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AnThongTinBangCoDinh();
        }
    }

    // --- SỰ KIỆN CLICK CHUỘT DÙNG ITEM ---
    public void OnClickDungItem()
    {
        if (itemDataHienTai != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SuDungItem(this);
        }
    }
}