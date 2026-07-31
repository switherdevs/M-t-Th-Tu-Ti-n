using System.Collections;
using UnityEngine;

public class BuildingInteraction : MonoBehaviour
{
    [Header("Cấu hình Phóng to (Scale)")]
    [SerializeField] private float targetScaleMultiplier = 1.2f; // Tỉ lệ to lên (1.2 tương đương to hơn 20%)
    [SerializeField] private float scaleSpeed = 10f;             // Tốc độ phóng to / thu nhỏ (số càng lớn chuyển động càng nhanh)

    [Header("Cấu hình Đổi màu (Highlight)")]
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f); // Màu trắng nhẹ khi đưa chuột vào
    private Color originalColor;                                            // Lưu lại màu gốc của công trình

    [Header("Cấu hình Giao diện (UI)")]
    [SerializeField] private GameObject uiGameObject;                     // Kéo GameObject UI vào đây trong Inspector

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;                                          // Lưu lại kích thước ban đầu
    private Vector3 targetScale;                                            // Kích thước mục tiêu khi đưa chuột vào
    private Coroutine scaleCoroutine;                                       // Quản lý tiến trình chạy mượt (Coroutine)

    private void Start()
    {
        // Lấy component SpriteRenderer để xử lý đổi màu
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Lưu lại kích thước gốc của công trình
        originalScale = transform.localScale;
        targetScale = originalScale * targetScaleMultiplier;

        // Đảm bảo ban đầu UI được ẩn đi
        if (uiGameObject != null)
        {
            uiGameObject.SetActive(false);
        }
    }

    // --- CÁC SỰ KIỆN TƯƠNG TÁC CHUỘT (Dùng cho Collider2D) ---

    // Khi con trỏ chuột bắt đầu di chuyển vào vùng Collider2D của công trình
    private void OnMouseEnter()
    {
        // 1. Xử lý đổi màu trắng nhẹ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }

        // 2. Xử lý phóng to từ từ mượt mà
        StartSmoothScale(targetScale);
    }

    // Khi con trỏ chuột rời khỏi vùng Collider2D của công trình
    private void OnMouseExit()
    {
        // 1. Trả lại màu sắc ban đầu
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // 2. Xử lý thu nhỏ lại kích thước ban đầu mượt mà
        StartSmoothScale(originalScale);
    }

    // Khi người chơi nhấn chuột trái vào công trình có Collider2D
    private void OnMouseDown()
    {
        if (uiGameObject != null)
        {
            // Đảo trạng thái hiển thị của UI (Đang tắt thì bật lên, đang bật thì tắt đi)
            bool isActive = uiGameObject.activeSelf;
            uiGameObject.SetActive(!isActive);

            Debug.Log("Đã click vào công trình, trạng thái UI: " + !isActive);
        }
        else
        {
            Debug.LogWarning("Chưa gán GameObject UI vào script!");
        }
    }

    // --- HÀM THUẬT TOÁN HỖ TRỢ ---

    // Hàm quản lý Coroutine để tránh bị chồng chéo lệnh scale khi di chuyển chuột nhanh ra vào
    private void StartSmoothScale(Vector3 endScale)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleRoutine(endScale));
    }

    // Thuật toán Lerp giúp đối tượng to lên hoặc nhỏ lại từ từ nhìn thấy được
    private IEnumerator ScaleRoutine(Vector3 destinationScale)
    {
        // Lặp liên tục qua từng khung hình cho đến khi kích thước hiện tại gần sát với kích thước mục tiêu
        while (Vector3.Distance(transform.localScale, destinationScale) > 0.01f)
        {
            // Mathf.Lerp giúp thay đổi giá trị một cách từ từ, mượt mà dựa theo thời gian thực (Time.deltaTime)
            transform.localScale = Vector3.Lerp(transform.localScale, destinationScale, scaleSpeed * Time.deltaTime);
            yield return null; // Chờ đến khung hình tiếp theo
        }

        // Gán chính xác kích thước đích ở khung hình cuối cùng để tránh sai số
        transform.localScale = destinationScale;
    }
}