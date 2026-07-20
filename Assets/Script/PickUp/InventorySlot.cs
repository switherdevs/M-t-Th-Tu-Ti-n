using System;
using UnityEngine;

// ĐẢM BẢO CHÍNH XÁC NAMESPACE NÀY
namespace InventorySystem.UI
{
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity;

        public ItemData ItemData => itemData;
        public int Quantity => quantity;

        public InventorySlot()
        {
            Clear();
        }

        public bool IsEmpty() => itemData == null || quantity <= 0;

        public bool CanStackWith(ItemData newItem, out int remainingSpace)
        {
            remainingSpace = 0;
            if (IsEmpty())
            {
                remainingSpace = newItem.MaxStack;
                return true;
            }

            if (itemData.Id == newItem.Id && itemData.IsStackable && quantity < itemData.MaxStack)
            {
                remainingSpace = itemData.MaxStack - quantity;
                return true;
            }

            return false;
        }

        // Đảm bảo có hàm này với phạm vi truy cập là public
        public void Add(ItemData newItem, int amount)
        {
            if (IsEmpty())
            {
                itemData = newItem;
                quantity = amount;
                return;
            }

            if (itemData.Id == newItem.Id)
            {
                quantity = Mathf.Min(quantity + amount, itemData.MaxStack);
            }
        }

        // Đảm bảo có hàm này với phạm vi truy cập là public
        public void Remove(int amount)
        {
            if (IsEmpty()) return;

            quantity -= amount;
            if (quantity <= 0)
            {
                Clear();
            }
        }

        public void Clear()
        {
            itemData = null;
            quantity = 0;
        }
    }
}