using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStats))]
    public abstract class MonsterAIBase : MonoBehaviour
    {
        [Header("=== CẤU HÌNH DI CHUYỂN CƠ BẢN ===")]
        [SerializeField] protected float moveSpeed = 3.5f;
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected float attackRange = 2f;

        [Header("=== CẤU HÌNH BỎ CHẠY KHI PLAYER TIỂU ===")]
        [SerializeField] protected float fleeSpeedMultiplier = 1.5f;

        protected Transform playerTransform;
        protected CharacterStats playerStats;
        protected CharacterStats myStats;
        protected Rigidbody2D rb;
        protected Animator anim;

        protected bool isFleeing = false;
        protected bool isPerformingSkill = false;
        protected float skillCooldownTimer = 0f;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            myStats = GetComponent<CharacterStats>();
            // Lấy Animator luôn từ GameObject con (Model/Sprite)
            anim = GetComponentInChildren<Animator>();
        }

        protected virtual void Start()
        {
            FindPlayer();

            // Đăng ký sự kiện lắng nghe Player xả tiểu
            if (GameCore.Player.PlayerUrinationMechanism.Instance != null)
            {
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionStart += OnPlayerStartUrination;
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionEnd += OnPlayerEndUrination;
            }
        }

        protected virtual void OnDestroy()
        {
            if (GameCore.Player.PlayerUrinationMechanism.Instance != null)
            {
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionStart -= OnPlayerStartUrination;
                GameCore.Player.PlayerUrinationMechanism.Instance.OnExecutionEnd -= OnPlayerEndUrination;
            }
        }

        protected virtual void Update()
        {
            if (myStats != null && myStats.IsDead)
            {
                StopMoving();
                return;
            }

            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            if (skillCooldownTimer > 0)
            {
                skillCooldownTimer -= Time.deltaTime;
            }

            // Xử lý khi Player xả tiểu (Quái hoảng sợ bỏ chạy)
            if (isFleeing)
            {
                ExecuteFleeing();
                return;
            }

            if (isPerformingSkill) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRadius)
            {
                FlipTowards(playerTransform.position);

                if (CanUseSkill() && distanceToPlayer <= attackRange)
                {
                    SelectAndExecuteSkill();
                }
                else
                {
                    MoveTowards(playerTransform.position, moveSpeed);
                }
            }
            else
            {
                StopMoving();
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

        protected void MoveTowards(Vector2 targetPosition, float speed)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * speed;

            if (anim != null)
            {
                anim.SetBool("isMoving", true);
            }
        }

        protected void StopMoving()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (anim != null)
            {
                anim.SetBool("isMoving", false);
            }
        }

        protected void FlipTowards(Vector3 targetPos)
        {
            if (targetPos.x > transform.position.x)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (targetPos.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        protected virtual void ExecuteFleeing()
        {
            if (playerTransform == null) return;

            // Bỏ chạy ra xa Player
            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            rb.linearVelocity = fleeDirection * (moveSpeed * fleeSpeedMultiplier);
            FlipTowards(transform.position + (Vector3)fleeDirection);

            if (anim != null)
            {
                anim.SetBool("isMoving", true);
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

        protected abstract bool CanUseSkill();
        protected abstract void SelectAndExecuteSkill();

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}