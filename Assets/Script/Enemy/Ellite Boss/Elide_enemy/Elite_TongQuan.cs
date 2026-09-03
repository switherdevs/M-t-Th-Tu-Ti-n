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
    [SerializeField] private GameObject blockShieldPrefab; // Prefab lá chắn
    [SerializeField] private Transform shieldSpawnPoint;    // Vị trí cố định để sinh ra lá chắn
    [SerializeField] private string playerSwordTag = "PlayerSword";

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPrepareAttack;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxBlock;
    [SerializeField] private AudioClip sfxStun;
    [SerializeField] private AudioClip sfxDeath;

    [Header("--- ANIMATION PARAMETERS ---")]
    [SerializeField] private string animWalk = "isWalking";
    [SerializeField] private string animAttack = "Attack";
    [SerializeField] private string animPrepareJump = "PrepareJump";
    [SerializeField] private string animJumpAir = "JumpAir";
    [SerializeField] private string animLand = "Land";
    [SerializeField] private string animIsBlocking = "isBlocking";
    [SerializeField] private string animIsStunned = "isStunned";

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;
    private float skillTimer;

    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isBlocking = false;
    private bool isStunned = false;
    private bool isDeadHandled = false;

    private GameObject currentSpawnedShield; // Lưu lá chắn được sinh ra trong World
    private Coroutine currentBehaviorCoroutine;

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

        // Dọn dẹp lá chắn nếu object bị xóa giữa chừng
        DestroyCurrentShield();
    }

    private void Update()
    {
        if (CheckAndHandleDeath()) return;
        if (isBusy || isStunned) return;

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
                isBlocking = false;
                isStunned = false;

                if (mainCollider != null) mainCollider.enabled = false;
                DestroyCurrentShield();

                animator.SetBool(animWalk, false);
                animator.SetBool(animIsBlocking, false);
                animator.SetBool(animIsStunned, false);

                PlaySFX(sfxDeath);
            }
            return true;
        }
        return false;
    }

    private void HandleDamaged(float damageTaken)
    {
        if (isBlocking && !isStunned && !stats.IsDead)
        {
            if (currentBehaviorCoroutine != null) StopCoroutine(currentBehaviorCoroutine);
            StartCoroutine(Routine_GetStunned());
        }
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

    private IEnumerator Routine_NormalAttack()
    {
        isBusy = true;
        animator.SetBool(animWalk, false);

        PlaySFX(sfxPrepareAttack);
        yield return new WaitForSeconds(0.2f);

        animator.SetTrigger(animAttack);
        PlaySFX(sfxAttack);

        yield return new WaitForSeconds(0.8f);
        isBusy = false;
    }

    private IEnumerator Routine_BlockThenJumpSlam()
    {
        isBusy = true;
        isBlocking = true;
        skillTimer = skillCooldown;

        animator.SetBool(animWalk, false);
        animator.SetBool(animIsBlocking, true);

        // Tạo Prefab lá chắn ngoài World (Không đặt parent để hoàn toàn độc lập vị trí và Collider)
        SpawnShield();
        PlaySFX(sfxBlock);

        yield return new WaitForSeconds(blockDuration);

        // Hết thời gian đỡ đòn mà không dính đòn -> Hủy khiên và nhảy bổ
        DestroyCurrentShield();
        animator.SetBool(animIsBlocking, false);
        isBlocking = false;

        isWindingUp = true;
        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animPrepareJump);
        yield return new WaitForSeconds(0.3f);

        animator.SetTrigger(animJumpAir);
        PlaySFX(sfxAttack);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform != null ? playerTransform.position : transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / jumpDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        Vector3 hitPoint = slamHitboxPoint != null ? slamHitboxPoint.position : transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(hitPoint, slamRadius, playerLayer);
        foreach (var target in targets)
        {
            target.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
        }

        animator.SetTrigger(animLand);
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private IEnumerator Routine_GetStunned()
    {
        isBlocking = false;
        isWindingUp = false;
        isStunned = true;
        isBusy = true;

        DestroyCurrentShield();
        animator.SetBool(animIsBlocking, false);
        animator.SetBool(animIsStunned, true);
        PlaySFX(sfxStun);

        yield return new WaitForSeconds(stunDuration);

        animator.SetBool(animIsStunned, false);
        isStunned = false;
        isBusy = false;
    }

    private void SpawnShield()
    {
        DestroyCurrentShield();

        if (blockShieldPrefab != null)
        {
            Vector3 spawnPos = shieldSpawnPoint != null ? shieldSpawnPoint.position : GetAttackCenter();
            Quaternion spawnRot = shieldSpawnPoint != null ? shieldSpawnPoint.rotation : Quaternion.identity;

            // Instantiate trực tiếp ra World (không truyền 'transform' làm parent)
            currentSpawnedShield = Instantiate(blockShieldPrefab, spawnPos, spawnRot);

            // Gắn component xử lý va chạm với kiếm người chơi lên Prefab vừa tạo
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

// Script phụ tự động được đính kèm vào Prefab Lá Chắn khi tạo ra
public class ShieldBlocker : MonoBehaviour
{
    private string targetSwordTag;

    public void Init(string swordTag)
    {
        targetSwordTag = swordTag;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(targetSwordTag) && collision.CompareTag(targetSwordTag))
        {
            Destroy(collision.gameObject);
        }
    }
}