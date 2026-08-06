using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // Unity 6 / Cinemachine 3.x (Nếu dùng Cinemachine 2.x cũ, đổi thành: Cinemachine)

public class TeleportStatue2D : MonoBehaviour
{
    [Header("Cấu hình Dịch Chuyển")]
    [Tooltip("Transform của Empty GameObject đặt tại vị trí đích (Điểm B)")]
    [SerializeField] private Transform destinationB;

    [Header("Cấu hình Camera (Cinemachine)")]
    [Tooltip("Kéo Virtual Camera chính vào đây")]
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Cấu hình Giới Hạn Bản Đồ (Map Collider)")]
    [Tooltip("Collider 2D giới hạn khung hình của Map hiện tại (Map A)")]
    [SerializeField] private BoxCollider2D oldMapCollider;

    [Tooltip("Collider 2D giới hạn khung hình của Map mới (Map B)")]
    [SerializeField] private BoxCollider2D newMapCollider;

    [Header("Tùy chỉnh Thời Gian (Chỉnh trên Inspector)")]
    [Tooltip("Thời gian chờ trước khi đổi vị trí (cho hiệu ứng biến mất)")]
    [SerializeField] private float delayBeforeTeleport = 0.2f;

    [Tooltip("Thời gian chuyển đổi Collider để Camera cập nhật vùng giới hạn mới")]
    [SerializeField] private float colliderSwitchDuration = 0.2f;

    [Header("Hiệu ứng (VFX)")]
    [SerializeField] private GameObject teleportVFXPrefab;

    private CinemachineConfiner2D cameraConfiner;
    private bool isTeleporting = false;

    private void Awake()
    {
        // Tự động tìm Component CinemachineConfiner2D gắn trên Virtual Camera
        if (virtualCamera != null)
        {
            cameraConfiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        if (collision.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportRoutine(collision.gameObject));
        }
    }

    private IEnumerator TeleportRoutine(GameObject player)
    {
        isTeleporting = true;

        // 1. TẠO HIỆU ỨNG TẠI BỆ THỜ A (Nơi biến mất)
        if (teleportVFXPrefab != null)
        {
            Instantiate(teleportVFXPrefab, player.transform.position, Quaternion.identity);
        }

        // 2. KHÓA VẬN TỐC PLAYER (Tránh bị trượt đà khi dịch chuyển)
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(delayBeforeTeleport);

        // 3. TẮT COLLIDER MAP CŨ
        if (oldMapCollider != null)
        {
            oldMapCollider.enabled = false;
        }

        // 4. BẬT COLLIDER MAP MỚI VÀ GÁN CHO CINEMACHINE CONFINER 2D
        if (newMapCollider != null)
        {
            newMapCollider.enabled = true; 

            if (cameraConfiner != null)
            {
                // Gán PolygonCollider2D của Map B vào mục Bounding Shape của Confiner
                cameraConfiner.BoundingShape2D = newMapCollider;

                // Ép Cinemachine xóa bộ nhớ đệm khung viền cũ để nhận khung viền mới ngay lập tức
                //cameraConfiner.InvalidateCache();
            }
        }

        // 5. DỊCH CHUYỂN PLAYER SANG ĐIỂM B
        if (destinationB != null)
        {
            player.transform.position = destinationB.position;
        }

        // 6. ÉP CAMERA NHẢY TỨC THÌ TỚI TỌA ĐỘ MỚI (Không bị trượt dài)
        if (virtualCamera != null)
        {
            virtualCamera.PreviousStateIsValid = false;
        }

        // 7. THỜI GIAN ĐỜI ĐỂ CAMERA CỐ ĐỊNH VÀO KHUNG MAP MỚI
        yield return new WaitForSeconds(colliderSwitchDuration);

        // 8. TẠO HIỆU ỨNG TẠI BỆ THỜ B (Nơi xuất hiện)
        if (teleportVFXPrefab != null && destinationB != null)
        {
            Instantiate(teleportVFXPrefab, destinationB.position, Quaternion.identity);
        }

        isTeleporting = false;
    }
}