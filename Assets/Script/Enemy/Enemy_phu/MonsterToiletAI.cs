using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterToiletAI : MonsterAIBase
    {
        [Header("=== PREFAB ĐẠN TOILET ===")]
        [SerializeField] private GameObject frozenPebblePrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== THÔNG SỐ SKILL TOILET MEME ===")]
        [SerializeField] private float globalSkillCooldown = 3.5f;
        [SerializeField] private float healAmount = 50f;
        [SerializeField] private float buffAttackBonus = 20f;
        [SerializeField] private float buffDuration = 10f;
        [SerializeField] private float suctionForce = 8f;
        [SerializeField] private float suctionDuration = 2f;
        [SerializeField] private float biteDamage = 60f;

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
                    StartCoroutine(Routine_Skill1_FrozenPebble());
                    break;
                case 2:
                    StartCoroutine(Routine_Skill2_PurificationFlush());
                    break;
                case 3:
                    StartCoroutine(Routine_Skill3_VortexSuction());
                    break;
            }
        }

        // Skill 1: Khối Băng Hoang Thạch (Bắn đạn xoáy dơ bẩn)
        private IEnumerator Routine_Skill1_FrozenPebble()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill1");
            yield return new WaitForSeconds(0.3f);

            if (frozenPebblePrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = launchPoint != null ? launchPoint.position : transform.position;
                GameObject pebble = Instantiate(frozenPebblePrefab, spawnPos, Quaternion.identity);

                Vector2 dir = (playerTransform.position - spawnPos).normalized;
                Rigidbody2D pebbleRb = pebble.GetComponent<Rigidbody2D>();
                if (pebbleRb != null)
                {
                    pebbleRb.linearVelocity = dir * 10f;
                }
            }

            yield return new WaitForSeconds(0.4f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Nước Thần Tẩy Rửa (Hồi máu xoáy bồn cầu + Buff 10s)
        private IEnumerator Routine_Skill2_PurificationFlush()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            if (myStats != null)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.6f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        // Skill 3: Xoáy Nước Nuốt Chửng (Kéo Player lại gần và cắn mạnh)
        private IEnumerator Routine_Skill3_VortexSuction()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill3");

            float timer = 0f;
            Rigidbody2D playerRb = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;

            while (timer < suctionDuration)
            {
                if (playerTransform != null && playerRb != null)
                {
                    // Lực hút kéo Player về phía Toilet
                    Vector2 pullDir = (transform.position - playerTransform.position).normalized;
                    playerRb.AddForce(pullDir * suctionForce, ForceMode2D.Force);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // Đập nắp bồn cầu gây sát thương cận chiến nếu Player ở trong phạm vi
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= 2f)
            {
                if (playerStats != null)
                {
                    playerStats.TakeDamage(biteDamage + myStats.Attack.Value);
                }
            }

            yield return new WaitForSeconds(0.5f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}