using System.Collections;
using UnityEngine;

public class Elite_TongQuan : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẤN CÔNG ---")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.6f;

    [Header("--- KỸ NĂNG JUMP SLAM ---")]
    [SerializeField] private float skillCooldown = 8f;
    [SerializeField] private float windupTime = 1.5f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private Transform slamHitboxPoint;

    [Header("--- KỸ NĂNG ĐỠ ĐÒN & CHOÁNG ---")]
    [SerializeField] private float blockDuration = 2.5f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private GameObject blockShieldPrefab;
    [SerializeField] private Transform shieldSpawnPoint;
    [SerializeField] private string playerSwordTag = "PlayerSword";

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPrepareAttack;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxJumpLaunch; // NÂNG CẤP: Âm thanh khi bắt đầu nhảy
    [SerializeField] private AudioClip sfxLandSlam;   // NÂNG CẤP: Âm thanh khi tiếp đất giậm nổ
    [SerializeField] private AudioClip sfxBlock;
    [SerializeField] private AudioClip sfxStun;
    [SerializeField] private AudioClip sfxDeath;

    [Header("--- ANIMATION PARAMETERS ---")]
    [SerializeField] private string boolIsWalking = "boolIsWalking";
    [SerializeField] private string triggerAttack = "triggerAttack";
    [SerializeField] private string triggerPrepareJump = "triggerPrepareJump";
    [SerializeField] private string triggerJumpAir = "triggerJumpAir";
    [SerializeField] private string triggerLand = "triggerLand";
    [SerializeField] private string boolIsBlocking = "boolIsBlocking";
    [SerializeField] private string boolIsStunned = "boolIsStunned";

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;
    private float skillTimer;

    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isBlocking = false;
    private bool isStunned = false;
    private bool isExecutionStunned = false;
    private bool isBeingExecuted = false;
    private bool isDeadHandled = false;

    private GameObject currentSpawnedShield;
    private Coroutine currentBehaviorCoroutine;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();
        mainCollider = GetComponent<Collider2D>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // CẤU HÌNH CHỐNG TẮT/MÉO TIẾNG KHI QUÁI ÁP SÁT
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;     // Ép về âm thanh 2D chuẩn
            audioSource.bypassEffects = true;  // Bỏ qua lọc môi trường
        }
    }

    private void Start()
    {
        skillTimer = skillCooldown;

        if (stats != null)
        {
            stats.OnDamaged += HandleDamaged;
        }
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDamaged -= HandleDamaged;
        }
        DestroyCurrentShield();
    }

    private void Update()
    {
        if (CheckAndHandleDeath()) return;

        // BẮT BUỘC KHÓA HOÀN TOÀN KHI DÍNH CÁC TRẠNG THÁI KHỐNG CHẾ
        if (isExecutionStunned || isBeingExecuted || isStunned || isBusy) return;

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
            currentBehaviorCoroutine = StartCoroutine(Routine_BlockThenJumpSlam());
        }
        else if (distance <= attackRange)
        {
            currentBehaviorCoroutine = StartCoroutine(Routine_NormalAttack());
        }
        else
        {
            if (animator != null) animator.SetBool(boolIsWalking, true);
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

    private void InterruptAllActions()
    {
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
            currentBehaviorCoroutine = null;
        }

        isBusy = true;
        isWindingUp = false;
        isBlocking = false;

        DestroyCurrentShield();

        if (animator != null)
        {
            animator.SetBool(boolIsWalking, false);
            animator.SetBool(boolIsBlocking, false);

            animator.ResetTrigger(triggerAttack);
            animator.ResetTrigger(triggerPrepareJump);
            animator.ResetTrigger(triggerJumpAir);
            animator.ResetTrigger(triggerLand);
        }
    }

    private void HandleDamaged(float damageTaken)
    {
        if (isExecutionStunned || isBeingExecuted) return;

        if (isBlocking && !isStunned && !stats.IsDead)
        {
            ApplyStun(stunDuration);
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDeadHandled || isBeingExecuted || isExecutionStunned) return;

        InterruptAllActions();
        currentBehaviorCoroutine = StartCoroutine(Routine_GetStunned(duration));
    }

    public void ApplyExecutionStun()
    {
        if (isDeadHandled || isBeingExecuted) return;

        InterruptAllActions();

        isExecutionStunned = true;
        isStunned = true;
        isBusy = true;

        if (animator != null) animator.SetBool(boolIsStunned, true);
        PlaySFX(sfxStun);
    }

    public void EndStun()
    {
        if (isBeingExecuted || isDeadHandled) return;

        if (animator != null) animator.SetBool(boolIsStunned, false);

        isExecutionStunned = false;
        isStunned = false;
        isBusy = false;
    }

    private IEnumerator Routine_GetStunned(float duration)
    {
        isStunned = true;
        isBusy = true;

        if (animator != null) animator.SetBool(boolIsStunned, true);
        PlaySFX(sfxStun);

        yield return new WaitForSeconds(duration);

        EndStun();
    }

    public void OnStartExecution()
    {
        InterruptAllActions();

        isBeingExecuted = true;
        isExecutionStunned = false;
        isStunned = false;
        isBusy = true;

        if (animator != null) animator.SetBool(boolIsStunned, false);
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 1.5f)
            {
                playerTransform = null;
                if (animator != null) animator.SetBool(boolIsWalking, false);
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void MoveSmoothly(Vector3 targetPosition, float speed)
    {
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

    private IEnumerator Routine_NormalAttack()
    {
        isBusy = true;
        if (animator != null) animator.SetBool(boolIsWalking, false);

        PlaySFX(sfxPrepareAttack);
        yield return new WaitForSeconds(0.2f);

        if (isStunned || isExecutionStunned || isBeingExecuted || isDeadHandled) yield break;

        if (animator != null) animator.SetTrigger(triggerAttack);
        PlaySFX(sfxAttack);

        yield return new WaitForSeconds(0.8f);
        isBusy = false;
    }

    private IEnumerator Routine_BlockThenJumpSlam()
    {
        isBusy = true;
        isBlocking = true;
        skillTimer = skillCooldown;

        if (animator != null)
        {
            animator.SetBool(boolIsWalking, false);
            animator.SetBool(boolIsBlocking, true);
        }

        SpawnShield();
        PlaySFX(sfxBlock);

        yield return new WaitForSeconds(blockDuration);

        DestroyCurrentShield();
        if (animator != null) animator.SetBool(boolIsBlocking, false);
        isBlocking = false;

        if (isStunned || isExecutionStunned || isBeingExecuted || isDeadHandled) yield break;

        isWindingUp = true;
        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator != null ? animator.speed : 1f;
        moveSpeed *= slowMultiplier;
        if (animator != null) animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        if (isStunned || isExecutionStunned || isBeingExecuted || isDeadHandled)
        {
            moveSpeed = originalSpeed;
            if (animator != null) animator.speed = originalAnimSpeed;
            yield break;
        }

        isWindingUp = false;
        moveSpeed = originalSpeed;
        if (animator != null) animator.speed = originalAnimSpeed;

        if (animator != null) animator.SetTrigger(triggerPrepareJump);
        yield return new WaitForSeconds(0.3f);

        if (animator != null) animator.SetTrigger(triggerJumpAir);

        // 🎯 NÂNG CẤP 1: ÂM THANH BẮT ĐẦU NHẢY UPGRADE
        PlaySFX(sfxJumpLaunch);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform != null ? playerTransform.position : transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            if (isStunned || isExecutionStunned || isBeingExecuted || isDeadHandled) yield break;
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        // 🎯 NÂNG CẤP 2: ÂM THANH TIẾP ĐẤT GIẬM NỔ UPGRADE
        PlaySFX(sfxLandSlam);

        Vector3 hitPoint = slamHitboxPoint != null ? slamHitboxPoint.position : transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(hitPoint, slamRadius, playerLayer);
        foreach (var target in targets)
        {
            target.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
        }

        if (animator != null) animator.SetTrigger(triggerLand);
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private void SpawnShield()
    {
        DestroyCurrentShield();

        if (blockShieldPrefab != null)
        {
            Vector3 spawnPos = shieldSpawnPoint != null ? shieldSpawnPoint.position : GetAttackCenter();
            Quaternion spawnRot = shieldSpawnPoint != null ? shieldSpawnPoint.rotation : Quaternion.identity;

            currentSpawnedShield = Instantiate(blockShieldPrefab, spawnPos, spawnRot);

            ShieldBlocker shieldScript = currentSpawnedShield.GetComponent<ShieldBlocker>();
            if (shieldScript == null)
            {
                shieldScript = currentSpawnedShield.AddComponent<ShieldBlocker>();
            }
            shieldScript.Init(playerSwordTag);
        }
    }

    private void DestroyCurrentShield()
    {
        if (currentSpawnedShield != null)
        {
            Destroy(currentSpawnedShield);
            currentSpawnedShield = null;
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
        if (isStunned || isExecutionStunned || isBeingExecuted) return;
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
    }
}