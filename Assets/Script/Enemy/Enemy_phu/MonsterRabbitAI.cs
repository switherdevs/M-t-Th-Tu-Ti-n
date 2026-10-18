using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterRabbitAI : MonsterAIBase
    {
        [Header("=== CẤU HÌNH PREFAB ĐẠN / EFFECT ===")]
        [SerializeField] private GameObject rockProjectilePrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== THÔNG SỐ SKILL THỎ ===")]
        [SerializeField] private float globalSkillCooldown = 3f;
        [SerializeField] private float healAmount = 35f;
        [SerializeField] private float buffAttackBonus = 15f;
        [SerializeField] private float buffDuration = 10f;
        [SerializeField] private float pounceDamage = 40f;

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
                    StartCoroutine(Routine_Skill1_RangedPebble());
                    break;
                case 2:
                    StartCoroutine(Routine_Skill2_BuffAndHeal());
                    break;
                case 3:
                    StartCoroutine(Routine_Skill3_CarrotPounce());
                    break;
            }
        }

        // Skill 1: Thạch Thỏ Cuồng Bắn (Ném đá tầm xa)
        private IEnumerator Routine_Skill1_RangedPebble()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill1");
            yield return new WaitForSeconds(0.3f);

            if (rockProjectilePrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject rock = Instantiate(rockProjectilePrefab, spawnPos, Quaternion.identity);

                Vector2 launchDir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
                if (rockRb != null)
                {
                    rockRb.linearVelocity = launchDir * 12f;
                }
            }

            yield return new WaitForSeconds(0.5f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Cuồng Nộ Ma Thỏ (Hồi máu + Buff công 10s)
        private IEnumerator Routine_Skill2_BuffAndHeal()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            // Hồi máu
            if (myStats != null)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.5f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            // Đếm ngược 10s tự động trả lại chỉ số Attack cũ
            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        // Skill 3: Đột Kích Thỏ Cụ (Lao vút đè bẹp diện rộng)
        private IEnumerator Routine_Skill3_CarrotPounce()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill3");
            yield return new WaitForSeconds(0.2f);

            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position;
                float jumpTime = 0.5f;
                float elapsed = 0f;
                Vector3 startPos = transform.position;

                while (elapsed < jumpTime)
                {
                    transform.position = Vector3.Lerp(startPos, targetPos, elapsed / jumpTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.position = targetPos;

                // Gây sát thương AoE khi tiếp đất
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        CharacterStats target = hit.GetComponent<CharacterStats>();
                        if (target != null) target.TakeDamage(pounceDamage + myStats.Attack.Value);
                    }
                }
            }

            yield return new WaitForSeconds(0.4f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}