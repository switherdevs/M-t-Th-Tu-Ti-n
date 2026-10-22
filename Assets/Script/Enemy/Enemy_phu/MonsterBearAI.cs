using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterBearAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [SerializeField] private GameObject boulderPrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH ANIMATION PARAMETER ===")]
        [SerializeField] private string moveAnimName = "isMoving";
        [SerializeField] private string skill1AnimName = "Skill1";
        [SerializeField] private AnimParamType skill1ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill2AnimName = "Skill2";
        [SerializeField] private AnimParamType skill2ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill3AnimName = "Skill3";
        [SerializeField] private AnimParamType skill3ParamType = AnimParamType.Trigger;

        [Header("=== CẤU HÌNH TRỌNG SỐ TUNG SKILL ===")]
        [SerializeField, Range(1, 100)] private int skill1Weight = 40;
        [SerializeField, Range(1, 100)] private int skill2Weight = 30;
        [SerializeField, Range(1, 100)] private int skill3Weight = 30;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [SerializeField] private float globalSkillCooldown = 4f;

        [Header("=== THÔNG SỐ SKILL 1: BÃO ĐÁ MA BÃO (NÉM ĐÁ TẦM XA) ===")]
        [SerializeField] private float boulderFlySpeed = 8f;
        [SerializeField] private float castTimeSkill1 = 1.0f;

        [Header("=== THÔNG SỐ SKILL 2: BÀNG HÃI TIẾNG GẦM (BUFF & HEAL) ===")]
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
            return skillCooldownTimer <= 0f && !isPerformingSkill && (myStats == null || !myStats.IsDead);
        }

        protected override void SelectAndExecuteSkill()
        {
            if (myStats != null && myStats.IsDead) return;

            int totalWeight = skill1Weight + skill2Weight + skill3Weight;
            int randomValue = Random.Range(0, totalWeight);

            if (randomValue < skill1Weight)
            {
                StartCoroutine(Routine_Skill1_BoulderingSmash());
            }
            else if (randomValue < skill1Weight + skill2Weight)
            {
                StartCoroutine(Routine_Skill2_BeastHysteria());
            }
            else
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

        private IEnumerator Routine_Skill1_BoulderingSmash()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill1AnimName, skill1ParamType, true);
            yield return new WaitForSeconds(0.4f);

            if (myStats != null && myStats.IsDead)
            {
                isPerformingSkill = false;
                yield break;
            }

            if (boulderPrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject boulder = Instantiate(boulderPrefab, spawnPos, Quaternion.identity);

                Vector2 dir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D boulderRb = boulder.GetComponent<Rigidbody2D>();
                if (boulderRb != null)
                {
                    boulderRb.linearVelocity = dir * boulderFlySpeed;
                }
            }

            yield return new WaitForSeconds(castTimeSkill1);

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