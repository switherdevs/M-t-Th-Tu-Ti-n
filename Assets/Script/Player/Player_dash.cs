using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Luot : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 2f;

    //[Header("UI References")]
    //[SerializeField] private Image dashIcon; // Sprite biểu tượng tốc biến
    //[SerializeField] private TextMeshProUGUI cooldownText; // Text hiện thời gian

    private Rigidbody2D rb;
    private float lastDashTime = -100f; // Để có thể lướt ngay khi bắt đầu game
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //UpdateUI(true);
    }

    void Update()
    {
        // 1. Lấy hướng lướt dựa trên phím di chuyển hiện tại (W, A, S, D)
        // Cách này lấy input legacy để xác định hướng ưu tiên
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y = 1;
        else if (Input.GetKey(KeyCode.S)) input.y = -1;

        if (Input.GetKey(KeyCode.A)) input.x = -1;
        else if (Input.GetKey(KeyCode.D)) input.x = 1;

        if (input != Vector2.zero) dashDirection = input.normalized;

        // 2. Kiểm tra phím Q (Legacy Input)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            PerformDash();
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

    void PerformDash()
    {
        lastDashTime = Time.time;
        rb.MovePosition(rb.position + (dashDirection * dashDistance));

        // Cập nhật trạng thái ngay lập tức
        //dashIcon.color = new Color(1, 1, 1, 0.5f);
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