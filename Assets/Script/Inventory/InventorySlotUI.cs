using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("--- UI THÀNH PHẦN TRONG Ô ---")]
    public Image imageIcon;
    public TextMeshProUGUI textSoLuong;

    [HideInInspector] public ItemData itemDataHienTai;
    [HideInInspector] public int soLuongHienTai;

    public void CapNhatOUI(ItemData data, int soLuong)
    {
        itemDataHienTai = data;
        soLuongHienTai = soLuong;

        if (itemDataHienTai != null && soLuongHienTai > 0)
        {
            if (imageIcon != null)
            {
                imageIcon.gameObject.SetActive(true);
                imageIcon.sprite = itemDataHienTai.iconItem;
            }

            CapNhatGiaoDienSoLuong();
        }
        else
        {
            XoaRongO();
        }
    }

    /// <summary>
    /// Hàm trực tiếp cập nhật hiển thị số lượng chữ trên UI
    /// </summary>
    public void CapNhatGiaoDienSoLuong()
    {
        if (textSoLuong != null)
        {
            textSoLuong.text = soLuongHienTai > 1 ? soLuongHienTai.ToString() : "";
        }
    }

    public void XoaRongO()
    {
        itemDataHienTai = null;
        soLuongHienTai = 0;
        if (imageIcon != null) imageIcon.gameObject.SetActive(false);
        if (textSoLuong != null) textSoLuong.text = "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemDataHienTai != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.HienThongTinBangCoDinh(itemDataHienTai);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AnThongTinBangCoDinh();
        }
    }

    /// <summary>
    /// HÀM ĐÃ SỬA TRIỆT ĐỂ: Gọi sang Manager mà không can thiệp số lượng
    /// </summary>
    public void OnClickDungItem()
    {
        if (itemDataHienTai != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SuDungItem(this);
        }
    }
}