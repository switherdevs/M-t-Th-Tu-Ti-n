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

        // 2. Gán trực tiếp vận tốc để phanh khẩn cấp, triệt tiêu hiện tượng trượt (drifting)
        rb.linearVelocity = moveInput * targetSpeed;
    }

    private void XoayMatTheoChuot()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 lookDir = (Vector2)mouseWorldPos - (Vector2)transform.position;

        if (lookDir.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (lookDir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}