using System.Collections;
using UnityEngine;

public class TimelineFadeOut : MonoBehaviour
{
    // =========================================================
    // CẤU HÌNH THỜI GIAN
    // =========================================================

    [Header("===== THỜI GIAN CHẠY XỬ LÝ =====")]
    [Tooltip("Thời gian chờ (tính bằng Giây) trước khi bắt đầu thực hiện")]
    [SerializeField] private float delayStartTime = 7.58f;

    [Tooltip("Thời gian ẩn hoàn toàn (SetActive false) trước khi hiện lại (tính bằng Giây)")]
    [SerializeField] private float hideDuration = 2f;


    // =========================================================
    // CẤU HÌNH ANIMATION HOẶC FADE
    // =========================================================

    [Header("===== CẤU HÌNH ANIMATION / FADE =====")]
    [Tooltip("Tích vào đây nếu muốn chạy Animation thay vì mờ dần (Fade)")]
    [SerializeField] private bool waitForAnimation = false;

    [Tooltip("Tên Parameter Trigger trong Animator dùng để kích hoạt Animation biến mất")]
    [SerializeField] private string animTriggerName = "Disappear";

    [Tooltip("Thời gian mờ từ 1 -> 0 (Dùng khi KHÔNG tích Đợi Animation)")]
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private Animator animator;


    // =========================================================
    // BIẾN NỘI BỘ (PRIVATE)
    // =========================================================

    private SpriteRenderer[] childSprites;

    private void Awake()
    {
        // Lấy tất cả SpriteRenderer của các bộ phận con
        childSprites = GetComponentsInChildren<SpriteRenderer>();

        // Tự động tìm Animator nếu chưa gán
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }

    private void Start()
    {
        // Bắt đầu chuỗi xử lý (Animation/Fade -> Tắt -> Hiện lại)
        StartCoroutine(ActionSequenceRoutine());
    }

    private IEnumerator ActionSequenceRoutine()
    {
        // 1. Chờ hết thời gian delay ban đầu
        yield return new WaitForSeconds(delayStartTime);

        // 2. Xử lý Chạy Animation hoặc Mờ dần (Fade)
        if (waitForAnimation && animator != null && !string.IsNullOrEmpty(animTriggerName))
        {
            // --- TRƯỜNG HỢP 1: CHẠY ANIMATION ---
            animator.SetTrigger(animTriggerName);

            // Chờ 1 frame để Animator chuyển state
            yield return null;

            // Lấy thời gian dài của Clip Animation hiện tại để chờ
            float animLength = GetCurrentAnimationDuration();
            yield return new WaitForSeconds(animLength);
        }
        else
        {
            // --- TRƯỜNG HỢP 2: LÀM MỜ FADE ALPHA ---
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                SetAlpha(currentAlpha);
                yield return null;
            }
            SetAlpha(0f);
        }

        // 3. Biến mất hoàn toàn (Active = false)
        // Tạo 1 Helper Runner ngoài Scene để đếm ngược thời gian vì Object bị Active False sẽ dừng Coroutine
        GameObject runnerObj = new GameObject("FadeRunner_Temp");
        FadeRunner runner = runnerObj.AddComponent<FadeRunner>();

        // Giao việc chờ hideDuration cho Runner và tắt Object hiện tại
        runner.StartReactivateTimer(this.gameObject, hideDuration, childSprites);
        this.gameObject.SetActive(false);
    }

    // Hàm hỗ trợ lấy độ dài (Giây) của Animation Clip đang chạy
    private float GetCurrentAnimationDuration()
    {
        if (animator == null) return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    private void SetAlpha(float alpha)
    {
        if (childSprites == null) return;

        foreach (SpriteRenderer sr in childSprites)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }
    }
}

// =========================================================
// HELPER RUNNER (TỰ ĐỘNG BẬT LẠI OBJECT KHI BỊ ACTIVE FALSE)
// =========================================================
public class FadeRunner : MonoBehaviour
{
    public void StartReactivateTimer(GameObject targetObj, float waitTime, SpriteRenderer[] sprites)
    {
        StartCoroutine(ReactivateRoutine(targetObj, waitTime, sprites));
    }

    private IEnumerator ReactivateRoutine(GameObject targetObj, float waitTime, SpriteRenderer[] sprites)
    {
        yield return new WaitForSeconds(waitTime);

        if (targetObj != null)
        {
            // Trả lại Alpha = 1 cho các Sprite
            if (sprites != null)
            {
                foreach (SpriteRenderer sr in sprites)
                {
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 1f;
                        sr.color = c;
                    }
                }
            }

            // Bật lại Object
            targetObj.SetActive(true);
        }

        // Xóa Object Runner tạm thời sau khi hoàn thành
        Destroy(this.gameObject);
    }
}