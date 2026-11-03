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

        [Header("=== CẤU HÌNH ÂM THANH SKILL & SLIDER AM LUONG ===")]
        [SerializeField] private AudioClip skill1Sound;
        [SerializeField, Range(0f, 1f)] private float skill1Volume = 1f;
        [SerializeField] private AudioClip skill2Sound;
        [SerializeField, Range(0f, 1f)] private float skill2Volume = 1f;
        [SerializeField] private AudioClip skill3Sound;
        [SerializeField, Range(0f, 1f)] private float skill3Volume = 1f;

        [Header("=== CẤU HÌNH TRỌNG SỐ TUNG SKILL ===")]
        [SerializeField, Range(1, 100)] private int skill2Weight = 30;
        [SerializeField, Range(1, 100)] private int skill3Weight = 30;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [SerializeField] private float globalSkillCooldown = 4f;

        [Header("=== THÔNG SỐ SKILL 1: CỌC GỖ ĐỊA GAI (TỪ QUÁI RA HƯỚNG PLAYER) ===")]
        [SerializeField] private float castTimeSkill1 = 0.8f;
        [Tooltip("Số lượng cọc gỗ muốn hiển thị (Tùy ý chỉnh trên Inspector)")]
        [SerializeField] private int spikeCount = 5;
        [Tooltip("Khoảng cách tối thiểu từ chân quái đến cọc gỗ đầu tiên (tránh mọc ngay chân)")]
        [SerializeField] private float spawnOffsetX = 1.5f;
        [Tooltip("Khoảng cách giữa các cọc gỗ tiếp theo tính theo trục X")]
        [SerializeField] private float spikeSpacingX = 1.2f;
        [Tooltip("Khoảng thời gian trễ (giây) giữa các lần cọc gỗ trồi lên")]
        [SerializeField] private float delayBetweenSpikes = 0.15f;
        [Tooltip("Góc xoay tối thiểu và tối đa (độ) để tạo độ xoay random ngẫu nhiên")]
        [SerializeField] private float minRotationAngle = -15f;
        [SerializeField] private float maxRotationAngle = 15f;
        [Tooltip("Âm thanh phát ra mỗi khi 1 cọc gỗ trồi lên")]
        [SerializeField] private AudioClip spikeEmergeSound;
        [SerializeField, Range(0f, 1f)] private float spikeEmergeVolume = 1f;

        [Header("=== THÔNG SỐ SKILL 2: BÀNG HÃI TIẾNG GẦM (BUFF & HEAL - DƯỚI 50% MÁU) ===")]
        [SerializeField] private float healAmount = 60f;
        [SerializeField] private float buffAttackBonus = 25f;
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỊA LẠC CUỒNG CÀO (ỦI CÀN QUÉT) ===")]
        [SerializeField] private float chargeSpeed = 10f;
        [SerializeField] private float chargeDuration = 0.6f;
        [SerializeField] private float chargeDamage = 50f;
        [SerializeField] private float chargeHitRadius = 1.5f;

        protected override bool CanUseSkill()
        {
            return skillCooldownTimer <= 0f && !isPerformingSkill && !isPerformingBasicAttack && (myStats == null || !myStats.IsDead);
        }

        protected override void SelectAndExecuteSkill(bool isRangedDistance)
        {
            if (myStats != null && myStats.IsDead) return;

            if (isRangedDistance)
            {
                if (useSkill1) StartCoroutine(Routine_Skill1_BoulderingSmash());
                return;
            }

            bool canUseHealBuff = useSkill2 && (myStats.CurrentHealth <= myStats.MaxHealth.Value * 0.5f);
            bool canUseCharge = useSkill3;

            if (!canUseHealBuff && !canUseCharge) return;

            int totalWeight = 0;
            if (canUseHealBuff) totalWeight += skill2Weight;
            if (canUseCharge) totalWeight += skill3Weight;

            if (totalWeight <= 0) return;

            int randomValue = Random.Range(0, totalWeight);

            if (canUseHealBuff && randomValue < skill2Weight)
            {
                StartCoroutine(Routine_Skill2_BeastHysteria());
            }
            else if (canUseCharge)
            {
                StartCoroutine(Routine_Skill3_BerserkCharge());
            }
        }

        private void PlayAnim(string paramName, AnimParamType type, bool boolValue = true)
        {
            if (anim == null || string.IsNullOrEmpty(paramName)) return;

            if (type == AnimParamType.Trigger)
                anim.SetTrigger(paramName);
            else if (type == AnimParamType.Bool)
                anim.SetBool(paramName, boolValue);
        }

        // 🎯 SKILL 1: CỌC GỖ XUẤT HIỆN TUẦN TỰ, CÁCH CHÂN QUÁI MỘT ĐOẠN (spawnOffsetX) VÀ ĐÚNG HƯỚNG TRÁI/PHẢI
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
                if (playerCol != null)
                {
                    targetPlayer = playerCol.transform;
                }
            }

            if (woodenSpikePrefab != null)
            {
                Vector3 originPos = transform.position;

                // Xác định hướng xuất phát dựa vào vị trí Player so với quái (hoặc hướng scale của quái)
                float dirX = 1f;
                if (targetPlayer != null)
                {
                    float diffX = targetPlayer.position.x - originPos.x;
                    if (Mathf.Abs(diffX) > 0.01f)
                    {
                        dirX = Mathf.Sign(diffX);
                    }
                    else
                    {
                        dirX = transform.localScale.x >= 0 ? 1f : -1f;
                    }
                }
                else
                {
                    dirX = transform.localScale.x >= 0 ? 1f : -1f;
                }

                // Vòng lặp sinh cọc gỗ tuần tự lan dần ra ngoài, cộng thêm spawnOffsetX để không dính chân quái
                for (int i = 0; i < spikeCount; i++)
                {
                    if (myStats != null && myStats.IsDead) break;

                    // Công thức vị trí: Vị trí quái + Hướng nhân với (Khoảng cách an toàn + khoảng cách từng cọc)
                    float posX = originPos.x + (dirX * (spawnOffsetX + (i * spikeSpacingX)));
                    Vector3 spawnPos = new Vector3(posX, originPos.y, originPos.z);

                    // Xoay ngẫu nhiên
                    float randomAngle = Random.Range(minRotationAngle, maxRotationAngle);
                    Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomAngle);

                    // Spawn cọc gỗ và phát âm thanh
                    Instantiate(woodenSpikePrefab, spawnPos, spawnRotation);
                    PlaySound(spikeEmergeSound, spikeEmergeVolume);

                    // Độ trễ để cọc trồi lên lần lượt từng cái một đúng thứ tự
                    yield return new WaitForSeconds(delayBetweenSpikes);
                }
            }

            if (skill1ParamType == AnimParamType.Bool) PlayAnim(skill1AnimName, skill1ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_BeastHysteria()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill2AnimName, skill2ParamType, true);
            PlaySound(skill2Sound, skill2Volume);

            if (myStats != null && !myStats.IsDead)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.8f);

            if (skill2ParamType == AnimParamType.Bool) PlayAnim(skill2AnimName, skill2ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        private IEnumerator Routine_Skill3_BerserkCharge()
        {
            isPerformingSkill = true;

            PlayAnim(skill3AnimName, skill3ParamType, true);
            PlaySound(skill3Sound, skill3Volume);

            yield return new WaitForSeconds(0.3f);

            if (playerTransform != null)
            {
                Vector2 chargeDir = (playerTransform.position - transform.position).normalized;
                float timer = 0f;

                while (timer < chargeDuration)
                {
                    if (myStats != null && myStats.IsDead)
                    {
                        isPerformingSkill = false;
                        yield break;
                    }

                    rb.linearVelocity = chargeDir * chargeSpeed;
                    timer += Time.deltaTime;

                    Collider2D hit = Physics2D.OverlapCircle(transform.position, chargeHitRadius, LayerMask.GetMask("Player"));
                    if (hit != null)
                    {
                        CharacterStats target = hit.GetComponent<CharacterStats>();
                        if (target != null)
                        {
                            target.TakeDamage(chargeDamage + myStats.Attack.Value);
                            break;
                        }
                    }
                    yield return null;
                }
            }

            StopMoving();
            yield return new WaitForSeconds(0.5f);

            if (skill3ParamType == AnimParamType.Bool) PlayAnim(skill3AnimName, skill3ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}