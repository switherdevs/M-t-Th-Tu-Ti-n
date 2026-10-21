using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterToiletAI : MonsterAIBase
    {
        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [Tooltip("Prefab của viên đạn bồn cầu bắn ra ở Skill 1 (Có Rigidbody2D và DamageDealer)")]
        [SerializeField] private GameObject frozenPebblePrefab;

        [Tooltip("Điểm xuất phát bắn đạn (Transform vị trí miệng bồn cầu)")]
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [Tooltip("Khoảng thời gian nghỉ (giây) giữa các lần tung Skill")]
        [SerializeField] private float globalSkillCooldown = 3.5f;

        [Header("=== THÔNG SỐ SKILL 1: KHỐI BĂNG HOANG THẠCH (BẮN ĐẠN DƠ BẨN) ===")]
        [Tooltip("Tốc độ bay của viên đạn bồn cầu")]
        [SerializeField] private float pebbleFlySpeed = 10f;

        [Header("=== THÔNG SỐ SKILL 2: NƯỚC THẦN TẨY RỬA (BUFF & HEAL) ===")]
        [Tooltip("Lượng máu bồn cầu tự xoáy nước hồi phục")]
        [SerializeField] private float healAmount = 50f;

        [Tooltip("Số điểm Sát Thương (Attack) cộng thêm cho bồn cầu trong thời gian Buff")]
        [SerializeField] private float buffAttackBonus = 20f;

        [Tooltip("Thời gian duy trì trạng thái Buff Sát Thương (giây)")]
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: XOÁY NƯỚC NUỐT CHỬNG (KÉO & CẮN) ===")]
        [Tooltip("Cường độ lực hút kéo Player về phía bồn cầu")]
        [SerializeField] private float suctionForce = 8f;

        [Tooltip("Thời gian duy trì lực hút xoáy nước (giây)")]
        [SerializeField] private float suctionDuration = 2f;

        [Tooltip("Sát thương cơ bản khi đập nắp bồn cầu cắn Player (Cộng thêm với Attack hiện tại)")]
        [SerializeField] private float biteDamage = 60f;

        [Tooltip("Khoảng cách tối đa để bồn cầu cắn trúng Player sau khi hết thời gian hút")]
        [SerializeField] private float biteRange = 2f;

        protected override bool CanUseSkill()
        {
            return skillCooldownTimer <= 0f && !isPerformingSkill && (myStats == null || !myStats.IsDead);
        }

        protected override void SelectAndExecuteSkill()
        {
            if (myStats != null && myStats.IsDead) return;

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

        // Skill 1: Khối Bắn Bồn Cầu
        private IEnumerator Routine_Skill1_FrozenPebble()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill1");
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

            yield return new WaitForSeconds(0.4f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Nước Thần Tẩy Rửa
        private IEnumerator Routine_Skill2_PurificationFlush()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            if (myStats != null && !myStats.IsDead)
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

        // Skill 3: Xoáy Nước Nuốt Chửng
        private IEnumerator Routine_Skill3_VortexSuction()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill3");

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
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}