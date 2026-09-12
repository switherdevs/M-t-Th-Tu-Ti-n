using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class FinishCircleEffect : MonoBehaviour
{
    // =========================================================
    // CẤU HÌNH THỜI GIAN & FADE
    // =========================================================

    [Header("===== CẤU HÌNH THỜI GIAN =====")]
    [Tooltip("Thời gian làm mờ dần mờ -> rõ (Giây)")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Tooltip("Thời gian chờ đứng yên trước khi bật lại Animation & Gây Damage (Giây)")]
    [SerializeField] private float waitTimeBeforeAnim = 1.0f;

    [Tooltip("Thời gian chạy Animation / Hiển thị gây sát thương (Giây)")]
    [SerializeField] private float displayDuration = 0.8f;

    [Tooltip("Thời gian làm mờ dần rõ -> mờ trước khi hủy (Giây)")]
    [SerializeField] private float fadeOutDuration = 0.5f;


    // =========================================================
    // CẤU HÌNH SÁT THƯƠNG & COLLIDER
    // =========================================================

    [Header("===== CẤU HÌNH SÁT THƯƠNG =====")]
    [Tooltip("Sát thương gây ra khi hiệu ứng bùng nổ")]
    [SerializeField] private float damageAmount = 50f;

    [Tooltip("Bán kính vùng gây sát thương (Nếu bằng 0 sẽ tự lấy theo bán kính CircleCollider2D)")]
    [SerializeField] private float damageRadius = 0f;

    [Tooltip("Layer của mục tiêu sẽ nhận sát thương (VD: Player hoặc Enemy)")]
    [SerializeField] private LayerMask targetLayer;


    // =========================================================
    // COMPONENTS INTERNAL
    // =========================================================

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CircleCollider2D circleCollider;


    // =========================================================
    // KHỞI TẠO VÀ LẮNG NGHE
    // =========================================================

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Nếu người chơi chưa nhập bán kính sát thương, tự động lấy theo CircleCollider2D
        if (damageRadius <= 0f && circleCollider != null)
        {
            damageRadius = circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        }
    }

    private void Start()
    {
        StartCoroutine(Routine_ExecuteFinishCircle());
    }


    // =========================================================
    // TIẾN TRÌNH XỬ LÝ CHÍNH
    // =========================================================

    private IEnumerator Routine_ExecuteFinishCircle()
    {
        // --- BƯỚC 1: TẮT ANIMATOR & CHỈNH ALPHA VỀ 0 ---
        if (animator != null)
        {
            animator.enabled = false;
        }

        if (circleCollider != null)
        {
            circleCollider.enabled = true;
        }

        SetSpriteAlpha(0f);

        // --- BƯỚC 2: FADE IN (TỪ MỜ -> RÕ) ---
        yield return StartCoroutine(Routine_FadeSprite(0f, 1f, fadeInDuration));

        // --- BƯỚC 3: CHỜ ĐẾN MỐC THỜI GIAN YÊU CẦU ---
        if (waitTimeBeforeAnim > 0f)
        {
            yield return new WaitForSeconds(waitTimeBeforeAnim);
        }

        // --- BƯỚC 4: BẬT LẠI ANIMATOR & GÂY SÁT THƯƠNG ---
        if (animator != null)
        {
            animator.enabled = true;
        }

        DealDamageInCircle();

        // --- BƯỚC 5: GIỮ HIỆU ỨNG TRONG KHOẢNG DISPLAY DURATION ---
        if (displayDuration > 0f)
        {
            yield return new WaitForSeconds(displayDuration);
        }

        // --- BƯỚC 6: TẮT COLLIDER & FADE OUT (TỪ RÕ -> MỜ) ---
        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        yield return StartCoroutine(Routine_FadeSprite(1f, 0f, fadeOutDuration));

        // --- BƯỚC 7: TỰ HỦY BẢN THÂN ---
        Destroy(gameObject);
    }


    // =========================================================
    // XỬ LÝ FADE VÀ SÁT THƯƠNG
    // =========================================================

    private IEnumerator Routine_FadeSprite(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            SetSpriteAlpha(endAlpha);
            yield break;
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            SetSpriteAlpha(currentAlpha);
            yield return null;
        }

        SetSpriteAlpha(endAlpha);
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void DealDamageInCircle()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, damageRadius, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            if (target == null) continue;

            // Kiểm tra Interface IDamageable (bao gồm CharacterStats)
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damageAmount);
            }
        }
    }


    // =========================================================
    // HIỂN THỊ ĐỒ HỌA GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float radiusToDraw = damageRadius;

        if (radiusToDraw <= 0f && circleCollider != null)
        {
            radiusToDraw = circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        }

        Gizmos.DrawWireSphere(transform.position, radiusToDraw);
    }
}