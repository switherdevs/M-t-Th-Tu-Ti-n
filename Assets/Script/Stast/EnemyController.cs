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
            Debug.Log($"{gameObject.name} đã chết! Chuẩn bị biến mất sau 2 giây...");

            // Bạn có thể tắt Collider / AI Movement ở đây để quái không cản đường nữa
            // GetComponent<Collider>().enabled = false;

            StartCoroutine(DestroyAfterDelay(2f));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}