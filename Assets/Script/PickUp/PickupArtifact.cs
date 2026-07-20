using UnityEngine;

namespace InventorySystem.UI
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupArtifact : MonoBehaviour
    {
        [Header("=== ARTIFACT DATA ===")]
        [SerializeField] private ItemData itemData;
        [SerializeField, Min(1)] private int quantity = 1;

        // Kích hoạt khi Player đi chạm vào Collider của Cổ Vật này
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Bạn có thể check Tag "Player" hoặc check Layer của nhân vật tại đây
            if (collision.CompareTag("Player"))
            {
                if (InventoryManager.Instance != null)
                {
                    // Thực hiện thêm vào dữ liệu core
                    bool isPickedUp = InventoryManager.Instance.AddItem(itemData, quantity);

                    if (isPickedUp)
                    {
                        Debug.Log($"Đã nhặt thành công Cổ Vật: {itemData.ItemName} x{quantity}");
                        Destroy(gameObject); // Xóa vật phẩm khỏi map sau khi nhặt thành công
                    }
                }
            }
        }
    }
}