using System.Collections;
using StatsSystem.Components;
using UnityEngine;

public class ExecutableEnemy : MonoBehaviour
{
    [Header("=== CẤU HÌNH CHOÁNG KẾT LIỄU ===")]
    [Tooltip("Phần trăm máu để chuyển sang trạng thái choáng (VD: 0.3 = 30%)")]
    [SerializeField] private float executionHealthThreshold = 0.3f;
    [Tooltip("Thời gian quái đứng choáng chờ kết liễu (giây)")]
    [SerializeField] private float stunDuration = 5f;
    [Tooltip("Điểm đứng chuẩn của Player khi thực hiện kết liễu")]
    [SerializeField] private Transform executionPoint;

    [Header("=== ANIMATION & VFX ===")]
    [SerializeField] private string stunAnimName = "Stunned";       // Anim đứng chịu đòn
    [SerializeField] private string beingExecutedAnimName = "BeingExecuted"; // Anim đang bị chém
    [SerializeField] private string die2AnimName = "die 2";        // Anim chết sau khi kết liễu
    [SerializeField] private GameObject executionVFXPrefab;        // Effect bùng nổ/máu khi bị chém

    [Header("=== UI GỢI Ý ===")]
    [SerializeField] private GameObject executePromptUI;           // Canvas/Icon "Press E" hiện trên đầu Boss

    private CharacterStats stats;
    private Animator anim;
    private bool isCanBeExecuted = false; // Đã chạm ngưỡng < 30% máu hay chưa
    private bool isBeingExecuted = false; // Đang trong quá trình diễn Animation kết liễu
    private Coroutine stunCoroutine;

    public bool IsCanBeExecuted => isCanBeExecuted;
    public bool IsBeingExecuted => isBeingExecuted;
    public Transform ExecutionPoint => executionPoint != null ? executionPoint : transform;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();

        if (executePromptUI != null) executePromptUI.SetActive(false);
    }

    private void Update()
    {
        if (isBeingExecuted || stats == null) return;

        // KIỂM TRA ĐIỀU KIỆN MÁU DƯỚI 30% (ĐÃ SỬA LỖI TRUY CẬP STAT VALUE)
        float maxHp = stats.MaxHealth != null ? stats.MaxHealth.Value : 1f; // Tránh lỗi chia cho 0 hoặc null
        float currentHealthPercent = (float)stats.CurrentHealth / maxHp;

        if (!isCanBeExecuted && currentHealthPercent <= executionHealthThreshold && currentHealthPercent > 0)
        {
            EnterExecutionStun();
        }
    }

    // 1. CHUYỂN SANG TRẠNG THÁI CHOÁNG KẾT LIỄU
    private void EnterExecutionStun()
    {
        isCanBeExecuted = true;
        if (executePromptUI != null) executePromptUI.SetActive(true);

        if (anim != null && !string.IsNullOrEmpty(stunAnimName))
        {
            anim.Play(stunAnimName);
        }

        // Tự động đếm ngược hết thời gian choáng nếu Player không bấm E
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunTimerRoutine());
    }

    private IEnumerator StunTimerRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        // Hết thời gian choáng mà không bị kết liễu -> Hồi lại 1 ít máu hoặc trở lại bình thường
        ExitExecutionStun();
    }

    private void ExitExecutionStun()
    {
        isCanBeExecuted = false;
        if (executePromptUI != null) executePromptUI.SetActive(false);
    }

    // 2. KÍCH HOẠT QUÁ TRÌNH BỊ KẾT LIỄU (ĐƯỢC GỌI TỪ PLAYER)
    public void Execute(Transform playerTransform, System.Action onExecutionComplete)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        isCanBeExecuted = false;
        isBeingExecuted = true;

        if (executePromptUI != null) executePromptUI.SetActive(false);

        // Phát Animation đang bị kết liễu
        if (anim != null && !string.IsNullOrEmpty(beingExecutedAnimName))
        {
            anim.Play(beingExecutedAnimName);
        }

        StartCoroutine(ExecutionProcessRoutine(onExecutionComplete));
    }

    private IEnumerator ExecutionProcessRoutine(System.Action onExecutionComplete)
    {
        // Sinh ra hiệu ứng VFX tại vị trí quái
        if (executionVFXPrefab != null)
        {
            Instantiate(executionVFXPrefab, transform.position, Quaternion.identity);
        }

        // Chờ thời gian thực hiện animation kết liễu (Ví dụ: 2 giây)
        yield return new WaitForSeconds(2.0f);

        // Chuyển sang Animation die 2
        if (anim != null && !string.IsNullOrEmpty(die2AnimName))
        {
            anim.Play(die2AnimName);
        }

        // Báo cho Player biết đã xong để mở khóa di chuyển
        onExecutionComplete?.Invoke();

        // Hủy bớt Collider để không cản đường
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Xử lý chết chính thức (Destroy sau 3s)
        Destroy(gameObject, 3.0f);
    }
}