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
    [SerializeField, Tooltip("Tùy chỉnh offset X và Y cho tâm quét né tường")]
    private Vector2 wallCheckOffset = Vector2.zero;

    [Header("--- SÁT THƯƠNG & HITBOX ĐÁNH THƯỜNG ---")]
    [SerializeField] private float normalDamage = 15f;
    [SerializeField, Tooltip("GameObject Effect/Hitbox chỉ hiện lên khi tung đòn đánh")]
    private GameObject attackHitbox;

    [Header("--- TÙY CHỌN ẨN / HIỆN GAMEOBJECT ---")]
    [SerializeField, Tooltip("GameObject sẽ ẩn/hiện (Nếu để trống sẽ mặc định dùng GameObject của Boss)")]
    private GameObject targetVisualObject;
    [SerializeField] private float stealthShowDuration = 2.5f; // Thời gian hiện lên khi tấn công
    [SerializeField] private float stealthHideDuration = 1.5f; // Thời gian ẩn đi

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

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxNormalAttack;   // Âm thanh đánh thường
    [SerializeField] private AudioClip sfxSpecialPrepare; // Âm thanh chuẩn bị lao vào bắt người chơi
    [SerializeField] private AudioClip sfxSpecialCast;    // Âm thanh khi bắt trúng người chơi
    [SerializeField] private AudioClip sfxKidnappingLoop; // Âm thanh phát liên tục/định kỳ trong lúc đang bắt người chơi

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCharge = "isCharging";
    [SerializeField] private string animSpearThrust = "SpearThrust";
    [SerializeField] private string animGrabBool = "isGrabbing";
    [SerializeField] private string animThrowPlayer = "ThrowPlayer";
    [SerializeField] private string animDie = "Die";

    private Transform playerTransform;
    private CharacterStats playerStats;
    private CharacterStats bossStats;
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
        bossStats = GetComponent<CharacterStats>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (targetVisualObject == null)
        {
            targetVisualObject = this.gameObject;
        }
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

    private void HandleBossDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        if (targetVisualObject != null)
        {
            targetVisualObject.SetActive(true);
        }

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

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

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

        // Bật hiển thị Visual GameObject trong khoảng thời gian stealthShowDuration
        if (targetVisualObject != null) targetVisualObject.SetActive(true);

        animator.SetBool(animCharge, false);
        animator.SetTrigger(animSpearThrust);
        PlaySFX(sfxNormalAttack); // Âm thanh đánh thường

        if (attackHitbox != null) attackHitbox.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(GetAttackCenter(), playerTransform.position);
            if (distance <= attackRange * 1.2f)
            {
                if (playerStats != null)
                {
                    playerStats.TakeDamage(normalDamage);
                }
            }
        }

        yield return new WaitForSeconds(stealthShowDuration);

        if (attackHitbox != null) attackHitbox.SetActive(false);

        // Nút ẩn Visual GameObject sau khi hết thời gian tấn công
        if (targetVisualObject != null && targetVisualObject != this.gameObject)
        {
            targetVisualObject.SetActive(false);
            yield return new WaitForSeconds(stealthHideDuration);
        }

        basicAttackCount++;
        isBusy = false;
    }

    private IEnumerator Routine_KidnapSkill()
    {
        isBusy = true;
        isKidnapping = true;

        if (targetVisualObject != null) targetVisualObject.SetActive(true);

        animator.SetBool(animCharge, true);
        PlaySFX(sfxSpecialPrepare); // Âm thanh chuẩn bị lao vào bắt

        if (attackHitbox != null) attackHitbox.SetActive(false);

        float kidnapSearchTime = 0f;
        while (Vector2.Distance(transform.position, playerTransform.position) > attackRange && kidnapSearchTime < 5f)
        {
            kidnapSearchTime += Time.deltaTime;
            MoveSmoothly(playerTransform.position, kidnapMoveSpeed);
            FlipTowards(playerTransform.position);
            yield return null;
        }

        PlaySFX(sfxSpecialCast); // Âm thanh khi đã tiếp cận bắt trúng

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
                PlaySFX(sfxKidnappingLoop); // Âm thanh định kỳ/vô hiệu hóa trong lúc bắt người chơi

                if (playerStats != null)
                {
                    playerStats.TakeDamage(kidnapDamage);
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
        Vector2 currentPos = GetWallCheckCenter();

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GetWallCheckCenter(), avoidRadius);
    }
}