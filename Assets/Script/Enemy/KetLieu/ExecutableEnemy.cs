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

    [Header("=== ANIMATOR BOOL PARAMETER NAMES ===")]
    [Tooltip("Tên tham số Bool trong Animator cho trạng thái Choáng")]
    [SerializeField] private string isStunnedParam = "IsStunned";
    [Tooltip("Tên tham số Bool trong Animator cho trạng thái Đang Bị Kết Liễu")]
    [SerializeField] private string isBeingExecutedParam = "IsBeingExecuted";
    [Tooltip("Tên tham số Bool/Trigger trong Animator cho trạng thái Chết Sau Kết Liễu")]
    [SerializeField] private string isDie2Param = "IsDie2";

    [Header("=== VFX & UI GỢI Ý ===")]
    [SerializeField] private GameObject executionVFXPrefab;        // Effect bùng nổ/máu khi bị chém
    [SerializeField] private GameObject executePromptUI;           // Canvas/Icon "Press E" hiện trên đầu Boss

    private CharacterStats stats;
    private Animator anim;
    private bool isCanBeExecuted = false; // Đã chạm ngưỡng < 30% máu hay chưa
    private bool isBeingExecuted = false; // Đang trong quá trình diễn Animation kết liễu
    private Coroutine stunCoroutine;

    // Mã hóa tên Parameter sang ID Hash để tối ưu hiệu năng CPU
    private int isStunnedHash;
    private int isBeingExecutedHash;
    private int isDie2Hash;

    public bool IsCanBeExecuted => isCanBeExecuted;
    public bool IsBeingExecuted => isBeingExecuted;
    public Transform ExecutionPoint => executionPoint != null ? executionPoint : transform;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();

        // Chuyển đổi String sang Hash ID (Giúp Animator xử lý nhanh hơn)
        if (!string.IsNullOrEmpty(isStunnedParam)) isStunnedHash = Animator.StringToHash(isStunnedParam);
        if (!string.IsNullOrEmpty(isBeingExecutedParam)) isBeingExecutedHash = Animator.StringToHash(isBeingExecutedParam);
        if (!string.IsNullOrEmpty(isDie2Param)) isDie2Hash = Animator.StringToHash(isDie2Param);

        if (executePromptUI != null) executePromptUI.SetActive(false);
    }

    private void Update()
    {
        if (isBeingExecuted || stats == null) return;

        // KIỂM TRA ĐIỀU KIỆN MÁU DƯỚI 30%
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

        // Chuyển Animator Parameter 'IsStunned' sang true
        if (anim != null && isStunnedHash != 0)
        {
            anim.SetBool(isStunnedHash, true);
        }

        // Tự động đếm ngược hết thời gian choáng nếu Player không bấm E
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunTimerRoutine());
    }

    private IEnumerator StunTimerRoutine()
    {
        yield return new WaitForSeconds(stunDuration);

        // Hết thời gian choáng mà không bị kết liễu -> Trở lại bình thường
        ExitExecutionStun();
    }

    private void ExitExecutionStun()
    {
        isCanBeExecuted = false;
        if (executePromptUI != null) executePromptUI.SetActive(false);

        // Tắt Bool Choáng
        if (anim != null && isStunnedHash != 0)
        {
            anim.SetBool(isStunnedHash, false);
        }
    }

    // 2. KÍCH HOẠT QUÁ TRÌNH BỊ KẾT LIỄU (ĐƯỢC GỌI TỪ PLAYER)
    public void Execute(Transform playerTransform, System.Action onExecutionComplete)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        isCanBeExecuted = false;
        isBeingExecuted = true;

        if (executePromptUI != null) executePromptUI.SetActive(false);

        // Tắt trạng thái Choáng và Bật trạng thái Bị Kết Liễu
        if (anim != null)
        {
            if (isStunnedHash != 0) anim.SetBool(isStunnedHash, false);
            if (isBeingExecutedHash != 0) anim.SetBool(isBeingExecutedHash, true);
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

        // Chờ thời gian thực hiện animation kết liễu (2 giây)
        yield return new WaitForSeconds(2.0f);

        // Chuyển Animator Parameter 'IsBeingExecuted' sang false và 'IsDie2' sang true
        if (anim != null)
        {
            if (isBeingExecutedHash != 0) anim.SetBool(isBeingExecutedHash, false);
            if (isDie2Hash != 0) anim.SetBool(isDie2Hash, true);
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