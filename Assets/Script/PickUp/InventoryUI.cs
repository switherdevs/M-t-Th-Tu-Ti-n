using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Đã thêm để nhận dạng Input Action Asset chuẩn

namespace InventorySystem.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("=== INPUT SYSTEM (NEW) ===")]
        [SerializeField] private InputActionReference toggleInventoryAction; // Kéo Action Asset vào đây

        [Header("=== VISUAL REFERENCES ===")]
        [SerializeField] private GameObject inventoryPanel; // Object cha chứa toàn bộ UI kho đồ
        [SerializeField] private Transform gridContainer;   // Object chứa component Grid Layout Group
        [SerializeField] private GameObject slotPrefab;     // Prefab của một ô Slot UI

        private List<InventoryItemUI> uiSlotsList = new List<InventoryItemUI>();
        private bool isInventoryOpen = false;

        // Các Event thông báo trạng thái điều khiển Player ra GameManager (Decoupling)
        public static event Action<bool> OnInventoryToggleState;

        private void OnEnable()
        {
            // Đăng ký sự kiện lắng nghe phím Tab từ Input Action
            if (toggleInventoryAction != null)
            {
                toggleInventoryAction.action.Enable();
                toggleInventoryAction.action.performed += OnToggleInventoryPressed;
            }
        }

        private void OnDisable()
        {
            // Hủy đăng ký để tránh lỗi bộ nhớ
            if (toggleInventoryAction != null)
            {
                toggleInventoryAction.action.performed -= OnToggleInventoryPressed;
                toggleInventoryAction.action.Disable();
            }
        }

        private void Start()
        {
            // Đợi InventoryManager khởi tạo xong dữ liệu thì tiến hành liên kết sinh UI ban đầu
            if (InventoryManager.Instance != null)
            {
                GenerateGridUI();
                InventoryManager.Instance.OnSlotChanged += UpdateSpecificSlot;
            }

            // Mặc định ban đầu vào game ẩn kho đồ đi
            CloseInventory();
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnSlotChanged -= UpdateSpecificSlot;
            }
        }

        // Lắng nghe phím từ Input Action Asset thay thế hoàn toàn cho hàm Update() cũ
        private void OnToggleInventoryPressed(InputAction.CallbackContext context)
        {
            Debug.Log("Đã bấm nút Tab thành công bằng Input Action Asset!");
            ToggleInventory();
        }

        #region TOGGLE WINDOW LOGIC
        private void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;

            if (isInventoryOpen)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }

        private void OpenInventory()
        {
            inventoryPanel.SetActive(true);

            // Hiện chuột và mở khóa cho di chuyển tự do trong UI
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Phát Event báo hiệu cho GameManager khóa hệ thống di chuyển/bắn súng của Player
            OnInventoryToggleState?.Invoke(true);
        }

        private void CloseInventory()
        {
            inventoryPanel.SetActive(false);

            // Ẩn chuột đi và khóa tâm lại
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Phát Event báo hiệu trả lại quyền điều khiển tự do cho Player
            OnInventoryToggleState?.Invoke(false);
        }
        #endregion

        #region GRID DRAW & RE-RENDER OPTIMIZATION
        private void GenerateGridUI()
        {
            // Xóa sạch các object con cũ nếu có trong Container phòng hờ
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }
            uiSlotsList.Clear();

            int totalSlotsCount = InventoryManager.Instance.InventorySize;
            var dataSlots = InventoryManager.Instance.Slots;

            // Khởi tạo một số lượng Slot UI cố định bằng đúng kích thước InventoryData ban đầu
            for (int i = 0; i < totalSlotsCount; i++)
            {
                GameObject newSlotObj = Instantiate(slotPrefab, gridContainer);
                InventoryItemUI itemUI = newSlotObj.GetComponent<InventoryItemUI>();

                // Cập nhật trạng thái hiển thị ảnh/số lượng ban đầu
                itemUI.UpdateSlotUI(dataSlots[i]);
                uiSlotsList.Add(itemUI);
            }
        }

        /// <summary>
        /// Hàm Tối Ưu Hiệu Suất: Chỉ render lại đúng ô Slot có chỉ số thay đổi, không vẽ lại cả bảng Grid!
        /// </summary>
        private void UpdateSpecificSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < uiSlotsList.Count)
            {
                uiSlotsList[slotIndex].UpdateSlotUI(InventoryManager.Instance.Slots[slotIndex]);
            }
        }
        #endregion
    }
}