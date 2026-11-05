using System.Collections;
using UnityEngine;

namespace GameCore.AI
{
    public class MonsterRabbitAI : MonsterAIBase
    {
        public enum AnimParamType { Trigger, Bool }

        [Header("=== CẤU HÌNH BẬT/TẮT SKILL (INSPECTOR TOGGLE) ===")]
        [SerializeField] private bool useSkill1 = true;
        [SerializeField] private bool useSkill2 = true;
        [SerializeField] private bool useSkill3 = true;

        [Header("=== CẤU HÌNH GAMEOBJECT & PREFAB ===")]
        [SerializeField] private GameObject rockProjectilePrefab;
        [SerializeField] private Transform launchPoint;

        [Header("=== CẤU HÌNH HIỆU ỨNG & ÂM THANH SKILL 3 ===")]
        [Tooltip("GameObject hiệu ứng (VFX) xuất hiện khi thỏ dùng chiêu 3 nhảy")]
        [SerializeField] private GameObject skill3Effect;
        [SerializeField] private AudioClip skill3Sound;
        [SerializeField, Range(0f, 1f)] private float skill3Volume = 1f;

        [Header("=== CẤU HÌNH ANIMATION PARAMETER SKILL ===")]
        [SerializeField] private string skill1AnimName = "Skill1";
        [SerializeField] private AnimParamType skill1ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill2AnimName = "Skill2";
        [SerializeField] private AnimParamType skill2ParamType = AnimParamType.Trigger;
        [SerializeField] private string skill3AnimName = "Skill3";
        [SerializeField] private AnimParamType skill3ParamType = AnimParamType.Trigger;

        [Header("=== CẤU HÌNH ÂM THANH SKILL KHÁC & SLIDER ÂM LƯỢNG ===")]
        [SerializeField] private AudioClip skill1Sound;
        [SerializeField, Range(0f, 1f)] private float skill1Volume = 1f;
        [SerializeField] private AudioClip skill2Sound;
        [SerializeField, Range(0f, 1f)] private float skill2Volume = 1f;

        [Header("=== CẤU HÌNH THỜI GIAN HỒI CHIÊU RIÊNG TỪNG SKILL ===")]
        [SerializeField] private float skill1Cooldown = 3f;
        [SerializeField] private float skill2Cooldown = 10f;
        [SerializeField] private float skill3Cooldown = 5f;

        [Header("=== THÔNG SỐ SKILL 1: THẠCH THỎ CUỒNG BẮN (NÉM ĐÁ TẦM XA) ===")]
        [SerializeField] private float rockFlySpeed = 12f;
        [SerializeField] private float castTimeSkill1 = 0.8f;

        [Header("=== THÔNG SỐ SKILL 2: CUỒNG NỘ MA THỎ (BUFF & HEAL - DƯỚI 50% MÁU) ===")]
        [SerializeField] private float healAmount = 35f;
        [SerializeField] private float buffAttackBonus = 15f;

        [Header("=== THÔNG SỐ SKILL 3: ĐỘT KÍCH THỎ CỤ (NHẢY PARABOL TỚI PLAYER) ===")]
        [SerializeField] private float pounceDamage = 40f;
        [SerializeField] private float pounceAoeRadius = 2.5f;
        [SerializeField] private float jumpDuration = 0.5f;
        [SerializeField] private float jumpHeight = 3.5f;

        private float skill1Timer = 0f;
        private float skill2Timer = 0f;
        private float skill3Timer = 0f;

        protected override void Awake()
        {
            base.Awake();
            if (skill3Effect != null)
            {
                skill3Effect.SetActive(false);
            }
        }

        protected override void UpdateCustomCooldowns()
        {
            if (skill1Timer > 0f) skill1Timer -= Time.deltaTime;
            if (skill2Timer > 0f) skill2Timer -= Time.deltaTime;
            if (skill3Timer > 0f) skill3Timer -= Time.deltaTime;
        }

        // 🎯 TẦM XA: Cho phép quái lựa chọn ngẫu nhiên giữa Skill 1 (Ném đá) và Skill 3 (Nhảy từ xa tới vị trí bạn) miễn là trong tầm nhìn
        protected override bool TryExecuteRangedSkill()
        {
            bool canUseSkill1 = useSkill1 && skill1Timer <= 0f;
            bool canUseSkill3 = useSkill3 && skill3Timer <= 0f && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius;

            if (!canUseSkill1 && !canUseSkill3) return false;

            // Nếu cả 2 đều sẵn sàng ở tầm xa, random 50/50 để thỏ chủ động nhảy đột kích từ xa hoặc ném đá
            if (canUseSkill1 && canUseSkill3)
            {
                if (Random.value < 0.5f)
                {
                    StartCoroutine(Routine_Skill1_RangedPebble());
                }
                else
                {
                    StartCoroutine(Routine_Skill3_CarrotPounce());
                }
                return true;
            }

            if (canUseSkill3)
            {
                StartCoroutine(Routine_Skill3_CarrotPounce());
                return true;
            }

            if (canUseSkill1)
            {
                StartCoroutine(Routine_Skill1_RangedPebble());
                return true;
            }

            return false;
        }

        // 🎯 TẦM GẦN: Random giữa Skill 2 (Buff khi máu < 50%) và Skill 3 (Nhảy dậm đất)
        protected override bool TryExecuteMeleeSkillOrBuff()
        {
            bool canUseSkill2 = useSkill2 && skill2Timer <= 0f && (myStats != null && myStats.CurrentHealth <= myStats.MaxHealth.Value * 0.5f);
            bool canUseSkill3 = useSkill3 && skill3Timer <= 0f && playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius;

            if (!canUseSkill2 && !canUseSkill3) return false;

            if (canUseSkill2 && canUseSkill3)
            {
                if (Random.value < 0.5f)
                {
                    StartCoroutine(Routine_Skill2_BuffAndHeal());
                }
                else
                {
                    StartCoroutine(Routine_Skill3_CarrotPounce());
                }
                return true;
            }

            if (canUseSkill2)
            {
                StartCoroutine(Routine_Skill2_BuffAndHeal());
                return true;
            }

            if (canUseSkill3)
            {
                StartCoroutine(Routine_Skill3_CarrotPounce());
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

        private IEnumerator Routine_Skill1_RangedPebble()
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

            skill1Timer = skill1Cooldown;
            isPerformingSkill = false;
        }

        private IEnumerator Routine_Skill2_BuffAndHeal()
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

        // 🎯 SKILL 3: NHẢY PARABOL TỪ MỌI KHOẢNG CÁCH TRONG VÙNG DETECTION TỚI ĐÚNG VỊ TRÍ PLAYER
        private IEnumerator Routine_Skill3_CarrotPounce()
        {
            isPerformingSkill = true;
            StopMoving();

            PlayAnim(skill3AnimName, skill3ParamType, true);

            PlaySound(skill3Sound, skill3Volume);
            if (skill3Effect != null) skill3Effect.SetActive(true);

            yield return new WaitForSeconds(0.2f);

            if (playerTransform != null && myStats != null && !myStats.IsDead)
            {
                Vector3 startPos = transform.position;
                Vector3 targetPos = playerTransform.position;

                FlipTowards(targetPos);

                if (meleeAttackHitbox != null)
                {
                    meleeAttackHitbox.SetActive(true);
                }

                float elapsed = 0f;

                while (elapsed < jumpDuration)
                {
                    if (myStats != null && myStats.IsDead)
                    {
                        if (skill3Effect != null) skill3Effect.SetActive(false);
                        if (meleeAttackHitbox != null) meleeAttackHitbox.SetActive(false);
                        isPerformingSkill = false;
                        yield break;
                    }

                    elapsed += Time.deltaTime;
                    float linearT = Mathf.Clamp01(elapsed / jumpDuration);

                    Vector3 currentPos = Vector3.Lerp(startPos, targetPos, linearT);
                    float currentHeight = Mathf.Sin(linearT * Mathf.PI) * jumpHeight;
                    currentPos.y += currentHeight;

                    transform.position = currentPos;
                    yield return null;
                }

                transform.position = targetPos;

                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pounceAoeRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        CharacterStats target = hit.GetComponent<CharacterStats>();
                        if (target != null)
                        {
                            target.TakeDamage(pounceDamage + myStats.Attack.Value);
                        }
                    }
                }
            }

            if (skill3Effect != null) skill3Effect.SetActive(false);
            if (meleeAttackHitbox != null) meleeAttackHitbox.SetActive(false);

            yield return new WaitForSeconds(0.4f);

            if (skill3AnimName != null && skill3ParamType == AnimParamType.Bool) PlayAnim(skill3AnimName, skill3ParamType, false);

            skill3Timer = skill3Cooldown;
            isPerformingSkill = false;
        }
    }
}