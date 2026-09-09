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
    private bool isExecutingMode = false; // Cờ khóa follow khi kết liễu

    private void Awake()
    {
        mainCam = Camera.main;
    }

    public void SetExecutingState(bool executing)
    {
        isExecutingMode = executing;
    }

    private void Update()
    {
        // Khi đang kết liễu hoặc thiếu target thì tạm dừng cập nhật vị trí điểm ngắm
        if (isExecutingMode || playerTarget == null) return;

        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // 1. LẤY TỌA ĐỘ CHUỘT AN TOÀN TRÁNH LỖI NaN
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // 2. TÍNH HƯỚNG VÀ KHOẢNG CÁCH TỪ PLAYER ĐẾN CHUỘT
        Vector3 dirToMouse = mouseWorldPos - playerTarget.position;

        // 3. THUẬT TOÁN GIỚI HẠN ĐỘ XA RIÊNG BỆNH CHO TRỤC X VÀ TRỤC Y (CLAMP)
        float clampedX = Mathf.Clamp(dirToMouse.x, -maxXOffset, maxXOffset);
        float clampedY = Mathf.Clamp(dirToMouse.y, -maxYOffset, maxYOffset);

        Vector3 clampedOffset = new Vector3(clampedX, clampedY, 0f);

        // 4. TÍNH VỊ TRÍ ĐÍCH CHO ĐIỂM NGẮM
        Vector3 targetPosition = playerTarget.position + clampedOffset;
        targetPosition.z = 0f;

        // 5. NỘI SUY DI CHUYỂN ĐIỂM NGẮM MƯỢT MÀ
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}