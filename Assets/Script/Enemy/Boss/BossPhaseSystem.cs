using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BossPhaseData
{
    [Tooltip("Tên gọi đại diện cho Phase")]
    public string phaseName = "Phase 1";

    [Header("===== CẤU HÌNH MÁU (PRIMARY) =====")]
    [Tooltip("Màu sắc đại diện trên thanh Slider Máu chính (Nằm phía trên)")]
    public Color sliderColor = Color.red;

    [Tooltip("Số máu tối đa và lượng máu hồi lại khi bước vào Phase này")]
    public float maxHealth = 1000f;

    [Header("===== CẤU HÌNH SLIDER MÁU THỨ 2 (LAYERED HEALTH) =====")]
    [Tooltip("Màu sắc cho thanh Slider Máu thứ 2 (Nằm phía dưới làm nền/máu đệm)")]
    public Color secondarySliderColor = Color.yellow;

    [Header("===== THÔNG SỐ KHÁC =====")]
    [Tooltip("Tốc độ di chuyển ghi đè cho Boss ở Phase này")]
    public float overrideMoveSpeed = 3.5f;

    [Tooltip("Thời gian đứng yên hồi máu / chuyển Phase (Giây)")]
    public float regenDuration = 3f;

    [Header("===== TÙY CHỌN TÍNH CHẤT PHASE =====")]
    [Tooltip("Tick chọn nếu đây là Phase đầu tiên (Boss sẽ không đứng yên quỳ mà dí đánh Player ngay)")]
    public bool isFirstPhase = false;

    [Tooltip("Tick chọn nếu đây là Phase cuối cùng (Hết Phase này Boss sẽ chết ngay)")]
    public bool isLastPhase = false;
}

public class BossPhaseSystem : MonoBehaviour
{
    // =========================================================
    // CẤU HÌNH PHASES & UI & COLLIDER
    // =========================================================

    [Header("===== CẤU HÌNH PHASES =====")]
    [Tooltip("Mảng chứa thông tin các Phase của Boss")]
    [SerializeField] private BossPhaseData[] phases;

    [Header("===== ANIMATION DÙNG CHUNG =====")]
    [Tooltip("Tên Animation State quỳ/chuyển Phase (Gõ đúng tên State trong Animator)")]
    [SerializeField] private string phaseChangeAnimName = "PhaseChange";

    [Tooltip("Tên Animation State quay lại bình thường (State Idle của Animator)")]
    [SerializeField] private string idleAnimName = "Idle";

    [Header("===== UI THANH SLIDER MÁU =====")]
    [Tooltip("Gán Slider Máu chính của Boss vào đây (Lớp máu 1)")]
    [SerializeField] private Slider bossHealthSlider;

    [Tooltip("Gán Slider Máu thứ 2 của Boss vào đây (Lớp máu 2 / Thanh đệm máu)")]
    [SerializeField] private Slider bossSecondarySlider;

    [Tooltip("Tốc độ trượt đuổi theo máu của Slider thứ 2 (Nếu dùng làm hiệu ứng đệm)")]
    [SerializeField] private float secondaryCatchupSpeed = 2f;

    [Header("===== BẢO VỆ COLLIDER =====")]
    [Tooltip("Gán Collider2D của Boss vào đây để tự động bật/tắt khi chuyển Phase")]
    [SerializeField] private Collider2D bossCollider;


    // =========================================================
    // HIỂN THỊ THÔNG SỐ HIỆN TẠI (READ ONLY IN INSPECTOR)
    // =========================================================

    [Header("===== THÔNG SỐ HIỆN TẠI =====")]
    [SerializeField, ReadOnlyInspector] private int currentPhaseIndex = 0;
    [SerializeField, ReadOnlyInspector] private float currentSpeed = 0f;
    [SerializeField, ReadOnlyInspector] private bool isPhaseChanging = false;


    // =========================================================
    // COMPONENTS VÀ PROPERTIES
    // =========================================================

    private CharacterStats stats;
    private BossDaSatMaQuan bossController;
    private Animator animator;
    private bool isInitializing = false;
    private Coroutine secondarySliderCoroutine;

    // Property kiểm tra trạng thái chuyển Phase tối cao
    public bool IsPhaseChanging => isPhaseChanging;

    // Kiểm tra Phase hiện tại có phải Phase cuối không dựa theo Tick trong Data
    public bool IsCurrentPhaseLast => (phases != null && currentPhaseIndex < phases.Length) ? phases[currentPhaseIndex].isLastPhase : false;


    // =========================================================
    // KHỞI TẠO VÀ LẮNG NGHE SỰ KIỆN
    // =========================================================

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        bossController = GetComponent<BossDaSatMaQuan>();

        // Ưu tiên lấy Animator trực thuộc Boss
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (bossCollider == null)
        {
            bossCollider = GetComponent<Collider2D>();
        }
    }

    private void Start()
    {
        if (phases == null || phases.Length == 0)
        {
            Debug.LogError("❌ BossPhaseSystem: Chưa cấu hình danh sách Phases trong Inspector!");
            return;
        }

        StartCoroutine(Routine_InitializePhase1());
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= HandleHealthChanged;
        }
    }


    // =========================================================
    // NẠP PHASE BAN ĐẦU
    // =========================================================

    private IEnumerator Routine_InitializePhase1()
    {
        isInitializing = true;
        currentPhaseIndex = 0;
        BossPhaseData phase1 = phases[0];

        // Cập nhật thông số cơ bản & màu sắc 2 Slider máu
        SetSliderColors(phase1.sliderColor, phase1.secondarySliderColor);

        currentSpeed = phase1.overrideMoveSpeed;
        if (bossController != null) bossController.SetMoveSpeed(currentSpeed);

        if (stats != null)
        {
            stats.MaxHealth.Value = phase1.maxHealth;
        }

        // KIỂM TRA TICK PHASE ĐẦU
        if (phase1.isFirstPhase)
        {
            if (stats != null) stats.Heal(phase1.maxHealth);
            UpdateAllHealthSlidersDirectly(phase1.maxHealth, phase1.maxHealth);
            SetBossColliderActive(true);
        }
        else
        {
            SetBossColliderActive(false);

            // Bật Animation Chuyển Phase Quyền Lực
            PlayPhaseAnimation(phaseChangeAnimName);

            yield return StartCoroutine(Routine_FillHealthBar(phase1.maxHealth, phase1.regenDuration, false));

            // Dừng Animation Chuyển Phase và chuyển qua Animation Idle
            PlayOverrideAnimation(idleAnimName);
            SetBossColliderActive(true);
        }

        isInitializing = false;
    }


    // =========================================================
    // XỬ LÝ SỰ KIỆN THAY ĐỔI MÁU & CHUYỂN PHASE
    // =========================================================

    private void HandleHealthChanged(float currentHP, float maxHP)
    {
        // 1. Cập nhật UI Slider Máu
        UpdateHealthSliders(currentHP, maxHP);

        // 2. Kiểm tra chuyển Phase
        if (isInitializing || isPhaseChanging) return;

        if (currentHP <= 0f)
        {
            if (IsCurrentPhaseLast)
            {
                SetBossColliderActive(false);
                if (bossController != null) bossController.StopMoving();
                return;
            }

            StartCoroutine(Routine_TransitionToNextPhase());
        }
    }

    private IEnumerator Routine_TransitionToNextPhase()
    {
        isPhaseChanging = true;
        SetBossColliderActive(false);

        currentPhaseIndex++;
        if (currentPhaseIndex >= phases.Length)
        {
            isPhaseChanging = false;
            yield break;
        }

        BossPhaseData nextPhase = phases[currentPhaseIndex];

        if (bossController != null)
        {
            bossController.StopMoving();
        }

        // Cập nhật màu sắc 2 Slider theo Phase mới
        SetSliderColors(nextPhase.sliderColor, nextPhase.secondarySliderColor);

        if (stats != null)
        {
            stats.MaxHealth.Value = nextPhase.maxHealth;
        }

        currentSpeed = nextPhase.overrideMoveSpeed;
        if (bossController != null)
        {
            bossController.SetMoveSpeed(currentSpeed);
        }

        if (nextPhase.isFirstPhase)
        {
            if (stats != null) stats.Heal(nextPhase.maxHealth);
            UpdateAllHealthSlidersDirectly(nextPhase.maxHealth, nextPhase.maxHealth);
        }
        else
        {
            // BẬT ANIMATION CHUYỂN PHASE QUYỀN LỰC NHẤT
            PlayPhaseAnimation(phaseChangeAnimName);

            yield return StartCoroutine(Routine_FillHealthBar(nextPhase.maxHealth, nextPhase.regenDuration, true));
        }

        // DỪNG ANIMATION CHUYỂN PHASE VÀ CHUYỂN SANG IDLE HOẶC ANIMATION MỚI
        PlayOverrideAnimation(idleAnimName);

        SetBossColliderActive(true);
        isPhaseChanging = false;
    }


    // =========================================================
    // QUẢN LÝ ANIMATION QUYỀN LỰC
    // =========================================================

    /// <summary>
    /// Kích hoạt Animation chuyển Phase với quyền ưu tiên cao nhất
    /// </summary>
    private void PlayPhaseAnimation(string animName)
    {
        if (animator != null && !string.IsNullOrEmpty(animName))
        {
            isPhaseChanging = true;
            animator.Play(animName, 0, 0f);
            animator.Update(0f); // Ép Animator cập nhật ngay lập tức
        }
    }

    /// <summary>
    /// Hàm Public cho phép Script Boss hoặc hệ thống gọi ngắt Animation Chuyển Phase để chuyển sang Animation khác
    /// </summary>
    public void PlayOverrideAnimation(string targetAnimName)
    {
        if (animator != null && !string.IsNullOrEmpty(targetAnimName))
        {
            animator.Play(targetAnimName, 0, 0f);
        }
    }


    // =========================================================
    // HELPER: CẬP NHẬT ĐỒNG BỘ 2 SLIDER MÁU
    // =========================================================

    private void UpdateHealthSliders(float currentHealth, float maxHealth)
    {
        // Slider chính (cập nhật ngay lập tức)
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = currentHealth;
        }

        // Slider thứ 2 (chạy mượt theo sau để làm hiệu ứng thanh máu 2/đệm máu)
        if (bossSecondarySlider != null)
        {
            bossSecondarySlider.maxValue = maxHealth;

            if (secondarySliderCoroutine != null)
            {
                StopCoroutine(secondarySliderCoroutine);
            }
            secondarySliderCoroutine = StartCoroutine(Routine_SmoothSecondarySlider(currentHealth));
        }
    }

    private void UpdateAllHealthSlidersDirectly(float currentHealth, float maxHealth)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = currentHealth;
        }

        if (bossSecondarySlider != null)
        {
            bossSecondarySlider.maxValue = maxHealth;
            bossSecondarySlider.value = currentHealth;
        }
    }

    private IEnumerator Routine_SmoothSecondarySlider(float targetValue)
    {
        while (Mathf.Abs(bossSecondarySlider.value - targetValue) > 0.01f)
        {
            bossSecondarySlider.value = Mathf.Lerp(bossSecondarySlider.value, targetValue, Time.deltaTime * secondaryCatchupSpeed);
            yield return null;
        }
        bossSecondarySlider.value = targetValue;
    }


    // =========================================================
    // HELPER: BẬT / TẮT COLLIDER
    // =========================================================

    public void SetBossColliderActive(bool active)
    {
        if (bossCollider != null)
        {
            bossCollider.enabled = active;
        }
    }


    // =========================================================
    // HELPER: ĐỔI MÀU 2 SLIDER & HỒI MÁU
    // =========================================================

    private void SetSliderColors(Color healthColor, Color secondaryColor)
    {
        // Đổi màu Slider Máu chính
        if (bossHealthSlider != null && bossHealthSlider.fillRect != null)
        {
            Image fillImage = bossHealthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                healthColor.a = 1f;
                fillImage.color = healthColor;
            }
        }

        // Đổi màu Slider Máu thứ 2
        if (bossSecondarySlider != null && bossSecondarySlider.fillRect != null)
        {
            Image fillImage = bossSecondarySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                secondaryColor.a = 1f;
                fillImage.color = secondaryColor;
            }
        }
    }

    private IEnumerator Routine_FillHealthBar(float targetMaxHealth, float duration, bool forceStopBoss)
    {
        float timer = 0f;

        UpdateAllHealthSlidersDirectly(0f, targetMaxHealth);

        if (stats != null)
        {
            float currentHP = stats.CurrentHealth;
            if (currentHP > 0)
            {
                stats.TakeDamage(currentHP);
            }
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float visualHealth = Mathf.Lerp(0f, targetMaxHealth, progress);

            UpdateAllHealthSlidersDirectly(visualHealth, targetMaxHealth);

            if (stats != null)
            {
                float amountToHeal = visualHealth - stats.CurrentHealth;
                if (amountToHeal > 0)
                {
                    stats.Heal(amountToHeal);
                }
            }

            if (forceStopBoss && bossController != null)
            {
                bossController.StopMoving();
            }

            yield return null;
        }

        if (stats != null)
        {
            stats.Heal(targetMaxHealth);
        }

        UpdateAllHealthSlidersDirectly(targetMaxHealth, targetMaxHealth);
    }
}