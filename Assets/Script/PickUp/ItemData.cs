using UnityEngine;

namespace InventorySystem.UI
{
    // Định nghĩa các Enum trực quan, không dùng string để check loại vật phẩm
    public enum ItemType { Equipment, Consumable, QuestItem, Material }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory System/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("=== IDENTITY ===")]
        [SerializeField] private string id;
        [SerializeField] private string itemName;
        [SerializeField, TextArea(2, 4)] private string description;

        [Header("=== VISUALS ===")]
        [SerializeField] private Sprite icon;

        [Header("=== PROPERTIES ===")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemRarity rarity;
        [SerializeField] private bool isStackable;
        [SerializeField] private int maxStack = 99;

        // Đóng gói thuộc tính (Encapsulation) để bảo vệ dữ liệu gốc khỏi bị ghi đè khi runtime
        public string Id => id;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemType ItemType => itemType;
        public ItemRarity Rarity => rarity;
        public bool IsStackable => isStackable;
        public int MaxStack => isStackable ? Mathf.Max(1, maxStack) : 1;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString();
        }
    }
}