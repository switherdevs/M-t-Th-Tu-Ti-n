using UnityEngine;

public class PhiKiem : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    [Header("Âm thanh & Hiệu ứng khi Trúng")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private CharacterStats shooterStats;
    private bool hasHit = false;

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
        if (hasHit) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            hasHit = true;

            var victim = collision.GetComponentInParent<IDamageable>();
            if (victim != null)
            {
                float atk = (shooterStats != null) ? shooterStats.GetStat(StatType.Attack).Value : 10f;

                Debug.Log($"<color=green>[BẮN TRÚNG]</color> {collision.name} ăn đòn!");
                victim.TakeDamage(atk);
            }

            PlayHitEffects();
            Destroy(gameObject);
        }
    }

    private void PlayHitEffects()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }
    }
}