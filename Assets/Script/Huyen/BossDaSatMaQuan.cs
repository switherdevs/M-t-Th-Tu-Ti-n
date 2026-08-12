using StatsSystem.Components;
using System.Collections;
using UnityEngine;

public class BossDaSatMaQuan : MonoBehaviour
{
    // =========================================================
    // PLAYER + PHÁT HIỆN
    // =========================================================

    [Header("===== PHÁT HIỆN PLAYER =====")]

    [SerializeField] private Transform player;

    [Tooltip("Layer của Player")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("Bán kính phát hiện Player bằng OverlapCircle")]
    [SerializeField] private float detectRange = 12f;

    [Tooltip("Khoảng cách Boss bắt đầu tấn công")]
    [SerializeField] private float attackRange = 2f;


    // =========================================================
    // VỊ TRÍ ATTACK RAGE
    // =========================================================

    [Header("===== ATTACK RAGE POSITION =====")]

    [Tooltip("Transform của điểm/khu vực AttackRage")]
    [SerializeField] private Transform attackRage;

    [Tooltip("Tọa độ Offset X của AttackRage so với Boss")]
    [SerializeField] private float attackRageOffsetX = 0f;

    [Tooltip("Tọa độ Offset Y của AttackRage so với Boss")]
    [SerializeField] private float attackRageOffsetY = 0f;


    // =========================================================
    // DI CHUYỂN
    // =========================================================

    [Header("===== DI CHUYỂN =====")]

    [SerializeField] private float moveSpeed = 2.5f;


    // =========================================================
    // THỂ LỰC
    // =========================================================

    [Header("===== THỂ LỰC =====")]

    [SerializeField] private int maxStamina = 10;

    [SerializeField] private int currentStamina = 0;

    [Tooltip("Thời gian Boss bị mệt")]
    [SerializeField] private float tiredTime = 5f;


    // =========================================================
    // ĐÁNH THƯỜNG
    // =========================================================

    [Header("===== ĐÁNH THƯỜNG =====")]

    [SerializeField] private Transform attackPoint1;

    [SerializeField] private Transform attackPoint2;

    [SerializeField] private int normalAttackDamage = 15;

    [SerializeField] private int normalAttackStamina = 1;

    [Tooltip("Thời gian chờ trước khi bật Collider")]
    [SerializeField] private float normalAttackDelay = 0.2f;

    [Tooltip("Thời gian Collider tồn tại")]
    [SerializeField] private float attackColliderTime = 0.2f;

    [SerializeField] private float attackCooldown = 0.8f;


    // =========================================================
    // SKILL 1
    // =========================================================

    [Header("===== SKILL 1 - TRÂU HÚC =====")]

    [SerializeField] private Transform chargePoint;

    [SerializeField] private int chargeDamage = 20;

    [SerializeField] private int chargeStamina = 3;

    [SerializeField] private float chargeSpeed = 8f;

    [SerializeField] private float chargeTime = 0.8f;

    [SerializeField] private float chargeDelay = 0.3f;

    [SerializeField] private float chargeCooldown = 1f;


    // =========================================================
    // SKILL 2
    // =========================================================

    [Header("===== SKILL 2 - BÙNG NĂNG LƯỢNG =====")]

    [SerializeField] private Transform skill2Point;

    [SerializeField] private int skill2Damage = 10;

    [SerializeField] private int skill2Stamina = 5;

    [SerializeField] private float skill2Delay = 0.3f;

    [SerializeField] private float skill2ColliderTime = 0.5f;

    [SerializeField] private float skill2Cooldown = 1f;


    // =========================================================
    // SKILL 3
    // =========================================================

    [Header("===== SKILL 3 - TRIỆU HỒI =====")]

    [SerializeField] private Transform summonPoint;

    [SerializeField] private GameObject minionPrefab;

    [SerializeField] private int summonCount = 3;

    [SerializeField] private int summonStamina = 2;

    [SerializeField] private float summonDelay = 0.5f;

    [SerializeField] private float summonCooldown = 1f;


    // =========================================================
    // ANIMATOR
    // =========================================================

    [Header("===== ANIMATOR =====")]

    [SerializeField] private Animator animator;


    [Tooltip("Tên State Animation Idle")]
    [SerializeField] private string idleAnimation = "Idle";

    [Tooltip("Tên State Animation Walk")]
    [SerializeField] private string walkAnimation = "Walk";

    [Tooltip("Tên State Animation Attack 1")]
    [SerializeField] private string attack1Animation = "Attack1";

    [Tooltip("Tên State Animation Attack 2")]
    [SerializeField] private string attack2Animation = "Attack2";

    [Tooltip("Tên State Animation Skill 1")]
    [SerializeField] private string skill1Animation = "Skill1";

    [Tooltip("Tên State Animation Skill 2")]
    [SerializeField] private string skill2Animation = "Skill2";

    [Tooltip("Tên State Animation Skill 3")]
    [SerializeField] private string skill3Animation = "Skill3";

    [Tooltip("Tên State Animation Tired")]
    [SerializeField] private string tiredAnimation = "Tired";


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("===== DEBUG =====")]

    [SerializeField] private bool showDebug = true;


    // =========================================================
    // PRIVATE
    // =========================================================

    private Rigidbody2D rb;

    private bool playerDetected = false;

    private bool isAttacking = false;

    private bool isUsingSkill = false;

    private bool isTired = false;

    private float attackTimer = 0f;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Lấy Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "❌ Boss chưa có Rigidbody2D!"
            );
        }

        // Lấy Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning(
                "⚠️ Boss chưa có Animator!"
            );
        }

        // Tìm Player nếu chưa kéo vào Inspector
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;

                if (showDebug)
                {
                    Debug.Log(
                        "✅ Đã tìm thấy Player: "
                        + player.name
                    );
                }
            }
        }

        // Tắt tất cả Collider đánh lúc bắt đầu
        DisableAllAttackColliders();

        // Animation ban đầu
        PlayAnimation(idleAnimation);

        if (showDebug)
        {
            Debug.Log(
                "👹 DẠ SÁT MA QUÂN ĐÃ KHỞI ĐỘNG!"
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Cập nhật vị trí AttackRage theo thời gian thực
        UpdateAttackRagePosition();

        // Giảm cooldown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        // Boss đang mệt
        if (isTired)
        {
            StopMoving();
            return;
        }

        // Đang đánh / dùng skill
        if (isAttacking || isUsingSkill)
        {
            StopMoving();
            return;
        }

        // Tìm Player bằng OverlapCircle
        FindPlayerWithOverlapCircle();

        // Không thấy Player
        if (!playerDetected || player == null)
        {
            StopMoving();

            PlayAnimation(idleAnimation);

            return;
        }

        // Khoảng cách đến Player
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // Đủ gần để đánh
        if (distance <= attackRange)
        {
            StopMoving();

            PlayAnimation(idleAnimation);

            if (attackTimer <= 0f)
            {
                ChooseAttackOrSkill();
            }

            return;
        }

        // Chưa đủ gần -> đuổi
        ChasePlayer();
    }


    // =========================================================
    // CẬP NHẬT VỊ TRÍ ATTACK RAGE
    // =========================================================

    private void UpdateAttackRagePosition()
    {
        if (attackRage == null)
            return;

        // Xác định hướng mặt của Boss dựa trên transform.localScale.x
        float facingDirection = Mathf.Sign(transform.localScale.x);

        // Tính vị trí mới real-time dựa trên vị trí Boss + Offset
        Vector3 targetPosition = new Vector3(
            transform.position.x + (attackRageOffsetX * facingDirection),
            transform.position.y + attackRageOffsetY,
            attackRage.position.z
        );

        attackRage.position = targetPosition;
    }


    // =========================================================
    // OVERLAP CIRCLE
    // =========================================================

    private void FindPlayerWithOverlapCircle()
    {
        playerDetected = false;

        Collider2D[] hits;

        // Nếu đã chọn Player Layer
        if (playerLayer.value != 0)
        {
            hits = Physics2D.OverlapCircleAll(
                transform.position,
                detectRange,
                playerLayer
            );
        }
        else
        {
            // Nếu quên chọn Layer thì vẫn thử tìm
            hits = Physics2D.OverlapCircleAll(
                transform.position,
                detectRange
            );
        }

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Transform foundPlayer =
                GetPlayerTransform(hit);

            if (foundPlayer != null)
            {
                player = foundPlayer;
                playerDetected = true;

                return;
            }
        }
    }


    // =========================================================
    // TÌM PLAYER TỪ COLLIDER
    // =========================================================

    private Transform GetPlayerTransform(
        Collider2D collider
    )
    {
        Transform current =
            collider.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }


    // =========================================================
    // ĐUỔI PLAYER
    // =========================================================

    private void ChasePlayer()
    {
        if (rb == null || player == null)
            return;

        float directionX = Mathf.Sign(
            player.position.x -
            transform.position.x
        );

        // Unity 6
        rb.linearVelocity = new Vector2(
            directionX * moveSpeed,
            rb.linearVelocity.y
        );

        // Animation Walk
        PlayAnimation(walkAnimation);

        // Quay mặt
        FacePlayer(directionX);
    }


    // =========================================================
    // DỪNG
    // =========================================================

    private void StopMoving()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }


    // =========================================================
    // QUAY MẶT
    // =========================================================

    private void FacePlayer(float directionX)
    {
        if (directionX == 0)
            return;

        Vector3 scale =
            transform.localScale;

        scale.x =
            Mathf.Abs(scale.x) * directionX;

        transform.localScale = scale;
    }


    // =========================================================
    // CHỌN ATTACK / SKILL
    // =========================================================

    private void ChooseAttackOrSkill()
    {
        float random =
            Random.Range(0f, 100f);

        if (random < 50f)
        {
            StartCoroutine(
                NormalAttack()
            );
        }
        else if (random < 70f)
        {
            StartCoroutine(
                Skill1Charge()
            );
        }
        else if (random < 85f)
        {
            StartCoroutine(
                Skill2Burst()
            );
        }
        else
        {
            StartCoroutine(
                Skill3Summon()
            );
        }

        attackTimer = 1f;
    }


    // =========================================================
    // ĐÁNH THƯỜNG
    // =========================================================

    private IEnumerator NormalAttack()
    {
        isAttacking = true;

        StopMoving();

        bool attack1 =
            Random.Range(0, 2) == 0;

        if (attack1)
        {
            PlayAnimation(
                attack1Animation
            );

            if (showDebug)
            {
                Debug.Log(
                    "⚔️ Boss dùng ATTACK 1"
                );
            }

            yield return new WaitForSeconds(
                normalAttackDelay
            );

            EnableAttackCollider(
                attackPoint1,
                normalAttackDamage
            );
        }
        else
        {
            PlayAnimation(
                attack2Animation
            );

            if (showDebug)
            {
                Debug.Log(
                    "⚔️ Boss dùng ATTACK 2"
                );
            }

            yield return new WaitForSeconds(
                normalAttackDelay
            );

            EnableAttackCollider(
                attackPoint2,
                normalAttackDamage
            );
        }

        // +1 thể lực
        AddStamina(
            normalAttackStamina
        );

        // Collider tồn tại
        yield return new WaitForSeconds(
            attackColliderTime
        );

        DisableAllAttackColliders();

        // Cooldown
        yield return new WaitForSeconds(
            attackCooldown
        );

        isAttacking = false;

        if (!isTired)
        {
            PlayAnimation(
                idleAnimation
            );
        }
    }


    // =========================================================
    // SKILL 1 - TRÂU HÚC
    // =========================================================

    private IEnumerator Skill1Charge()
    {
        isUsingSkill = true;

        StopMoving();

        PlayAnimation(
            skill1Animation
        );

        if (showDebug)
        {
            Debug.Log(
                "🐂 Boss dùng SKILL 1 - TRÂU HÚC!"
            );
        }

        yield return new WaitForSeconds(
            chargeDelay
        );

        if (player != null)
        {
            float directionX =
                Mathf.Sign(
                    player.position.x -
                    transform.position.x
                );

            FacePlayer(directionX);

            EnableAttackCollider(
                chargePoint,
                chargeDamage
            );

            float timer = 0f;

            while (timer < chargeTime)
            {
                if (rb != null)
                {
                    rb.linearVelocity =
                        new Vector2(
                            directionX *
                            chargeSpeed,
                            rb.linearVelocity.y
                        );
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }

        StopMoving();

        DisableCollider(
            chargePoint
        );

        AddStamina(
            chargeStamina
        );

        yield return new WaitForSeconds(
            chargeCooldown
        );

        isUsingSkill = false;

        if (!isTired)
        {
            PlayAnimation(
                idleAnimation
            );
        }
    }


    // =========================================================
    // SKILL 2 - BÙNG NĂNG LƯỢNG
    // =========================================================

    private IEnumerator Skill2Burst()
    {
        isUsingSkill = true;

        StopMoving();

        PlayAnimation(
            skill2Animation
        );

        if (showDebug)
        {
            Debug.Log(
                "💥 Boss dùng SKILL 2 - BÙNG NĂNG LƯỢNG!"
            );
        }

        yield return new WaitForSeconds(
            skill2Delay
        );

        EnableAttackCollider(
            skill2Point,
            skill2Damage
        );

        if (showDebug)
        {
            Debug.Log(
                "🔴 SKILL 2 COLLIDER: ON"
            );
        }

        yield return new WaitForSeconds(
            skill2ColliderTime
        );

        DisableCollider(
            skill2Point
        );

        if (showDebug)
        {
            Debug.Log(
                "⚪ SKILL 2 COLLIDER: OFF"
            );
        }

        AddStamina(
            skill2Stamina
        );

        yield return new WaitForSeconds(
            skill2Cooldown
        );

        isUsingSkill = false;

        if (!isTired)
        {
            PlayAnimation(
                idleAnimation
            );
        }
    }


    // =========================================================
    // SKILL 3 - TRIỆU HỒI
    // =========================================================

    private IEnumerator Skill3Summon()
    {
        isUsingSkill = true;

        StopMoving();

        PlayAnimation(
            skill3Animation
        );

        if (showDebug)
        {
            Debug.Log(
                "👹 Boss dùng SKILL 3 - TRIỆU HỒI!"
            );
        }

        yield return new WaitForSeconds(
            summonDelay
        );

        if (minionPrefab == null)
        {
            Debug.LogWarning(
                "⚠️ Chưa kéo Minion Prefab vào Inspector!"
            );
        }
        else
        {
            Vector3 spawnPosition;

            if (summonPoint != null)
            {
                spawnPosition =
                    summonPoint.position;
            }
            else
            {
                spawnPosition =
                    transform.position;
            }

            for (
                int i = 0;
                i < summonCount;
                i++
            )
            {
                Vector3 position =
                    spawnPosition;

                position.x +=
                    Random.Range(
                        -1.5f,
                        1.5f
                    );

                position.y +=
                    Random.Range(
                        -0.5f,
                        0.5f
                    );

                Instantiate(
                    minionPrefab,
                    position,
                    Quaternion.identity
                );

                yield return new WaitForSeconds(
                    0.1f
                );
            }
        }

        AddStamina(
            summonStamina
        );

        yield return new WaitForSeconds(
            summonCooldown
        );

        isUsingSkill = false;

        if (!isTired)
        {
            PlayAnimation(
                idleAnimation
            );
        }
    }


    // =========================================================
    // BẬT COLLIDER + DAMAGE
    // =========================================================

    private void EnableAttackCollider(
        Transform attackPoint,
        int damage
    )
    {
        if (attackPoint == null)
        {
            Debug.LogWarning(
                "⚠️ Attack Point đang trống!"
            );

            return;
        }

        Collider2D col =
            attackPoint.GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogWarning(
                "⚠️ " +
                attackPoint.name +
                " chưa có Collider2D!"
            );

            return;
        }

        BossAttackDamage damageScript =
            attackPoint.GetComponent<BossAttackDamage>();

        if (damageScript == null)
        {
            damageScript =
                attackPoint.gameObject.AddComponent<BossAttackDamage>();
        }

        damageScript.SetDamage(
            damage
        );

        col.enabled = true;
    }


    // =========================================================
    // TẮT COLLIDER
    // =========================================================

    private void DisableCollider(
        Transform attackPoint
    )
    {
        if (attackPoint == null)
            return;

        Collider2D col =
            attackPoint.GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }
    }


    // =========================================================
    // TẮT TẤT CẢ COLLIDER
    // =========================================================

    private void DisableAllAttackColliders()
    {
        DisableCollider(
            attackPoint1
        );

        DisableCollider(
            attackPoint2
        );

        DisableCollider(
            chargePoint
        );

        DisableCollider(
            skill2Point
        );
    }


    // =========================================================
    // THỂ LỰC
    // =========================================================

    private void AddStamina(
        int amount
    )
    {
        if (isTired)
            return;

        currentStamina += amount;

        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0,
                maxStamina
            );

        if (showDebug)
        {
            Debug.Log(
                "⚡ THỂ LỰC: "
                + currentStamina
                + " / "
                + maxStamina
            );
        }

        if (
            currentStamina >=
            maxStamina
        )
        {
            StartTired();
        }
    }


    // =========================================================
    // BOSS MỆT
    // =========================================================

    private void StartTired()
    {
        if (isTired)
            return;

        isTired = true;

        StopMoving();

        DisableAllAttackColliders();

        PlayAnimation(
            tiredAnimation
        );

        if (showDebug)
        {
            Debug.Log(
                "😴 DẠ SÁT MA QUÂN ĐÃ MỆT!"
            );
        }

        StartCoroutine(
            TiredCoroutine()
        );
    }


    // =========================================================
    // HẾT MỆT
    // =========================================================

    private IEnumerator TiredCoroutine()
    {
        yield return new WaitForSeconds(
            tiredTime
        );

        currentStamina = 0;

        isTired = false;

        if (showDebug)
        {
            Debug.Log(
                "🔥 DẠ SÁT MA QUÂN ĐÃ HẾT MỆT!"
            );
        }

        PlayAnimation(
            idleAnimation
        );
    }


    // =========================================================
    // ANIMATOR
    // =========================================================

    private void PlayAnimation(
        string animationName
    )
    {
        if (animator == null)
            return;

        if (
            string.IsNullOrEmpty(
                animationName
            )
        )
            return;

        int hash =
            Animator.StringToHash(
                animationName
            );

        if (
            !animator.HasState(
                0,
                hash
            )
        )
        {
            if (showDebug)
            {
                Debug.LogWarning(
                    "⚠️ Animator không có State: "
                    + animationName
                );
            }

            return;
        }

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(
                0
            );

        if (
            state.shortNameHash ==
            hash
        )
        {
            return;
        }

        animator.Play(
            hash,
            0,
            0f
        );
    }


    // =========================================================
    // GIZMOS (ĐÃ SỬA VỊ TRÍ ATTACK RAGE)
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // Vòng phát hiện Player (Detect Range) - vẽ từ vị trí Boss
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        // Vòng tầm đánh (Attack Range)
        // Nếu có attackRage -> Vẽ tại tâm của attackRage
        // Nếu không có -> Vẽ tại vị trí của Boss
        Gizmos.color = Color.red;

        if (attackRage != null)
        {
            Gizmos.DrawWireSphere(
                attackRage.position,
                attackRange
            );
        }
        else
        {
            Gizmos.DrawWireSphere(
                transform.position,
                attackRange
            );
        }
    }


    // =========================================================
    // PUBLIC
    // =========================================================

    public int GetCurrentStamina()
    {
        return currentStamina;
    }

    public int GetMaxStamina()
    {
        return maxStamina;
    }

    public bool IsTired()
    {
        return isTired;
    }


    // =========================================================
    // DAMAGE SCRIPT NỘI BỘ
    // =========================================================

    private class BossAttackDamage : MonoBehaviour
    {
        private int damage;

        private bool hasHit = false;


        public void SetDamage(int newDamage)
        {
            damage = newDamage;
            hasHit = false;
        }


        private void OnEnable()
        {
            hasHit = false;
        }


        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            if (hasHit)
                return;

            if (!other.CompareTag("Player"))
                return;

            Component stats =
                other.GetComponentInParent<CharacterStats>();

            if (stats != null)
            {
                stats.SendMessage(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );

                hasHit = true;

                Debug.Log(
                    "👹 Boss gây "
                    + damage
                    + " damage!"
                );
            }
        }
    }
}