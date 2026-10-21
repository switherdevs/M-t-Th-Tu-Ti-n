using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterBearAI : MonsterAIBase
    {
        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [Tooltip("Prefab của mảng đá to mà gấu ném ra ở Skill 1 (Có Rigidbody2D và DamageDealer)")]
        [SerializeField] private GameObject boulderPrefab;

        [Tooltip("Điểm xuất phát của mảng đá khi ném (Transform vị trí bàn tay gấu)")]
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU CHUNG ===")]
        [Tooltip("Khoảng thời gian nghỉ (giây) giữa các lần tung Skill")]
        [SerializeField] private float globalSkillCooldown = 4f;

        [Header("=== THÔNG SỐ SKILL 1: BÃO ĐÁ MA BÃO (NÉM TẢO ĐÁ) ===")]
        [Tooltip("Tốc độ bay của mảng đá to")]
        [SerializeField] private float boulderFlySpeed = 8f;

        [Header("=== THÔNG SỐ SKILL 2: BÀNG HÃI TIẾNG GẦM (BUFF & HEAL) ===")]
        [Tooltip("Lượng máu gấu tự phục hồi ngay khi gầm lên")]
        [SerializeField] private float healAmount = 60f;

        [Tooltip("Số điểm Sát Thương (Attack) cộng thêm cho gấu trong thời gian Buff")]
        [SerializeField] private float buffAttackBonus = 25f;

        [Tooltip("Thời gian duy trì trạng thái Buff Sát Thương (giây)")]
        [SerializeField] private float buffDuration = 10f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỊA LẠC CUỒNG CÀO (ỦI CÀN QUÉT) ===")]
        [Tooltip("Tốc độ gấu lao thẳng về phía Player khi ủi")]
        [SerializeField] private float chargeSpeed = 10f;

        [Tooltip("Thời gian tối đa gấu duy trì đòn lao ủi (giây)")]
        [SerializeField] private float chargeDuration = 0.6f;

        [Tooltip("Sát thương cơ bản gây ra khi ủi trúng Player (Cộng thêm với Attack hiện tại)")]
        [SerializeField] private float chargeDamage = 50f;

        [Tooltip("Bán kính vòng tròn va chạm kiểm tra ủi trúng Player")]
        [SerializeField] private float chargeHitRadius = 1.5f;

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

        // Skill 1: Bão Đá Ma Bão
        private IEnumerator Routine_Skill1_BoulderingSmash()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill1");
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

            yield return new WaitForSeconds(0.6f);
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }

        // Skill 2: Bàng Hãi Tiếng GẦm
        private IEnumerator Routine_Skill2_BeastHysteria()
        {
            isPerformingSkill = true;
            StopMoving();

            if (anim != null) anim.SetTrigger("Skill2");

            if (myStats != null && !myStats.IsDead)
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

        // Skill 3: Địa Lạc Cuồng Cào
        private IEnumerator Routine_Skill3_BerserkCharge()
        {
            isPerformingSkill = true;

            if (anim != null) anim.SetTrigger("Skill3");
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
            skillCooldownTimer = globalSkillCooldown;
            isPerformingSkill = false;
        }
    }
}