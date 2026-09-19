using System.Collections;
using UnityEngine;

public class Elite_TyHuu : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẤN CÔNG ---")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.5f;

    [Header("--- KỸ NĂNG 1: STONE BREATH (BẮN ĐÁ) ---")]
    [SerializeField] private float skillCooldown = 6f;
    [SerializeField] private float windupTime = 1.5f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private SimpleObjectPool stoneProjectilePool;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int shootsBeforeRoar = 3;

    [Header("--- KỸ NĂNG 2: ROAR SKILL (GẦM) ---")]
    [SerializeField] private float roarWindupTime = 1f;
    [SerializeField] private float roarDuration = 2f;
    [SerializeField] private AudioClip sfxRoar; // ÂM THANH GẦM
    [SerializeField] private float roarCameraShakeIntensity = 2.5f;
    [SerializeField] private float roarCameraShakeDuration = 0.8f;
    [SerializeField] private float playerSlowMultiplier = 0.3f;
    [SerializeField] private float playerSlowDuration = 2.5f;
    [SerializeField] private float roarAffectRadius = 10f;

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPrepareAttack;
    [SerializeField] private AudioClip sfxClaw1; // Bổ sung âm thanh vuốt 1
    [SerializeField] private AudioClip sfxClaw2; // Bổ sung âm thanh vuốt 2
    [SerializeField] private AudioClip sfxBreath; // Âm thanh khi bắn đá
    [SerializeField] private AudioClip sfxStun;
    [SerializeField] private AudioClip sfxDeath;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animWalk = "isWalking";
    [SerializeField] private string animClaw1 = "Claw1";
    [SerializeField] private string animClaw2 = "Claw2";
    [SerializeField] private string animAttack = "Attack";
    [SerializeField] private string animIsRoaring = "isRoaring";
    [SerializeField] private string animIsStunned = "IsStunned";

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;
    private float skillTimer;

    private int shootCount = 0;
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isStunned = false;
    private bool isBeingExecuted = false;
    private bool isDeadHandled = false;

    private Coroutine currentBehaviorCoroutine;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();
        mainCollider = GetComponent<Collider2D>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Cấu hình chống mất tiếng / méo tiếng 2D
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;     // Ép về 2D Audio chuẩn
            audioSource.bypassEffects = true;
        }
    }

    private void Start()
    {
        skillTimer = skillCooldown;
        shootCount = 0;
    }

    private void Update()
    {
        if (CheckAndHandleDeath()) return;

        // KHÓA TUYỆT ĐỐI: Nếu Choáng, Bị kết liễu hoặc Bận -> Dừng toàn bộ AI
        if (isStunned || isBeingExecuted || isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        Vector3 attackCenter = GetAttackCenter();
        float distance = Vector2.Distance(attackCenter, playerTransform.position);
        FlipTowards(playerTransform.position);

        if (isWindingUp)
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
            return;
        }

        if (skillTimer <= 0)
        {
            if (shootCount < shootsBeforeRoar)
            {
                currentBehaviorCoroutine = StartCoroutine(Routine_StoneBreath());
            }
            else
            {
                currentBehaviorCoroutine = StartCoroutine(Routine_RoarSkill());
            }
        }
        else if (distance <= attackRange)
        {
            currentBehaviorCoroutine = StartCoroutine(Routine_DoubleClaw());
        }
        else
        {
            if (animator != null) animator.SetBool(animWalk, true);
            MoveSmoothly(playerTransform.position, moveSpeed);
        }
    }

    private bool CheckAndHandleDeath()
    {
        if (stats != null && stats.IsDead)
        {
            if (!isDeadHandled)
            {
                isDeadHandled = true;
                InterruptAllActions();
                if (mainCollider != null) mainCollider.enabled = false;
                PlaySFX(sfxDeath);
            }
            return true;
        }
        return false;
    }

    public void InterruptAllActions()
    {
        StopAllCoroutines();
        currentBehaviorCoroutine = null;

        isBusy = true;
        isWindingUp = false;

        if (animator != null)
        {
            animator.SetBool(animWalk, false);
            animator.SetBool(animIsRoaring, false);
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDeadHandled || isBeingExecuted) return;

        InterruptAllActions();
        currentBehaviorCoroutine = StartCoroutine(Routine_GetStunned(duration));
    }

    public void EndStun()
    {
        if (isBeingExecuted || isDeadHandled) return;

        SetAnimatorBoolSafe(animIsStunned, false);
        isStunned = false;
        isBusy = false;
        isWindingUp = false;
    }

    private IEnumerator Routine_GetStunned(float duration)
    {
        isStunned = true;
        isBusy = true;
        isWindingUp = false;

        if (animator != null)
        {
            animator.SetBool(animWalk, false);
            animator.SetBool(animIsRoaring, false);
            SetAnimatorBoolSafe(animIsStunned, true);
        }

        PlaySFX(sfxStun);

        yield return new WaitForSeconds(duration);

        EndStun();
    }

    public void OnStartExecution()
    {
        InterruptAllActions();
        isBeingExecuted = true;
        isStunned = false;
        isBusy = true;
        isWindingUp = false;

        SetAnimatorBoolSafe(animIsStunned, false);
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 1.5f)
            {
                playerTransform = null;
                if (animator != null) animator.SetBool(animWalk, false);
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void MoveSmoothly(Vector3 targetPosition, float speed)
    {
        // Khóa tuyệt đối di chuyển khi Stun hoặc Bị kết liễu
        if (isStunned || isBeingExecuted) return;

        Vector2 currentPos = transform.position;
        Vector2 dirToTarget = ((Vector2)targetPosition - currentPos).normalized;
        Vector2 moveDir = dirToTarget;

        RaycastHit2D hit = Physics2D.CircleCast(currentPos, avoidRadius, dirToTarget, 1f, obstacleLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Vector2 slideDir = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(dirToTarget, slideDir) < 0) slideDir = -slideDir;
            moveDir = (dirToTarget + slideDir * 1.5f).normalized;
        }

        transform.position += (Vector3)(moveDir * (speed * Time.deltaTime));
    }

    private IEnumerator Routine_DoubleClaw()
    {
        isBusy = true;
        if (animator != null) animator.SetBool(animWalk, false);

        PlaySFX(sfxPrepareAttack);
        yield return new WaitForSeconds(0.2f);

        if (isStunned || isBeingExecuted || isDeadHandled) yield break;

        // Phát Claw 1
        if (animator != null) animator.SetTrigger(animClaw1);
        PlaySFX(sfxClaw1);
        yield return new WaitForSeconds(0.4f);

        if (isStunned || isBeingExecuted || isDeadHandled) yield break;

        // Phát Claw 2
        if (animator != null) animator.SetTrigger(animClaw2);
        PlaySFX(sfxClaw2);
        yield return new WaitForSeconds(0.6f);

        isBusy = false;
    }

    private IEnumerator Routine_StoneBreath()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator != null ? animator.speed : 1f;
        moveSpeed *= slowMultiplier;
        if (animator != null) animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        if (isStunned || isBeingExecuted || isDeadHandled)
        {
            moveSpeed = originalSpeed;
            if (animator != null) animator.speed = originalAnimSpeed;
            isWindingUp = false;
            yield break;
        }

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        if (animator != null) animator.speed = originalAnimSpeed;

        if (animator != null)
        {
            animator.SetBool(animWalk, false);
            animator.SetTrigger(animAttack);
        }
        PlaySFX(sfxBreath);

        yield return new WaitForSeconds(0.3f);

        if (isStunned || isBeingExecuted || isDeadHandled) yield break;

        float[] angles = { -15f, 0f, 15f };
        Vector3 spawnPos = mouthPoint != null ? mouthPoint.position : GetAttackCenter();
        Vector2 baseDir = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 finalDir = rotation * baseDir;

            if (stoneProjectilePool != null)
            {
                GameObject stone = stoneProjectilePool.GetFromPool(spawnPos, Quaternion.identity);
                if (stone != null && stone.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    rb.linearVelocity = finalDir * projectileSpeed;
                }
            }
        }

        shootCount++;

        yield return new WaitForSeconds(0.8f);
        isBusy = false;
    }

    private IEnumerator Routine_RoarSkill()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator != null ? animator.speed : 1f;
        moveSpeed *= slowMultiplier;
        if (animator != null) animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(roarWindupTime);

        if (isStunned || isBeingExecuted || isDeadHandled)
        {
            moveSpeed = originalSpeed;
            if (animator != null) animator.speed = originalAnimSpeed;
            isWindingUp = false;
            yield break;
        }

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        if (animator != null) animator.speed = originalAnimSpeed;

        if (animator != null)
        {
            animator.SetBool(animWalk, false);
            animator.SetBool(animIsRoaring, true);
        }

        ExecuteRoarEffects();

        yield return new WaitForSeconds(roarDuration);

        if (animator != null) animator.SetBool(animIsRoaring, false);

        shootCount = 0;
        isBusy = false;
    }

    private void ExecuteRoarEffects()
    {
        // Phát tiếng gầm Roar
        PlaySFX(sfxRoar);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(roarCameraShakeIntensity, roarCameraShakeDuration);
        }

        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer <= roarAffectRadius)
            {
                if (playerTransform.TryGetComponent<PlayerController>(out PlayerController playerController))
                {
                    playerController.ApplySlow(playerSlowMultiplier, playerSlowDuration);
                }
            }
        }
    }

    private void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void FlipTowards(Vector3 target)
    {
        if (isStunned || isBeingExecuted) return;
        Vector3 scale = transform.localScale;
        scale.x = target.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public Vector3 GetAttackCenter()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(attackOffset.x * direction, attackOffset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, roarAffectRadius);
    }
}