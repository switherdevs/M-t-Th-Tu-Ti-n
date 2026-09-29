using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Luot : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 0.2f;   // Cooldown ngắn để có thể dash liên tục
    [SerializeField] private float dashDuration = 0.1f;   // Thời gian thực hiện cú lướt
    [SerializeField] private float trailDuration = 0.2f;  // Thời gian vệt sáng tồn tại

    [Header("Asian Mode Meme Settings")]
    [SerializeField] private int maxDashAllowed = 7;      // Giới hạn 7 lần
    [SerializeField] private float dashResetWindow = 3f;  // Thời gian cửa sổ đếm liên tục (3s)

    [Header("Invincible Settings")]
    [SerializeField, Tooltip("Thời gian bất tử (không dính sát thương) khi lướt")]
    private float invincibleDuration = 2f;

    [Header("Animation Settings")]
    [SerializeField] private string dashTriggerName = "Dash";

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStats characterStats;
    private float lastDashTime = -100f;
    private Vector2 dashDirection = Vector2.right;
    private bool isDashing = false;

    // Các biến đếm lướt liên tục
    private int consecutiveDashCount = 0;
    private float lastDashTrackerTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        characterStats = GetComponent<CharacterStats>();

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    void Update()
    {
        if (isDashing) return;

        // Reset đếm lướt liên tục nếu đã quá thời gian quy định
        if (Time.time - lastDashTrackerTime > dashResetWindow)
        {
            consecutiveDashCount = 0;
        }

        // 1. Lấy hướng lướt dựa trên phím di chuyển hiện tại (W, A, S, D)
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.y = 1;
        else if (Input.GetKey(KeyCode.S)) input.y = -1;

        if (Input.GetKey(KeyCode.A)) input.x = -1;
        else if (Input.GetKey(KeyCode.D)) input.x = 1;

        if (input != Vector2.zero)
        {
            dashDirection = input.normalized;
        }

        // 2. Kiểm tra phím Space (Lướt)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDashRoutine());
        }
    }

    private IEnumerator PerformDashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // BẪY ASIAN MODE: Đếm số lần lướt liên tục
        consecutiveDashCount++;
        lastDashTrackerTime = Time.time;

        int currentDifficulty = PlayerPrefs.GetInt("GameDifficulty", 1);
        if (currentDifficulty == 3 && consecutiveDashCount >= maxDashAllowed)
        {
            Debug.LogWarning("<color=red>[Asian Mode Meme]</color> Lướt liên tục quá 7 lần! Đột quỵ chết!");
            if (characterStats != null)
            {
                characterStats.StopAllCoroutines();
                characterStats.TakeDamage(9999999f);
            }
            isDashing = false;
            yield break;
        }

        // Gọi hàm kích hoạt bất tử
        if (characterStats != null)
        {
            characterStats.SetInvincible(invincibleDuration);
        }

        // 0. KÍCH HOẠT TRIGGER ANIMATION LƯỚT
        if (animator != null)
        {
            animator.SetTrigger(dashTriggerName);
        }

        // 1. KÍCH HOẠT TRAIL RENDERER
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        // 2. TÍNH TOÁN VỊ TRÍ VÀ THỰC HIỆN LƯỚT TỊNH TIẾN
        Vector2 startPosition = rb.position;
        Vector2 targetPosition = rb.position + (dashDirection * dashDistance);
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPosition);
        isDashing = false;

        // 3. TỰ ĐỘNG TẮT TRAIL RENDERER SAU THỜI GIAN CÀI ĐẶT
        yield return new WaitForSeconds(trailDuration);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }
}