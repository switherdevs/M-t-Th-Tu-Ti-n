using System;
using System.Collections.Generic;
using UnityEngine;
using PersistenceSystem;

namespace InventorySystem.UI
{
    public class InventoryManager : MonoBehaviour, IPlayerSaveable
    {
        // Sử dụng Singleton hợp lý vì đây là Core Manager cần truy cập toàn cục để nhặt đồ
        public static InventoryManager Instance { get; private set; }

        [Header("=== CONFIGURATION ===")]
        [SerializeField, Min(1)] private int inventorySize = 24;

        [Header("=== RUNTIME DATA ===")]
        [SerializeField] private List<InventorySlot> slots;

        // Sự kiện thông báo khi có một Slot cụ thể thay đổi chỉ số (truyền vào Index của slot đó)
        public event Action<int> OnSlotChanged;

        public int InventorySize => inventorySize;
        public IReadOnlyList<InventorySlot> Slots => slots;

        private void Awake()
        {

            Instance = this;

            InitializeInventory();
        }

        private void InitializeInventory()
        {
            slots = new List<InventorySlot>(inventorySize);
            for (int i = 0; i < inventorySize; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        #region CORE LOGIC (ADD / REMOVE)
        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            int amountLeft = amount;

            // Bước 1: Ưu tiên dồn vào các Stack cũ đang có sẵn
            if (item.IsStackable)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].CanStackWith(item, out int remainingSpace))
                    {
                        int addedAmount = Mathf.Min(amountLeft, remainingSpace);
                        slots[i].Add(item, addedAmount);
                        amountLeft -= addedAmount;

                        OnSlotChanged?.Invoke(i); // Chỉ kích hoạt UI cập nhật đúng slot này

                        if (amountLeft <= 0) return true;
                    }
                }
            }

            // Bước 2: Điền vào ô trống đầu tiên (Tự động xếp từ trái sang phải, từ trên xuống dưới nhờ Grid)
            while (amountLeft > 0)
            {
                int emptyIndex = slots.FindIndex(s => s.IsEmpty());
                if (emptyIndex == -1)
                {
                    Debug.LogWarning("Kho đồ đã đầy! Không thể nhặt thêm.");
                    return false; // Trả về false nhưng giữ lại phần đã nhặt trước đó
                }

                int addedAmount = Mathf.Min(amountLeft, item.MaxStack);
                slots[emptyIndex].Add(item, addedAmount);
                amountLeft -= addedAmount;

                OnSlotChanged?.Invoke(emptyIndex); // Kích hoạt UI cập nhật ô trống mới điền vào
            }

            return true;
        }
        #endregion
        #region PERSISTENCE SYSTEM
        public void SaveToData(PlayerData data)
        {
            data.InventorySlots.Clear();

            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty())
                {
                    // Lấy đúng tên file ScriptableObject của Item đang có trong Slot
                    string itemFileName = slots[i].ItemData.name;
                    int itemAmount = slots[i].Quantity; // Hoặc slots[i].Amount tùy theo tên biến trong InventorySlot của bạn

                    data.InventorySlots.Add(new SavedSlotData(i, itemFileName, itemAmount));
                    Debug.Log($"[SaveInventory] Đã lưu Item: {itemFileName} x{itemAmount} ở slot {i}");
                }
            }
        }

        public void LoadFromData(PlayerData data)
        {
            InitializeInventory();

            foreach (var savedSlot in data.InventorySlots)
            {
                if (savedSlot.SlotIndex < slots.Count)
                {
                    // Tìm file ScriptableObject trong folder Assets/Resources/Items/
                    ItemData loadedItem = Resources.Load<ItemData>($"Items/{savedSlot.ItemSOPath}");

                    if (loadedItem != null)
                    {
                        slots[savedSlot.SlotIndex].Add(loadedItem, savedSlot.Amount);
                        OnSlotChanged?.Invoke(savedSlot.SlotIndex);
                        Debug.Log($"<color=yellow>[LoadInventory] Đã load lại Item: {loadedItem.name} x{savedSlot.Amount} vào slot {savedSlot.SlotIndex}</color>");
                    }
                    else
                    {
                        Debug.LogError($"[LoadInventory] KHÔNG TÌM THẤY Item Asset có tên '{savedSlot.ItemSOPath}' trong folder Assets/Resources/Items/!");
                    }
                }
            }
        }
        #endregion
    }
}