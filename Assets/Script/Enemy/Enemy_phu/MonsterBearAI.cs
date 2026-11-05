using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterBearAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH BẬT/TẮT SKILL (INSPECTOR TOGGLE) ===")]
        [SerializeField] private bool useSkill1 = true;
        [SerializeField] private bool useSkill2 = true;
        [SerializeField] private bool useSkill3 = true;

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [SerializeField] private GameObject woodenSpikePrefab;

        [Header("=== CẤU HÌNH TÌM KIẾM PLAYER (TẦM XA) ===")]
        [SerializeField] private LayerMask playerLayerMask;

        [Header("=== CẤU HÌNH ANIMATION PARAMETER SKILL ===")]
        [SerializeField] private string skill1AnimName = "Skill1";
        [SerializeField] private AnimParamType skill1ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill2AnimName = "Skill2";
        [SerializeField] private AnimParamType skill2ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill3AnimName = "Skill3";
        [SerializeField] private AnimParamType skill3ParamType = AnimParamType.Trigger;

        [Header("=== CẤU HÌNH ANIMATION RIÊNG CHO SKILL 3 ===")]
        [Tooltip("Biến bool walk dùng riêng cho animation di chuyển khi đang xài skill 3")]
        [SerializeField] private string skill3WalkBoolName = "walk";
        [Tooltip("Hệ số nhân tốc độ animation walk dựa theo chargeSpeed (càng lớn tốc độ chạy animation càng khớp với tốc độ lao)")]
        [SerializeField] private float walkSpeedAnimMultiplier = 0.15f;

        [Header("=== CẤU HÌNH ÂM THANH SKILL & SLIDER ÂM LƯỢNG ===")]
        [SerializeField] private AudioClip skill1Sound;
        [SerializeField, Range(0f, 1f)] private float skill1Volume = 1f;
        [SerializeField] private AudioClip skill2Sound;
        [SerializeField, Range(0f, 1f)] private float skill2Volume = 1f;
        [SerializeField] private AudioClip skill3Sound;
        [SerializeField, Range(0f, 1f)] private float skill3Volume = 1f;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU RIÊNG TỪNG SKILL ===")]
        [SerializeField] private float skill1Cooldown = 4f;
        [SerializeField] private float skill2Cooldown = 10f;
        [SerializeField] private float skill3Cooldown = 5f;

        [Header("=== THÔNG SỐ SKILL 1: CỌC GỖ ĐỊA GAI (TỪ QUÁI RA HƯỚNG PLAYER) ===")]
        [SerializeField] private float castTimeSkill1 = 0.8f;
        [SerializeField] private int spikeCount = 5;
        [SerializeField] private float spawnOffsetX = 1.5f;
        [SerializeField] private float spikeSpacingX = 1.2f;
        [SerializeField] private float delayBetweenSpikes = 0.15f;
        [SerializeField] private float minRotationAngle = -15f;
        [SerializeField] private float maxRotationAngle = 15f;
        [SerializeField] private AudioClip spikeEmergeSound;
        [SerializeField, Range(0f, 1f)] private float spikeEmergeVolume = 1f;

        [Header("=== THÔNG SỐ SKILL 2: BÀNG HÃI TIẾNG GẦM (BUFF & HEAL - DƯỚI 50% MÁU) ===")]
        [SerializeField] private float healAmount = 60f;
        [SerializeField] private float buffAttackBonus = 25f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỊA LẠC CUỒNG CÀO (ỦI CÀN QUÉT) ===")]
        [SerializeField] private float chargeSpeed = 10f;
        [SerializeField] private float chargeDuration = 0.6f;
        [SerializeField] private float chargeDamage = 50f;
        [SerializeField] private float chargeHitRadius = 1.5f;

        private float skill1Timer = 0f;
        private float skill2Timer = 0f;
        private float skill3Timer = 0f;

        protected override void UpdateCustomCooldowns()
        {
            if (skill1Timer > 0f) skill1Timer -= Time.deltaTime;
            if (skill2Timer > 0f) skill2Timer -= Time.deltaTime;
            if (skill3Timer > 0f) skill3Timer -= Time.deltaTime;
        }

        // 🎯 TẦM XA: Cho phép quái lựa chọn ngẫu nhiên giữa Skill 1 (Cọc gỗ) và Skill 3 (Lao tới từ xa) trong vùng nhìn
        protected override bool TryExecuteRangedSkill()
        {
            bool canUseSkill1 = useSkill1 && skill1Timer <= 0f;
            bool canUseSkill3 = useSkill3 && skill3Timer <= 0f && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius;

            if (!canUseSkill1 && !canUseSkill3) return false;

            if (canUseSkill1 && canUseSkill3)
            {
                if (Random.value < 0.5f)
                {
                    StartCoroutine(Routine_Skill1_BoulderingSmash());
                }
                else
                {
                    StartCoroutine(Routine_Skill3_BerserkCharge());
                }
                return true;
            }

            if (canUseSkill3)
            {
                StartCoroutine(Routine_Skill3_BerserkCharge());
                return true;
            }

            if (canUseSkill1)
            {
                StartCoroutine(Routine_Skill1_BoulderingSmash());
                return true;
            }

            return false;
        }

        // 🎯 TẦM GẦN: Random công bằng giữa Skill 2 (Buff khi máu < 50%) và Skill 3 (Lao húc)
        protected override bool TryExecuteMeleeSkillOrBuff()
        {
            bool canUseSkill2 = useSkill2 && skill2Timer <= 0f && (myStats != null && myStats.CurrentHealth <= myStats.MaxHealth.Value * 0.5f);
            bool canUseSkill3 = useSkill3 && skill3Timer <= 0f && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius;

            if (!canUseSkill2 && !canUseSkill3) return false;

            if (canUseSkill2 && canUseSkill3)
            {
                if (Random.value < 0.5f)
                {
                    StartCoroutine(Routine_Skill2_BeastHysteria());
                }
                else
                {
                    StartCoroutine(Routine_Skill3_BerserkCharge());
                }
                return true;
            }

            if (canUseSkill2)
            {
                StartCoroutine(Routine_Skill2_BeastHysteria());
                return true;
            }

            if (canUseSkill3)
            {
                StartCoroutine(Routine_Skill3_BerserkCharge());
                return true;
            }

            return false;
        }

        private void PlayAnim(string paramName, AnimParamType type, bool boolValue = true)
        {
            if (anim == null || string.IsNullOrEmpty(paramName)) return;

            if (type == AnimParamType.Trigger)
                anim.SetTrigger(paramName);
            else if (type == AnimParamType.Bool)
                anim.SetBool(paramName, boolValue);
        }

        private void SetSkill3WalkAnim(bool isWalking)
        {
            if (anim != null && !string.IsNullOrEmpty(skill3WalkBoolName))
            {
                anim.SetBool(skill3WalkBoolName, isWalking);

                // Nếu đang bật walk, điều chỉnh tốc độ chạy animation (Speed) tùy thuộc vào chargeSpeed
                if (isWalking)
                {
                    anim.SetFloat("AnimSpeed", chargeSpeed * walkSpeedAnimMultiplier);
                }
                else
                {
                    anim.SetFloat("AnimSpeed", 1f); // Trả về mặc định khi dừng
                }
            }
        }

        private IEnumerator Routine_Skill1_BoulderingSmash()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill1AnimName, skill1ParamType, true);
            PlaySound(skill1Sound, skill1Volume);

            yield return new WaitForSeconds(castTimeSkill1);

            if (myStats != null && myStats.IsDead)
            {
                isPerformingSkill = false;
                yield break;
            }

            Transform targetPlayer = playerTransform;
            if (targetPlayer == null)
            {
                Collider2D playerCol = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayerMask);
                if (playerCol != null) targetPlayer = playerCol.transform;
            }

            if (woodenSpikePrefab != null)
            {
                Vector3 originPos = transform.position;
                float dirX = targetPlayer != null ? Mathf.Sign(targetPlayer.position.x - originPos.x) : (transform.localScale.x >= 0 ? 1f : -1f);

                for (int i = 0; i < spikeCount; i++)
                {
                    if (myStats != null && myStats.IsDead) break;

                    float posX = originPos.x + (dirX * (spawnOffsetX + (i * spikeSpacingX)));
                    Vector3 spawnPos = new Vector3(posX, originPos.y, originPos.z);

                    float randomAngle = Random.Range(minRotationAngle, maxRotationAngle);
                    Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomAngle);

                    Instantiate(woodenSpikePrefab, spawnPos, spawnRotation);
                    PlaySound(spikeEmergeSound, spikeEmergeVolume);

                    yield return new WaitForSeconds(delayBetweenSpikes);
                }
            }

            if (skill1ParamType == AnimParamType.Bool) PlayAnim(skill1AnimName, skill1ParamType, false);

            skill1Timer = skill1Cooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_BeastHysteria()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill2AnimName, skill2ParamType, true);
            PlaySound(skill2Sound, skill2Volume);

            StartCoroutine(ApplyBuffAndHealRoutine(healAmount, buffAttackBonus));

            yield return new WaitForSeconds(0.6f);

            if (skill2ParamType == AnimParamType.Bool) PlayAnim(skill2AnimName, skill2ParamType, false);

            skill2Timer = skill2Cooldown;
            isPerformingSkill = false;
        }

        // 🎯 SKILL 3 NÂNG CẤP: DÙNG ĐƯỢC TỪ XA TRONG DETECTION, TĂNG TỐC ĐỘ ANIMATION WALK THEO CHARGE SPEED, NGẮT KHI CHẠM WALL
        private IEnumerator Routine_Skill3_BerserkCharge()
        {
            isPerformingSkill = true;
            StopMoving();

            if (playerTransform != null)
            {
                FlipTowards(playerTransform.position);
            }

            PlayAnim(skill3AnimName, skill3ParamType, true);
            PlaySound(skill3Sound, skill3Volume);

            // Bật biến walk và tự động scale tốc độ animation khớp với chargeSpeed
            SetSkill3WalkAnim(true);

            yield return new WaitForSeconds(0.2f);

            if (playerTransform != null)
            {
                Vector2 chargeDir = (playerTransform.position - transform.position).normalized;
                float timer = 0f;

                while (timer < chargeDuration)
                {
                    if (myStats != null && myStats.IsDead)
                    {
                        break;
                    }

                    rb.linearVelocity = chargeDir * chargeSpeed;
                    timer += Time.deltaTime;

                    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chargeHitRadius);
                    bool hitWallOrObstacle = false;

                    foreach (var hit in hits)
                    {
                        if (hit.gameObject == gameObject) continue;

                        if (hit.CompareTag("Player"))
                        {
                            CharacterStats target = hit.GetComponent<CharacterStats>();
                            if (target != null)
                            {
                                target.TakeDamage(chargeDamage + myStats.Attack.Value);
                            }
                        }
                        else if (!hit.isTrigger || hit.CompareTag("Wall"))
                        {
                            hitWallOrObstacle = true;
                            break;
                        }
                    }

                    if (hitWallOrObstacle)
                    {
                        break;
                    }

                    yield return null;
                }
            }

            SetSkill3WalkAnim(false);

            StopMoving();
            yield return new WaitForSeconds(0.4f);

            if (skill3AnimName != null && skill3ParamType == AnimParamType.Bool) PlayAnim(skill3AnimName, skill3ParamType, false);

            skill3Timer = skill3Cooldown;
            isPerformingSkill = false;
        }
    }
}