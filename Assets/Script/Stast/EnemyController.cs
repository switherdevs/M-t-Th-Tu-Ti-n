using System.Collections;
using UnityEngine;

namespace StatsSystem.Components
{
    [RequireComponent(typeof(CharacterStats))]
    public class EnemyController : MonoBehaviour
    {
        [Header("--- THÔNG TIN QUÁI ---")]
        [SerializeField, Tooltip("ID của loại quái này (VD: 1 = Bộ Xương, 2 = Quái Cây)")]
        private int idQuai = 1;

        [SerializeField, Tooltip("Lượng Kinh Nghiệm (EXP) người chơi nhận được khi diệt quái này")]
        private float expNhanDuoc = 1f;

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
            animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            stats.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
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

            // 2. CỘNG EXP CHO PLAYER THÔNG QUA LEVEL SYSTEM
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.AddExp(expNhanDuoc);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy LevelSystem.Instance trong Scene!");
            }

            // 3. GHI NHẬN TIẾN TRÌNH QUEST
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.GhiNhanDietQuai(idQuai, 1);
            }
            else
            {
                Debug.LogWarning("[EnemyController] Không tìm thấy QuestSaveSystem.Instance trong Scene!");
            }

            // 4. TẮT COLLIDER CỦA QUÁI
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