using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Boss_UMinhThiDe : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TỐC ĐỘ BAY ---")]
    [SerializeField] private float detectionRange = 18f;
    [SerializeField, Tooltip("Tầm đánh: Nếu Player ở trong tầm này, Boss sẽ ĐỨNG YÊN bắn/dùng skill")]
    private float attackRange = 8f;
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
    [SerializeField] private Transform handPoint;
    [SerializeField] private float blastSpeed = 12f;

    [Header("--- SKILL 2: TRIỆU HỒI LINH HỒN (MA ĐẠO) ---")]
    [SerializeField] private float skillCooldown = 12f;
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.3f;
    [SerializeField] private SimpleObjectPool soulPool;
    [SerializeField] private Transform soulSummonPoint;
    [SerializeField] private float summonDistance = 5f;
    [SerializeField] private float soulSpeed = 8f;

    [Header("--- SKILL 3: TRIỆU HỒI QUÁI CON (MINIONS) ---")]
    [SerializeField] private float minionSummonCooldown = 20f;
    [SerializeField, Tooltip("Danh sách các vị trí sẽ triệu hồi quái con")]
    private Transform[] minionSummonPoints;
    [SerializeField, Tooltip("Pool hoặc Prefab của quái con")]
    private GameObject minionPrefab;
    [SerializeField, Tooltip("Hiệu ứng (VFX) xuất hiện tại điểm triệu hồi trước khi quái con ra")]
    private GameObject summonVFXPrefab;
    [SerializeField, Tooltip("Thời gian delay giữa mỗi lần sinh 1 quái con theo thứ tự mảng")]
    private float delayBetweenMinions = 0.3f;

    [Header("--- SKILL 4: VÒNG TRÒN TỬ THẦN (DEATH CIRCLE) ---")]
    [SerializeField] private float deathCircleCooldown = 15f;
    [SerializeField, Tooltip("Prefab vòng tròn tử thần")]
    private GameObject deathCirclePrefab;
    [SerializeField, Tooltip("Khoảng thời gian dự đoán hướng di chuyển của Player (giây)")]
    private float leadTime = 1.2f;

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSpecialPrepare;
    [SerializeField] private AudioClip sfxSpecialCast;
    [SerializeField, Tooltip("Âm thanh chuẩn bị triệu hồi quái con")]
    private AudioClip sfxSummonMinionsPrepare;
    [SerializeField, Tooltip("Âm thanh chuẩn bị xuất hiện Vòng Tròn Tử Thần")]
    private AudioClip sfxDeathCirclePrepare;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCastBlast = "Slash";
    [SerializeField] private string animSummon = "SummonCast";
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats;
    private Animator animator;
    private ExecutableEnemy executableEnemy;
    private Rigidbody2D playerRb;

    private float skillTimer;
    private float blastTimer;
    private float minionSummonTimer;
    private float deathCircleTimer;

    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isDead = false;

    private Vector3 lastPlayerPos;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        bossStats = GetComponent<CharacterStats>();
        executableEnemy = GetComponent<ExecutableEnemy>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        skillTimer = skillCooldown;
        blastTimer = 0f;
        minionSummonTimer = minionSummonCooldown;
        deathCircleTimer = deathCircleCooldown;
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
        // KHÓA DI CHUYỂN & SKILL NẾU BOSS BỊ KẾT LIỄU/STUN HOẶC ĐÃ CHẾT
        if (isDead || (executableEnemy != null && executableEnemy.IsStunned))
        {
            if (isBusy || isWindingUp)
            {
                StopAllCoroutines();
                isBusy = false;
                isWindingUp = false;
            }
            return;
        }

        if (isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        // Đếm ngược các Cooldown
        if (skillTimer > 0) skillTimer -= Time.deltaTime;
        if (blastTimer > 0) blastTimer -= Time.deltaTime;
        if (minionSummonTimer > 0) minionSummonTimer -= Time.deltaTime;
        if (deathCircleTimer > 0) deathCircleTimer -= Time.deltaTime;

        // Luôn quay mặt về phía Player
        FlipTowards(playerTransform.position);

        Vector3 attackCenter = GetAttackCenter();
        float distanceToPlayer = Vector2.Distance(attackCenter, playerTransform.position);

        // --- NẾU ĐANG GẬN CHIÊU (WINDING UP): CHỈ DI CHUYỂN NẾU PLAYER Ở NGOÀI ATTACK RANGE ---
        if (isWindingUp)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveSmoothly(playerTransform.position, moveSpeed * slowMultiplier);
            }
            return;
        }

        // --- HỆ THỐNG ƯU TIÊN SỬ DỤNG SKILL / DI CHUYỂN ---

        // 1. Skill Triệu Hồi Quái Con (Tùy chọn dùng bất kể khoảng cách)
        if (minionSummonTimer <= 0)
        {
            StartCoroutine(Routine_SummonMinions());
        }
        // 2. Skill Vòng Tròn Tử Thần
        else if (deathCircleTimer <= 0)
        {
            StartCoroutine(Routine_SpawnDeathCircle());
        }
        // 3. Skill Triệu Hồi Linh Hồn
        else if (skillTimer <= 0)
        {
            StartCoroutine(Routine_SummonSouls());
        }
        // 4. Player nằm trong Tầm Đánh (attackRange) -> ĐỨNG YÊN BẮN (Soul Blast)
        else if (distanceToPlayer <= attackRange)
        {
            if (blastTimer <= 0)
            {
                StartCoroutine(Routine_SoulBlast());
            }
            // Nếu đòn bắn đang Cooldown và Player vẫn trong tầm, Boss đứng yên chờ, KHÔNG di chuyển.
        }
        // 5. Player vượt khỏi Tầm Đánh (distanceToPlayer > attackRange) -> DI CHUYỂN ĐUỔI THEO
        else
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
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
                playerRb = null;
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null)
        {
            playerTransform = hit.transform;
            playerStats = playerTransform.GetComponent<CharacterStats>();
            playerRb = playerTransform.GetComponent<Rigidbody2D>();
            lastPlayerPos = playerTransform.position;
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
        PlaySFX(sfxNormalAttack);

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

        PlaySFX(sfxSpecialPrepare);

        float originalAnimSpeed = animator.speed;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animSummon);
        PlaySFX(sfxSpecialCast);

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

    private IEnumerator Routine_SummonMinions()
    {
        isBusy = true;
        minionSummonTimer = minionSummonCooldown;

        PlaySFX(sfxSummonMinionsPrepare);

        if (animator != null && !string.IsNullOrEmpty(animSummon))
        {
            animator.SetTrigger(animSummon);
        }

        yield return new WaitForSeconds(0.6f);

        if (minionSummonPoints != null && minionSummonPoints.Length > 0)
        {
            for (int i = 0; i < minionSummonPoints.Length; i++)
            {
                Transform point = minionSummonPoints[i];
                if (point != null)
                {
                    if (summonVFXPrefab != null)
                    {
                        Instantiate(summonVFXPrefab, point.position, Quaternion.identity);
                    }

                    if (minionPrefab != null)
                    {
                        Instantiate(minionPrefab, point.position, Quaternion.identity);
                    }
                }

                yield return new WaitForSeconds(delayBetweenMinions);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private IEnumerator Routine_SpawnDeathCircle()
    {
        isBusy = true;
        deathCircleTimer = deathCircleCooldown;

        PlaySFX(sfxDeathCirclePrepare != null ? sfxDeathCirclePrepare : sfxSpecialPrepare);

        if (animator != null && !string.IsNullOrEmpty(animSummon))
        {
            animator.SetTrigger(animSummon);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 predictedPosition = CalculatePredictedPlayerPosition();

        if (deathCirclePrefab != null)
        {
            Instantiate(deathCirclePrefab, predictedPosition, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    private Vector3 CalculatePredictedPlayerPosition()
    {
        if (playerTransform == null) return transform.position;

        Vector2 playerVelocity = Vector2.zero;

        if (playerRb != null)
        {
            playerVelocity = playerRb.linearVelocity;
        }
        else
        {
            playerVelocity = ((Vector2)playerTransform.position - (Vector2)lastPlayerPos) / Time.deltaTime;
            lastPlayerPos = playerTransform.position;
        }

        Vector3 predictedPos = playerTransform.position + (Vector3)(playerVelocity * leadTime);

        return predictedPos;
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

    public Vector3 GetWallCheckCenter()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(wallCheckOffset.x * direction, wallCheckOffset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vòng tròn tầm đánh (attackRange) màu đỏ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetWallCheckCenter(), avoidRadius);

        if (minionSummonPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in minionSummonPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}