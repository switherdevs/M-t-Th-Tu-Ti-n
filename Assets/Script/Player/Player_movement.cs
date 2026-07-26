using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;

    [Header("Cấu Hình Tên Animation (Có thể đổi tên trên Inspector)")]
    [SerializeField] private string walkAnimName = "IsWalking";
    [SerializeField] private string walkBackAnimName = "IsWalkingBack";         // Đi lùi
    [SerializeField] private string sprintAnimName = "IsSprinting";             // Chạy tiến
    [SerializeField] private string sprintBackAnimName = "IsSprintingBack";     // Chạy lùi
    [SerializeField] private string attackAnimName = "Attack";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Camera mainCam;

    private Vector2 moveInput;
    private Vector2 lookDirection = Vector2.right; // Hướng nhìn từ nhân vật đến con trỏ chuột
    private bool isSprinting = false;
    private bool isFacingRight = true;              // Biến kiểm tra hướng mặt hiện tại

    // Các biến lưu mã Hash của Animation
    private int walkAnimHash;
    private int walkBackAnimHash;
    private int sprintAnimHash;
    private int sprintBackAnimHash;
    private int attackAnimHash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tự động tìm SpriteRenderer và Animator trên chính nó hoặc trên con
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        mainCam = Camera.main;

        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Khởi tạo Hash ID cho Animation
        InitAnimationHashes();
    }

    private void InitAnimationHashes()
    {
        if (!string.IsNullOrEmpty(walkAnimName)) walkAnimHash = Animator.StringToHash(walkAnimName);
        if (!string.IsNullOrEmpty(walkBackAnimName)) walkBackAnimHash = Animator.StringToHash(walkBackAnimName);
        if (!string.IsNullOrEmpty(sprintAnimName)) sprintAnimHash = Animator.StringToHash(sprintAnimName);
        if (!string.IsNullOrEmpty(sprintBackAnimName)) sprintBackAnimHash = Animator.StringToHash(sprintBackAnimName);
        if (!string.IsNullOrEmpty(attackAnimName)) attackAnimHash = Animator.StringToHash(attackAnimName);
    }

    private bool HasParameter(int paramHash)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.nameHash == paramHash) return true;
        }
        return false;
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
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        rb.linearVelocity = moveInput * targetSpeed;
    }

    // Cập nhật trạng thái Animation (Đi tiến/lùi, Chạy tiến/lùi)
    private void UpdateAnimations()
    {
        bool isMoving = moveInput.sqrMagnitude > 0f;

        // TÍCH VÔ HƯỚNG (Dot Product): So sánh Hướng di chuyển phím và Hướng nhìn theo chuột
        // - Kết quả < 0: Bấm di chuyển ngược chiều hướng nhìn chuột -> Đang đi LÙI
        bool isMovingBackward = false;
        if (isMoving)
        {
            float dot = Vector2.Dot(moveInput, lookDirection);
            isMovingBackward = dot < 0f;
        }

        // 1. Animation Đi Lùi (Không đè Shift)
        if (HasParameter(walkBackAnimHash))
        {
            anim.SetBool(walkBackAnimHash, isMoving && isMovingBackward && !isSprinting);
        }

        // 2. Animation Đi Bộ Tiến (Không đè Shift)
        if (HasParameter(walkAnimHash))
        {
            anim.SetBool(walkAnimHash, isMoving && !isMovingBackward && !isSprinting);
        }

        // 3. Animation Chạy Lùi (Giữ Shift + Di chuyển lùi)
        if (HasParameter(sprintBackAnimHash))
        {
            anim.SetBool(sprintBackAnimHash, isMoving && isMovingBackward && isSprinting);
        }

        // 4. Animation Chạy Tiến (Giữ Shift + Di chuyển tiến)
        if (HasParameter(sprintAnimHash))
        {
            anim.SetBool(sprintAnimHash, isMoving && !isMovingBackward && isSprinting);
        }
    }

    private void XoayMatTheoChuot()
    {
        if (mainCam == null) return;

        // BƯỚC TÍNH TỌA ĐỘ CHUỘT CHUẨN TRONG THẾ GIỚI 2D:
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z); // Lấy trị tuyệt đối độ sâu Z Camera

        // Chuyển sang tọa độ thế giới (World Position)
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // Tính Vector hướng từ Nhân vật đến con trỏ chuột
        lookDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        // BẬT / TẮT FLIP THEO 2 CÁCH (ĐẢM BẢO XOAY TRÍCH ĐỂ):

        // 1. Nếu chuột bên PHẢI mà nhân vật đang nhìn TRÁI -> Đổi hướng sang PHẢI
        if (lookDirection.x > 0f && !isFacingRight)
        {
            FlipCharacter(true);
        }
        // 2. Nếu chuột bên TRÁI mà nhân vật đang nhìn PHẢI -> Đổi hướng sang TRÁI
        else if (lookDirection.x < 0f && isFacingRight)
        {
            FlipCharacter(false);
        }
    }

    // Thuật toán Lật Nhân Vật Triệt Để (Dùng cả FlipX và LocalScale)
    private void FlipCharacter(bool faceRight)
    {
        isFacingRight = faceRight;

        // Cách A: Sử dụng SpriteRenderer FlipX
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }

        // Cách B: Lật Scale của Đối tượng chứa SpriteRenderer/Animator (Phòng trường hợp Animation bị khóa FlipX)
        Transform targetTransform = (spriteRenderer != null) ? spriteRenderer.transform : transform;

        // Nếu SpriteRenderer nằm trên chính Player GameObject thì dùng cách lật Scale X của con
        if (targetTransform == transform && transform.childCount > 0)
        {
            // Lật tất cả con của Player
            foreach (Transform child in transform)
            {
                Vector3 childScale = child.localScale;
                childScale.x = faceRight ? Mathf.Abs(childScale.x) : -Mathf.Abs(childScale.x);
                child.localScale = childScale;
            }
        }
        else
        {
            Vector3 scale = targetTransform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            targetTransform.localScale = scale;
        }
    }

    // Hàm gọi Animation Tấn Công công khai
    public void TriggerAttackAnimation()
    {
        if (HasParameter(attackAnimHash))
        {
            anim.SetTrigger(attackAnimHash);
        }
    }
}