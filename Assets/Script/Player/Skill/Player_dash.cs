using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Luot : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float dashDuration = 0.1f;    // Thời gian thực hiện cú lướt
    [SerializeField] private float trailDuration = 0.2f;   // Thời gian vệt sáng tồn tại

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer; // Gắn TrailRenderer vào đây

    //[Header("UI References")]
    //[SerializeField] private Image dashIcon; // Sprite biểu tượng tốc biến
    //[SerializeField] private TextMeshProUGUI cooldownText; // Text hiện thời gian

    private Rigidbody2D rb;
    private float lastDashTime = -100f; // Để có thể lướt ngay khi bắt đầu game
    private Vector2 dashDirection = Vector2.right; // Hướng mặc định nếu không bấm phím
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Đảm bảo ban đầu luôn TẮT vệt sáng Trail Renderer
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        //UpdateUI(true);
    }

    void Update()
    {
        // Khi đang lướt thì không nhận lệnh lướt đè lên
        if (isDashing) return;

        // 1. Lấy hướng lướt dựa trên phím di chuyển hiện tại (W, A, S, D)
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y = 1;
        else if (Input.GetKey(KeyCode.S)) input.y = -1;

        if (Input.GetKey(KeyCode.A)) input.x = -1;
        else if (Input.GetKey(KeyCode.D)) input.x = 1;

        if (input != Vector2.zero)
        {
            dashDirection = input.normalized;
        }

        // 2. Kiểm tra phím Space (Lướt)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDashRoutine());
        }

        //// 3. Cập nhật UI đếm ngược
        //float timeLeft = (lastDashTime + dashCooldown) - Time.time;
        //if (timeLeft > 0)
        //{
        //    cooldownText.text = timeLeft.ToString("F1");
        //    dashIcon.color = new Color(1, 1, 1, 0.5f); // Làm mờ sprite
        //}
        //else
        //{
        //    UpdateUI(true);
        //}
    }

    // Tiến trình xử lý lướt và bật/tắt vệt Trail
    private IEnumerator PerformDashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 1. KÍCH HOẠT TRAIL RENDERER
        if (trailRenderer != null)
        {
            trailRenderer.Clear(); // Xóa tàn dư vệt cũ còn sót lại
            trailRenderer.emitting = true; // Bật phát hiệu ứng
        }

        // 2. TÍNH TOÁN VỊ TRÍ VÀ THỰC HIỆN LƯỚT TỊNH TIẾN
        Vector2 startPosition = rb.position;
        Vector2 targetPosition = rb.position + (dashDirection * dashDistance);
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPosition);
        isDashing = false;

        // 3. TỰ ĐỘNG TẮT TRAIL RENDERER SAU THỜI GIAN CÀI ĐẶT
        yield return new WaitForSeconds(trailDuration);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false; // Tắt vệt sáng khi lướt xong
        }
    }

    //void UpdateUI(bool isReady)
    //{
    //    if (isReady)
    //    {
    //        cooldownText.text = "";
    //        dashIcon.color = new Color(1, 1, 1, 1f); // Hiện rõ sprite
    //    }
    //}
}