using System.Collections;
using StatsSystem.Components;
using UnityEngine;

public class VanKiemBatPhuong : MonoBehaviour
{
    [Header("Cấu Hình Rung Camera")]
    [SerializeField] private float shakeIntensity = 3.5f;
    [SerializeField] private float shakeDuration = 0.2f;

    [Header("Cấu Hình Âm Thanh Skill")]
    [SerializeField] private AudioClip spawnSound;       // Âm thanh khi kiếm vừa sinh ra
    [SerializeField] private AudioClip launchSound;      // Âm thanh khi kiếm bắt đầu lao đi
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.8f;

    [Header("Cấu Hình Di Chuyển Kiếm")]
    [SerializeField] private float initialSpeed = 10f;    // Tốc độ bay tỏa ra ban đầu
    [SerializeField] private float scatterDuration = 0.4f;// Thời gian bay tỏa ra trước khi dừng lại tụ
    [SerializeField] private float homingSpeed = 18f;     // Tốc độ lao đi sau khi tụ hướng
    [SerializeField] private float maxLifeTime = 3f;      // Thời gian sống tối đa của kiếm

    private float damage;
    private float stunDuration;
    private Vector2 moveDirection;
    private CharacterStats characterStats;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(Vector2 direction, float damageValue, float stunTime, CharacterStats stats)
    {
        moveDirection = direction;
        damage = damageValue;
        stunDuration = stunTime;
        characterStats = stats;

        // Xoay hướng của kiếm theo hướng bay tỏa tròn
        UpdateRotation(direction);

        // Phát âm thanh khi vừa tạo kiếm 1 lần duy nhất
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position, soundVolume);
        }

        // KÍCH HOẠT RUNG CAMERA KHI BẮN KIẾM
        if (CameraShake.Instance != null)
        {
            float finalIntensity = shakeIntensity > 0f ? shakeIntensity : 3.5f;
            float finalDuration = shakeDuration > 0f ? shakeDuration : 0.2f;
            CameraShake.Instance.Shake(finalIntensity, finalDuration);
        }

        // Bắt đầu chu trình: Tỏa ra -> Dừng/Tụ lại -> Lao đi theo chuột hoặc quái
        StartCoroutine(SwordBehaviorRoutine());

        // Tự hủy sau thời gian tối đa để tránh rác bộ nhớ
        Destroy(gameObject, maxLifeTime);
    }

    private IEnumerator SwordBehaviorRoutine()
    {
        // === GIAI ĐOẠN 1: Bay tỏa ra xung quanh ===
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * initialSpeed;
        }

        yield return new WaitForSeconds(scatterDuration);

        // === GIAI ĐOẠN 2: Dừng lại / Khoảnh khắc tụ khí ===
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(0.2f); // Thời gian khựng lại nhẹ tạo hiệu ứng tụ kiếm

        // === GIAI ĐOẠN 3: Xác định hướng lao đi ===
        Vector2 targetDir = GetTargetDirection();

        // Xoay đầu kiếm về hướng mục tiêu mới
        UpdateRotation(targetDir);

        // Phát âm thanh khi kiếm bắt đầu lao đi 1 lần
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position, soundVolume);
        }

        // Lao đi với tốc độ cao
        if (rb != null)
        {
            rb.linearVelocity = targetDir * homingSpeed;
        }
    }

    private Vector2 GetTargetDirection()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 10f);
        float minDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            return (closestEnemy.position - transform.position).normalized;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 dirToMouse = (mouseWorldPos - transform.position).normalized;

        if (dirToMouse != Vector2.zero)
        {
            return dirToMouse;
        }

        return moveDirection;
    }

    private void UpdateRotation(Vector2 dir)
    {
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            IStunable stunableEnemy = collision.GetComponentInParent<IStunable>();
            if (stunableEnemy != null)
            {
                stunableEnemy.ApplyStun(stunDuration);
            }

            Destroy(gameObject);
        }
    }
}