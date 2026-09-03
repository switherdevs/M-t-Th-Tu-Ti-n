using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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

    // Slow Debuff Variables
    private float currentSlowMultiplier = 1f;
    private Coroutine slowCoroutine;

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

        // Áp dụng hệ số làm chậm currentSlowMultiplier vào tốc độ
        float targetSpeed = (isSprinting ? sprintSpeed : moveSpeed) * currentSlowMultiplier;
        rb.linearVelocity = moveInput * targetSpeed;
    }

    // ==========================================
    // DEBUFF SLOW LOGIC
    // ==========================================
    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(Routine_ApplySlow(slowMultiplier, duration));
    }

    private IEnumerator Routine_ApplySlow(float slowMultiplier, float duration)
    {
        currentSlowMultiplier = Mathf.Clamp01(slowMultiplier);
        yield return new WaitForSeconds(duration);
        currentSlowMultiplier = 1f;
        slowCoroutine = null;
    }

    private void HandleMouseRotation()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();

        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width ||
            mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
        {
            return;
        }

        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        lookDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

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
        transform.eulerAngles = faceRight ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 180f, 0f);
    }

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