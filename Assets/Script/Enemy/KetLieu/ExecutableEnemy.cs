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

    [Header("=== TÊN STATE ANIMATION (GÕ ĐÚNG TÊN STATE TRONG ANIMATOR) ===")]
    [Tooltip("Tên State Choáng trong Animator")]
    [SerializeField] private string stunStateName = "Stun";
    [Tooltip("Tên State Bị Kết Liễu trong Animator")]
    [SerializeField] private string executionStateName = "BeingExecuted";
    [Tooltip("Tên State Chết 2 trong Animator (ĐỂ TRỐNG nếu quái không có animation này)")]
    [SerializeField] private string die2StateName = "Die2";

    [Header("=== VFX & UI GỢI Ý ===")]
    [SerializeField] private GameObject executionVFXPrefab;
    [Tooltip("Kéo điểm Spawn VFX (VD: ngực/tim quái) vào đây. Nếu để trống sẽ sinh ra ở ExecutionPoint")]
    [SerializeField] private Transform vfxSpawnPoint;
    [SerializeField] private GameObject executePromptUI;
    [SerializeField] private GameObject stunVFXObject;

    private CharacterStats stats;
    private Animator anim;
    private Elite_TyHuu aiTyHuu;
    private Elite_TongQuan aiTongQuan;

    private bool isCanBeExecuted = false;
    private bool isBeingExecuted = false;
    private Coroutine stunCoroutine;

    public bool IsCanBeExecuted => isCanBeExecuted;
    public bool IsBeingExecuted => isBeingExecuted;
    public bool IsStunned => isCanBeExecuted || isBeingExecuted;
    public Transform ExecutionPoint => executionPoint != null ? executionPoint : transform;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();
        aiTyHuu = GetComponent<Elite_TyHuu>();
        aiTongQuan = GetComponent<Elite_TongQuan>();

        if (executePromptUI != null) executePromptUI.SetActive(false);
        if (stunVFXObject != null) stunVFXObject.SetActive(false);
    }

    private void Update()
    {
        if (isBeingExecuted || isCanBeExecuted || stats == null) return;

        float maxHp = stats.MaxHealth != null ? stats.MaxHealth.Value : 1f;
        float currentHealthPercent = (float)stats.CurrentHealth / maxHp;

        if (!isCanBeExecuted && currentHealthPercent <= executionHealthThreshold && currentHealthPercent > 0)
        {
            EnterExecutionStun();
        }
    }

    private void EnterExecutionStun()
    {
        isCanBeExecuted = true;

        if (executePromptUI != null) executePromptUI.SetActive(true);
        if (stunVFXObject != null) stunVFXObject.SetActive(true);

        PlayAnimationDirectly(stunStateName);

        if (aiTyHuu != null) aiTyHuu.ApplyStun(stunDuration);
        if (aiTongQuan != null) aiTongQuan.ApplyExecutionStun();

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunTimerRoutine());
    }

    private IEnumerator StunTimerRoutine()
    {
        yield return new WaitForSeconds(stunDuration);
        ExitExecutionStun();
    }

    private void ExitExecutionStun()
    {
        isCanBeExecuted = false;

        if (executePromptUI != null) executePromptUI.SetActive(false);
        if (stunVFXObject != null) stunVFXObject.SetActive(false);

        if (aiTyHuu != null) aiTyHuu.EndStun();
        if (aiTongQuan != null) aiTongQuan.EndStun();
    }

    public void Execute(Transform playerTransform, System.Action onExecutionComplete)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        isCanBeExecuted = false;
        isBeingExecuted = true;

        if (executePromptUI != null) executePromptUI.SetActive(false);

        if (aiTyHuu != null) aiTyHuu.OnStartExecution();
        if (aiTongQuan != null) aiTongQuan.OnStartExecution();

        PlayAnimationDirectly(executionStateName);

        StartCoroutine(ExecutionProcessRoutine(onExecutionComplete));
    }

    private IEnumerator ExecutionProcessRoutine(System.Action onExecutionComplete)
    {
        // SINH VFX KẾT LIỄU CHUẨN VỊ TRÍ & HƯỚNG QUAY
        if (executionVFXPrefab != null)
        {
            Transform targetPoint = vfxSpawnPoint != null ? vfxSpawnPoint : (executionPoint != null ? executionPoint : transform);
            GameObject vfx = Instantiate(executionVFXPrefab, targetPoint.position, targetPoint.rotation);

            // Đồng bộ Scale X để VFX lật theo hướng quay mặt của Quái
            Vector3 vfxScale = vfx.transform.localScale;
            vfxScale.x *= Mathf.Sign(transform.localScale.x != 0 ? transform.localScale.x : 1f);
            vfx.transform.localScale = vfxScale;
        }

        // Chờ 2.0 giây cho Animation đâm chém kết liễu diễn ra
        yield return new WaitForSeconds(2.0f);

        if (stunVFXObject != null) stunVFXObject.SetActive(false);

        // ✨ KIỂM TRA TÙY CHỌN DIE2: Chỉ gọi nếu biến die2StateName có điền tên và State thực sự tồn tại trong Animator
        if (!string.IsNullOrEmpty(die2StateName) && HasState(anim, die2StateName))
        {
            PlayAnimationDirectly(die2StateName);
        }

        // Sau đó trừ máu về 0 để quái chính thức kết thúc
        if (stats != null && !stats.IsDead)
        {
            stats.TakeDamage(stats.CurrentHealth + 9999f);
        }

        onExecutionComplete?.Invoke();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3.0f);
    }

    // Hàm bổ trợ gọi ép phát Animation theo tên State
    private void PlayAnimationDirectly(string stateName)
    {
        if (anim != null && !string.IsNullOrEmpty(stateName))
        {
            anim.Play(stateName, 0, 0f);
        }
    }

    // ✨ Hàm kiểm tra an toàn xem Animator có chứa State cần tìm hay không
    private bool HasState(Animator animator, string stateName)
    {
        if (animator == null) return false;
        // Kiểm tra layer 0 xem có chứa Hash của state đó không
        return animator.HasState(0, Animator.StringToHash(stateName));
    }
}