using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // Nhập thư viện Cinemachine (Dành cho Unity 6 / Cinemachine v3)

[RequireComponent(typeof(CharacterStats))]
public class Boss_MocGiaoYeuVuong : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TỐC ĐỘ BAY ---")]
    [SerializeField] private float detectionRange = 150f;
    [SerializeField] private float flySpeed = 3.5f;

    [Header("--- VÙNG TẤN CÔNG CẬN CHIẾN (HÌNH CHỮ NHẬT / VUÔNG) ---")]
    [SerializeField, Tooltip("Kích thước vùng đánh: X là Chiều dài (Ngang), Y là Chiều rộng (Dọc)")]
    private Vector2 attackBoxSize = new Vector2(6f, 4f);
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ TƯỜNG (OBSTACLE/WALL) ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private float avoidRadius = 5.71f;
    [SerializeField] private float wallPushForce = 3f;
    [SerializeField, Tooltip("Tùy chỉnh offset X và Y của tâm né tường/vật cản")]
    private Vector2 wallCheckOffset = Vector2.zero;

    [Header("--- SÁT THƯƠNG & COOLDOWN ĐÁNH THƯỜNG ---")]
    [SerializeField] private float tailSwingDamage = 25f;
    [SerializeField, Tooltip("Thời gian hồi chiêu / nghỉ giữa các đòn đánh cận chiến (giây)")]
    private float meleeCooldown = 2.5f;
    [SerializeField, Tooltip("GameObject Hitbox đòn quật đuôi (chỉ mở khi đánh)")]
    private GameObject tailAttackHitbox;

    [Header("--- HIỆU ỨNG RUNG CAM (PERLIN NOISE) ---")]
    [SerializeField, Tooltip("Cường độ rung màn hình")]
    private float shakeIntensity = 2f;
    [SerializeField, Tooltip("Thời gian rung (giây)")]
    private float shakeDuration = 0.3f;

    [Header("--- SKILL: TRIPLE WOOD ORB ---")]
    [SerializeField] private float skillCooldown = 7f;
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.2f;
    [SerializeField] private SimpleObjectPool orbPool;
    [SerializeField] private Transform mouthPoint;
    [SerializeField] private float orbSpeed = 10f;

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxNormalAttack;
    [SerializeField] private AudioClip sfxSpecialPrepare;
    [SerializeField] private AudioClip sfxSpecialCast;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animSwing = "TailSwing";
    [SerializeField] private string animSpit = "SpitStart";
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats;
    private Animator animator;
    private ExecutableEnemy executableEnemy;

    private float skillTimer;
    private float meleeTimer; // Biến đếm thời gian hồi cận chiến
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isDead = false;

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
        meleeTimer = 0f; // Bắt đầu vào game có thể đánh được ngay

        if (tailAttackHitbox != null)
        {
            tailAttackHitbox.SetActive(false);
        }
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
                if (tailAttackHitbox != null) tailAttackHitbox.SetActive(false);
            }
            return;
        }

        if (isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        // Đếm ngược thời gian hồi chiêu
        if (skillTimer > 0) skillTimer -= Time.deltaTime;
        if (meleeTimer > 0) meleeTimer -= Time.deltaTime;

        FlipTowards(playerTransform.position);

        Vector3 attackCenter = GetAttackCenter();

        // Kiểm tra Player có nằm trong vùng hình vuông/chữ nhật hay không
        bool isPlayerInAttackBox = IsPlayerInAttackBox(attackCenter);

        if (isWindingUp)
        {
            FlyAndAvoidWalls(playerTransform.position, flySpeed * slowMultiplier);
            return;
        }

        // Ưu tiên dùng Skill khè cầu gỗ
        if (skillTimer <= 0)
        {
            StartCoroutine(Routine_TripleWoodOrbLine());
        }
        // Đánh cận chiến nếu Player ở trong vùng và ĐÃ HẾT COOLDOWN ĐÁNH CẬN CHIẾN
        else if (isPlayerInAttackBox && meleeTimer <= 0)
        {
            StartCoroutine(Routine_TailSwing());
        }
        // Nếu không thỏa mãn điều kiện đánh thì áp sát Player
        else
        {
            FlyAndAvoidWalls(playerTransform.position, flySpeed);
        }
    }

    private void FlyAndAvoidWalls(Vector3 targetPosition, float speed)
    {
        Vector2 checkOrigin = GetWallCheckCenter();
        Vector2 dirToTarget = ((Vector2)targetPosition - checkOrigin).normalized;
        Vector2 finalMoveDir = dirToTarget;

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

    /// <summary>
    /// Kiểm tra xem Player có nằm trong vùng đòn đánh hình chữ nhật/vuông hay không
    /// </summary>
    private bool IsPlayerInAttackBox(Vector3 center)
    {
        if (playerTransform == null) return false;

        Collider2D hit = Physics2D.OverlapBox(center, attackBoxSize, 0f, playerLayer);
        return hit != null && hit.transform == playerTransform;
    }

    private IEnumerator Routine_TailSwing()
    {
        isBusy = true;
        meleeTimer = meleeCooldown; // Kích hoạt Cooldown cho đòn cận chiến ngay lập tức

        animator.SetTrigger(animSwing);
        PlaySFX(sfxNormalAttack);

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        // --- TÍNH SÁT THƯƠNG BẰNG HÌNH CHỮ NHẬT / VUÔNG ---
        if (playerTransform != null && IsPlayerInAttackBox(GetAttackCenter()))
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(tailSwingDamage);
            }
        }

        // --- KÍCH HOẠT RUNG CAM SAU KHU ĐÁNH XONG ---
        StartCoroutine(Routine_TriggerPerlinCameraShake());

        yield return new WaitForSeconds(0.6f);

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(false);

        isBusy = false;
    }

    private IEnumerator Routine_TripleWoodOrbLine()
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

        animator.SetTrigger(animSpit);
        PlaySFX(sfxSpecialCast);

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

    /// <summary>
    /// Coroutine tự động tìm kiếm Channel Perlin trong Scene và điều chỉnh Amplitude Gain để tạo hiệu ứng rung
    /// </summary>
    private IEnumerator Routine_TriggerPerlinCameraShake()
    {
        CinemachineBasicMultiChannelPerlin perlin = FindFirstObjectByType<CinemachineBasicMultiChannelPerlin>();

        if (perlin != null)
        {
            perlin.AmplitudeGain = shakeIntensity;
            yield return new WaitForSeconds(shakeDuration);
            perlin.AmplitudeGain = 0f;
        }
        else
        {
            Debug.LogWarning("<color=yellow>[Boss]</color> Không tìm thấy CinemachineBasicMultiChannelPerlin nào trong Scene!");
            yield return null;
        }
    }

    private void HandleBossDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (tailAttackHitbox != null) tailAttackHitbox.SetActive(false);

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
        // Tầm phát hiện Player
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vùng đánh cận chiến HÌNH CHỮ NHẬT / VUÔNG (Màu đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackCenter(), attackBoxSize);

        // Vùng né tường (Màu xanh)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetWallCheckCenter(), avoidRadius);
    }
}