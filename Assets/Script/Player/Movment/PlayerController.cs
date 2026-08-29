using UnityEngine;
using UnityEngine.InputSystem;
using StatsSystem.Components;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("=== MOVEMENT SETTINGS ===")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [SerializeField] private string walkAnimName = "IsWalking";
    [SerializeField] private string sprintAnimName = "IsSprinting";
    [SerializeField] private string attackAnimName = "Attack";

    // Component References
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Camera mainCam;
    private CharacterStats stats;

    // Movement & Facing Variables
    private Vector2 moveInput;
    private Vector2 lookDirection = Vector2.right;
    private bool isSprinting = false;
    private bool isFacingRight = true;

    // Animation Hashes
    private int walkAnimHash;
    private int sprintAnimHash;
    private int attackAnimHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();
        mainCam = Camera.main;

        // Cấu hình Rigidbody2D chuẩn xác để chống trôi và va chạm mượt
        rb.gravityScale = 0f;
        rb.linearDamping = 5f; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        InitAnimationHashes();
    }

    private void InitAnimationHashes()
    {
        if (!string.IsNullOrEmpty(walkAnimName)) walkAnimHash = Animator.StringToHash(walkAnimName);
        if (!string.IsNullOrEmpty(sprintAnimName)) sprintAnimHash = Animator.StringToHash(sprintAnimName);
        if (!string.IsNullOrEmpty(attackAnimName)) attackAnimHash = Animator.StringToHash(attackAnimName);
    }

    // ==========================================
    // 1. INPUT SYSTEM CALLBACKS
    // ==========================================
    public void OnMove(InputAction.CallbackContext context)
    {
        if (stats != null && stats.IsDead)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (stats != null && stats.IsDead)
        {
            isSprinting = false;
            return;
        }

        if (context.performed) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    // ==========================================
    // 2. GAME LOOP (UPDATE & FIXED UPDATE)
    // ==========================================
    private void Update()
    {
        if (stats != null && stats.IsDead)
        {
            moveInput = Vector2.zero;
            UpdateAnimations();
            return;
        }

        HandleMouseRotation();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (stats != null && stats.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Di chuyển bằng linearVelocity mượt mà, không dùng Force đẩy văng
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        rb.linearVelocity = moveInput * targetSpeed;
    }

    // ==========================================
    // 3. LOGIC LẬT MẶT & CẬP NHẬT CHUỘT
    // ==========================================
    private void HandleMouseRotation()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // Lấy vị trí chuột
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();

        // CHỐNG LỖI VĂNG Y: Kiểm tra nếu chuột ra khỏi cửa sổ Game View
        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width ||
            mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
        {
            return;
        }

        // Chuyển tọa độ màn hình sang tọa độ thế giới 2D an toàn
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        lookDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        // Lật mặt bằng eulerAngles Y (không can thiệp scale âm gây đè Collider)
        if (lookDirection.x > 0f && !isFacingRight)
        {
            FlipCharacter(true);
        }
        else if (lookDirection.x < 0f && isFacingRight)
        {
            FlipCharacter(false);
        }
    }

    private void FlipCharacter(bool faceRight)
    {
        isFacingRight = faceRight;

        // Dùng phương pháp quay Y = 0 hoặc 180 chuẩn 2D/3D (Tương thích 100% với Bone Animation)
        transform.eulerAngles = faceRight ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 180f, 0f);
    }

    // ==========================================
    // 4. ANIMATION & PUBLIC METHODS
    // ==========================================
    private void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0f;

        if (walkAnimHash != 0) anim.SetBool(walkAnimHash, isMoving && !isSprinting);
        if (sprintAnimHash != 0) anim.SetBool(sprintAnimHash, isMoving && isSprinting);
    }

    public void TriggerAttackAnimation()
    {
        if (stats != null && stats.IsDead) return;

        if (anim != null && attackAnimHash != 0)
        {
            anim.SetTrigger(attackAnimHash);
        }
    }
}