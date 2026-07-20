using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class PickupDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Inventory playerInventory;

        [Header("Settings")]
        [SerializeField] private KeyCode pickupKey = KeyCode.F;
        [SerializeField, Tooltip("Dùng layer để lọc chính xác Item, tối ưu hiệu năng Collision")]
        private LayerMask itemLayer;

        // Danh sách lưu các Item đang nằm trong tầm nhặt
        private readonly List<ItemPickup> itemsInRange = new();
        private ItemPickup closestItem;

        private void Awake()
        {
            if (playerInventory == null)
            {
                playerInventory = GetComponentInParent<Inventory>();
            }
        }

        private void Update()
        {
            TargetClosestItem();
            HandlePickupInput();
        }

        private void TargetClosestItem()
        {
            if (itemsInRange.Count == 0)
            {
                if (closestItem != null)
                {
                    closestItem = null;
                    HidePickupUI();
                }
                return;
            }

            ItemPickup newClosest = null;
            float minDistance = float.MaxValue;
            Vector2 currentPosition = transform.position;

            // Loại bỏ các phần tử Null (nếu bị hủy bởi hệ thống khác) giải quyết bài toán bộ nhớ rác
            for (int i = itemsInRange.Count - 1; i >= 0; i--)
            {
                if (itemsInRange[i] == null)
                {
                    itemsInRange.RemoveAt(i);
                    continue;
                }

                float distance = Vector2.SqrMagnitude((Vector2)itemsInRange[i].transform.position - currentPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    newClosest = itemsInRange[i];
                }
            }

            if (newClosest != closestItem)
            {
                closestItem = newClosest;
                if (closestItem != null)
                {
                    ShowPickupUI(closestItem);
                }
            }
        }

        private void HandlePickupInput()
        {
            if (closestItem == null) return;

            if (Input.GetKeyDown(pickupKey))
            {
                // Thực hiện add vào inventory của Player trước
                bool success = playerInventory.AddItem(closestItem.ItemData, closestItem.Quantity);

                if (success)
                {
                    ItemPickup itemToPickup = closestItem;
                    itemsInRange.Remove(itemToPickup);
                    closestItem = null;
                    HidePickupUI();

                    itemToPickup.OnPickedUp();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Kiểm tra LayerMask tối ưu
            if (((1 << collision.gameObject.layer) & itemLayer) != 0)
            {
                if (collision.TryGetComponent<ItemPickup>(out var item))
                {
                    if (!itemsInRange.Contains(item))
                    {
                        itemsInRange.Add(item);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & itemLayer) != 0)
            {
                if (collision.TryGetComponent<ItemPickup>(out var item))
                {
                    if (itemsInRange.Contains(item))
                    {
                        itemsInRange.Remove(item);
                        if (closestItem == item)
                        {
                            closestItem = null;
                            HidePickupUI();
                        }
                    }
                }
            }
        }

        private void ShowPickupUI(ItemPickup item)
        {
            // Log mô phỏng thay thế UI thật
            Debug.Log($"<color=orange>[UI Prompt]</color> Press {pickupKey} to Pick Up: {item.ItemData.ItemName} (x{item.Quantity})");
        }

        private void HidePickupUI()
        {
            Debug.Log("<color=orange>[UI Prompt]</color> Clear Prompt UI.");
        }

        // Vẽ Gizmos hiển thị bán kính quét trong Scene View của Editor
        private void OnDrawGizmosSelected()
        {
            var triggerCollider = GetComponent<CircleCollider2D>();
            if (triggerCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, triggerCollider.radius);
            }
        }
    }
}