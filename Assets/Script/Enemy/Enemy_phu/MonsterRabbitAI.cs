using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterRabbitAI : MonsterAIBase
    {
        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [Tooltip("Prefab của viên đá thỏ ném ra ở Skill 1 (Phải có Rigidbody2D và DamageDealer)")]
        [SerializeField] private GameObject rockProjectilePrefab;

        [Tooltip("Điểm xuất phát của viên đá (Kéo Transform vị trí miệng/tay thỏ vào đây)")]
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [Tooltip("Khoảng thời gian nghỉ (giây) giữa các lần tung Skill")]
        [SerializeField] private float globalSkillCooldown = 3f;

        [Header("=== THÔNG SỐ SKILL 1: THẠCH THỎ CUỒNG BẮN (NÉM ĐÁ) ===")]
        [Tooltip("Tốc độ bay của viên đá khi thỏ ném ra")]
        [SerializeField] private float rockFlySpeed = 12f;

        [Header("=== THÔNG SỐ SKILL 2: CUỒNG NỘ MA THỎ (BUFF & HEAL) ===")]
        [Tooltip("Lượng máu thỏ được hồi ngay lập tức khi bật Buff")]
        [SerializeField] private float healAmount = 35f;

        [Tooltip("Số điểm Sát Thương (Attack) cộng thêm cho thỏ trong thời gian Buff")]
        [SerializeField] private float buffAttackBonus = 15f;

        [Tooltip("Thời gian duy trì trạng thái Buff Sát Thương (giây)")]
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỘT KÍCH THỎ CỤ (DẬM ĐẤT AOE) ===")]
        [Tooltip("Sát thương cơ bản gây ra khi thỏ dập xuống đất (Cộng thêm với Attack hiện tại)")]
        [SerializeField] private float pounceDamage = 40f;

        [Tooltip("Bán kính vòng tròn gây sát thương diện rộng (AoE) khi thỏ tiếp đất")]
        [SerializeField] private float pounceAoeRadius = 2f;

        [Tooltip("Thời gian thỏ di chuyển trên không trung khi nhảy tới vị trí Player (giây)")]
        [SerializeField] private float jumpDuration = 0.5f;

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

        // Skill 1: Thạch Thỏ Cuồng Bắn
        private IEnumerator Routine_Skill1_RangedPebble()
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

            yield return new WaitForSeconds(0.5f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Cuồng Nộ Ma Thỏ
        private IEnumerator Routine_Skill2_BuffAndHeal()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            if (myStats != null && !myStats.IsDead)
            {
                myStats.Heal(healAmount);
                myStats.Attack.Value += buffAttackBonus;
            }

            yield return new WaitForSeconds(0.5f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;

            yield return new WaitForSeconds(buffDuration);
            if (myStats != null)
            {
                myStats.Attack.Value -= buffAttackBonus;
            }
        }

        // Skill 3: Đột Kích Thỏ Cụ
        private IEnumerator Routine_Skill3_CarrotPounce()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill3");
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
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}