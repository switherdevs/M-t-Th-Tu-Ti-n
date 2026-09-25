using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Luot : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float dashDuration = 0.1f;    // Thời gian thực hiện cú lướt
    [SerializeField] private float trailDuration = 0.2f;   // Thời gian vệt sáng tồn tại

    [Header("Invincible Settings")]
    [SerializeField, Tooltip("Thời gian bất tử (không dính sát thương) khi lướt")]
    private float invincibleDuration = 2f;

    [Header("Animation Settings")]
    [SerializeField] private string dashTriggerName = "Dash"; // Tên Trigger animation lướt

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer; // Gắn TrailRenderer vào đây

    private Rigidbody2D rb;
    private Animator animator;
    private CharacterStats characterStats;
    private float lastDashTime = -100f;
    private Vector2 dashDirection = Vector2.right;
    private bool isDashing = false;

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

        // Gọi hàm kích hoạt bất tử 2s
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