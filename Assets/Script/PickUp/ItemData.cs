using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory System/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for database and serialization sync.")]
        private string id;

        [SerializeField] private string itemName;
        [SerializeField, TextArea(3, 5)] private string description;

        [Header("Visuals & Prefabs")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject worldPrefab;

        [Header("Properties")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemRarity rarity;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStack = 99;

        // Các thuộc tính đóng gói (Encapsulation) - Chỉ Read-only từ bên ngoài
        public string Id => id;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;
        public ItemType ItemType => itemType;
        public ItemRarity Rarity => rarity;
        public bool IsStackable => isStackable;
        public int MaxStack => isStackable ? Mathf.Max(1, maxStack) : 1;

        // Tự động generate ID nếu dev quên điền trong Inspector
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
        }
    }
}