using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterRabbitAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [Tooltip("Prefab của viên đá thỏ ném ra ở Skill 1 (Phải có Rigidbody2D và DamageDealer)")]
        [SerializeField] private GameObject rockProjectilePrefab;

        [Tooltip("Điểm xuất phát của viên đá (Kéo Transform vị trí miệng/tay thỏ vào đây)")]
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
        [SerializeField, Range(1, 100)] private int skill1Weight = 50;
        [SerializeField, Range(1, 100)] private int skill2Weight = 25;
        [SerializeField, Range(1, 100)] private int skill3Weight = 25;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [SerializeField] private float globalSkillCooldown = 3f;

        [Header("=== THÔNG SỐ SKILL 1: THẠCH THỎ CUỒNG BẮN (NÉM ĐÁ TẦM XA) ===")]
        [SerializeField] private float rockFlySpeed = 12f;
        [SerializeField] private float castTimeSkill1 = 0.8f;

        [Header("=== THÔNG SỐ SKILL 2: CUỒNG NỘ MA THỎ (BUFF & HEAL) ===")]
        [SerializeField] private float healAmount = 35f;
        [SerializeField] private float buffAttackBonus = 15f;
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỘT KÍCH THỎ CỤ (DẬM ĐẤT AOE) ===")]
        [SerializeField] private float pounceDamage = 40f;
        [SerializeField] private float pounceAoeRadius = 2f;
        [SerializeField] private float jumpDuration = 0.5f;

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
                StartCoroutine(Routine_Skill1_RangedPebble());
            }
            else if (randomValue < skill1Weight + skill2Weight)
            {
                StartCoroutine(Routine_Skill2_BuffAndHeal());
            }
            else
            {
                StartCoroutine(Routine_Skill3_CarrotPounce());
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

        private IEnumerator Routine_Skill1_RangedPebble()
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

            if (rockProjectilePrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject rock = Instantiate(rockProjectilePrefab, spawnPos, Quaternion.identity);

                Vector2 launchDir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
                if (rockRb != null)
                {
                    rockRb.linearVelocity = launchDir * rockFlySpeed;
                }
            }

            yield return new WaitForSeconds(castTimeSkill1);

            if (skill1ParamType == AnimParamType.Bool) PlayAnim(skill1AnimName, skill1ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_BuffAndHeal()
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

        private IEnumerator Routine_Skill3_CarrotPounce()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill3AnimName, skill3ParamType, true);
            yield return new WaitForSeconds(0.2f);

            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position;
                float elapsed = 0f;
                Vector3 startPos = transform.position;

                while (elapsed < jumpDuration)
                {
                    if (myStats != null && myStats.IsDead)
                    {
                        isPerformingSkill = false;
                        yield break;
                    }

                    transform.position = Vector3.Lerp(startPos, targetPos, elapsed / jumpDuration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.position = targetPos;

                if (myStats != null && !myStats.IsDead)
                {
                    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pounceAoeRadius);
                    foreach (var hit in hits)
                    {
                        if (hit.CompareTag("Player"))
                        {
                            CharacterStats target = hit.GetComponent<CharacterStats>();
                            if (target != null) target.TakeDamage(pounceDamage + myStats.Attack.Value);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.4f);

            if (skill3ParamType == AnimParamType.Bool) PlayAnim(skill3AnimName, skill3ParamType, false);

            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}