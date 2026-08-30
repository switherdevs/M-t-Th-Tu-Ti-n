using System.Collections;
using UnityEngine;

public class Boss_TongTrieuMaTuong : MonoBehaviour
{
    [Header("--- TẦM NHÌN & DI CHUYỂN ---")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.8f;
    [SerializeField] private float wallCheckDistance = 1.5f;

    [Header("--- SÁT THƯƠNG & HITBOX ĐÁNH THƯỜNG ---")]
    [SerializeField] private float normalDamage = 15f;
    [SerializeField, Tooltip("GameObject Effect/Hitbox chỉ hiện lên khi tung đòn đánh")]
    private GameObject attackHitbox;

    [Header("--- ĐIỀU KIỆN KÍCH HOẠT SKILL ĐẶC BIỆT ---")]
    [SerializeField] private int attacksToSpecial = 5;
    [SerializeField] private float maxWaitTimeForSpecial = 45f;

    [Header("--- SKILL ĐẶC BIỆT: BẮT GIỮ & VĂNG PLAYER ---")]
    [SerializeField] private float grabDuration = 15f;
    [SerializeField] private float kidnapDamage = 20f;
    [SerializeField] private float kidnapDamageInterval = 5f;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private float kidnapMoveSpeed = 12f;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwDistance = 3f;

    [Header("--- TRẠNG THÁI HỒI SỨC SAU KHI VĂNG PLAYER ---")]
    [SerializeField] private float stepBackDistance = 3f;
    [SerializeField] private float slowWalkSpeed = 2f;
    [SerializeField] private float slowWalkDuration = 3f;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCharge = "isCharging";
    [SerializeField] private string animSpearThrust = "SpearThrust";
    [SerializeField] private string animGrabBool = "isGrabbing";
    [SerializeField] private string animThrowPlayer = "ThrowPlayer";
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats; // Tham chiếu CharacterStats của chính Boss[cite: 3]
    private Animator animator;
    private SpriteRenderer bossSprite;

    private int basicAttackCount = 0;
    private float specialSkillTimer = 0f;

    private bool isBusy = false;
    private bool isKidnapping = false;
    private bool isDead = false;
    private Vector2 currentStraightDir;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        bossSprite = GetComponentInChildren<SpriteRenderer>();
        bossStats = GetComponent<CharacterStats>(); // Đọc CharacterStats của Boss[cite: 3]
    }

    private void Start()
    {
        basicAttackCount = 0;
        specialSkillTimer = 0f;

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath += HandleBossDeath; // Đăng ký sự kiện khi chết từ CharacterStats[cite: 3]
        }
    }

    private void OnDisable()
    {
        if (bossStats != null)
        {
            bossStats.OnDeath -= HandleBossDeath; // Hủy đăng ký sự kiện khi Disable[cite: 3]
        }
    }

    private void Update()
    {
        // Nếu đã chết hoặc đang bận làm hành động khác thì không thực hiện Update logic
        if (isDead || isBusy || isKidnapping) return;

        FindPlayer();
        if (playerTransform == null) return;

        specialSkillTimer += Time.deltaTime;
        FlipTowards(playerTransform.position);

        if (basicAttackCount >= attacksToSpecial || specialSkillTimer >= maxWaitTimeForSpecial)
        {
            StartCoroutine(Routine_KidnapSkill());
            return;
        }

        ExecuteChargePhase();
    }

    /// <summary>
    /// Xử lý logic khi Boss tử trận (lắng nghe từ CharacterStats)[cite: 3]
    /// </summary>
    private void HandleBossDeath()
    {
        if (isDead) return;
        isDead = true;

        // 1. Dừng mọi Coroutine hành động đang chạy
        StopAllCoroutines();

        // 2. Tắt toàn bộ Hitbox tấn công
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        // 3. Nếu đang giữ Player thì lập tức nhả Player ra để không bị dính vào Boss đã chết
        if (isKidnapping && playerTransform != null)
        {
            playerTransform.SetParent(null);

            Collider2D pCol = playerTransform.GetComponent<Collider2D>();
            Rigidbody2D pRb = playerTransform.GetComponent<Rigidbody2D>();
            SpriteRenderer pSprite = playerTransform.GetComponentInChildren<SpriteRenderer>();

            if (pCol != null) pCol.enabled = true;
            if (pRb != null) pRb.simulated = true;
            if (pSprite != null) pSprite.enabled = true;
        }

        if (bossSprite != null) bossSprite.enabled = true;

        // 4. Vô hiệu hóa TOÀN BỘ Collider trên Boss để Player / đạn đi xuyên qua
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        // 5. Chạy Animation chết
        if (animator != null)
        {
            animator.SetBool(animCharge, false);
            animator.SetBool(animGrabBool, false);
            if (!string.IsNullOrEmpty(animDie))
            {
                animator.SetTrigger(animDie);
            }
        }

        Debug.Log("<color=red>[Boss Manager]</color> Boss đã bị tiêu diệt! Đã tắt toàn bộ Collider và dừng di chuyển.");
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 2f)
            {
                playerTransform = null;
                playerStats = null;
                animator.SetBool(animCharge, false);
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null)
        {
            playerTransform = hit.transform;
            playerStats = playerTransform.GetComponent<CharacterStats>(); // Lấy CharacterStats của Player[cite: 3]
        }
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

    private void ExecuteChargePhase()
    {
        Vector3 attackCenter = GetAttackCenter();
        float distance = Vector2.Distance(attackCenter, playerTransform.position);

        animator.SetBool(animCharge, true);
        MoveSmoothly(playerTransform.position, chargeSpeed);

        if (distance <= attackRange)
        {
            StartCoroutine(Routine_SpearThrust());
        }
    }

    private IEnumerator Routine_SpearThrust()
    {
        isBusy = true;
        animator.SetBool(animCharge, false);
        animator.SetTrigger(animSpearThrust);

        if (attackHitbox != null) attackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(GetAttackCenter(), playerTransform.position);
            if (distance <= attackRange * 1.2f)
            {
                if (playerStats != null)
                {
                    playerStats.TakeDamage(normalDamage); // Trừ máu Player qua CharacterStats[cite: 3]
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (attackHitbox != null) attackHitbox.SetActive(false);

        basicAttackCount++;
        isBusy = false;
    }

    private IEnumerator Routine_KidnapSkill()
    {
        isBusy = true;
        isKidnapping = true;
        animator.SetBool(animCharge, true);

        if (attackHitbox != null) attackHitbox.SetActive(false);

        float kidnapSearchTime = 0f;
        while (Vector2.Distance(transform.position, playerTransform.position) > attackRange && kidnapSearchTime < 5f)
        {
            kidnapSearchTime += Time.deltaTime;
            MoveSmoothly(playerTransform.position, kidnapMoveSpeed);
            FlipTowards(playerTransform.position);
            yield return null;
        }

        animator.SetBool(animGrabBool, true);
        animator.SetBool(animCharge, true);

        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        SpriteRenderer playerSprite = playerTransform.GetComponentInChildren<SpriteRenderer>();

        if (playerCollider != null) playerCollider.enabled = false;
        if (playerRb != null) playerRb.simulated = false;

        Transform attachTarget = grabPoint != null ? grabPoint : transform;
        playerTransform.SetParent(attachTarget);
        playerTransform.localPosition = Vector3.zero;

        currentStraightDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        float timer = 0f;
        float damageTimer = 0f;
        float blinkTimer = 0f;
        bool isVisible = true;

        while (timer < grabDuration)
        {
            timer += Time.deltaTime;
            damageTimer += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (damageTimer >= kidnapDamageInterval)
            {
                damageTimer = 0f;
                if (playerStats != null)
                {
                    playerStats.TakeDamage(kidnapDamage); // Trừ máu định kỳ[cite: 3]
                    if (playerStats.IsDead)
                    {
                        break;
                    }
                }
            }

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                isVisible = !isVisible;

                if (bossSprite != null) bossSprite.enabled = isVisible;
                if (playerSprite != null) playerSprite.enabled = isVisible;
            }

            MoveStraightAndTurnOnWall();

            yield return null;
        }

        if (bossSprite != null) bossSprite.enabled = true;
        if (playerSprite != null) playerSprite.enabled = true;

        Vector2 safeThrowDirection = GetSafeThrowDirection();
        FlipTowards(transform.position + (Vector3)safeThrowDirection);

        animator.SetBool(animGrabBool, false);
        animator.SetBool(animCharge, false);
        animator.SetTrigger(animThrowPlayer);

        playerTransform.SetParent(null);

        if (playerCollider != null) playerCollider.enabled = true;
        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.AddForce(safeThrowDirection * throwForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 fleeTarget = transform.position - (Vector3)safeThrowDirection * stepBackDistance;
        float stepBackTimer = 0f;

        while (stepBackTimer < 0.6f)
        {
            stepBackTimer += Time.deltaTime;
            MoveSmoothly(fleeTarget, chargeSpeed);
            yield return null;
        }

        float slowWalkTimer = 0f;
        animator.SetBool(animCharge, true);

        while (slowWalkTimer < slowWalkDuration)
        {
            slowWalkTimer += Time.deltaTime;
            if (playerTransform != null)
            {
                FlipTowards(playerTransform.position);
                MoveSmoothly(playerTransform.position, slowWalkSpeed);
            }
            yield return null;
        }

        animator.SetBool(animCharge, false);

        basicAttackCount = 0;
        specialSkillTimer = 0f;

        isKidnapping = false;
        isBusy = false;
    }

    private Vector2 GetSafeThrowDirection()
    {
        Vector2 defaultDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 testDir = Quaternion.Euler(0, 0, angle) * defaultDir;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, testDir, throwDistance, obstacleLayer);
            if (hit.collider == null)
            {
                return testDir.normalized;
            }
        }

        return defaultDir;
    }

    private void MoveStraightAndTurnOnWall()
    {
        Vector2 currentPos = transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(currentPos, avoidRadius, currentStraightDir, wallCheckDistance, obstacleLayer);

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Vector2 turnDir1 = Vector2.Perpendicular(hit.normal).normalized;
            Vector2 turnDir2 = -turnDir1;

            if (Physics2D.CircleCast(currentPos, avoidRadius, turnDir1, wallCheckDistance, obstacleLayer).collider == null)
            {
                currentStraightDir = turnDir1;
            }
            else
            {
                currentStraightDir = turnDir2;
            }
        }

        FlipTowards(transform.position + (Vector3)currentStraightDir);
        transform.position += (Vector3)(currentStraightDir * (kidnapMoveSpeed * Time.deltaTime));
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