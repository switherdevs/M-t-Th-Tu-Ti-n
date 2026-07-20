using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Min(1)] private int inventorySize = 24;

        [Header("Runtime State")]
        [SerializeField] private List<InventorySlot> slots;

        // Event thông báo cho các hệ thống UI hoặc Save/Load cập nhật mà không gây phụ thuộc ngược
        public event Action OnInventoryChanged;

        public int Size => inventorySize;
        public IReadOnlyList<InventorySlot> Slots => slots;

        private void Awake()
        {
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

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            int amountLeftToStack = amount;

            // Bước 1: Tìm kiếm các slot đang có sẵn item cùng loại để nhét vào stack trước
            if (item.IsStackable)
            {
                foreach (var slot in slots)
                {
                    if (slot.CanStackWith(item, out int remainingSpace))
                    {
                        int addedAmount = Mathf.Min(amountLeftToStack, remainingSpace);
                        slot.Add(item, addedAmount);
                        amountLeftToStack -= addedAmount;

                        Debug.Log($"<color=cyan>[Inventory]</color> Added {addedAmount} {item.ItemName} to existing stack.");

                        if (amountLeftToStack <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            // Bước 2: Nếu vẫn còn dư item, tìm slot trống mới để xếp vào
            while (amountLeftToStack > 0)
            {
                InventorySlot emptySlot = FindEmptySlot();
                if (emptySlot == null)
                {
                    Debug.LogWarning($"<color=red>[Inventory]</color> Inventory Full! Could not fit remaining {amountLeftToStack}x {item.ItemName}.");
                    OnInventoryChanged?.Invoke();
                    return false; // Trả về false nhưng không làm mất lượng đã nhặt thành công ở bước 1
                }

                int addedAmount = Mathf.Min(amountLeftToStack, item.MaxStack);
                emptySlot.Add(item, addedAmount);
                amountLeftToStack -= addedAmount;

                Debug.Log($"<color=green>[Inventory]</color> Created new stack for {addedAmount}x {item.ItemName}.");
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (!HasItem(item, amount)) return false;

            int amountToRemove = amount;
            // Xóa từ cuối danh sách lên đầu (hoặc các slot ít trước) tùy logic game, ở đây duyệt từ đầu
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty() && slot.ItemData.Id == item.Id)
                {
                    int currentQuantity = slot.Quantity;
                    int removeAmountFromThisSlot = Mathf.Min(amountToRemove, currentQuantity);

                    slot.Remove(removeAmountFromThisSlot);
                    amountToRemove -= removeAmountFromThisSlot;

                    if (amountToRemove <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemData item, int totalRequiredQuantity = 1)
        {
            return GetItemCount(item) >= totalRequiredQuantity;
        }

        public int GetItemCount(ItemData item)
        {
            if (item == null) return 0;

            int count = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty() && slot.ItemData.Id == item.Id)
                {
                    count += slot.Quantity;
                }
            }
            return count;
        }

        private InventorySlot FindEmptySlot()
        {
            return slots.Find(slot => slot.IsEmpty());
        }
    }
}