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
    [SerializeField] private float roarWindupTime = 1f;              // Thời gian gồng trước khi gầm
    [SerializeField] private float roarDuration = 2f;                // Thời gian giữ tư thế gầm (Bool = true)
    [SerializeField] private AudioClip sfxRoar;                       // Âm thanh tiếng gầm riêng biệt
    [SerializeField] private float roarCameraShakeIntensity = 2.5f;  // Độ rung màn hình
    [SerializeField] private float roarCameraShakeDuration = 0.8f;   // Thời gian rung màn hình
    [SerializeField] private float playerSlowMultiplier = 0.3f;     // Tốc độ di chuyển người chơi bị giảm còn 30%
    [SerializeField] private float playerSlowDuration = 2.5f;        // Thời gian người chơi bị làm chậm
    [SerializeField] private float roarAffectRadius = 10f;           // Tầm ảnh hưởng của tiếng gầm

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPrepareAttack;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxDeath;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animWalk = "isWalking";
    [SerializeField] private string animClaw1 = "Claw1";
    [SerializeField] private string animClaw2 = "Claw2";
    [SerializeField] private string animAttack = "Attack";     // Trigger bắn đá
    [SerializeField] private string animIsRoaring = "isRoaring"; // Bool gầm (mới)

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;
    private float skillTimer;

    private int shootCount = 0;
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isDeadHandled = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();
        mainCollider = GetComponent<Collider2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        skillTimer = skillCooldown;
        shootCount = 0;
    }

    private void Update()
    {
        if (CheckAndHandleDeath()) return;
        if (isBusy) return;

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
                StartCoroutine(Routine_StoneBreath());
            }
            else
            {
                StartCoroutine(Routine_RoarSkill());
            }
        }
        else if (distance <= attackRange)
        {
            StartCoroutine(Routine_DoubleClaw());
        }
        else
        {
            animator.SetBool(animWalk, true);
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
                isBusy = true;
                if (mainCollider != null) mainCollider.enabled = false;
                if (animator != null)
                {
                    animator.SetBool(animWalk, false);
                    animator.SetBool(animIsRoaring, false);
                }
                PlaySFX(sfxDeath);
            }
            return true;
        }
        return false;
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 1.5f)
            {
                playerTransform = null;
                animator.SetBool(animWalk, false);
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

    private IEnumerator Routine_DoubleClaw()
    {
        isBusy = true;
        animator.SetBool(animWalk, false);

        PlaySFX(sfxPrepareAttack);
        yield return new WaitForSeconds(0.2f);

        animator.SetTrigger(animClaw1);
        PlaySFX(sfxAttack);
        yield return new WaitForSeconds(0.4f);

        animator.SetTrigger(animClaw2);
        PlaySFX(sfxAttack);
        yield return new WaitForSeconds(0.6f);

        isBusy = false;
    }

    private IEnumerator Routine_StoneBreath()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        animator.SetBool(animWalk, false);
        animator.SetTrigger(animAttack);
        PlaySFX(sfxAttack);

        yield return new WaitForSeconds(0.3f);

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
                if (stone.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    rb.linearVelocity = finalDir * projectileSpeed;
                }
            }
        }

        shootCount++;

        yield return new WaitForSeconds(0.8f);
        isBusy = false;
    }

    // SKILL 2: TIẾNG GẦM (ROAR SKILL - DÙNG ANIMATION BOOL)
    private IEnumerator Routine_RoarSkill()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        // Thời gian gồng chiêu
        yield return new WaitForSeconds(roarWindupTime);

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        // Dừng đi bộ và Bật Bool Gầm thành true
        animator.SetBool(animWalk, false);
        animator.SetBool(animIsRoaring, true);

        // Kích hoạt âm thanh, rung màn hình và làm chậm người chơi
        ExecuteRoarEffects();

        // Giữ trạng thái Gầm trong khoảng thời gian roarDuration (Tỳ Hưu đứng yên tại chỗ)
        yield return new WaitForSeconds(roarDuration);

        // Hết thời gian Gầm -> Tắt Bool Gầm thành false
        animator.SetBool(animIsRoaring, false);

        // Reset biến đếm để quay lại chu kỳ bắn đá 3 lần
        shootCount = 0;
        isBusy = false;
    }

    private void ExecuteRoarEffects()
    {
        PlaySFX(sfxRoar != null ? sfxRoar : sfxAttack);

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

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void FlipTowards(Vector3 target)
    {
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