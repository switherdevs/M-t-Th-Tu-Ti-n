using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Camera mainCam; // Tối ưu: cache camera lại

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main; // Gán 1 lần duy nhất lúc khởi tạo

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    void Update()
    {
        // Xử lý xoay trong Update để đảm bảo mượt mà theo chuyển động chuột
        XoayMatTheoChuot();
    }

    void FixedUpdate()
    {
        // Chỉ xử lý vật lý di chuyển trong FixedUpdate
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void XoayMatTheoChuot()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 lookDir = (Vector2)mouseWorldPos - (Vector2)transform.position;

        if (lookDir.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (lookDir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}