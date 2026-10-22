using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterToiletAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [SerializeField] private GameObject frozenPebblePrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH ANIMATION PARAMETER ===")]
        [SerializeField] private string skill1AnimName = "Skill1";
        [SerializeField] private AnimParamType skill1ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill2AnimName = "Skill2";
        [SerializeField] private AnimParamType skill2ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill3AnimName = "Skill3";
        [SerializeField] private AnimParamType skill3ParamType = AnimParamType.Trigger;

        [Header("=== CẤU HÌNH TRỌNG SỐ TUNG SKILL ===")]
        [SerializeField, Range(1, 100)] private int skill1Weight = 50;
        [SerializeField, Range(1, 100)] private int skill2Weight = 20;
        [SerializeField, Range(1, 100)] private int skill3Weight = 30;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [SerializeField] private float globalSkillCooldown = 3.5f;

        [Header("=== THÔNG SỐ SKILL 1: KHỐI BĂNG HOANG THẠCH (BẮN ĐẠN TẦM XA) ===")]
        [SerializeField] private float pebbleFlySpeed = 10f;
        [SerializeField] private float castTimeSkill1 = 0.7f;

        [Header("=== THÔNG SỐ SKILL 2: NƯỚC THẦN TẨY RỬA (BUFF & HEAL) ===")]
        [SerializeField] private float healAmount = 50f;
        [SerializeField] private float buffAttackBonus = 20f;
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: XOÁY NƯỚC NUỐT CHỬNG (KÉO & CẮN) ===")]
        [SerializeField] private float suctionForce = 8f;
        [SerializeField] private float suctionDuration = 2f;
        [SerializeField] private float biteDamage = 60f;
        [SerializeField] private float biteRange = 2f;

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
                StartCoroutine(Routine_Skill1_FrozenPebble());
            }
            else if (randomValue < skill1Weight + skill2Weight)
            {
                StartCoroutine(Routine_Skill2_PurificationFlush());
            }
            else
            {
                StartCoroutine(Routine_Skill3_VortexSuction());
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

        private IEnumerator Routine_Skill1_FrozenPebble()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill1AnimName, skill1ParamType, true);
            yield return new WaitForSeconds(0.3f);

            if (myStats != null && myStats.IsDead)
            {
                isPerformingSkill = false;
                yield break;
            }

            if (frozenPebblePrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject pebble = Instantiate(frozenPebblePrefab, spawnPos, Quaternion.identity);

                Vector2 dir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D pebbleRb = pebble.GetComponent<Rigidbody2D>();
                if (pebbleRb != null)
                {
                    pebbleRb.linearVelocity = dir * pebbleFlySpeed;
                }
            }

            yield return new WaitForSeconds(castTimeSkill1);

            if (skill1ParamType == AnimParamType.Bool) PlayAnim(skill1AnimName, skill1ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_PurificationFlush()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill2AnimName, skill2ParamType, true);

            if (myStats != null && !myStats.IsDead)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.6f);

            if (skill2ParamType == AnimParamType.Bool) PlayAnim(skill2AnimName, skill2ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        private IEnumerator Routine_Skill3_VortexSuction()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill3AnimName, skill3ParamType, true);

            float timer = 0f;
            Rigidbody2D playerRb = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;

            while (timer < suctionDuration)
            {
                if (myStats != null && myStats.IsDead)
                {
                    isPerformingSkill = false;
                    yield break;
                }

                if (playerTransform != null && playerRb != null)
                {
                    Vector2 pullDir = (transform.position - playerTransform.position).normalized;
                    playerRb.AddForce(pullDir * suctionForce, ForceMode2D.Force);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (myStats != null && !myStats.IsDead && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= biteRange)
            {
                if (playerStats != null)
                {
                    playerStats.TakeDamage(biteDamage + myStats.Attack.Value);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (skill3ParamType == AnimParamType.Bool) PlayAnim(skill3AnimName, skill3ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}