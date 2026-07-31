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
    [SerializeField] private string sprintAnimName = "IsSprinting";
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
    private int sprintAnimHash;
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
        if (!string.IsNullOrEmpty(sprintAnimName)) sprintAnimHash = Animator.StringToHash(sprintAnimName);
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

    // Cập nhật trạng thái Animation (Đi bộ / Chạy đơn giản)
    private void UpdateAnimations()
    {
        bool isMoving = moveInput.sqrMagnitude > 0f;

        // 1. Animation Đi Bộ (Khi di chuyển và KHÔNG đè Shift)
        if (HasParameter(walkAnimHash))
        {
            anim.SetBool(walkAnimHash, isMoving && !isSprinting);
        }

        // 2. Animation Chạy (Khi di chuyển VÀ CÓ đè Shift)
        if (HasParameter(sprintAnimHash))
        {
            anim.SetBool(sprintAnimHash, isMoving && isSprinting);
        }
    }

    private void XoayMatTheoChuot()
    {
        if (mainCam == null) return;

        // Lấy tọa độ chuột chuẩn trong thế giới 2D
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // Tính Vector hướng từ Nhân vật đến con trỏ chuột
        lookDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        // Lật mặt theo hướng chuột
        if (lookDirection.x > 0f && !isFacingRight)
        {
            FlipCharacter(true);
        }
        else if (lookDirection.x < 0f && isFacingRight)
        {
            FlipCharacter(false);
        }
    }

    // Thuật toán Lật Nhân Vật Triệt Để
    private void FlipCharacter(bool faceRight)
    {
        isFacingRight = faceRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }

        Transform targetTransform = (spriteRenderer != null) ? spriteRenderer.transform : transform;

        if (targetTransform == transform && transform.childCount > 0)
        {
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