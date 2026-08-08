using System;
using System.Collections.Generic;

namespace PersistenceSystem
{
    [Serializable]
    public class PlayerData
    {
        // Stats Data
        public float CurrentHealth;
        public float BaseMaxHealth;
        public float BaseAttack;
        public float BaseDefense;

        // Inventory Data
        public List<SavedSlotData> InventorySlots = new List<SavedSlotData>();
    }

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
}