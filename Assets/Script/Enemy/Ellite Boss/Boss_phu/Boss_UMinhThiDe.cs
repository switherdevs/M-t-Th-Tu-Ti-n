using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Boss_UMinhThiDe : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TỐC ĐỘ BAY ---")]
    [SerializeField] private float detectionRange = 18f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN (OBSTACLE/WALL) ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.8f;
    [SerializeField, Tooltip("Tùy chỉnh offset X và Y cho tâm vòng tròn quét né vật cản")]
    private Vector2 wallCheckOffset = Vector2.zero;

    [Header("--- SKILL 1: U MINH LONG CHƯỞNG ---")]
    [SerializeField] private float blastCooldown = 2.5f;
    [SerializeField] private SimpleObjectPool blastPool;
    [SerializeField] private Transform handPoint;               // Vị trí bắn chưởng
    [SerializeField] private float blastSpeed = 12f;

    [Header("--- SKILL 2: TRIỆU HỒI LINH HỒN (MA ĐẠO) ---")]
    [SerializeField] private float skillCooldown = 12f;
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.3f;
    [SerializeField] private SimpleObjectPool soulPool;
    [SerializeField] private Transform soulSummonPoint;         // MỐC TÂM TRIỆU HỒI (Gán tay/trán Boss để đạn đẩy ra ngoài hẳn Boss)
    [SerializeField] private float summonDistance = 5f;          // Bán kính đẩy đạn ra ngoài
    [SerializeField] private float soulSpeed = 8f;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCastBlast = "Slash";     // Animation bắn chưởng[cite: 4]
    [SerializeField] private string animSummon = "SummonCast";   // Animation triệu hồi[cite: 4]
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats;
    private Animator animator;

    private float skillTimer;
    private float blastTimer;
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        bossStats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        skillTimer = skillCooldown;
        blastTimer = 0f;
    }

    private void OnEnable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath += HandleBossDeath;
        }
    }

    private void OnDisable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath -= HandleBossDeath;
        }
    }

    private void Update()
    {
        if (isDead || isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        // Cập nhật bộ đếm Cooldown
        if (skillTimer > 0) skillTimer -= Time.deltaTime;
        if (blastTimer > 0) blastTimer -= Time.deltaTime;

        // Luôn xoay mặt về phía Player khi đang phát hiện
        FlipTowards(playerTransform.position);

        Vector3 attackCenter = GetAttackCenter();
        float distance = Vector2.Distance(attackCenter, playerTransform.position);

        // Trường hợp 1: Đang vận công Skill 2 (Triệu hồi) -> Di chuyển chậm theo thiết lập windup
        if (isWindingUp)
        {
            MoveSmoothly(playerTransform.position, moveSpeed * slowMultiplier);
            return;
        }

        // Trường hợp 2: Người chơi đã VÀO TẦM ĐÁNH (distance <= attackRange)
        if (distance <= attackRange)
        {
            // DỪNG LẠI TẠI CHỖ và kiểm tra thi triển chiêu thức
            if (skillTimer <= 0)
            {
                StartCoroutine(Routine_SummonSouls());
            }
            else if (blastTimer <= 0)
            {
                StartCoroutine(Routine_SoulBlast());
            }
            // Nếu cả 2 skill đều đang hồi chiêu (Cooldown), Boss giữ nguyên vị trí đứng yên chờ đạn hồi
        }
        // Trường hợp 3: Người chơi ngoài tầm đánh -> Rượt đuổi cho đến khi vào lại tầm đánh
        else
        {
            // Ngoại lệ: Nếu Skill Triệu Hồi Linh Hồn đã hồi xong kể cả khi ở xa, Boss vẫn tung chiêu triệu hồi từ xa
            if (skillTimer <= 0)
            {
                StartCoroutine(Routine_SummonSouls());
            }
            else
            {
                MoveSmoothly(playerTransform.position, moveSpeed);
            }
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 2f)
            {
                playerTransform = null;
                playerStats = null;
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null)
        {
            playerTransform = hit.transform;
            playerStats = playerTransform.GetComponent<CharacterStats>();
        }
    }

    private void MoveSmoothly(Vector3 targetPosition, float speed)
    {
        Vector2 checkOrigin = GetWallCheckCenter();
        Vector2 dirToTarget = ((Vector2)targetPosition - checkOrigin).normalized;
        Vector2 moveDir = dirToTarget;

        RaycastHit2D hit = Physics2D.CircleCast(checkOrigin, avoidRadius, dirToTarget, 1f, obstacleLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Vector2 slideDir = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(dirToTarget, slideDir) < 0) slideDir = -slideDir;
            moveDir = (dirToTarget + slideDir * 1.5f).normalized;
        }

        transform.position += (Vector3)(moveDir * (speed * Time.deltaTime));
    }

    private IEnumerator Routine_SoulBlast()
    {
        isBusy = true;
        blastTimer = blastCooldown;
        animator.SetTrigger(animCastBlast);

        yield return new WaitForSeconds(0.3f);

        Vector3 spawnPos = handPoint != null ? handPoint.position : transform.position;
        Vector2 targetDir = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        if (blastPool != null)
        {
            GameObject blast = blastPool.GetFromPool(spawnPos, Quaternion.identity);
            if (blast != null)
            {
                Rigidbody2D rb = blast.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.linearVelocity = targetDir * blastSpeed;
                }
            }
        }

        yield return new WaitForSeconds(0.4f);
        isBusy = false;
    }

    private IEnumerator Routine_SummonSouls()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        float originalAnimSpeed = animator.speed;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animSummon);

        yield return new WaitForSeconds(0.5f);

        Vector3 centerPos = soulSummonPoint != null ? soulSummonPoint.position : transform.position;

        Vector3[] spawnOffsets = {
            Vector3.up * summonDistance,
            Vector3.down * summonDistance,
            Vector3.left * summonDistance,
            Vector3.right * summonDistance
        };

        foreach (Vector3 offset in spawnOffsets)
        {
            Vector3 spawnPos = centerPos + offset;
            if (soulPool != null)
            {
                GameObject soul = soulPool.GetFromPool(spawnPos, Quaternion.identity);
                if (soul != null)
                {
                    soul.SendMessage("SetTarget", playerTransform, SendMessageOptions.DontRequireReceiver);

                    Rigidbody2D rb = soul.GetComponent<Rigidbody2D>();
                    if (rb != null && playerTransform != null)
                    {
                        rb.linearVelocity = Vector2.zero;

                        Vector2 launchDir = ((Vector2)playerTransform.position - (Vector2)spawnPos).normalized;
                        rb.linearVelocity = launchDir * soulSpeed;
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private void HandleBossDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        if (animator != null && !string.IsNullOrEmpty(animDie))
        {
            animator.SetTrigger(animDie);
        }

        Debug.Log("<color=purple>[U Minh Thí Đế]</color> Boss đã bị tiêu diệt!");
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

    public Vector3 GetWallCheckCenter()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(wallCheckOffset.x * direction, wallCheckOffset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetWallCheckCenter(), avoidRadius);

        Vector3 centerPos = soulSummonPoint != null ? soulSummonPoint.position : transform.position;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(centerPos, summonDistance);
    }
}