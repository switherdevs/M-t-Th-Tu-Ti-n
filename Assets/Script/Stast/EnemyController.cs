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
        private bool isDeadHandled = false; // Biến cờ bảo vệ chống kích hoạt chết 2 lần

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
            animator = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.OnDeath -= HandleDeath;
            }
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
            if (isStunned || isDeadHandled) return;
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
            if (animator != null && !isDeadHandled)
            {
                animator.ResetTrigger(dieTriggerName);
            }
        }

        // ==========================================
        // XỬ LÝ CHẾT HOÀN TOÀN TỪ CHARACTERSTATS
        // ==========================================
        private void HandleDeath()
        {
            // Tránh chạy trùng lặp nhiều lần nếu có nhiều nguồn gây sát thương cùng lúc
            if (isDeadHandled) return;
            isDeadHandled = true;

            Debug.Log($"{gameObject.name} (ID Quái: {idQuai}) đã chết!");

            // Dừng di chuyển ngay lập tức
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; // Tắt vật lý để tránh va chạm khi đã chết
            }

            // Tắt Collider ngay lập tức
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Kích hoạt Animation Chết
            if (animator != null)
            {
                animator.SetTrigger(dieTriggerName);
            }

            // Cộng EXP cho Player (Kiểm tra an toàn)
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.AddExp(expNhanDuoc);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy LevelSystem.Instance trong Scene!");
            }

            // Ghi nhận nhiệm vụ
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.GhiNhanDietQuai(idQuai, 1);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy QuestSaveSystem.Instance trong Scene!");
            }

            // Dừng các Coroutine cũ (như Stun, AI Skill) trên GameObject này
            StopAllCoroutines();

            // Chạy Coroutine tự hủy sau thời gian chờ
            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}