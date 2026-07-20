using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace InventorySystem.UI
{
    // Kế thừa các Interface Pointer để bắt sự kiện chuột mà không dùng hàm Update tốn hiệu năng
    public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("=== UI COMPONENTS ===")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;

        private ItemData currentItemData;

        /// <summary>
        /// Cập nhật hiển thị giao diện dựa trên dữ liệu thật của Slot dữ liệu truyền vào
        /// </summary>
        public void UpdateSlotUI(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty())
            {
                currentItemData = null;
                iconImage.sprite = null;
                iconImage.enabled = false; // Tắt Image đi khi ô trống để tối ưu Overdraw UI
                countText.text = string.Empty;
                return;
            }

            currentItemData = slot.ItemData;
            iconImage.sprite = currentItemData.Icon;
            iconImage.enabled = true;

            // Nếu số lượng > 1 và vật phẩm dồn được thì hiện số, ngược lại ẩn đi
            if (currentItemData.IsStackable && slot.Quantity > 1)
            {
                countText.text = slot.Quantity.ToString();
            }
            else
            {
                countText.text = string.Empty;
            }
        }

        #region MOUSE HOVER EVENTS (INTERFACE IMPLEMENTATION)
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItemData != null && TooltipUI.Instance != null)
            {
                TooltipUI.Instance.ShowTooltip(currentItemData.ItemName, currentItemData.Description);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipUI.Instance != null)
            {
                TooltipUI.Instance.HideTooltip();
            }
        }

        private void OnDisable()
        {
            // Tránh lỗi Tooltip bị kẹt dính khi người chơi bấm Tab đóng kho đồ lúc đang hover chuột
            if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
        }
        #endregion
    }
}