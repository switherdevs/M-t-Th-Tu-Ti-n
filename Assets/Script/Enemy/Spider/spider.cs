using UnityEngine;
using System.Collections;

public class SpiderEnemy : MonoBehaviour
{
    [Header("Đạn độc")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 2f;

    [Header("Cấu hình Burst (Bắn chùm)")]
    [Tooltip("Tick chọn để bật chế độ bắn chùm nhiều viên liên tiếp")]
    [SerializeField] private bool isBurst = false;

    [Tooltip("Số lượng đạn bắn ra trong một loạt Burst")]
    [SerializeField] private int burstBulletCount = 3;

    [Tooltip("Khoảng thời gian giãn cách giữa các viên đạn trong 1 loạt Burst (giây)")]
    [SerializeField] private float burstDelay = 0.1f;

    [Header("Tên Tham Số Animation (Animator)")]
    [Tooltip("Tên biến Bool kích hoạt trạng thái ngắm bắn (Animator)")]
    [SerializeField] private string aimingAnimBool = "isAiming";

    [Tooltip("Tên biến Trigger kích hoạt Animation bắn 1 lần (Animator)")]
    [SerializeField] private string shootAnimTrigger = "Shoot";

    [Header("Phát hiện Player")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Kiểm tra Vật Cản Đường Bắn")]
    [Tooltip("Layer của tường, địa hình hoặc chướng ngại vật cản đạn")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Né Đồng Đội (Separation)")]
    [Tooltip("Bán kính quét để phát hiện và né quái đồng đội")]
    [SerializeField] private float avoidDistance = 1.2f;

    [Tooltip("Độ mạnh của lực đẩy né đồng đội")]
    [SerializeField] private float avoidWeight = 1.5f;

    [Tooltip("Tag của quái đồng đội cần né")]
    [SerializeField] private string allyTag = "Enemy";

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Tấn công")]
    [SerializeField] private float attackRange = 2f;

    [Header("ÂM THANH (AUDIO)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxShoot;
    [SerializeField] private AudioClip sfxDeath;

    [Header("Animation Chết")]
    [SerializeField] private string dieAnimTrigger = "Die";

    private Transform player;
    private Animator animator;
    private CharacterStats characterStats;
    private Collider2D enemyCollider;

    private float shootTimer;
    private bool isDead = false;
    private bool isShootingBurst = false;
    private int strafeDirection = 1;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        shootTimer = 0f;

        strafeDirection = Random.value > 0.5f ? 1 : -1;

        characterStats = GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            characterStats.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (characterStats != null)
        {
            characterStats.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        if (isShootingBurst) return;

        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer
        );

        if (playerCollider != null)
        {
            player = playerCollider.transform;

            FlipTowardsPlayer();

            Vector2 avoidanceForce = CalculateAllyAvoidance();
            bool isLineOfSightBlocked = CheckLineOfSightBlocked();
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (isLineOfSightBlocked)
            {
                animator.SetBool("isWalking", true);

                Vector2 dirToPlayer = (player.position - transform.position).normalized;
                Vector2 strafeDir = new Vector2(-dirToPlayer.y, dirToPlayer.x) * strafeDirection;

                Vector2 finalMoveDir = (strafeDir + avoidanceForce * avoidWeight).normalized;
                transform.position += (Vector3)finalMoveDir * moveSpeed * Time.deltaTime;
            }
            else if (distanceToPlayer <= attackRange)
            {
                animator.SetBool("isWalking", false);

                if (shootTimer <= 0)
                {
                    if (isBurst)
                    {
                        StartCoroutine(Routine_BurstShoot());
                    }
                    else
                    {
                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                        ShootOneBullet();
                    }

                    shootTimer = shootCooldown;
                }
            }
            else
            {
                animator.SetBool("isWalking", true);

                Vector2 dirToPlayer = (player.position - transform.position).normalized;
                Vector2 finalMoveDir = (dirToPlayer + avoidanceForce * avoidWeight).normalized;

                transform.position += (Vector3)finalMoveDir * moveSpeed * Time.deltaTime;
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    private bool CheckLineOfSightBlocked()
    {
        if (player == null) return false;

        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = player.position - startPos;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction.normalized, distance, obstacleLayer);

        return hit.collider != null;
    }

    private Vector2 CalculateAllyAvoidance()
    {
        Vector2 avoidanceVector = Vector2.zero;
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, avoidDistance);

        int neighborCount = 0;
        foreach (Collider2D ally in allies)
        {
            if (ally != null && ally.gameObject != gameObject && ally.CompareTag(allyTag))
            {
                Vector2 diff = (Vector2)(transform.position - ally.transform.position);
                avoidanceVector += diff.normalized / Mathf.Max(diff.magnitude, 0.1f);
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            avoidanceVector /= neighborCount;
        }

        return avoidanceVector;
    }

    private void HandleDeath()
    {
        isDead = true;

        StopAllCoroutines();

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool(aimingAnimBool, false);
            animator.ResetTrigger("Attack");
            animator.ResetTrigger(shootAnimTrigger);

            if (!string.IsNullOrEmpty(dieAnimTrigger))
            {
                animator.SetTrigger(dieAnimTrigger);
            }
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        PlaySFX(sfxDeath);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (player.position.x < transform.position.x)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void ShootOneBullet()
    {
        if (enemyBullet == null || firePoint == null || player == null || isDead) return;

        GameObject bullet = Instantiate(
            enemyBullet,
            firePoint.position,
            Quaternion.identity
        );

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();

        if (bulletScript != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            bulletScript.SetDirection(direction);
        }

        PlaySFX(sfxShoot);
    }

    private IEnumerator Routine_BurstShoot()
    {
        isShootingBurst = true;

        if (animator != null && !string.IsNullOrEmpty(aimingAnimBool))
        {
            animator.SetBool(aimingAnimBool, true);
        }

        for (int i = 0; i < burstBulletCount; i++)
        {
            if (isDead) yield break;

            if (animator != null && !string.IsNullOrEmpty(shootAnimTrigger))
            {
                animator.SetTrigger(shootAnimTrigger);
            }

            ShootOneBullet();

            yield return new WaitForSeconds(burstDelay);
        }

        if (animator != null && !string.IsNullOrEmpty(aimingAnimBool))
        {
            animator.SetBool(aimingAnimBool, false);
        }

        isShootingBurst = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidDistance);

        if (Application.isPlaying && player != null)
        {
            Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
            Gizmos.color = CheckLineOfSightBlocked() ? Color.red : Color.green;
            Gizmos.DrawLine(startPos, player.position);
        }
    }
}