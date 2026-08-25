using UnityEngine;

public class CinemachineMouseTarget : MonoBehaviour
{
    [Header("=== MỤC TIÊU ===")]
    [SerializeField, Tooltip("Kéo GameObject Player vào đây")]
    private Transform playerTarget;

    [Header("=== GIỚI HẠN LIA CHUỘT (X & Y ĐỘC LẬP) ===")]
    [SerializeField, Tooltip("Độ xa tối đa Camera có thể lia theo chuột theo chiều NGANG (Trục X)")]
    private float maxXOffset = 4f;

    [SerializeField, Tooltip("Độ xa tối đa Camera có thể lia theo chuột theo chiều DỌC (Trục Y) - Giảm nhỏ xuống để không bị quá cao")]
    private float maxYOffset = 1.5f;

    [SerializeField, Tooltip("Tốc độ mượt khi di chuyển điểm ngắm (Khuyên dùng: 5 đến 10)")]
    private float smoothSpeed = 8f;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Kiểm tra an toàn: Tránh lỗi NullReference khi chưa có Player hoặc Camera
        if (playerTarget == null) return;
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // 1. LẤY TỌA ĐỘ CHUỘT AN TOÀN TRÁNH LỖI NaN
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);

        // Chuyển tọa độ chuột từ Screen Space sang World Space 2D
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // 2. TÍNH HƯỚNG VÀ KHOẢNG CÁCH TỪ PLAYER ĐẾN CHUỘT
        Vector3 dirToMouse = mouseWorldPos - playerTarget.position;

        // 3. THUẬT TOÁN GIỚI HẠN ĐỘ XA RIÊNG BỆNH CHO TRỤC X VÀ TRỤC Y (CLAMP)
        // Dùng Mathf.Clamp để khóa chính xác khoảng cách cho từng trục
        float clampedX = Mathf.Clamp(dirToMouse.x, -maxXOffset, maxXOffset);
        float clampedY = Mathf.Clamp(dirToMouse.y, -maxYOffset, maxYOffset);

        Vector3 clampedOffset = new Vector3(clampedX, clampedY, 0f);

        // 4. TÍNH VỊ TRÍ ĐÍCH CHO ĐIỂM NGẮM
        Vector3 targetPosition = playerTarget.position + clampedOffset;
        targetPosition.z = 0f; // Khóa trục Z trên mặt phẳng 2D

        // 5. NỘI SUY DI CHUYỂN ĐIỂM NGẮM MƯỢT MÀ
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}