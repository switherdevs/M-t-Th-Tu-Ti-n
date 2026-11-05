using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(ExecutableEnemy))]
    public abstract class MonsterAIBase : MonoBehaviour
    {
        [Header("=== CẤU HÌNH DI CHUYỂN & TẦM NHÌN ===")]
        [SerializeField] protected float moveSpeed = 2.5f;
        [SerializeField] protected float slowMoveSpeed = 0.8f;
        [SerializeField] protected float detectionRadius = 12f;
        [SerializeField] protected float attackRange = 2.5f;
        [SerializeField] protected float rangedAttackRange = 8f;
        [SerializeField] protected float skillPrepTime = 0.6f;

        [Header("=== CẤU HÌNH NÉ TƯỜNG & TÌM ĐƯỜNG THÔNG MINH (SMART AVOIDANCE) ===")]
        [SerializeField] protected LayerMask wallLayer;
        [SerializeField] protected float wallDetectDistance = 2.5f;
        [SerializeField] protected float wallBufferDistance = 1.2f;
        [SerializeField] protected float avoidanceSmoothing = 12f;

        [Header("=== CẤU HÌNH ĐÁNH THƯỜNG & HITBOX ===")]
        [SerializeField] protected GameObject meleeAttackHitbox;
        [SerializeField] protected float hitboxActiveDuration = 0.3f;
        [SerializeField] protected float basicAttackCooldown = 1.5f;
        [SerializeField] protected float basicAttackDamage = 15f;
        [SerializeField] protected string attack1AnimName = "Attack1";
        [SerializeField] protected string attack2AnimName = "Attack2";

        [Header("=== CẤU HÌNH ÂM THANH & CHẾT ===")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip basicAttackSound;
        [SerializeField, Range(0f, 1f)] protected float basicAttackVolume = 1f;
        [SerializeField] protected AudioClip dieSound;
        [SerializeField, Range(0f, 1f)] protected float dieVolume = 1f;

        [Header("=== CẤU HÌNH ANIMATION CHUNG ===")]
        [SerializeField] protected bool hasWalkAnimation = true;

        [Header("=== CẤU HÌNH BỎ CHẠY KHI PLAYER TIỂU ===")]
        [SerializeField] protected float fleeSpeedMultiplier = 1.5f;

        [Header("=== CẤU HÌNH HIỆU ỨNG BUFF (SKILL 2) ===")]
        [SerializeField] protected GameObject buffEffectObject;
        [SerializeField] protected float buffEffectDuration = 10f;

        protected Transform playerTransform;
        protected CharacterStats playerStats;
        protected CharacterStats myStats;
        protected Rigidbody2D rb;
        protected Animator anim;
        protected ExecutableEnemy executableEnemy;

        protected bool isFleeing = false;
        protected bool isPerformingSkill = false;
        protected bool isPerformingBasicAttack = false;
        protected float basicAttackCooldownTimer = 0f;
        protected float attackTimer = 0f;
        protected Vector2 currentVelocityVector = Vector2.zero;

        private float stuckTimer = 0f;
        private Vector2 lastPosition;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            myStats = GetComponent<CharacterStats>();
            anim = GetComponentInChildren<Animator>();
            executableEnemy = GetComponent<ExecutableEnemy>();

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (meleeAttackHitbox != null)
            {
                meleeAttackHitbox.SetActive(false);
            }

            if (buffEffectObject != null)
            {
                buffEffectObject.SetActive(false);
            }
        }

        protected virtual void Start()
        {
            FindPlayer();
            lastPosition = transform.position;

            if (myStats != null)
            {
                myStats.OnDeath += PlayDieSound;
            }

            if (GameCore.Player.PlayerUrinationMechanism.Instance != null)
            {
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionStart += OnPlayerStartUrination;
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionEnd += OnPlayerEndUrination;
            }
        }

        protected virtual void OnDestroy()
        {
            if (myStats != null)
            {
                myStats.OnDeath -= PlayDieSound;
            }

            if (GameCore.Player.PlayerUrinationMechanism.Instance != null)
            {
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionStart -= OnPlayerStartUrination;
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionEnd -= OnPlayerEndUrination;
            }
        }

        protected void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null && volume > 0f)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        protected virtual void PlayDieSound()
        {
            PlaySound(dieSound, dieVolume);
        }

        protected virtual void Update()
        {
            if (myStats != null && myStats.IsDead)
            {
                StopMoving();
                if (meleeAttackHitbox != null) meleeAttackHitbox.SetActive(false);
                return;
            }

            if (executableEnemy != null && executableEnemy.IsStunned)
            {
                StopMoving();
                if (meleeAttackHitbox != null) meleeAttackHitbox.SetActive(false);
                return;
            }

            if (isPerformingSkill || isPerformingBasicAttack)
            {
                StopMoving();
                return;
            }

            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            if (attackTimer > 0f) attackTimer -= Time.deltaTime;
            if (basicAttackCooldownTimer > 0f) basicAttackCooldownTimer -= Time.deltaTime;

            if (isFleeing)
            {
                ExecuteFleeing();
                return;
            }

            UpdateCustomCooldowns();

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRadius)
            {
                FlipTowards(playerTransform.position);

                if (distanceToPlayer > attackRange && distanceToPlayer <= rangedAttackRange)
                {
                    if (TryExecuteRangedSkill())
                    {
                        return;
                    }
                }

                if (distanceToPlayer <= attackRange)
                {
                    StopMoving();

                    if (attackTimer <= 0f && TryExecuteMeleeSkillOrBuff())
                    {
                        attackTimer = 1.0f;
                        return;
                    }

                    if (basicAttackCooldownTimer <= 0f)
                    {
                        StartCoroutine(Routine_ExecuteBasicAttack());
                    }
                    return;
                }

                MoveTowardsPlayerSmoothly(playerTransform.position);
            }
            else
            {
                StopMoving();
            }
        }

        protected virtual bool TryExecuteRangedSkill() { return false; }
        protected virtual bool TryExecuteMeleeSkillOrBuff() { return false; }
        protected virtual void UpdateCustomCooldowns() { }

        protected IEnumerator SkillPreparation(AudioClip prepareClip = null, float prepareVol = 1f)
        {
            if (prepareClip != null) PlaySound(prepareClip, prepareVol);

            if (playerTransform != null)
            {
                FlipTowards(playerTransform.position);
                Vector2 slowDir = (playerTransform.position - transform.position).normalized;
                rb.linearVelocity = slowDir * slowMoveSpeed;
                SetWalkAnim(true);
            }

            yield return new WaitForSeconds(skillPrepTime);
            StopMoving();
        }

        protected IEnumerator ChasePlayerUntilClose(float targetRange)
        {
            while (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist <= targetRange) break;

                MoveTowardsPlayerSmoothly(playerTransform.position);
                yield return null;
            }
            StopMoving();
        }

        protected virtual IEnumerator Routine_ExecuteBasicAttack()
        {
            isPerformingBasicAttack = true;
            StopMoving();

            string selectedAttackAnim = (Random.Range(0, 2) == 0) ? attack1AnimName : attack2AnimName;
            if (anim != null && !string.IsNullOrEmpty(selectedAttackAnim))
            {
                anim.SetTrigger(selectedAttackAnim);
            }

            PlaySound(basicAttackSound, basicAttackVolume);

            yield return new WaitForSeconds(0.2f);

            if (meleeAttackHitbox != null && myStats != null && !myStats.IsDead)
            {
                meleeAttackHitbox.SetActive(true);
            }

            if (myStats != null && !myStats.IsDead && playerTransform != null)
            {
                if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange + 0.5f)
                {
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(basicAttackDamage + myStats.Attack.Value);
                    }
                }
            }

            yield return new WaitForSeconds(hitboxActiveDuration);

            if (meleeAttackHitbox != null)
            {
                meleeAttackHitbox.SetActive(false);
            }

            yield return new WaitForSeconds(0.2f);
            basicAttackCooldownTimer = basicAttackCooldown;
            isPerformingBasicAttack = false;
        }

        protected void MoveTowardsPlayerSmoothly(Vector2 targetPosition)
        {
            if (isPerformingSkill || isPerformingBasicAttack) return;

            if (Vector2.Distance(transform.position, lastPosition) < 0.02f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 0.4f)
                {
                    Vector2 dirToPlayer = ((Vector2)(playerTransform.position - transform.position)).normalized;
                    Vector2 perpDir = Vector2.Perpendicular(dirToPlayer);
                    Vector2 escapePush = (Random.value > 0.5f ? perpDir : -perpDir);
                    rb.linearVelocity = escapePush * moveSpeed * 1.2f;
                    stuckTimer = 0f;
                    return;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPosition = transform.position;

            Collider2D overlapCollider = Physics2D.OverlapCircle(transform.position, 0.45f, wallLayer);
            if (overlapCollider != null)
            {
                Vector2 closestPoint = overlapCollider.ClosestPoint(transform.position);
                Vector2 escapeDir = ((Vector2)transform.position - closestPoint).normalized;
                if (escapeDir == Vector2.zero) escapeDir = Vector2.up;

                rb.linearVelocity = escapeDir * moveSpeed * 1.5f;
                SetWalkAnim(true);
                return;
            }

            Vector2 targetDir = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 safeDir = CalculateSmartAvoidanceDirection(targetDir);

            currentVelocityVector = Vector2.Lerp(currentVelocityVector, safeDir, Time.deltaTime * avoidanceSmoothing);
            rb.linearVelocity = currentVelocityVector * moveSpeed;

            SetWalkAnim(true);

            if (currentVelocityVector.x != 0)
            {
                FlipTowards(transform.position + (Vector3)currentVelocityVector);
            }
        }

        private Vector2 CalculateSmartAvoidanceDirection(Vector2 targetDir)
        {
            float[] testAngles = new float[] { 0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f, 90f, -90f, 120f, -120f, 150f, -150f, 180f };
            Vector2 bestDir = targetDir;
            float maxScore = -999f;
            bool foundOpenPath = false;

            foreach (float angle in testAngles)
            {
                Vector2 checkDir = Quaternion.Euler(0, 0, angle) * targetDir;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDir, wallDetectDistance, wallLayer);

                if (hit.collider == null)
                {
                    float alignmentScore = Vector2.Dot(checkDir.normalized, targetDir.normalized) * 10f;

                    if (alignmentScore > maxScore)
                    {
                        maxScore = alignmentScore;
                        bestDir = checkDir;
                        foundOpenPath = true;
                    }
                }
                else
                {
                    float clearanceScore = hit.distance;
                    if (clearanceScore > maxScore && !foundOpenPath)
                    {
                        maxScore = clearanceScore;
                        bestDir = checkDir;
                    }
                }
            }

            return foundOpenPath ? bestDir.normalized : Vector2.Perpendicular(targetDir).normalized;
        }

        // 🎯 Dùng chung 1 biến boolean "isMoving" chuẩn cho mọi loại quái
        protected void SetWalkAnim(bool isWalking)
        {
            if (anim != null && hasWalkAnimation)
            {
                anim.SetBool("isMoving", isWalking);
            }
        }

        protected void StopMoving()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            currentVelocityVector = Vector2.zero;
            SetWalkAnim(false);
        }

        protected void FlipTowards(Vector3 targetPos)
        {
            if (targetPos.x > transform.position.x)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (targetPos.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        protected virtual IEnumerator ApplyBuffAndHealRoutine(float healAmount, float attackBonus)
        {
            if (myStats == null || myStats.IsDead) yield break;

            myStats.Heal(healAmount);
            myStats.Attack.Value += attackBonus;

            if (buffEffectObject != null)
            {
                buffEffectObject.SetActive(true);
            }

            yield return new WaitForSeconds(buffEffectDuration);

            if (buffEffectObject != null)
            {
                buffEffectObject.SetActive(false);
            }

            if (myStats != null)
            {
                myStats.Attack.Value -= attackBonus;
            }
        }

        protected void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerStats = playerObj.GetComponent<CharacterStats>();
            }
        }

        protected virtual void ExecuteFleeing()
        {
            if (playerTransform == null || isPerformingSkill || isPerformingBasicAttack) return;

            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            Vector2 smoothFleeDir = CalculateSmartAvoidanceDirection(fleeDirection);

            rb.linearVelocity = smoothFleeDir * (moveSpeed * fleeSpeedMultiplier);
            FlipTransformForFlee(smoothFleeDir);
            SetWalkAnim(true);
        }

        private void FlipTransformForFlee(Vector2 dir)
        {
            if (dir.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(dir.x), transform.localScale.y, transform.localScale.z);
            }
        }

        private void OnPlayerStartUrination()
        {
            isFleeing = true;
        }

        private void OnPlayerEndUrination()
        {
            isFleeing = false;
            StopMoving();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, wallDetectDistance);
        }
    }
}