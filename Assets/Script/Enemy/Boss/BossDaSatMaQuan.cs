using StatsSystem.Components;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossDaSatMaQuan : MonoBehaviour
{
    // =========================================================
    // PLAYER + PHÁT HIỆN
    // =========================================================

    [Header("===== PHÁT HIỆN PLAYER =====")]

    [Tooltip("Layer của Player để tìm kiếm")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("Bán kính phát hiện Player bằng OverlapCircle")]
    [SerializeField] private float detectRange = 12f;

    [Tooltip("Khoảng cách Boss bắt đầu tấn công")]
    [SerializeField] private float attackRange = 2f;


    // =========================================================
    // NÉ TƯỜNG MƯỢT MÀ (SMOOTH OBSTACLE AVOIDANCE)
    // =========================================================

    [Header("===== NÉ TƯỜNG MƯỢT MÀ =====")]

    [Tooltip("Layer chứa các vật cản/tường")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Khoảng cách tia Raycast phát hiện tường để né")]
    [SerializeField] private float wallDetectDistance = 1.8f;

    [Tooltip("Khoảng cách đệm duy trì với tường để không bị dính vào tường")]
    [SerializeField] private float wallBufferDistance = 0.8f;

    [Tooltip("Độ mượt khi bẻ lái né tường (Giá trị nhỏ = mượt hơn)")]
    [SerializeField] private float avoidanceSmoothing = 6f;


    // =========================================================
    // HIỆU ỨNG & ĐƯỜNG BÁO CHIÊU (TELEGRAPH)
    // =========================================================

    [Header("===== HIỆU ỨNG & CẢNH BÁO CHIÊU =====")]

    [Tooltip("GameObject Hiệu ứng chung (sẽ tự tắt và chỉ bật khi đánh/dùng skill)")]
    [SerializeField] private GameObject attackEffect;

    [Tooltip("GameObject đường báo hiệu hướng húc của Skill 1 (Mặc định sẽ ẩn)")]
    [SerializeField] private GameObject skill1LineWarning;

    [Tooltip("GameObject vùng báo hiệu phạm vi của Skill 2 (Mặc định sẽ ẩn)")]
    [SerializeField] private GameObject skill2AreaWarning;


    // =========================================================
    // VỊ TRÍ ATTACK RAGE
    // =========================================================

    [Header("===== ATTACK RAGE POSITION =====")]

    [Tooltip("Transform của điểm/khu vực AttackRage")]
    [SerializeField] private Transform attackRage;

    [Tooltip("Tọa độ Offset X của AttackRage so meo Boss")]
    [SerializeField] private float attackRageOffsetX = 0f;

    [Tooltip("Tọa độ Offset Y của AttackRage so với Boss")]
    [SerializeField] private float attackRageOffsetY = 0f;


    // =========================================================
    // DI CHUYỂN
    // =========================================================

    [Header("===== DI CHUYỂN =====")]

    [SerializeField] private float moveSpeed = 2.5f;

    [Tooltip("Tốc độ di chuyển khi Boss đang chuẩn bị ra Skill (Thường chỉnh nhỏ lại để đi chậm)")]
    [SerializeField] private float slowMoveSpeed = 0.8f;

    [Tooltip("Thời gian Boss khựng/đi chậm trước khi tung Skill (Giây)")]
    [SerializeField] private float skillPrepTime = 0.6f;

    [Tooltip("Khoảng cách chấp nhận được về chiều cao Y để tránh giật rung")]
    [SerializeField] private float stopDistanceY = 0.15f;


    // =========================================================
    // THỂ LỰC
    // =========================================================

    [Header("===== THỂ LỰC =====")]

    [SerializeField] private int maxStamina = 10;

    [SerializeField] private int currentStamina = 0;

    [Tooltip("Thời gian Boss bị mệt")]
    [SerializeField] private float tiredTime = 5f;

    [Tooltip("Slider hiển thị thể lực của Boss")]
    [SerializeField] private Slider staminaSlider;


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
    // SKILL 1 - TRÂU HÚC
    // =========================================================

    [Header("===== SKILL 1 - TRÂU HÚC =====")]

    [SerializeField] private Transform chargePoint;

    [SerializeField] private int chargeDamage = 20;

    [SerializeField] private int chargeStamina = 3;

    [SerializeField] private float chargeSpeed = 8f;

    [SerializeField] private float chargeTime = 0.8f;

    [SerializeField] private float chargeDelay = 0.3f;

    [SerializeField] private float chargeCooldown = 1f;

    [Tooltip("Kích thước vùng quét kiểm tra tường khi đang húc")]
    [SerializeField] private Vector2 chargeCheckSize = new Vector2(1f, 1f);


    // =========================================================
    // SKILL 2 - BÙNG NĂNG LƯỢNG (DÙNG PREFAB TẠI TRANSFORMS BOSS)
    // =========================================================

    [Header("===== SKILL 2 - BÙNG NĂNG LƯỢNG =====")]

    [Tooltip("Prefab của Skill 2 (Tự sinh ra tại vị trí Boss)")]
    [SerializeField] private GameObject skill2Prefab;

    [SerializeField] private int skill2Stamina = 5;

    [SerializeField] private float skill2Delay = 0.3f;

    [SerializeField] private float skill2Cooldown = 1f;


    // =========================================================
    // SKILL 3 - TRIỆU HỒI
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

    [Tooltip("Tên Parameter (Bool) Animation Walk")]
    [SerializeField] private string walkAnimation = "Walk";

    [Tooltip("Tên Parameter (Trigger) Animation Attack 1")]
    [SerializeField] private string attack1Animation = "Attack1";

    [Tooltip("Tên Parameter (Trigger) Animation Attack 2")]
    [SerializeField] private string attack2Animation = "Attack2";

    [Tooltip("Tên Parameter (Trigger) Animation Skill 1")]
    [SerializeField] private string skill1Animation = "Skill1";

    [Tooltip("Tên Parameter (Trigger) Animation Skill 2")]
    [SerializeField] private string skill2Animation = "Skill2";

    [Tooltip("Tên Parameter (Trigger) Animation Skill 3")]
    [SerializeField] private string skill3Animation = "Skill3";

    [Tooltip("Tên Parameter (Bool) Animation Tired")]
    [SerializeField] private string tiredAnimation = "Tired";


    // =========================================================
    // DEBUG & TESTING
    // =========================================================

    [Header("===== DEBUG & TEST SKILL =====")]

    [SerializeField] private bool showDebug = true;

    [Tooltip("Bật chế độ bấm phím 1, 2, 3, 4 để test Skill thủ công")]
    [SerializeField] private bool enableHotkeyTesting = true;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private Transform playerTransform;

    private Rigidbody2D rb;

    private bool playerDetected = false;

    private bool isAttacking = false;

    private bool isUsingSkill = false;

    private bool isTired = false;

    // PUBLIC PROPERTY ĐỂ CÁC SCRIPT KHÁC (NHƯ CHARACTERSTATS) CÓ THỂ ĐỌC ĐƯỢC TRẠNG THÁI MỆT
    public bool IsTired => isTired;

    private float attackTimer = 0f;

    private int comboIndex = 0;

    private Vector2 currentVelocityVector = Vector2.zero;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("❌ Boss chưa có Rigidbody2D!");
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (attackEffect != null)
        {
            attackEffect.SetActive(false);
        }

        // Tắt mặc định các đường/vùng cảnh báo Skill
        SetWarningSkill1Active(false);
        SetWarningSkill2Active(false);

        DisableAllAttackColliders();
        UpdateStaminaUI();

        if (showDebug)
        {
            Debug.Log("👹 DẠ SÁT MA QUÂN ĐÃ KHỞI ĐỘNG!");
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        UpdateAttackRagePosition();

        if (enableHotkeyTesting)
        {
            HandleHotkeyTesting();
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (isTired || isAttacking || isUsingSkill)
        {
            return;
        }

        FindPlayerWithOverlapCircle();

        if (!playerDetected || playerTransform == null)
        {
            StopMoving();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= attackRange)
        {
            StopMoving();

            if (attackTimer <= 0f)
            {
                ExecuteComboSequence();
            }

            return;
        }

        ChasePlayerSmoothly();
    }


    // =========================================================
    // DI CHUYỂN ĐUỔI PLAYER VÀ NÉ TƯỜNG CỰC MƯỢT
    // =========================================================

    private void ChasePlayerSmoothly()
    {
        if (rb == null || playerTransform == null) return;

        Vector2 targetDir = (playerTransform.position - transform.position).normalized;
        Vector2 smoothDir = CalculateSmoothAvoidanceDirection(targetDir);

        currentVelocityVector = Vector2.Lerp(currentVelocityVector, smoothDir, Time.deltaTime * avoidanceSmoothing);

        rb.linearVelocity = currentVelocityVector * moveSpeed;

        SetBoolAnimation(walkAnimation, true);

        if (currentVelocityVector.x != 0)
        {
            FacePlayer(currentVelocityVector.x);
        }
    }

    private Vector2 CalculateSmoothAvoidanceDirection(Vector2 targetDir)
    {
        float[] angles = new float[] { 0f, 22.5f, -22.5f, 45f, -45f };
        Vector2 bestDir = targetDir;
        bool wallDetected = false;

        foreach (float angle in angles)
        {
            Vector2 checkDir = Quaternion.Euler(0, 0, angle) * targetDir;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDir, wallDetectDistance, wallLayer);

            Debug.DrawRay(transform.position, checkDir * wallDetectDistance, Color.cyan);

            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                wallDetected = true;

                if (hit.distance < wallBufferDistance)
                {
                    Vector2 pushAway = (Vector2)transform.position - hit.point;
                    bestDir += pushAway.normalized * (wallBufferDistance - hit.distance);
                }
            }
            else if (wallDetected)
            {
                bestDir = checkDir;
                break;
            }
        }

        return bestDir.normalized;
    }


    // =========================================================
    // DỪNG & QUAY MẶT
    // =========================================================

    private void StopMoving()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        currentVelocityVector = Vector2.zero;
        SetBoolAnimation(walkAnimation, false);
    }

    private void FacePlayer(float directionX)
    {
        if (directionX == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
        transform.localScale = scale;
    }


    // =========================================================
    // QUẢN LÝ THỨ TỰ SKILL (COMBO SEQUENCE)
    // =========================================================

    private void ExecuteComboSequence()
    {
        switch (comboIndex)
        {
            case 0:
            case 1:
            case 2:
                StartCoroutine(NormalAttack());
                break;
            case 3:
                StartCoroutine(Skill1Charge());
                break;
            case 4:
                StartCoroutine(Skill2Burst());
                break;
            case 5:
                StartCoroutine(Skill3Summon());
                break;
        }

        comboIndex = (comboIndex + 1) % 6;
        attackTimer = 1f;
    }


    // =========================================================
    // HÀM BỌC: ĐI CHẬM/KHỰNG LẠI TRƯỚC KHI THI TRUYỂN SKILL
    // =========================================================

    private IEnumerator SkillPreparation()
    {
        if (playerTransform != null)
        {
            float dirX = playerTransform.position.x - transform.position.x;
            FacePlayer(dirX);

            Vector2 slowDir = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = slowDir * slowMoveSpeed;
            SetBoolAnimation(walkAnimation, true);
        }

        yield return new WaitForSeconds(skillPrepTime);

        StopMoving();
    }


    // =========================================================
    // ĐÁNH THƯỜNG
    // =========================================================

    private IEnumerator NormalAttack()
    {
        isAttacking = true;
        StopMoving();

        bool attack1 = Random.Range(0, 2) == 0;

        if (attack1)
        {
            SetTriggerAnimation(attack1Animation);
            yield return new WaitForSeconds(normalAttackDelay);

            SetEffectActive(true);
            EnableAttackCollider(attackPoint1, normalAttackDamage);
        }
        else
        {
            SetTriggerAnimation(attack2Animation);
            yield return new WaitForSeconds(normalAttackDelay);

            SetEffectActive(true);
            EnableAttackCollider(attackPoint2, normalAttackDamage);
        }

        AddStamina(normalAttackStamina);

        yield return new WaitForSeconds(attackColliderTime);

        DisableAllAttackColliders();
        SetEffectActive(false);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }


    // =========================================================
    // SKILL 1 - TRÂU HÚC (HIỆN ĐƯỜNG BÁO HÚC)
    // =========================================================

    private IEnumerator Skill1Charge()
    {
        isUsingSkill = true;

        SetWarningSkill1Active(true);

        yield return StartCoroutine(SkillPreparation());

        SetTriggerAnimation(skill1Animation);
        yield return new WaitForSeconds(chargeDelay);

        if (playerTransform != null)
        {
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            float directionY = Mathf.Sign(playerTransform.position.y - transform.position.y);

            FacePlayer(directionX);

            SetEffectActive(true);
            EnableAttackCollider(chargePoint, chargeDamage);

            float timer = 0f;
            bool hitWall = false;

            while (timer < chargeTime)
            {
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(directionX * chargeSpeed, directionY * chargeSpeed);
                }

                Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, chargeCheckSize, 0f, wallLayer);
                foreach (Collider2D col in hits)
                {
                    if (col.CompareTag("Wall"))
                    {
                        hitWall = true;
                        break;
                    }
                }

                if (hitWall)
                {
                    Debug.LogWarning("💥 BOSS HÚC TRÚNG TƯỜNG! BỊ BÀNG HOÀNG / MỆT!");
                    StopMoving();
                    DisableCollider(chargePoint);
                    SetEffectActive(false);
                    SetWarningSkill1Active(false);
                    isUsingSkill = false;

                    StartTired();
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        SetWarningSkill1Active(false);

        StopMoving();
        DisableCollider(chargePoint);
        SetEffectActive(false);

        AddStamina(chargeStamina);
        yield return new WaitForSeconds(chargeCooldown);
        isUsingSkill = false;
    }


    // =========================================================
    // SKILL 2 - BÙNG NĂNG LƯỢNG (HIỆN VÙNG BÁO & SPAWN TẠI BOSS)
    // =========================================================

    private IEnumerator Skill2Burst()
    {
        isUsingSkill = true;

        SetWarningSkill2Active(true);

        yield return StartCoroutine(SkillPreparation());

        SetTriggerAnimation(skill2Animation);
        yield return new WaitForSeconds(skill2Delay);

        SetEffectActive(true);

        if (skill2Prefab != null)
        {
            Instantiate(skill2Prefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("❌ Chưa gán Skill 2 Prefab trong Inspector!");
        }

        SetWarningSkill2Active(false);

        yield return new WaitForSeconds(0.3f);

        SetEffectActive(false);
        AddStamina(skill2Stamina);
        yield return new WaitForSeconds(skill2Cooldown);
        isUsingSkill = false;
    }


    // =========================================================
    // SKILL 3 - TRIỆU HỒI
    // =========================================================

    private IEnumerator Skill3Summon()
    {
        isUsingSkill = true;

        yield return StartCoroutine(SkillPreparation());

        SetTriggerAnimation(skill3Animation);
        yield return new WaitForSeconds(summonDelay);

        SetEffectActive(true);

        if (minionPrefab != null)
        {
            Vector3 spawnPosition = (summonPoint != null) ? summonPoint.position : transform.position;

            for (int i = 0; i < summonCount; i++)
            {
                Vector3 position = spawnPosition;
                position.x += Random.Range(-1.5f, 1.5f);
                position.y += Random.Range(-0.5f, 0.5f);

                Instantiate(minionPrefab, position, Quaternion.identity);
                yield return new WaitForSeconds(0.1f);
            }
        }

        SetEffectActive(false);
        AddStamina(summonStamina);
        yield return new WaitForSeconds(summonCooldown);
        isUsingSkill = false;
    }


    // =========================================================
    // HELPER ẨN/HIỆN BÁO HIỆU SKILL
    // =========================================================

    private void SetWarningSkill1Active(bool active)
    {
        if (skill1LineWarning != null)
        {
            skill1LineWarning.SetActive(active);
        }
    }

    private void SetWarningSkill2Active(bool active)
    {
        if (skill2AreaWarning != null)
        {
            skill2AreaWarning.SetActive(active);
        }
    }


    // =========================================================
    // TÌM PLAYER / PHÁT HIỆN
    // =========================================================

    private void FindPlayerWithOverlapCircle()
    {
        playerDetected = false;
        playerTransform = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange, playerLayer);

        if (hits != null && hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                Transform foundPlayer = GetPlayerTransform(hit);

                if (foundPlayer != null)
                {
                    playerTransform = foundPlayer;
                    playerDetected = true;
                    return;
                }
            }
        }
    }

    private Transform GetPlayerTransform(Collider2D collider)
    {
        Transform current = collider.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return current;
            }
            current = current.parent;
        }

        return collider.transform;
    }


    // =========================================================
    // CẬP NHẬT RAGE POSITION & EFFECT
    // =========================================================

    private void UpdateAttackRagePosition()
    {
        if (attackRage == null) return;

        float facingDirection = Mathf.Sign(transform.localScale.x);

        Vector3 targetPosition = new Vector3(
            transform.position.x + (attackRageOffsetX * facingDirection),
            transform.position.y + attackRageOffsetY,
            attackRage.position.z
        );

        attackRage.position = targetPosition;
    }

    private void SetEffectActive(bool active)
    {
        if (attackEffect != null)
        {
            attackEffect.SetActive(active);
        }
    }


    // =========================================================
    // QUẢN LÝ COLLIDER GÂY SÁT THƯƠNG
    // =========================================================

    private void EnableAttackCollider(Transform attackPoint, int damage)
    {
        if (attackPoint == null) return;

        Collider2D col = attackPoint.GetComponent<Collider2D>();
        if (col == null) return;

        BossAttackDamage damageScript = attackPoint.GetComponent<BossAttackDamage>();
        if (damageScript == null)
        {
            damageScript = attackPoint.gameObject.AddComponent<BossAttackDamage>();
        }

        damageScript.SetDamage(damage);
        col.enabled = true;
    }

    private void DisableCollider(Transform attackPoint)
    {
        if (attackPoint == null) return;

        Collider2D col = attackPoint.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void DisableAllAttackColliders()
    {
        DisableCollider(attackPoint1);
        DisableCollider(attackPoint2);
        DisableCollider(chargePoint);
    }


    // =========================================================
    // THỂ LỰC & CẬP NHẬT UI
    // =========================================================

    private void AddStamina(int amount)
    {
        if (isTired) return;

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();

        if (currentStamina >= maxStamina)
        {
            StartTired();
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    private void StartTired()
    {
        if (isTired) return;

        isTired = true;
        StopMoving();
        DisableAllAttackColliders();
        SetEffectActive(false);

        SetWarningSkill1Active(false);
        SetWarningSkill2Active(false);

        SetBoolAnimation(tiredAnimation, true);
        StartCoroutine(TiredCoroutine());
    }

    private IEnumerator TiredCoroutine()
    {
        yield return new WaitForSeconds(tiredTime);

        currentStamina = 0;
        UpdateStaminaUI();
        isTired = false;

        SetBoolAnimation(tiredAnimation, false);
    }


    // =========================================================
    // HOTKEYS TEST SKILL
    // =========================================================

    private void HandleHotkeyTesting()
    {
        if (isAttacking || isUsingSkill || isTired) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("🧪 TEST: Bấm 1 -> Đánh Thường");
            StartCoroutine(NormalAttack());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("🧪 TEST: Bấm 2 -> Skill 1 (Trâu Húc)");
            StartCoroutine(Skill1Charge());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("🧪 TEST: Bấm 3 -> Skill 2 (Bùng Năng LƯợng - Spawn Prefab tại Boss)");
            StartCoroutine(Skill2Burst());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("🧪 TEST: Bấm 4 -> Skill 3 (Triệu Hồi)");
            StartCoroutine(Skill3Summon());
        }
    }


    // =========================================================
    // ANIMATOR HELPER
    // =========================================================

    private void SetTriggerAnimation(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return;
        animator.SetTrigger(paramName);
    }

    private void SetBoolAnimation(string paramName, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return;
        animator.SetBool(paramName, value);
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        if (attackRage != null)
            Gizmos.DrawWireSphere(attackRage.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, chargeCheckSize);
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            if (!other.CompareTag("Player")) return;

            Component stats = other.GetComponentInParent<CharacterStats>();
            if (stats != null)
            {
                stats.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                hasHit = true;
            }
        }
    }
}