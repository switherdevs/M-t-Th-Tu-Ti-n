using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterBearAI : MonsterAIBase
    {
        [Header("=== PREFAB ĐẠN BẢO ĐÁ ===")]
        [SerializeField] private GameObject boulderPrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== THÔNG SỐ SKILL GẤU ===")]
        [SerializeField] private float globalSkillCooldown = 4f;
        [SerializeField] private float healAmount = 60f;
        [SerializeField] private float buffAttackBonus = 25f;
        [SerializeField] private float buffDuration = 10f;
        [SerializeField] private float chargeSpeed = 10f;
        [SerializeField] private float chargeDamage = 50f;

        protected override bool CanUseSkill()
        {
            return skillCooldownTimer <= 0f && !isPerformingSkill;
        }

        protected override void SelectAndExecuteSkill()
        {
            int randomSkill = Random.Range(1, 4);

            switch (randomSkill)
            {
                case 1:
                    StartCoroutine(Routine_Skill1_BoulderingSmash());
                    break;
                case 2:
                    StartCoroutine(Routine_Skill2_BeastHysteria());
                    break;
                case 3:
                    StartCoroutine(Routine_Skill3_BerserkCharge());
                    break;
            }
        }

        // Skill 1: Bão Đá Ma Bão (Ném khối đá văng tầm xa)
        private IEnumerator Routine_Skill1_BoulderingSmash()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill1");
            yield return new WaitForSeconds(0.4f);

            if (boulderPrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject boulder = Instantiate(boulderPrefab, spawnPos, Quaternion.identity);

                Vector2 dir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D boulderRb = boulder.GetComponent<Rigidbody2D>();
                if (boulderRb != null)
                {
                    boulderRb.linearVelocity = dir * 8f;
                }
            }

            yield return new WaitForSeconds(0.6f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Bàng Hãi Tiếng Gầm (Hồi máu lớn + Buff 10s)
        private IEnumerator Routine_Skill2_BeastHysteria()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            if (myStats != null)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.8f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        // Skill 3: Địa Lạc Cuồng Cào (Lúc ủi càn quét)
        private IEnumerator Routine_Skill3_BerserkCharge()
        {
            isPerformingSkill = true;

            if (anim != null) anim.SetTrigger("Skill3");
            yield return new WaitForSeconds(0.3f);

            if (playerTransform != null)
            {
                Vector2 chargeDir = (playerTransform.position - transform.position).normalized;
                float chargeDuration = 0.6f;
                float timer = 0f;

                while (timer < chargeDuration)
                {
                    rb.linearVelocity = chargeDir * chargeSpeed;
                    timer += Time.deltaTime;

                    // Kiểm tra ủi trúng Player
                    Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.5f, LayerMask.GetMask("Player"));
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
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}