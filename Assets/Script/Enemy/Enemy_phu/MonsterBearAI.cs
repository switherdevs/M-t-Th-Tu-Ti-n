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

        [Header("=== CẤU HÌNH TRỌNG SỐ TUNG SKILL ===")]
        [SerializeField, Range(1, 100)] private int skill2Weight = 30;
        [SerializeField, Range(1, 100)] private int skill3Weight = 30;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [SerializeField] private float globalSkillCooldown = 4f;

        [Header("=== THÔNG SỐ SKILL 1: CỌC GỖ ĐỊA GAI (TẦM XA TRỒI ĐẤT) ===")]
        [Tooltip("Thời gian gấu thực hiện xong Animation đập đất rồi mới sinh xiên gỗ (giây)")]
        [SerializeField] private float castTimeSkill1 = 0.8f;

        [Tooltip("Số lượng xiên gỗ sinh ra mỗi lần tung chiêu")]
        [SerializeField] private int spikeCount = 3;

        [Tooltip("Khoảng cách giữa các xiên gỗ trồi lên")]
        [SerializeField] private float spikeSpacing = 1.5f;

        [Tooltip("Góc xoay ngẫu nhiên tối thiểu (Độ)")]
        [SerializeField] private float minRotationAngle = 45f;

        [Tooltip("Góc xoay ngẫu nhiên tối đa (Độ)")]
        [SerializeField] private float maxRotationAngle = 60f;

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

        // Skill 1: Cọc Gỗ Địa Gai (Tầm xa)
        private IEnumerator Routine_Skill1_BoulderingSmash()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill1AnimName, skill1ParamType, true);
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

            if (woodenSpikePrefab != null && targetPlayer != null)
            {
                Vector3 originPos = transform.position;
                Vector3 targetDir = (targetPlayer.position - originPos).normalized;

                for (int i = 1; i <= spikeCount; i++)
                {
                    Vector3 spawnPos = originPos + (targetDir * (i * spikeSpacing));
                    spawnPos.z = originPos.z;

                    // 1. Tính góc nghiêng ngẫu nhiên trong khoảng [minRotationAngle, maxRotationAngle]
                    float randomAngle = Random.Range(minRotationAngle, maxRotationAngle);

                    // 2. Ngẫu nhiên lật góc nghiêng sang trái hoặc phải để dáng gai cắm tự nhiên hơn
                    if (Random.value > 0.5f)
                    {
                        randomAngle = -randomAngle;
                    }

                    // 3. Tạo Quaternion góc xoay trên trục Z
                    Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomAngle);

                    // 4. Sinh ra cọc gỗ với vị trí và góc xoay ngẫu nhiên
                    Instantiate(woodenSpikePrefab, spawnPos, spawnRotation);
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