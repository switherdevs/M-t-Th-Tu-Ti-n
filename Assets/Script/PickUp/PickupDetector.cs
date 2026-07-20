using UnityEngine;
using InventorySystem.UI;

namespace InventorySystem
{
    public class PickupDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Inventory playerInventory;

        [Header("Settings")]
        [SerializeField, Tooltip("Dùng layer để lọc chính xác Item, tối ưu hiệu năng")]
        private LayerMask itemLayer;

        private void Awake()
        {
            if (playerInventory == null)
            {
                playerInventory = GetComponentInParent<Inventory>();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Kiểm tra xem đối tượng va chạm có thuộc Layer Item hay không
            if (((1 << collision.gameObject.layer) & itemLayer) != 0)
            {
                if (collision.TryGetComponent<ItemPickup>(out var item))
                {
                    ProcessAutoPickup(item);
                }
            }
        }

        private void ProcessAutoPickup(ItemPickup item)
        {
            if (item == null || item.ItemData == null) return;

            // Tiến hành thêm vào túi đồ của Player
            bool success = playerInventory.AddItem(item.ItemData, item.Quantity);

            if (success)
            {
                // Nếu thêm thành công, kích hoạt âm thanh, hiệu ứng và xóa item ngoài thế giới
                item.OnPickedUp();
            }
            else
            {
                Debug.LogWarning($"<color=red>[Inventory]</color> Cannot pickup {item.ItemData.ItemName}. Inventory is full!");
            }
        }
    }
}