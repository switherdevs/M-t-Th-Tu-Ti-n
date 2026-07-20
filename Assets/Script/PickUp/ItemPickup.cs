using UnityEngine;

namespace InventorySystem
{
    [SelectionBase]
    [RequireComponent(typeof(Collider2D))] // Đảm bảo bắt buộc phải có Collider2D bình thường
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Config")]
        [SerializeField] private ItemData itemData;
        [SerializeField, Min(1)] private int quantity = 1;

        public ItemData ItemData => itemData;
        public int Quantity => quantity;

        private void OnValidate()
        {
            // Tự động đồng bộ Sprite Renderer của World Object theo Icon nếu có
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && itemData != null && itemData.Icon != null)
            {
                spriteRenderer.sprite = itemData.Icon;
            }

            // Đảm bảo collider trên Item không bị biến thành Trigger
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = false;
            }
        }

        public void OnPickedUp()
        {
            Debug.Log($"<color=yellow>[World]</color> Picked up {itemData.ItemName} x{quantity} from world.");
            Destroy(gameObject); // Hủy object trên Scene sau khi nhặt thành công
        }
    }
}