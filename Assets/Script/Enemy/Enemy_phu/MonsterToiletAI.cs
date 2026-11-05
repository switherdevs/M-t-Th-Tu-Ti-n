using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterToiletAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH BẬT/TẮT SKILL (INSPECTOR TOGGLE) ===")]
        [SerializeField] private bool useSkill1 = true;
        [SerializeField] private bool useSkill2 = true;
        [SerializeField] private bool useSkill3 = true;

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [SerializeField] private GameObject frozenPebblePrefab;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private LayerMask playerLayerMask;

        [Header("=== CẤU HÌNH HIỆU ỨNG SKILL 3 ===")]
        [SerializeField] private GameObject suctionEffectObject; // GameObject hiệu ứng xoáy nước

        [Header("=== CẤU HÌNH ANIMATION PARAMETER SKILL ===")]
        [SerializeField] private string skill1AnimName = "Skill1";
        [SerializeField] private AnimParamType skill1ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill2AnimName = "Skill2";
        [SerializeField] private AnimParamType skill2ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill3AnimName = "Skill3";
        [SerializeField] private AnimParamType skill3ParamType = AnimParamType.Bool;

        [Header("=== CẤU HÌNH ÂM THANH SKILL & SLIDER ÂM LƯỢNG ===")]
        [SerializeField] private AudioClip skill1Sound;
        [SerializeField, Range(0f, 1f)] private float skill1Volume = 1f;
        [SerializeField] private AudioClip skill2Sound;
        [SerializeField, Range(0f, 1f)] private float skill2Volume = 1f;
        [SerializeField] private AudioClip skill3Sound;
        [SerializeField, Range(0f, 1f)] private float skill3Volume = 1f;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU RIÊNG TỪNG SKILL ===")]
        [SerializeField] private float skill1Cooldown = 3.5f;
        [SerializeField] private float skill2Cooldown = 10f;
        [SerializeField] private float skill3Cooldown = 6f;

        [Header("=== THÔNG SỐ SKILL 1: KHỐI BĂNG HOANG THẠCH (BẮN ĐẠN TẦM XA) ===")]
        [SerializeField] private float castTimeSkill1 = 0.7f;
        [SerializeField] private int bulletCount = 3;
        [SerializeField] private float spawnRadius = 0.6f;
        [SerializeField] private float spreadAngle = 15f;

        [Header("=== THÔNG SỐ SKILL 2: NƯỚC THẦN TẨY RỬA (BUFF & HEAL - DƯỚI 50% MÁU) ===")]
        [SerializeField] private float healAmount = 50f;
        [SerializeField] private float buffAttackBonus = 20f;

        [Header("=== THÔNG SỐ SKILL 3: XOÁY NƯỚC NUỐT CHỬNG (KÉO & CẮN) ===")]
        [SerializeField] private float suctionForce = 8f;
        [SerializeField] private float suctionDuration = 2f;
        [SerializeField] private float suctionRadius = 15f;
        [SerializeField] private float biteDamage = 60f;
        [SerializeField] private float biteRange = 2f;

        private float skill1Timer = 0f;
        private float skill2Timer = 0f;
        private float skill3Timer = 0f;

        protected override void Awake()
        {
            base.Awake();
            if (suctionEffectObject != null)
            {
                suctionEffectObject.SetActive(false); // Đảm bảo hiệu ứng ẩn khi khởi động game
            }
        }

        protected override void UpdateCustomCooldowns()
        {
            if (skill1Timer > 0f) skill1Timer -= Time.deltaTime;
            if (skill2Timer > 0f) skill2Timer -= Time.deltaTime;
            if (skill3Timer > 0f) skill3Timer -= Time.deltaTime;
        }

        protected override bool TryExecuteRangedSkill()
        {
            bool canUseSkill1 = useSkill1 && skill1Timer <= 0f;
            bool canUseSkill3 = useSkill3 && skill3Timer <= 0f && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= suctionRadius;

            if (!canUseSkill1 && !canUseSkill3) return false;

            if (canUseSkill1 && canUseSkill3)
            {
                if (Random.value < 0.5f)
                {
                    StartCoroutine(Routine_Skill1_FrozenPebble());
                }
                else
                {
                    StartCoroutine(Routine_Skill3_VortexSuction());
                }
                return true;
            }

            if (canUseSkill3)
            {
                StartCoroutine(Routine_Skill3_VortexSuction());
                return true;
            }

            if (canUseSkill1)
            {
                StartCoroutine(Routine_Skill1_FrozenPebble());
                return true;
            }

            return false;
        }

        protected override bool TryExecuteMeleeSkillOrBuff()
        {
            bool canUseSkill2 = useSkill2 && skill2Timer <= 0f && (myStats != null && myStats.CurrentHealth <= myStats.MaxHealth.Value * 0.5f);

            if (canUseSkill2)
            {
                StartCoroutine(Routine_Skill2_PurificationFlush());
                return true;
            }

            return false;
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
            PlaySound(skill1Sound, skill1Volume);

            yield return new WaitForSeconds(0.3f);

            if (myStats != null && myStats.IsDead)
            {
                isPerformingSkill = false;
                yield break;
            }

            Transform targetPlayer = playerTransform;
            if (targetPlayer == null)
            {
                Collider2D playerCol = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayerMask);
                if (playerCol != null) targetPlayer = playerCol.transform;
            }

            if (frozenPebblePrefab != null && targetPlayer != null)
            {
                Vector3 centerPos = launchPoint != null ? launchPoint.position : transform.position;
                Vector2 baseDir = (targetPlayer.position - centerPos).normalized;
                float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

                for (int i = 0; i < bulletCount; i++)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                    Vector3 spawnPos = centerPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

                    GameObject pebble = Instantiate(frozenPebblePrefab, spawnPos, Quaternion.identity);

                    float currentAngle = baseAngle;
                    if (bulletCount > 1)
                    {
                        float angleStep = spreadAngle / (bulletCount - 1);
                        currentAngle = baseAngle - (spreadAngle / 2f) + (angleStep * i);
                    }

                    Vector2 dir = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

                    EnemyBullet bulletComp = pebble.GetComponent<EnemyBullet>();
                    if (bulletComp != null)
                    {
                        bulletComp.SetDirection(dir);
                    }
                }
            }

            yield return new WaitForSeconds(castTimeSkill1);

            if (skill1ParamType == AnimParamType.Bool) PlayAnim(skill1AnimName, skill1ParamType, false);

            skill1Timer = skill1Cooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_PurificationFlush()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill2AnimName, skill2ParamType, true);
            PlaySound(skill2Sound, skill2Volume);

            StartCoroutine(ApplyBuffAndHealRoutine(healAmount, buffAttackBonus));

            yield return new WaitForSeconds(0.6f);

            if (skill2ParamType == AnimParamType.Bool) PlayAnim(skill2AnimName, skill2ParamType, false);

            skill2Timer = skill2Cooldown;
            isPerformingSkill = false;
        }

        // 🎯 SKILL 3: BẬT HIỆU ỨNG KHI DÙNG, TẮT HIỆU ỨNG KHI KẾT THÚC HOẶC BỊ TIÊU DIỆT
        private IEnumerator Routine_Skill3_VortexSuction()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill3AnimName, skill3ParamType, true);
            PlaySound(skill3Sound, skill3Volume);

            // Bật GameObject hiệu ứng
            if (suctionEffectObject != null)
            {
                suctionEffectObject.SetActive(true);
            }

            float timer = 0f;
            Rigidbody2D playerRb = playerTransform != null ? playerTransform.GetComponent<Rigidbody2D>() : null;
            bool hasBitten = false;

            while (timer < suctionDuration)
            {
                if (myStats != null && myStats.IsDead)
                {
                    PlayAnim(skill3AnimName, skill3ParamType, false);
                    if (suctionEffectObject != null) suctionEffectObject.SetActive(false);
                    isPerformingSkill = false;
                    yield break;
                }

                if (playerTransform != null && playerRb != null)
                {
                    float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

                    if (distToPlayer <= suctionRadius)
                    {
                        Vector2 pullDir = (transform.position - playerTransform.position).normalized;
                        playerRb.AddForce(pullDir * suctionForce, ForceMode2D.Force);
                    }

                    if (distToPlayer <= biteRange)
                    {
                        if (playerStats != null)
                        {
                            playerStats.TakeDamage(biteDamage + myStats.Attack.Value);
                        }
                        hasBitten = true;
                        break;
                    }
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (!hasBitten && myStats != null && !myStats.IsDead && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= biteRange)
            {
                if (playerStats != null)
                {
                    playerStats.TakeDamage(biteDamage + myStats.Attack.Value);
                }
            }

            yield return new WaitForSeconds(0.2f);

            PlayAnim(skill3AnimName, skill3ParamType, false);

            // Tắt GameObject hiệu ứng khi skill hoàn tất
            if (suctionEffectObject != null)
            {
                suctionEffectObject.SetActive(false);
            }

            skill3Timer = skill3Cooldown;
            isPerformingSkill = false;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, suctionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, biteRange);
        }
    }
}