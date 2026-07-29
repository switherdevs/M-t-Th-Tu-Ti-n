using System.Collections;
using UnityEngine;
using StatsSystem.Components;

namespace StatsSystem.Components
{
    [RequireComponent(typeof(CharacterStats))]
    public class EnemyController : MonoBehaviour
    {
        private CharacterStats stats;

        private void Awake()
        {
            stats = GetComponent<CharacterStats>();
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
            Debug.Log($"{gameObject.name} đã chết!");

            // Thử cộng EXP và log ra console
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.AddExp(1f);
                Debug.Log("<color=green>Đã gọi AddExp thành công!</color>");
            }
            else
            {
                Debug.LogError("KHÔNG TÌM THẤY LevelSystem.Instance trong Scene!");
            }

            StartCoroutine(DestroyAfterDelay(2f));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}