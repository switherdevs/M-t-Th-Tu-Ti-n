using UnityEngine;
using System.Collections;
using StatsSystem.Components;

/// <summary>
/// Script điều khiển AI Sói: Chạy rượt đuổi & Khựng lại phóng vồ (Pounce) + Tấn công
/// Bổ sung: Tính năng xoay mặt (Rotation Y) tương thích với 2D Bone Animation + Dừng hoàn toàn khi chết.
/// </summary>
public class WolfAI : MonoBehaviour
{
    public enum WolfState { Idle, Chase, PreparePounce, Pouncing, Attacking, Returning, Dead }

    [Header("=== STATE CURRENT ===")]
    [SerializeField] private WolfState currentState = WolfState.Idle;

    [Header("=== DETECTION RANGES ===")]
    [Tooltip("Vùng 1 (Rộng): Bán kính phát hiện và chạy rượt theo người chơi")]
    [SerializeField] private float chaseRange = 12f;

    [Tooltip("Vùng 2 (Gần): Bán kính phát hiện để khựng lại rồi phóng tới")]
    [SerializeField] private float pounceRange = 4f;

    [Tooltip("Bán kính tối đa để mất dấu người chơi (khi vượt quá tầm này sói sẽ bỏ đi về)")]
    [SerializeField] private float loseRange = 15f;

    [Tooltip("Layer dành riêng cho Player để sói quét trúng")]
    [SerializeField] private LayerMask playerLayer;

    [Header("=== MOVEMENT & POUNCE ===")]
    [Tooltip("Tốc độ 1: Rượt theo người chơi bình thường")]
    [SerializeField] private float chaseSpeed = 5f;

    [Tooltip("Tốc độ 2: Phóng chớp nhoáng tới vị trí người chơi")]
    [SerializeField] private float pounceSpeed = 18f;

    [Tooltip("Tốc độ đi bộ quay về chỗ cũ khi mất dấu")]
    [SerializeField] private float returnSpeed = 2f;

    [Tooltip("Thời gian khựng lại chuẩn bị phóng (tính bằng giây)")]
    [SerializeField] private float preparePounceTime = 0.4f;

    [Tooltip("Thời gian hồi chiêu Phóng (tránh bị liên tục phóng không ngừng)")]
    [SerializeField] private float pounceCooldown = 5f;

    [Tooltip("Khoảng cách dừng lệch X để không bị chui vào giữa tâm Sprite người chơi")]
    [SerializeField] private float stopOffsetX = 1.2f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [Tooltip("Tên tham số Bool trong Animator cho trạng thái Chạy")]
    [SerializeField] private string runAnimBool = "IsRunning";

    [Tooltip("Tên Trigger trong Animator khi Phóng tới")]
    [SerializeField] private string pounceAnimTrigger = "Pounce";

    [Tooltip("Tên Trigger trong Animator khi Tấn công")]
    [SerializeField] private string attackAnimTrigger = "Attack";

    // Các biến riêng tư
    private Vector2 spawnPosition;
    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;

    private float lastPounceTime = -999f; // Đặt âm lớn để vào game dùng được kỹ năng ngay

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();

        // Đăng ký nghe event chết từ CharacterStats
        if (stats != null)
        {
            stats.OnDeath += OnWolfDeath;
        }

        spawnPosition = transform.position;
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= OnWolfDeath;
        }
    }

    // 🎯 HÀM TỰ ĐỘNG KHÓA VÀ DỪNG AI HOÀN TOÀN KHI HẾT MÁU
    private void OnWolfDeath()
    {
        currentState = WolfState.Dead;

        // Dừng toàn bộ các Coroutine đang chạy (chuỗi phóng vồ/tấn công)
        StopAllCoroutines();

        // Tắt animation chạy
        SetAnimBool(runAnimBool, false);

        // Vô hiệu hóa script này để không chạy Update nữa
        this.enabled = false;
    }

    private void Update()
    {
        // 🎯 KIỂM TRA ĐIỀU KIỆN CHẾT HOẶC ĐANG TRONG TRẠNG THÁI KHÔNG THỂ DI CHUYỂN
        if (currentState == WolfState.Dead || currentState == WolfState.PreparePounce || currentState == WolfState.Pouncing || currentState == WolfState.Attacking)
            return;

        ScanForPlayer();

        switch (currentState)
        {
            case WolfState.Idle:
                UpdateIdle();
                break;
            case WolfState.Chase:
                UpdateChase();
                break;
            case WolfState.Returning:
                UpdateReturning();
                break;
        }
    }

    // ==========================================
    // 1. TÌM KIẾM NGƯỜI CHƠI (SCANNING)
    // ==========================================
    private void ScanForPlayer()
    {
        if (currentState == WolfState.Dead) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, chaseRange, playerLayer);

        if (hit != null)
        {
            playerTransform = hit.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            bool isPounceReady = Time.time >= lastPounceTime + pounceCooldown;

            if (distanceToPlayer <= pounceRange && isPounceReady)
            {
                StartCoroutine(Routine_PounceSequence());
            }
            else if (currentState != WolfState.Chase)
            {
                currentState = WolfState.Chase;
            }
        }
        else if (currentState == WolfState.Chase)
        {
            currentState = WolfState.Returning;
        }
    }

    // ==========================================
    // 2. CHUỖI HÀNH ĐỘNG: KHỰNG -> PHÓNG -> TẤN CÔNG
    // ==========================================
    private IEnumerator Routine_PounceSequence()
    {
        currentState = WolfState.PreparePounce;
        lastPounceTime = Time.time;

        SetAnimBool(runAnimBool, false);
        Flip(playerTransform.position.x);

        // --- KHỰNG LẠI ---
        yield return new WaitForSeconds(preparePounceTime);

        if (currentState == WolfState.Dead) yield break;

        // --- TÍNH TOÁN ĐIỂM DỪNG (OFFSET X) ---
        currentState = WolfState.Pouncing;
        SetAnimTrigger(pounceAnimTrigger);

        float directionSign = (playerTransform.position.x >= transform.position.x) ? 1f : -1f;

        Vector2 targetPosition = new Vector2(
            playerTransform.position.x - (directionSign * stopOffsetX),
            playerTransform.position.y
        );

        // --- PHÓNG TỚI ---
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            if (currentState == WolfState.Dead) yield break;

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                pounceSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (currentState == WolfState.Dead) yield break;

        transform.position = targetPosition;

        // --- TẤN CÔNG ---
        currentState = WolfState.Attacking;
        SetAnimTrigger(attackAnimTrigger);

        yield return new WaitForSeconds(0.5f);

        if (currentState == WolfState.Dead) yield break;

        currentState = WolfState.Chase;
    }

    // ==========================================
    // 3. DI CHUYỂN BÌNH THƯỜNG
    // ==========================================
    private void UpdateIdle()
    {
        SetAnimBool(runAnimBool, false);
    }

    private void UpdateChase()
    {
        if (playerTransform == null)
        {
            currentState = WolfState.Returning;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance > loseRange)
        {
            playerTransform = null;
            currentState = WolfState.Returning;
            return;
        }

        SetAnimBool(runAnimBool, true);
        transform.position = Vector2.MoveTowards(
            transform.position,
            playerTransform.position,
            chaseSpeed * Time.deltaTime
        );

        Flip(playerTransform.position.x);
    }

    private void UpdateReturning()
    {
        SetAnimBool(runAnimBool, true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            spawnPosition,
            returnSpeed * Time.deltaTime
        );

        Flip(spawnPosition.x);

        if (Vector2.Distance(transform.position, spawnPosition) < 0.1f)
        {
            currentState = WolfState.Idle;
        }
    }

    // ==========================================
    // 4. HELPER FUNCTIONS
    // ==========================================

    private void Flip(float targetX)
    {
        if (targetX < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (targetX > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    private void SetAnimBool(string name, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(name))
            animator.SetBool(name, value);
    }

    private void SetAnimTrigger(string name)
    {
        if (animator != null && !string.IsNullOrEmpty(name))
            animator.SetTrigger(name);
    }

    // ==========================================
    // 5. GIZMOS (VẼ TẦM NHÌN DỄ TEST GAME)
    // ==========================================

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pounceRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}