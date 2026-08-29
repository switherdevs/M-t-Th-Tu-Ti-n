using UnityEngine;
using System.Collections;
using StatsSystem.Components;

/// <summary>
/// Script điều khiển AI Quạ:
/// - Idle vỗ cánh liên tục.
/// - Bay lên cao (Trục Y), X ngẫu nhiên (trái/phải). Nếu đụng Collider không phải Trigger sẽ dừng lại aim ngay.
/// - Xoay mặt (Y=0 hoặc Y=180) hướng về Player.
/// - Lao xuống tấn công, bật Collider nguy hiểm trong khoảng thời gian cố định rồi ẩn.
/// </summary>
public class CrowAI : MonoBehaviour
{
    public enum CrowState { Idle, Ascending, Targeting, Diving, Returning, Dead }

    [Header("=== STATE CURRENT ===")]
    [SerializeField] private CrowState currentState = CrowState.Idle;

    [Header("=== DETECTION RANGES ===")]
    [Tooltip("Khoảng cách phát hiện Player để bắt đầu bài bay lên")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("=== FLY & DIVE SETTINGS ===")]
    [Tooltip("Độ cao Y bay lên so với vị trí hiện tại")]
    [SerializeField] private float flyUpHeight = 4f;

    [Tooltip("Khoảng lệch X ngẫu nhiên khi bay lên (Trái/Phải)")]
    [SerializeField] private float randomOffsetRangeX = 3f;

    [Tooltip("Tốc độ bay lên vị trí chuẩn bị")]
    [SerializeField] private float ascendSpeed = 4f;

    [Tooltip("Thời gian đứng khựng trên không nhắm mục tiêu trước khi lao xuống")]
    [SerializeField] private float aimTime = 0.5f;

    [Tooltip("Tốc độ lao xuống tấn công")]
    [SerializeField] private float diveSpeed = 15f;

    [Tooltip("Thời gian hồi chiêu giữa các lần tấn công")]
    [SerializeField] private float attackCooldown = 4f;

    [Header("=== ATTACK HITBOX ===")]
    [Tooltip("GameObject chứa Collider gây sát thương của Quạ (mặc định sẽ ẩn)")]
    [SerializeField] private GameObject attackColliderObject;

    [Tooltip("Thời gian duy trì bật Collider khi lao xuống (tính bằng giây)")]
    [SerializeField] private float attackColliderDuration = 0.4f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [Tooltip("Tên tham số Bool trong Animator cho trạng thái Tấn công")]
    [SerializeField] private string attackAnimBool = "IsAttacking";

    // Các biến riêng tư
    private Vector2 spawnPosition;
    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;

    private float lastAttackTime = -999f;
    private bool hitObstacleWhileAscending = false; // Biến cờ đánh dấu khi đụng trần/chướng ngại vật

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();

        if (stats != null)
        {
            stats.OnDeath += OnCrowDeath;
        }

        spawnPosition = transform.position;

        // Tắt Hitbox tấn công khi vừa vào Game
        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= OnCrowDeath;
        }
    }

    private void OnCrowDeath()
    {
        currentState = CrowState.Dead;
        StopAllCoroutines();

        if (attackColliderObject != null)
            attackColliderObject.SetActive(false);

        SetAnimBool(attackAnimBool, false);
        this.enabled = false;
    }

    private void Update()
    {
        if (currentState == CrowState.Dead || currentState == CrowState.Ascending || currentState == CrowState.Targeting || currentState == CrowState.Diving)
            return;

        ScanForPlayer();
    }

    // ==========================================
    // 1. TÌM KIẾM NGƯỜI CHƠI
    // ==========================================
    private void ScanForPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (hit != null && Time.time >= lastAttackTime + attackCooldown)
        {
            playerTransform = hit.transform;
            StartCoroutine(Routine_AttackSequence());
        }
    }

    // ==========================================
    // 2. CHUỖI HÀNH ĐỘNG: BAY LÊN -> NHẮM -> LAO XUỐNG
    // ==========================================
    private IEnumerator Routine_AttackSequence()
    {
        lastAttackTime = Time.time;
        hitObstacleWhileAscending = false; // Reset cờ trước khi bay lên

        // --- BƯỚC 1: BAY LÊN CAO (Y tăng, X Random) ---
        currentState = CrowState.Ascending;
        SetAnimBool(attackAnimBool, false);

        float randomX = Random.Range(-randomOffsetRangeX, randomOffsetRangeX);
        Vector2 preparePosition = new Vector2(transform.position.x + randomX, transform.position.y + flyUpHeight);

        // Quay mặt theo hướng ngẫu nhiên khi bay lên
        Flip(preparePosition.x);

        // Vòng lặp bay lên: Thêm điều kiện !hitObstacleWhileAscending để ngắt khi đụng Collider cứng
        while (Vector2.Distance(transform.position, preparePosition) > 0.1f && !hitObstacleWhileAscending)
        {
            if (currentState == CrowState.Dead) yield break;

            transform.position = Vector2.MoveTowards(transform.position, preparePosition, ascendSpeed * Time.deltaTime);
            yield return null;
        }

        // --- BƯỚC 2: QUAY MẶT VỀ PHÍA PLAYER & KHỰNG NHẮM ---
        currentState = CrowState.Targeting;
        if (playerTransform != null)
        {
            Flip(playerTransform.position.x);
        }

        yield return new WaitForSeconds(aimTime);

        if (currentState == CrowState.Dead) yield break;

        // --- BƯỚC 3: LAO XUỐNG & BẬT COLLIDER TẤN CÔNG ---
        currentState = CrowState.Diving;
        SetAnimBool(attackAnimBool, true);

        // Lưu lại vị trí Player tại thời điểm lao xuống
        Vector2 diveTargetPosition = playerTransform != null ? (Vector2)playerTransform.position : spawnPosition;

        // Bật Collider đòn đánh trong khoảng thời gian cố định
        StartCoroutine(Routine_ToggleAttackCollider());

        while (Vector2.Distance(transform.position, diveTargetPosition) > 0.1f)
        {
            if (currentState == CrowState.Dead) yield break;

            transform.position = Vector2.MoveTowards(transform.position, diveTargetPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }

        if (currentState == CrowState.Dead) yield break;

        // --- BƯỚC 4: HOÀN THÀNH TẤN CÔNG -> CÂN BẰNG LẠI ---
        SetAnimBool(attackAnimBool, false);
        currentState = CrowState.Idle;
    }

    /// <summary>
    /// Coroutine bật Collider gây sát thương trong khoảng thời gian cố định rồi ẩn
    /// </summary>
    private IEnumerator Routine_ToggleAttackCollider()
    {
        if (attackColliderObject != null)
        {
            attackColliderObject.SetActive(true);
            yield return new WaitForSeconds(attackColliderDuration);
            attackColliderObject.SetActive(false);
        }
    }

    // ==========================================
    // 3. XỬ LÝ VA CHẠM (COLLISION & TRIGGER DETECT)
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu quạ đụng phải Collider KHÔNG PHẢI Trigger trong lúc đang bay lên
        if (currentState == CrowState.Ascending && !collision.isTrigger)
        {
            hitObstacleWhileAscending = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Xử lý bổ sung cho Rigidbody2D ở chế độ Non-Trigger / Dynamic / Kinematic
        if (currentState == CrowState.Ascending && !collision.collider.isTrigger)
        {
            hitObstacleWhileAscending = true;
        }
    }

    // ==========================================
    // 4. HELPER FUNCTIONS
    // ==========================================
    private void Flip(float targetX)
    {
        // Xoay mặt bằng eulerAngles Y (0 hoặc 180), giữ nguyên Z
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}