using System.Collections;
using UnityEngine;

namespace StatsSystem.Components
{
    [RequireComponent(typeof(CharacterStats))]
    public class EnemyController : MonoBehaviour, IStunable
    {
        [Header("--- THÔNG TIN QUÁI ---")]
        [SerializeField, Tooltip("ID của loại quái này (VD: 1 = Bộ Xương, 2 = Quái Cây)")]
        private int idQuai = 1;

        [SerializeField, Tooltip("Lượng Kinh Nghiệm (EXP) gốc người chơi nhận được khi diệt quái này")]
        private float expNhanDuoc = 1f;

        [Header("=== THỜI GIAN BIẾN MẤT ===")]
        [SerializeField, Tooltip("Thời gian (giây) quái biến mất hoàn toàn sau khi chết")]
        private float destroyDelay = 2f;

        [Header("=== ANIMATION CHẾT ===")]
        [SerializeField, Tooltip("Tên Trigger Animation chết trong Animator Controller")]
        private string dieTriggerName = "Die";

        private CharacterStats stats;
        private Animator animator;
        private Rigidbody2D rb;
        private bool isStunned = false;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
            animator = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            stats.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            stats.OnDeath -= HandleDeath;
        }

        /// <summary>
        /// Hàm nhân dồn EXP nhận được theo hệ số độ khó từ DifficultyManager
        /// </summary>
        /// <param name="heSoDoKho">Hệ số nhân (VD: 1.5x, 2.0x, 3.0x)</param>
        public void NhanExpTheoDoKho(float heSoDoKho)
        {
            expNhanDuoc *= heSoDoKho;
            Debug.Log($"[EnemyController] {gameObject.name} được dồn EXP mới: {expNhanDuoc} (Hệ số: x{heSoDoKho})");
        }

        // ==========================================
        // XỬ LÝ CHOÁNG (STUN) TỪ INTERFACE ISTUNABLE
        // ==========================================
        public void ApplyStun(float duration)
        {
            if (isStunned) return;
            StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            isStunned = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (animator != null)
            {
                animator.SetTrigger(dieTriggerName);
            }

            yield return new WaitForSeconds(duration);

            isStunned = false;
            if (animator != null)
            {
                animator.ResetTrigger(dieTriggerName);
            }
        }

        // ==========================================
        // XỬ LÝ CHẾT HOÀN TOÀN TỪ CHARACTERSTATS
        // ==========================================
        private void HandleDeath()
        {
            Debug.Log($"{gameObject.name} (ID Quái: {idQuai}) đã chết!");

            StopAllCoroutines();

            if (animator != null)
            {
                animator.SetTrigger(dieTriggerName);
            }

            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.AddExp(expNhanDuoc);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy LevelSystem.Instance trong Scene!");
            }

            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.GhiNhanDietQuai(idQuai, 1);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy QuestSaveSystem.Instance trong Scene!");
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}