using UnityEngine;
using System.Collections;
using StatsSystem.Components;

/// <summary>
/// Script điều khiển AI Sói: Chạy rượt đuổi & Khựng lại phóng vồ (Pounce) + Tấn công
/// Bổ sung: Tính năng né chướng ngại vật mượt qua Layer/Tag (Wall) + Xoay mặt 2D Bone + Dừng khi chết.
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

    [Header("=== OBSTACLE AVOIDANCE (NÉ CHƯỚNG NGẠI VẬT) ===")]
    [Tooltip("Layer dành cho tường/chướng ngại vật cần né")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Khoảng cách quét phía trước để phát hiện tường né sớm")]
    [SerializeField] private float obstacleCheckDistance = 1.8f;

    [Tooltip("Bán kính quét Raycast vòng xòe xung quanh để tìm đường né")]
    [SerializeField] private float avoidanceRadius = 1.2f;

    [Tooltip("Độ mượt khi bẻ hướng di chuyển né tường (càng cao bẻ hướng càng nhanh)")]
    [SerializeField] private float avoidanceSteerSmooth = 6f;

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

    private float lastPounceTime = -999f;
    private Vector2 currentMoveDirection; // Biến lưu hướng di chuyển mượt

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();

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

    private void OnWolfDeath()
    {
        currentState = WolfState.Dead;

        StopAllCoroutines();
        SetAnimBool(runAnimBool, false);
        this.enabled = false;
    }

    private void Update()
    {
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

        yield return new WaitForSeconds(preparePounceTime);

        if (currentState == WolfState.Dead) yield break;

        currentState = WolfState.Pouncing;
        SetAnimTrigger(pounceAnimTrigger);

        float directionSign = (playerTransform.position.x >= transform.position.x) ? 1f : -1f;

        Vector2 targetPosition = new Vector2(
            playerTransform.position.x - (directionSign * stopOffsetX),
            playerTransform.position.y
        );

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

        currentState = WolfState.Attacking;
        SetAnimTrigger(attackAnimTrigger);

        yield return new WaitForSeconds(0.5f);

        if (currentState == WolfState.Dead) yield break;

        currentState = WolfState.Chase;
    }

    // ==========================================
    // 3. DI CHUYỂN BÌNH THƯỜNG & NÉ CHƯỚNG NGẠI VẬT
    // ==========================================
    private void UpdateIdle()
    {
        SetAnimBool(runAnimBool, false);
        currentMoveDirection = Vector2.zero;
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

        // Hướng gốc muốn đi tới người chơi
        Vector2 targetDirection = (playerTransform.position - transform.position).normalized;

        // Tính hướng di chuyển đã xử lý né chướng ngại vật
        Vector2 finalDirection = CalculateAvoidanceDirection(targetDirection);

        // Nội suy Lerp giúp bẻ lái mượt mà không bị giật
        currentMoveDirection = Vector2.Lerp(currentMoveDirection, finalDirection, Time.deltaTime * avoidanceSteerSmooth);

        transform.Translate(currentMoveDirection * chaseSpeed * Time.deltaTime, Space.World);

        Flip(transform.position.x + currentMoveDirection.x);
    }

    private void UpdateReturning()
    {
        SetAnimBool(runAnimBool, true);

        Vector2 targetDirection = ((Vector3)spawnPosition - transform.position).normalized;
        Vector2 finalDirection = CalculateAvoidanceDirection(targetDirection);

        currentMoveDirection = Vector2.Lerp(currentMoveDirection, finalDirection, Time.deltaTime * avoidanceSteerSmooth);

        transform.Translate(currentMoveDirection * returnSpeed * Time.deltaTime, Space.World);

        Flip(transform.position.x + currentMoveDirection.x);

        if (Vector2.Distance(transform.position, spawnPosition) < 0.1f)
        {
            currentState = WolfState.Idle;
        }
    }

    /// <summary>
    /// Hàm thuật toán né chướng ngại vật thông minh (Context-Based Steering)
    /// Quét tia Raycast các góc xòe để tìm đường trống né tường
    /// </summary>
    private Vector2 CalculateAvoidanceDirection(Vector2 desiredDirection)
    {
        // 1. Kiểm tra tia thẳng phía trước
        RaycastHit2D hitCenter = Physics2D.Raycast(transform.position, desiredDirection, obstacleCheckDistance, obstacleLayer);

        // Nếu phía trước trống hoặc không đụng phải tường/vật cản Tag "Wall", đi thẳng
        if (!hitCenter || (!hitCenter.collider.CompareTag("Wall") && obstacleLayer == 0))
        {
            return desiredDirection;
        }

        // 2. Nếu vướng tường, bắn 8 tia xòe xung quanh để tìm đường đi không bị chắn
        float[] rayAngles = { 45f, -45f, 90f, -90f, 135f, -135f, 180f };
        Vector2 bestDirection = desiredDirection;
        float maxScore = -999f;

        foreach (float angle in rayAngles)
        {
            Vector2 checkDir = Quaternion.Euler(0, 0, angle) * desiredDirection;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDir, avoidanceRadius, obstacleLayer);

            bool isWall = hit && (hit.collider.CompareTag("Wall") || (obstacleLayer & (1 << hit.collider.gameObject.layer)) != 0);

            if (!isWall)
            {
                // Điểm số ưu tiên hướng gần nhất với hướng mục tiêu
                float score = Vector2.Dot(checkDir, desiredDirection);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestDirection = checkDir;
                }
            }
        }

        return bestDirection.normalized;
    }

    // ==========================================
    // 4. HELPER FUNCTIONS
    // ==========================================

    private void Flip(float targetX)
    {
        if (targetX < transform.position.x - 0.05f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (targetX > transform.position.x + 0.05f)
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

        // Vẽ tia quét chướng ngại vật trên Scene
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, currentMoveDirection * obstacleCheckDistance);
    }
}