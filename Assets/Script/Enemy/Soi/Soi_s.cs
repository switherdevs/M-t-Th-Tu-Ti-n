using UnityEngine;
using System.Collections;

/// <summary>
/// Script điều khiển AI Sói: Chạy rượt đuổi & Khựng lại phóng vồ (Pounce) + Tấn công
/// Bổ sung: Tính năng xoay mặt (Rotation Y) tương thích với 2D Bone Animation.
/// </summary>
public class WolfAI : MonoBehaviour
{
    public enum WolfState { Idle, Chase, PreparePounce, Pouncing, Attacking, Returning }

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

    private float lastPounceTime = -999f; // Đặt âm lớn để vào game dùng được kỹ năng ngay

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        spawnPosition = transform.position;
    }

    private void Update()
    {
        if (currentState == WolfState.PreparePounce || currentState == WolfState.Pouncing || currentState == WolfState.Attacking)
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
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                pounceSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;

        // --- TẤN CÔNG ---
        currentState = WolfState.Attacking;
        SetAnimTrigger(attackAnimTrigger);

        yield return new WaitForSeconds(0.5f);

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

    /// <summary>
    /// Hàm xoay mặt dựa trên tọa độ X của mục tiêu (Tối ưu cho 2D Bone Animation)
    /// </summary>
    /// <param name="targetX">Tọa độ X của điểm muốn quay mặt về</param>
    private void Flip(float targetX)
    {
        // Mục tiêu nằm bên Trái so với Sói -> Xoay Y = 180 độ
        if (targetX < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        // Mục tiêu nằm bên Phải so với Sói -> Xoay Y = 0 độ
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
        // 1. Vùng 1: Tầm Rượt đuổi (Màu Vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 2. Vùng 2: Tầm Phóng vồ (Màu Đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pounceRange);

        // 3. Vùng Mất dấu (Màu Xanh Dương)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}