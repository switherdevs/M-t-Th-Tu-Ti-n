using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCam;

    private bool isSprinting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        rb.gravityScale = 0f;

        // TỐI ƯU VẬT LÝ: Để lực cản vừa phải để phanh tự nhiên khi va chạm
        rb.linearDamping = 2f;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // CHỐNG RUNG/GIẬT: Chuyển chế độ nội suy vật lý sang Interpolate để di chuyển mượt với Camera
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }

    void Update()
    {
        XoayMatTheoChuot();
    }

    void FixedUpdate()
    {
        // 1. Xác định tốc độ hiện tại dựa trên phím Shift
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // 2. Thuật toán di chuyển Dynamic tối ưu:
        if (moveInput != Vector2.zero)
        {
            // Tính toán vận tốc mục tiêu mong muốn
            Vector2 targetVelocity = moveInput * targetSpeed;

            // Tìm hiệu số giữa vận tốc mong muốn và vận tốc hiện tại của Rigidbody
            Vector2 velocityChange = targetVelocity - rb.linearVelocity;

            // Áp dụng một lực vừa đủ (Force) để đưa nhân vật đạt vận tốc mục tiêu ngay lập tức
            // Sử dụng ForceMode2D.Impulse giúp phản hồi bấm nút nhạy bén, không có độ trễ
            rb.AddForce(velocityChange, ForceMode2D.Impulse);
        }
        else
        {
            // THUẬT TOÁN PHANH: Khi buông tay hoàn toàn và vận tốc còn rất nhỏ, triệt tiêu hẳn về 0
            // Điều này giúp nhân vật đứng im hoàn toàn, không bị hiện tượng trượt từ từ (drifting)
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void XoayMatTheoChuot()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 lookDir = (Vector2)mouseWorldPos - (Vector2)transform.position;

        if (lookDir.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (lookDir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}