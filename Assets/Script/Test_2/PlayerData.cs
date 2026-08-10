using System;
using System.Collections.Generic;

namespace PersistenceSystem
{
    [Serializable]
    public class SavedSlotData
    {
        public int SlotIndex;
        public string ItemSOPath;
        public int Amount;

        public SavedSlotData(int slotIndex, string itemSOPath, int amount)
        {
            SlotIndex = slotIndex;
            ItemSOPath = itemSOPath;
            Amount = amount;
        }
    }

    [Serializable]
    public class PlayerData
    {
        // 1. Dữ liệu Kho đồ (Inventory)
        public List<SavedSlotData> InventorySlots = new List<SavedSlotData>();

        // 2. Dữ liệu Level & EXP (Khai báo biến chuẩn cho JsonUtility)
        public int PlayerLevel = 1;
        public float CurrentExp = 0f;
        public float MaxExp = 5f;

        // 3. Dữ liệu Chỉ số Nhân vật (Stats)
        
    }
}