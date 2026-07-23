using StatsSystem.Components;
using StatsSystem.Core;
using StatsSystem.Interfaces;
using StatsSystem.Services;
using UnityEngine;

public class PhiKiem : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    private CharacterStats shooterStats;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Hàm này giúp phi kiếm xoay mũi theo hướng nó đang bay
    public void Setup(Vector2 direction, CharacterStats stats)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        shooterStats = stats;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            // Dùng GetComponentInParent để không bị hụt khi đụng Collider con của Enemy
            var victim = collision.GetComponentInParent<IDamageable>();

            if (victim != null)
            {
                CharacterStats victimStats = collision.GetComponentInParent<CharacterStats>();

                float atk = (shooterStats != null) ? shooterStats.GetStat(StatType.Attack).Value : 10f;
                float def = (victimStats != null) ? victimStats.GetStat(StatType.Defense).Value : 0f;

                float finalDamage = DamageCalculator.CalculateDamage(atk, def);

                Debug.Log($"<color=green>[BẮN TRÚNG]</color> {collision.name} nhận {finalDamage} sát thương!");

                victim.TakeDamage(finalDamage);
            }
            else
            {
                Debug.LogWarning($"[CẢNH BÁO] Đã chạm {collision.name} (Tag: Enemy) nhưng không tìm thấy IDamageable!");
            }

            Destroy(gameObject);
        }
    }
}