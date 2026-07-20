using InventorySystem.UI;
using UnityEngine;

namespace InventorySystem
{
    [SelectionBase]
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Config")]
        [SerializeField] private ItemData itemData;
        [SerializeField, Min(1)] private int quantity = 1;

        [Header("Juice & FX (Unity 6)")]
        [SerializeField, Tooltip("Âm thanh phát ra khi nhặt vật phẩm này")]
        private AudioClip pickupSound;

        [SerializeField, Tooltip("Prefab hiệu ứng (Particle, Pop up...) sinh ra khi nhặt")]
        private GameObject pickupEffectPrefab;

        public ItemData ItemData => itemData;
        public int Quantity => quantity;

        private void OnValidate()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && itemData != null && itemData.Icon != null)
            {
                spriteRenderer.sprite = itemData.Icon;
            }
        }

        /// <summary>
        /// Hàm xử lý khi vật phẩm được nhặt thành công
        /// </summary>
        public void OnPickedUp()
        {
            // 1. Xử lý Âm thanh độc lập (Không sợ bị ngắt khi Object bị Destroy)
            if (pickupSound != null)
            {
                // PlayClipAtPoint tự tạo ra một AudioSource tạm thời tại vị trí nhặt và tự xóa khi chạy xong
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 2. Xử lý Hiệu ứng hình ảnh
            if (pickupEffectPrefab != null)
            {
                GameObject fx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                // Nếu là Particle System, tự động hủy sau khi chạy xong để tránh rác bộ nhớ
                if (fx.TryGetComponent<ParticleSystem>(out var particle))
                {
                    Destroy(fx, particle.main.duration + particle.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(fx, 2f); // Mặc định hủy sau 2 giây nếu là prefab thường
                }
            }

            Debug.Log($"<color=yellow>[World]</color> Auto-picked up {itemData.ItemName} x{quantity}.");
            Destroy(gameObject); // Hủy vật phẩm trên scene
        }
    }
}