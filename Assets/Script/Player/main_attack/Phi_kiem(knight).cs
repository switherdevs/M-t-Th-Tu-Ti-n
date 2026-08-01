using StatsSystem.Components;
using StatsSystem.Core;
using StatsSystem.Interfaces;
using StatsSystem.Services;
using UnityEngine;

public class PhiKiem : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    [Header("Âm thanh & Hiệu ứng khi Trúng")]
    [SerializeField] private AudioClip hitSound;          // File âm thanh va chạm (WAV/MP3)
    [SerializeField] private GameObject hitEffectPrefab;  // Prefab hiệu ứng VFX
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f; // Độ lớn âm thanh (0 đến 1)

    private CharacterStats shooterStats;
    private bool hasHit = false; // Biến cờ ngăn ngừa việc tính va chạm trùng lặp trong 1 frame

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Setup(Vector2 direction, CharacterStats stats)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        shooterStats = stats;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Tránh chạy lại va chạm nếu đã trúng từ frame trước
        if (hasHit) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            hasHit = true; // Đánh dấu đã va chạm để đảm bảo âm thanh và hiệu ứng chỉ phát 1 LẦN DỰA TRÊN 1 LẦN TRÚNG

            // 1. Xử lý tính sát thương
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

            // 2. Xử lý Âm thanh và Hiệu ứng va chạm
            PlayHitEffects();

            // 3. Tiêu hủy phi kiếm ngay lập tức
            Destroy(gameObject);
        }
    }

    private void PlayHitEffects()
    {
        // Phát hiệu ứng Visual Effect tại đúng vị trí va chạm
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        }

        // Phát âm thanh va chạm 1 lần tại tọa độ phi kiếm (không bị ngắt khi phi kiếm bị Destroy)
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }
    }
}