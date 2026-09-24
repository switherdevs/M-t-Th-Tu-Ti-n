using StatsSystem.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct MinionSpawnData
{
    [Tooltip("Tên nhận diện quái (VD: Quái Xương, Sói Ma...)")]
    public string minionName;

    [Tooltip("Prefab của loại quái này")]
    public GameObject minionPrefab;

    [Tooltip("Số lượng muốn spawn cho loại quái này trong 1 đợt chọn")]
    public int spawnCount;

    [Tooltip("Tỉ lệ xuất hiện của quái (Ví dụ: 70 = 70%, 30 = 30%)")]
    [Range(0f, 100f)]
    public float spawnChance;
}

public class BossDaSatMaQuan : MonoBehaviour
{
    // =========================================================
    // PLAYER + PHÁT HIỆN
    // =========================================================

    [Header("===== PHÁT HIỆN PLAYER =====")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectRange = 12f;
    [SerializeField] private float attackRange = 2f;


    // =========================================================
    // NÉ TƯỜNG MƯỢT MÀ
    // =========================================================

    [Header("===== NÉ TƯỜNG MƯỢT MÀ =====")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallDetectDistance = 1.8f;
    [SerializeField] private float wallBufferDistance = 0.8f;
    [SerializeField] private float avoidanceSmoothing = 6f;


    // =========================================================
    // HIỆU ỨNG & ĐƯỜNG BÁO CHIÊU
    // =========================================================

    [Header("===== HIỆU ỨNG GAMEOBJECT & CẢNH BÁO CHIÊU =====")]
    [SerializeField] private GameObject attackEffect;
    [SerializeField] private GameObject skill1Effect;
    [SerializeField] private GameObject skill1LineWarning;
    [SerializeField] private GameObject skill2AreaWarning;


    // =========================================================
    // VỊ TRÍ ATTACK RAGE
    // =========================================================

    [Header("===== ATTACK RAGE POSITION =====")]
    [SerializeField] private Transform attackRage;
    [SerializeField] private float attackRageOffsetX = 0f;
    [SerializeField] private float attackRageOffsetY = 0f;


    // =========================================================
    // DI CHUYỂN & ANIMATION DI CHUYỂN
    // =========================================================

    [Header("===== DI CHUYỂN =====")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float slowMoveSpeed = 0.8f;
    [SerializeField] private float skillPrepTime = 0.6f;

    [Tooltip("Tên Parameter (BOOL) Animation Walk/Run ở Movement Layer")]
    [SerializeField] private string walkAnimation = "Walk";


    // =========================================================
    // THỂ LỰC & ANIMATION MỆT
    // =========================================================

    [Header("===== THỂ LỰC =====")]
    [SerializeField] private int maxStamina = 10;
    [SerializeField] private int currentStamina = 0;
    [SerializeField] private float tiredTime = 5f;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private string tiredAnimation = "Tired";


    // =========================================================
    // HỆ THỐNG ÂM THANH (AUDIO SOUND SYSTEM)
    // =========================================================

    [Header("===== HỆ THỐNG ÂM THANH (AUDIO SOURCE) =====")]
    [SerializeField] private AudioSource audioSource;

    [Header("--- 1. Âm Thanh Chuẩn Bị Tấn Công (Dùng Chung / Mặc Định) ---")]
    [SerializeField] private AudioClip sfxSkillPrepare;
    [SerializeField, Range(0f, 1f)] private float volSkillPrepare = 1f;

    [Header("--- 2. Đánh Thường (Normal Attack) ---")]
    [SerializeField] private AudioClip sfxNormalAttackHit;
    [SerializeField, Range(0f, 1f)] private float volNormalAttack = 1f;

    [Header("--- 3. Skill 1 - Trâu Húc ---")]
    [SerializeField] private AudioClip sfxSkill1Prepare;
    [SerializeField, Range(0f, 1f)] private float volSkill1Prepare = 1f;
    [SerializeField] private AudioClip sfxSkill1Charge;
    [SerializeField, Range(0f, 1f)] private float volSkill1Charge = 1f;

    [Header("--- 4. Skill 2 - Bùng Năng Lượng ---")]
    [SerializeField] private AudioClip sfxSkill2Prepare;
    [SerializeField, Range(0f, 1f)] private float volSkill2Prepare = 1f;
    [SerializeField] private AudioClip sfxSkill2Burst;
    [SerializeField, Range(0f, 1f)] private float volSkill2Burst = 1f;

    [Header("--- 5. Skill 3 - Triệu Hồi ---")]
    [SerializeField] private AudioClip sfxSkill3Prepare;
    [SerializeField, Range(0f, 1f)] private float volSkill3Prepare = 1f;
    [SerializeField] private AudioClip sfxSkill3Summon;
    [SerializeField, Range(0f, 1f)] private float volSkill3Summon = 1f;

    [Header("--- 6. Skill Húc 3 Lần (Triple Charge) ---")]
    [SerializeField] private AudioClip sfxTripleChargePrepare;
    [SerializeField, Range(0f, 1f)] private float volTripleChargePrepare = 1f;
    [SerializeField] private AudioClip sfxTripleChargeHit;
    [SerializeField, Range(0f, 1f)] private float volTripleChargeHit = 1f;

    [Header("--- 7. Skill Bẫy Ma Khí (Dark Trap) ---")]
    [SerializeField] private AudioClip sfxDarkTrapPrepare;
    [SerializeField, Range(0f, 1f)] private float volDarkTrapPrepare = 1f;
    [SerializeField] private AudioClip sfxDarkTrapSpawn;
    [SerializeField, Range(0f, 1f)] private float volDarkTrapSpawn = 1f;

    [Header("--- 8. Skill Mưa Thiên Thạch (Meteor Shower) ---")]
    [SerializeField] private AudioClip sfxMeteorPrepare;
    [SerializeField, Range(0f, 1f)] private float volMeteorPrepare = 1f;
    [SerializeField] private AudioClip sfxMeteorImpact;
    [SerializeField, Range(0f, 1f)] private float volMeteorImpact = 0.7f;


    // =========================================================
    // ĐÁNH THƯỜNG - ATTACK 1 & 2
    // =========================================================

    [Header("===== ĐÁNH THƯỜNG =====")]
    [Tooltip("Tick để cho phép Boss dùng Đánh thường")]
    [SerializeField] private bool useNormalAttack = true;
    [SerializeField] private int normalAttackStamina = 1;
    [SerializeField] private float normalAttackDelay = 0.2f;
    [SerializeField] private float attackColliderTime = 0.2f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float normalAttackStandTime = 0.3f;
    [SerializeField] private string attack1Animation = "Attack1";
    [SerializeField] private string attack2Animation = "Attack2";


    // =========================================================
    // SKILL 1 - TRÂU HÚC 1 LẦN
    // =========================================================

    [Header("===== SKILL 1 - TRÂU HÚC =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill 1")]
    [SerializeField] private bool useSkill1 = true;
    [SerializeField] private Transform chargePoint;
    [SerializeField] private int chargeStamina = 3;
    [SerializeField] private float chargeSpeed = 8f;
    [Tooltip("Khoảng cách lướt tối đa của Skill 1")]
    [SerializeField] private float chargeDistance = 6f;
    [SerializeField] private float chargeDelay = 0.3f;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float chargeStandTime = 0.5f;
    [SerializeField] private Vector2 chargeCheckSize = new Vector2(1f, 1f);
    [SerializeField] private string skill1Animation = "Skill1";


    // =========================================================
    // SKILL 2 - BÙNG NĂNG LƯỢNG (ÁP SÁT RỒI NỔ)
    // =========================================================

    [Header("===== SKILL 2 - BÙNG NĂNG LƯỢNG =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill 2")]
    [SerializeField] private bool useSkill2 = true;
    [SerializeField] private GameObject skill2Prefab;
    [SerializeField] private int skill2Stamina = 5;
    [SerializeField] private float skill2Delay = 0.3f;
    [SerializeField] private float skill2Cooldown = 8f;
    [SerializeField] private float skill2StandTime = 0.6f;
    [SerializeField] private string skill2Animation = "Skill2";


    // =========================================================
    // SKILL 3 - TRIỆU HỒI
    // =========================================================

    [Header("===== SKILL 3 - TRIỆU HỒI =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill 3")]
    [SerializeField] private bool useSkill3 = true;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private MinionSpawnData[] minionTypes;
    [SerializeField] private int summonStamina = 2;
    [SerializeField] private float summonDelay = 0.5f;
    [SerializeField] private float summonCooldown = 12f;
    [SerializeField] private float summonStandTime = 0.5f;
    [SerializeField] private string skill3Animation = "Skill3";


    // =========================================================
    // SKILL MỚI 1 - MA SÁT TUYỆT DIỆU (HÚC 3 LẦN)
    // =========================================================

    [Header("===== SKILL MỚI 1 - MA SÁT TUYỆT DIỆU (HÚC 3 LẦN) =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill Húc 3 Lần")]
    [SerializeField] private bool useSkillTripleCharge = true;
    [SerializeField] private int tripleChargeStamina = 2;
    [SerializeField] private float tripleChargeSpeed = 6f;
    [Tooltip("Khoảng cách lướt tối đa của mỗi lần húc trong Triple Charge")]
    [SerializeField] private float tripleChargeDistance = 5f;
    [Tooltip("Thời gian nghỉ/dừng lại giữa mỗi lần húc")]
    [SerializeField] private float tripleChargePauseDelay = 0.3f;
    [SerializeField] private float tripleChargeCooldown = 10f;
    [SerializeField] private float tripleChargeStandTime = 0.8f;

    [Tooltip("Tên State Animation chính xác trong Animator (Ví dụ: TripleCharge)")]
    [SerializeField] private string tripleChargeAnimStateName = "TripleCharge";


    // =========================================================
    // SKILL MỚI 2 - MA KHÍ TRẦM TÍCH (DARK TRAP)
    // =========================================================

    [Header("===== SKILL MỚI 2 - MA KHÍ TRẦM TÍCH (DARK TRAP) =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill Dark Trap")]
    [SerializeField] private bool useSkillDarkTrap = true;
    [SerializeField] private GameObject darkTrapPrefab;
    [SerializeField] private int darkTrapStamina = 2;
    [SerializeField] private float darkTrapDelay = 1.2f;
    [SerializeField] private float darkTrapDuration = 1.0f;
    [SerializeField] private float darkTrapCooldown = 7f;
    [SerializeField] private float darkTrapStandTime = 0.5f;

    [Tooltip("Tên Parameter (BOOL) Animation cho Skill Dark Trap")]
    [SerializeField] private string darkTrapAnimName = "DarkTrap";


    // =========================================================
    // SKILL MỚI 3 - MƯA THIÊN THẠCH (METEOR SHOWER)
    // =========================================================

    [Header("===== SKILL MỚI 3 - MƯA THIÊN THẠCH (METEOR SHOWER) =====")]
    [Tooltip("Tick để cho phép Boss dùng Skill Mưa Thiên Thạch")]
    [SerializeField] private bool useSkillMeteorShower = true;
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private int meteorCount = 12;
    [SerializeField] private int meteorStamina = 6;
    [SerializeField] private float meteorPrepDelay = 1.5f;
    [SerializeField] private float meteorCooldown = 15f;
    [SerializeField] private float meteorStandTime = 1.0f;

    [Tooltip("Tên Parameter (BOOL) Animation cho Skill Mưa Thiên Thạch")]
    [SerializeField] private string meteorAnimName = "MeteorShower";

    [Header("===== VÙNG SPAWN MƯA THIÊN THẠCH =====")]
    [Tooltip("Vị trí tương đối của vùng spawn so với Boss")]
    [SerializeField] private Vector2 meteorSpawnAreaOffset = new Vector2(0f, 6f);

    [Tooltip("Kích thước hình vuông vùng spawn trên đầu Boss")]
    [SerializeField] private Vector2 meteorSpawnAreaSize = new Vector2(10f, 3f);


    // =========================================================
    // ANIMATOR COMPONENTS
    // =========================================================

    [Header("===== ANIMATOR COMPONENT =====")]
    [SerializeField] private Animator animator;


    // =========================================================
    // DEBUG & TESTING
    // =========================================================

    [Header("===== DEBUG & TEST SKILL =====")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool enableHotkeyTesting = true;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private Transform playerTransform;
    private Rigidbody2D rb;
    private BossPhaseSystem phaseSystem;
    private bool playerDetected = false;
    private bool isAttacking = false;
    private bool isUsingSkill = false;
    private bool isTired = false;

    public bool IsTired => isTired;

    private float attackTimer = 0f;
    private int comboIndex = 0;
    private Vector2 currentVelocityVector = Vector2.zero;

    // Biến đếm thời gian Cooldown độc lập của từng skill
    private float normalAttackTimer = 0f;
    private float skill1Timer = 0f;
    private float skill2Timer = 0f;
    private float skill3Timer = 0f;
    private float tripleChargeTimer = 0f;
    private float darkTrapTimer = 0f;
    private float meteorTimer = 0f;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        phaseSystem = GetComponent<BossPhaseSystem>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (attackEffect != null) attackEffect.SetActive(false);
        if (skill1Effect != null) skill1Effect.SetActive(false);

        SetWarningSkill1Active(false);
        SetWarningSkill2Active(false);
        UpdateStaminaUI();
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    private void Update()
    {
        if (phaseSystem != null && phaseSystem.IsPhaseChanging)
        {
            StopMoving();
            return;
        }

        UpdateCooldownTimers();
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

    private void UpdateCooldownTimers()
    {
        if (normalAttackTimer > 0f) normalAttackTimer -= Time.deltaTime;
        if (skill1Timer > 0f) skill1Timer -= Time.deltaTime;
        if (skill2Timer > 0f) skill2Timer -= Time.deltaTime;
        if (skill3Timer > 0f) skill3Timer -= Time.deltaTime;
        if (tripleChargeTimer > 0f) tripleChargeTimer -= Time.deltaTime;
        if (darkTrapTimer > 0f) darkTrapTimer -= Time.deltaTime;
        if (meteorTimer > 0f) meteorTimer -= Time.deltaTime;
    }


    // =========================================================
    // HÀM HỖ TRỢ PHÁT ÂM THANH
    // =========================================================

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }


    // =========================================================
    // DI CHUYỂN & NÉ TƯỜNG
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

    public void StopMoving()
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
    // COMBO SEQUENCE
    // =========================================================

    private void ExecuteComboSequence()
    {
        if (!useNormalAttack && !useSkill1 && !useSkill2 && !useSkill3 &&
            !useSkillTripleCharge && !useSkillDarkTrap && !useSkillMeteorShower)
        {
            return;
        }

        int attempts = 0;
        bool skillExecuted = false;

        while (!skillExecuted && attempts < 7)
        {
            switch (comboIndex)
            {
                case 0:
                    if (useNormalAttack && normalAttackTimer <= 0f) { StartCoroutine(NormalAttack()); skillExecuted = true; }
                    break;
                case 1:
                    if (useSkill1 && skill1Timer <= 0f) { StartCoroutine(Skill1Charge()); skillExecuted = true; }
                    break;
                case 2:
                    if (useSkill2 && skill2Timer <= 0f) { StartCoroutine(Skill2Burst()); skillExecuted = true; }
                    break;
                case 3:
                    if (useSkill3 && skill3Timer <= 0f) { StartCoroutine(Skill3Summon()); skillExecuted = true; }
                    break;
                case 4:
                    if (useSkillTripleCharge && tripleChargeTimer <= 0f) { StartCoroutine(SkillMaSatTuyetDiu()); skillExecuted = true; }
                    break;
                case 5:
                    if (useSkillDarkTrap && darkTrapTimer <= 0f) { StartCoroutine(SkillMaKhiTramTich()); skillExecuted = true; }
                    break;
                case 6:
                    if (useSkillMeteorShower && meteorTimer <= 0f) { StartCoroutine(SkillMuaThienThach()); skillExecuted = true; }
                    break;
            }

            comboIndex = (comboIndex + 1) % 7;
            attempts++;
        }

        if (skillExecuted)
        {
            attackTimer = 0.5f;
        }
    }

    private IEnumerator SkillPreparation(AudioClip specificPrepareClip = null, float prepareVol = 1f)
    {
        AudioClip clipToPlay = specificPrepareClip != null ? specificPrepareClip : sfxSkillPrepare;
        float volToPlay = specificPrepareClip != null ? prepareVol : volSkillPrepare;
        PlaySFX(clipToPlay, volToPlay);

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

    private IEnumerator ChasePlayerUntilClose(float targetRange)
    {
        while (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= targetRange)
            {
                break;
            }

            ChasePlayerSmoothly();
            yield return null;
        }

        StopMoving();
    }


    // =========================================================
    // THỰC THI SKILL & STAND TIME
    // =========================================================

    private IEnumerator NormalAttack()
    {
        isAttacking = true;
        StopMoving();

        bool isAttack1 = Random.Range(0, 2) == 0;
        SetTriggerAnimation(isAttack1 ? attack1Animation : attack2Animation);

        yield return new WaitForSeconds(normalAttackDelay);

        PlaySFX(sfxNormalAttackHit, volNormalAttack);

        if (attackEffect != null) attackEffect.SetActive(true);
        AddStamina(normalAttackStamina);

        yield return new WaitForSeconds(attackColliderTime);

        if (attackEffect != null) attackEffect.SetActive(false);

        normalAttackTimer = attackCooldown;
        yield return new WaitForSeconds(normalAttackStandTime);

        isAttacking = false;
    }

    private IEnumerator Skill1Charge()
    {
        isUsingSkill = true;
        SetWarningSkill1Active(true);

        yield return StartCoroutine(SkillPreparation(sfxSkill1Prepare, volSkill1Prepare));

        PlayDirectAnimationState(skill1Animation);

        yield return new WaitForSeconds(chargeDelay);

        PlaySFX(sfxSkill1Charge, volSkill1Charge);

        if (playerTransform != null)
        {
            Vector2 chargeDir = (playerTransform.position - transform.position).normalized;
            FacePlayer(chargeDir.x);

            if (skill1Effect != null) skill1Effect.SetActive(true);

            Vector2 startPos = transform.position;
            float traveledDistance = 0f;
            float maxChargeDuration = chargeDistance / chargeSpeed;
            float timer = 0f;

            while (traveledDistance < chargeDistance && timer < maxChargeDuration)
            {
                if (rb != null)
                {
                    rb.linearVelocity = chargeDir * chargeSpeed;
                }

                traveledDistance = Vector2.Distance(startPos, transform.position);
                timer += Time.deltaTime;

                Collider2D wallHit = Physics2D.OverlapBox(transform.position, chargeCheckSize, 0f, wallLayer);
                Collider2D playerHit = Physics2D.OverlapBox(transform.position, chargeCheckSize, 0f, playerLayer);

                if (wallHit != null && wallHit.CompareTag("Wall"))
                {
                    StopMoving();
                    if (skill1Effect != null) skill1Effect.SetActive(false);
                    SetWarningSkill1Active(false);
                    skill1Timer = chargeCooldown;
                    isUsingSkill = false;
                    StartTired();
                    yield break;
                }

                if (playerHit != null)
                {
                    break;
                }

                yield return null;
            }
        }

        SetWarningSkill1Active(false);
        StopMoving();

        if (skill1Effect != null) skill1Effect.SetActive(false);

        AddStamina(chargeStamina);

        skill1Timer = chargeCooldown;
        yield return new WaitForSeconds(chargeStandTime);

        isUsingSkill = false;
    }

    private IEnumerator Skill2Burst()
    {
        isUsingSkill = true;
        SetWarningSkill2Active(true);

        PlaySFX(sfxSkill2Prepare, volSkill2Prepare);

        yield return StartCoroutine(ChasePlayerUntilClose(attackRange));

        SetTriggerAnimation(skill2Animation);
        yield return new WaitForSeconds(skill2Delay);

        PlaySFX(sfxSkill2Burst, volSkill2Burst);

        if (skill2Prefab != null)
        {
            Instantiate(skill2Prefab, transform.position, Quaternion.identity);
        }

        SetWarningSkill2Active(false);
        AddStamina(skill2Stamina);

        skill2Timer = skill2Cooldown;
        yield return new WaitForSeconds(skill2StandTime);

        isUsingSkill = false;
    }

    private IEnumerator Skill3Summon()
    {
        isUsingSkill = true;

        yield return StartCoroutine(SkillPreparation(sfxSkill3Prepare, volSkill3Prepare));

        SetTriggerAnimation(skill3Animation);
        yield return new WaitForSeconds(summonDelay);

        PlaySFX(sfxSkill3Summon, volSkill3Summon);

        if (spawnPoints != null && spawnPoints.Length > 0 && minionTypes != null && minionTypes.Length > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                MinionSpawnData selectedMinion = GetRandomMinionByChance();

                if (selectedMinion.minionPrefab != null)
                {
                    for (int i = 0; i < selectedMinion.spawnCount; i++)
                    {
                        Vector3 position = spawnPoint.position;
                        position.x += Random.Range(-0.8f, 0.8f);
                        position.y += Random.Range(-0.4f, 0.4f);

                        Instantiate(selectedMinion.minionPrefab, position, Quaternion.identity);
                        yield return new WaitForSeconds(0.08f);
                    }
                }
            }
        }

        AddStamina(summonStamina);

        skill3Timer = summonCooldown;
        yield return new WaitForSeconds(summonStandTime);

        isUsingSkill = false;
    }

    private MinionSpawnData GetRandomMinionByChance()
    {
        float totalChance = 0f;
        foreach (MinionSpawnData minion in minionTypes) totalChance += minion.spawnChance;

        float randomRoll = Random.Range(0f, totalChance);
        float currentSum = 0f;

        foreach (MinionSpawnData minion in minionTypes)
        {
            currentSum += minion.spawnChance;
            if (randomRoll <= currentSum) return minion;
        }

        return minionTypes[0];
    }

    private IEnumerator SkillMaSatTuyetDiu()
    {
        isUsingSkill = true;
        yield return StartCoroutine(SkillPreparation(sfxTripleChargePrepare, volTripleChargePrepare));

        for (int i = 0; i < 3; i++)
        {
            FindPlayerWithOverlapCircle();

            if (playerTransform != null)
            {
                StopMoving();

                Vector2 chargeDir = (playerTransform.position - transform.position).normalized;
                FacePlayer(chargeDir.x);

                SetWarningSkill1Active(true);
                yield return new WaitForSeconds(0.15f);
                SetWarningSkill1Active(false);

                PlayDirectAnimationState(tripleChargeAnimStateName);

                PlaySFX(sfxTripleChargeHit, volTripleChargeHit);

                if (skill1Effect != null) skill1Effect.SetActive(true);

                Vector2 startPos = transform.position;
                float traveledDistance = 0f;
                float maxDuration = tripleChargeDistance / tripleChargeSpeed;
                float timer = 0f;

                while (traveledDistance < tripleChargeDistance && timer < maxDuration)
                {
                    if (rb != null)
                    {
                        rb.linearVelocity = chargeDir * tripleChargeSpeed;
                    }

                    traveledDistance = Vector2.Distance(startPos, transform.position);
                    timer += Time.deltaTime;

                    Collider2D wallHit = Physics2D.OverlapBox(transform.position, chargeCheckSize, 0f, wallLayer);
                    Collider2D playerHit = Physics2D.OverlapBox(transform.position, chargeCheckSize, 0f, playerLayer);

                    if (wallHit != null && wallHit.CompareTag("Wall"))
                    {
                        StopMoving();
                        if (skill1Effect != null) skill1Effect.SetActive(false);

                        tripleChargeTimer = tripleChargeCooldown;
                        isUsingSkill = false;

                        // Reset Animation Walk khi đâm vào tường
                        SetBoolAnimation(walkAnimation, false);

                        StartTired();
                        yield break;
                    }

                    if (playerHit != null)
                    {
                        break;
                    }

                    yield return null;
                }

                StopMoving();
                if (skill1Effect != null) skill1Effect.SetActive(false);

                yield return new WaitForSeconds(tripleChargePauseDelay);
            }
        }

        // TẮT ANIMATION HÚC & RESET TRẠNG THÁI VỀ MẶC ĐỊNH
        SetBoolAnimation(walkAnimation, false);

        AddStamina(tripleChargeStamina);

        tripleChargeTimer = tripleChargeCooldown;
        yield return new WaitForSeconds(tripleChargeStandTime);

        isUsingSkill = false;
    }

    private IEnumerator SkillMaKhiTramTich()
    {
        isUsingSkill = true;
        yield return StartCoroutine(SkillPreparation(sfxDarkTrapPrepare, volDarkTrapPrepare));

        SetBoolAnimation(darkTrapAnimName, true);

        yield return new WaitForSeconds(darkTrapDelay);

        Vector3 targetPosition = GetCurrentPlayerPosition();

        PlaySFX(sfxDarkTrapSpawn, volDarkTrapSpawn);

        if (darkTrapPrefab != null)
        {
            GameObject trapObj = Instantiate(darkTrapPrefab, targetPosition, Quaternion.identity);
            trapObj.transform.localScale = darkTrapPrefab.transform.localScale;
        }

        yield return new WaitForSeconds(darkTrapDuration);

        SetBoolAnimation(darkTrapAnimName, false);

        AddStamina(darkTrapStamina);

        darkTrapTimer = darkTrapCooldown;
        yield return new WaitForSeconds(darkTrapStandTime);

        isUsingSkill = false;
    }

    private IEnumerator SkillMuaThienThach()
    {
        isUsingSkill = true;
        yield return StartCoroutine(SkillPreparation(sfxMeteorPrepare, volMeteorPrepare));

        SetBoolAnimation(meteorAnimName, true);

        yield return new WaitForSeconds(meteorPrepDelay);

        Vector3 spawnCenter = (Vector3)meteorSpawnAreaOffset + transform.position;

        for (int i = 0; i < meteorCount; i++)
        {
            float randomX = Random.Range(spawnCenter.x - meteorSpawnAreaSize.x / 2f, spawnCenter.x + meteorSpawnAreaSize.x / 2f);
            float randomY = Random.Range(spawnCenter.y - meteorSpawnAreaSize.y / 2f, spawnCenter.y + meteorSpawnAreaSize.y / 2f);

            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            PlaySFX(sfxMeteorImpact, volMeteorImpact);

            if (meteorPrefab != null)
            {
                Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.12f);
        }

        SetBoolAnimation(meteorAnimName, false);

        AddStamina(meteorStamina);

        meteorTimer = meteorCooldown;
        yield return new WaitForSeconds(meteorStandTime);

        isUsingSkill = false;
    }


    // =========================================================
    // HELPER SUPPORT & GIZMOS
    // =========================================================

    private Vector3 GetCurrentPlayerPosition()
    {
        if (playerTransform != null)
        {
            return playerTransform.root.position;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            return playerTransform.root.position;
        }

        return transform.position;
    }

    private void PlayDirectAnimationState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;

        int attackLayerIndex = animator.GetLayerIndex("Attack");
        if (attackLayerIndex != -1)
        {
            animator.Play(stateName, attackLayerIndex, 0f);
        }
        else
        {
            animator.Play(stateName, -1, 0f);
        }
    }

    private void SetWarningSkill1Active(bool active)
    {
        if (skill1LineWarning != null) skill1LineWarning.SetActive(active);
    }

    private void SetWarningSkill2Active(bool active)
    {
        if (skill2AreaWarning != null) skill2AreaWarning.SetActive(active);
    }

    private void FindPlayerWithOverlapCircle()
    {
        playerDetected = false;

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
        Transform root = collider.transform.root;
        if (root.CompareTag("Player")) return root;

        Transform current = collider.transform;
        while (current != null)
        {
            if (current.CompareTag("Player")) return current;
            current = current.parent;
        }
        return collider.transform;
    }

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

        if (attackEffect != null) attackEffect.SetActive(false);
        if (skill1Effect != null) skill1Effect.SetActive(false);
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

    private void HandleHotkeyTesting()
    {
        if (isAttacking || isUsingSkill || isTired) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && useNormalAttack) StartCoroutine(NormalAttack());
        if (Input.GetKeyDown(KeyCode.Alpha2) && useSkill1) StartCoroutine(Skill1Charge());
        if (Input.GetKeyDown(KeyCode.Alpha3) && useSkill2) StartCoroutine(Skill2Burst());
        if (Input.GetKeyDown(KeyCode.Alpha4) && useSkill3) StartCoroutine(Skill3Summon());
        if (Input.GetKeyDown(KeyCode.Alpha5) && useSkillTripleCharge) StartCoroutine(SkillMaSatTuyetDiu());
        if (Input.GetKeyDown(KeyCode.Alpha6) && useSkillDarkTrap) StartCoroutine(SkillMaKhiTramTich());
        if (Input.GetKeyDown(KeyCode.Alpha7) && useSkillMeteorShower) StartCoroutine(SkillMuaThienThach());
    }

    private void SetBoolAnimation(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    private void SetTriggerAnimation(string paramName)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetTrigger(paramName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wallDetectDistance);

        Gizmos.color = Color.magenta;
        Vector3 meteorCenter = (Vector3)meteorSpawnAreaOffset + transform.position;
        Gizmos.DrawWireCube(meteorCenter, meteorSpawnAreaSize);
    }
}