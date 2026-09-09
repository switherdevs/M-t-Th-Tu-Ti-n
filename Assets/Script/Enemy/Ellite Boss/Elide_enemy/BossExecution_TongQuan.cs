using System.Collections;
using StatsSystem.Components;
using UnityEngine;

[RequireComponent(typeof(Elite_TongQuan))]
public class BossExecution_TongQuan : MonoBehaviour
{
    [Header("=== CẤU HÌNH NGƯỠNG KẾT LIỄU ===")]
    [Tooltip("Phần trăm máu để Boss rơi vào trạng thái choáng kết liễu (0.3 = 30%)")]
    [SerializeField] private float executionHealthThreshold = 0.3f;
    [Tooltip("Thời gian Boss đứng chờ bị kết liễu (giây)")]
    [SerializeField] private float stunDuration = 10f;
    [Tooltip("Vị trí đứng của Player khi bấm kết liễu")]
    [SerializeField] private Transform executionPoint;

    [Header("=== TÊN CÁC STATE ANIMATION ===")]
    [SerializeField] private string stunStateName = "Stun";
    [SerializeField] private string executionStateName = "BeingExecuted";
    [SerializeField] private string die2StateName = "Die2";

    [Header("=== UI & HIỆU ỨNG (VFX) ===")]
    [SerializeField] private GameObject executePromptUI;
    [SerializeField] private GameObject stunVFXObject;
    [SerializeField] private GameObject executionVFXPrefab;
    [Tooltip("Kéo điểm Spawn VFX (VD: ngực/tim Boss) vào đây. Nếu để trống sẽ sinh ra ở ExecutionPoint")]
    [SerializeField] private Transform vfxSpawnPoint;

    private CharacterStats stats;
    private Animator anim;
    private Elite_TongQuan bossAI;

    private bool isCanBeExecuted = false;
    private bool isBeingExecuted = false;
    private Coroutine stunTimerCoroutine;

    public bool IsCanBeExecuted => isCanBeExecuted;
    public bool IsBeingExecuted => isBeingExecuted;
    public Transform ExecutionPoint => executionPoint != null ? executionPoint : transform;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();
        bossAI = GetComponent<Elite_TongQuan>();

        ResetDefaultVisuals();
    }

    private void Update()
    {
        if (isBeingExecuted || isCanBeExecuted || stats == null || stats.IsDead) return;

        CheckExecutionThreshold();
    }

    private void CheckExecutionThreshold()
    {
        float maxHp = stats.MaxHealth != null ? stats.MaxHealth.Value : 1f;
        float currentHealthPercent = (float)stats.CurrentHealth / maxHp;

        if (currentHealthPercent <= executionHealthThreshold && currentHealthPercent > 0)
        {
            EnterExecutionStun();
        }
    }

    private void EnterExecutionStun()
    {
        isCanBeExecuted = true;

        SetVisualsState(true);
        ForceFreezeAI();
        PlayAnimationDirectly(stunStateName);

        if (stunTimerCoroutine != null) StopCoroutine(stunTimerCoroutine);
        stunTimerCoroutine = StartCoroutine(Routine_StunTimer());
    }

    private IEnumerator Routine_StunTimer()
    {
        yield return new WaitForSeconds(stunDuration);
        ExitExecutionStun();
    }

    private void ExitExecutionStun()
    {
        isCanBeExecuted = false;

        SetVisualsState(false);

        if (bossAI != null)
        {
            bossAI.enabled = true;
            bossAI.EndStun();
        }
    }

    public void ExecuteBoss(Transform playerTransform, System.Action onExecutionComplete)
    {
        if (stunTimerCoroutine != null) StopCoroutine(stunTimerCoroutine);

        isCanBeExecuted = false;
        isBeingExecuted = true;

        if (executePromptUI != null) executePromptUI.SetActive(false);

        ForceFreezeAI();
        PlayAnimationDirectly(executionStateName);

        StartCoroutine(Routine_ExecutionProcess(onExecutionComplete));
    }

    private IEnumerator Routine_ExecutionProcess(System.Action onExecutionComplete)
    {
        // SINH VFX KẾT LIỄU CHUẨN VỊ TRÍ & HƯỚNG QUAY BOSS
        if (executionVFXPrefab != null)
        {
            Transform targetPoint = vfxSpawnPoint != null ? vfxSpawnPoint : (executionPoint != null ? executionPoint : transform);
            GameObject vfx = Instantiate(executionVFXPrefab, targetPoint.position, targetPoint.rotation);

            // Đồng bộ Scale X để VFX lật theo hướng quay mặt của Boss
            Vector3 vfxScale = vfx.transform.localScale;
            vfxScale.x *= Mathf.Sign(transform.localScale.x != 0 ? transform.localScale.x : 1f);
            vfx.transform.localScale = vfxScale;
        }

        if (stunVFXObject != null) stunVFXObject.SetActive(false);

        yield return new WaitForSeconds(2.0f);

        if (stats != null && !stats.IsDead)
        {
            stats.TakeDamage(stats.CurrentHealth + 9999f);
        }

        PlayAnimationDirectly(die2StateName);

        onExecutionComplete?.Invoke();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Destroy(gameObject, 3.0f);
    }

    private void ResetDefaultVisuals()
    {
        SetVisualsState(false);
    }

    private void SetVisualsState(bool active)
    {
        if (executePromptUI != null) executePromptUI.SetActive(active);
        if (stunVFXObject != null) stunVFXObject.SetActive(active);
    }

    private void ForceFreezeAI()
    {
        StopAllCoroutines();
        if (bossAI != null)
        {
            bossAI.StopAllCoroutines();
            bossAI.enabled = false;
        }
    }

    private void PlayAnimationDirectly(string stateName)
    {
        if (anim != null && !string.IsNullOrEmpty(stateName))
        {
            anim.Play(stateName, 0, 0f);
        }
    }
}