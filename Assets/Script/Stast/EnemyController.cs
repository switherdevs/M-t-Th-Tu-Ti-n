using System.Collections;
using UnityEngine;
using StatsSystem.Components;

namespace StatsSystem.Components
{
    [RequireComponent(typeof(CharacterStats))]
    public class EnemyController : MonoBehaviour
    {
        [Header("--- THÔNG TIN QUÁI ---")]
        [SerializeField, Tooltip("ID của loại quái này (VD: 1 = Bộ Xương, 2 = Quái Cây)")]
        private int idQuai = 1;

        [Header("=== THỜI GIAN BIẾN MẤT ===")]
        [SerializeField, Tooltip("Thời gian (giây) quái biến mất hoàn toàn sau khi chết")]
        private float destroyDelay = 2f;

        [Header("=== ANIMATION CHẾT ===")]
        [SerializeField, Tooltip("Tên Trigger Animation chết trong Animator Controller")]
        private string dieTriggerName = "Die";

        private CharacterStats stats;
        private Animator animator;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();

            // Luôn luôn lấy Animator ở các GameObject con
            animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            // Đăng ký nhận thông báo khi quái chết
            stats.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            // Hủy đăng ký khi object bị disable/destroy để tránh leak memory
            stats.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            Debug.Log($"{gameObject.name} (ID Quái: {idQuai}) đã chết!");

            // 1. KÍCH HOẠT ANIMATION CHẾT
            if (animator != null)
            {
                animator.SetTrigger(dieTriggerName);
            }

            // 2. CỘNG EXP CHO PLAYER
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.AddExp(1f);
                Debug.Log("<color=green>Đã gọi AddExp thành công!</color>");
            }
            else
            {
                Debug.LogError("KHÔNG TÌM THẤY LevelSystem.Instance trong Scene!");
            }

            // 3. NÂNG CẤP: GHI NHẬN QUÁI CHẾT VÀ LƯU VÀO QUEST SAVE SYSTEM
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.GhiNhanDietQuai(idQuai, 1);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy QuestSaveSystem.Instance trong Scene!");
            }

            // 4. TẮT COLLIDER/PHYSICS CỦA QUÁI (Đảm bảo quái chết không cản đường hay bị đánh tiếp)
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // 5. CHỜ THỜI GIAN VÀ XÓA GAMEOBJECT
            StartCoroutine(DestroyAfterDelay(destroyDelay));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}