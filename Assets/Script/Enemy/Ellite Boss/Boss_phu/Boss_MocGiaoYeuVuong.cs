using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Boss_MocGiaoYeuVuong : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TỐC ĐỘ BAY ---")]
    [SerializeField] private float detectionRange = 150f;
    [SerializeField] private float flySpeed = 3.5f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ TƯỜNG (OBSTACLE/WALL) ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private float avoidRadius = 5.71f;
    [SerializeField] private float wallPushForce = 3f;
    [SerializeField, Tooltip("Tùy chỉnh offset X và Y của tâm né tường/vật cản")]
    private Vector2 wallCheckOffset = Vector2.zero;

    [Header("--- SÁT THƯƠNG ĐÁNH THƯỜNG ---")]
    [SerializeField] private float tailSwingDamage = 25f;
    [SerializeField, Tooltip("GameObject Hitbox đòn quật đuôi (chỉ mở khi đánh)")]
    private GameObject tailAttackHitbox;

    [Header("--- SKILL: TRIPLE WOOD ORB ---")]
    [SerializeField] private float skillCooldown = 7f;
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.2f;
    [SerializeField] private SimpleObjectPool orbPool;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float orbSpeed = 10f;
    [SerializeField] private float orbDamage = 15f;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animSwing = "TailSwing";
    [SerializeField] private string animSpit = "SpitStart";
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats; // Tham chiếu CharacterStats để lắng nghe sự kiện chết[cite: 3]
    private Animator animator;

    private float skillTimer;
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

        if (tailAttackHitbox != null)
        {
            tailAttackHitbox.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath += HandleBossDeath; // Đăng ký sự kiện khi chết[cite: 3]
        }
    }

    private void OnDisable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath -= HandleBossDeath; // Hủy đăng ký[cite: 3]
        }
    }

    private void Update()
    {
        if (isDead || isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        if (skillTimer > 0) skillTimer -= Time.deltaTime;
        FlipTowards(playerTransform.position);

        Vector3 attackCenter = GetAttackCenter();
        float distance = Vector2.Distance(attackCenter, playerTransform.position);

        // Đang gồng chiêu -> Bay chậm bám theo Player
        if (isWindingUp)
        {
            FlyAndAvoidWalls(playerTransform.position, flySpeed * slowMultiplier);
            return;
        }

        // Ưu tiên 1: Kích hoạt Skill Bắn Cầu Gỗ
        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_TripleWoodOrbLine());
        }
        // Ưu tiên 2: Đánh thường (Quật đuôi)
        else if (distance <= attackRange)
        {
            StartCoroutine(Routine_TailSwing());
        }
        // Trạng thái thường: Bay tiến về phía Player và né tường
        else
        {
            FlyAndAvoidWalls(playerTransform.position, flySpeed);
        }
    }

    /// <summary>
    /// Di chuyển mượt mà và bẻ lái né Wall/Obstacle từ vị trí offset tùy chỉnh
    /// </summary>
    private void FlyAndAvoidWalls(Vector3 targetPosition, float speed)
    {
        Vector2 checkOrigin = GetWallCheckCenter();
        Vector2 dirToTarget = ((Vector2)targetPosition - checkOrigin).normalized;
        Vector2 finalMoveDir = dirToTarget;

        // Quét tìm vật cản bằng CircleCast từ vị trí offset X, Y
        RaycastHit2D hit = Physics2D.CircleCast(checkOrigin, avoidRadius, dirToTarget, 1.5f, obstacleLayer);

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.collider.CompareTag(wallTag) || (obstacleLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Vector2 avoidDir = Vector2.Perpendicular(hit.normal).normalized;
                if (Vector2.Dot(dirToTarget, avoidDir) < 0) avoidDir = -avoidDir;

                finalMoveDir = (dirToTarget + avoidDir * wallPushForce).normalized;
            }
        }

        transform.position += (Vector3)(finalMoveDir * (speed * Time.deltaTime));
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

    private IEnumerator Routine_TailSwing()
    {
        isBusy = true;
        animator.SetTrigger(animSwing);

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(GetAttackCenter(), playerTransform.position);
            if (distance <= attackRange * 1.2f && playerStats != null)
            {
                playerStats.TakeDamage(tailSwingDamage);
            }
        }

        yield return new WaitForSeconds(0.6f);

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(false);

        isBusy = false;
    }

    private IEnumerator Routine_TripleWoodOrbLine()
    {
        isWindingUp = true;
        skillTimer = skillCooldown;

        float originalAnimSpeed = animator.speed;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animSpit);

        yield return new WaitForSeconds(0.3f);

        Vector3 spawnPos = mouthPoint != null ? mouthPoint.position : transform.position;
        Vector2 targetDirection = playerTransform != null ? (Vector2)(playerTransform.position - spawnPos).normalized : (Vector2)transform.right;

        for (int i = 0; i < 3; i++)
        {
            if (orbPool != null)
            {
                GameObject orb = orbPool.GetFromPool(spawnPos, Quaternion.identity);
                Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = targetDirection * orbSpeed;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.4f);
        isBusy = false;
    }

    private void HandleBossDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(false);

        // Vô hiệu hóa toàn bộ Collider trên Boss
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        if (animator != null && !string.IsNullOrEmpty(animDie))
        {
            animator.SetTrigger(animDie);
        }

        Debug.Log("<color=green>[Mộc Giao Yêu Vương]</color> Boss đã bị tiêu diệt!");
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

        // Vòng tròn màu Cyan biểu diễn vị trí quét né tường (tính theo Offset X và Y)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetWallCheckCenter(), avoidRadius);
    }
}