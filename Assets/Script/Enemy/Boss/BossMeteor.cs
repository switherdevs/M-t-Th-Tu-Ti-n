using System.Collections;
using UnityEngine;
// Thư viện Cinemachine chuẩn Unity 6
using Unity.Cinemachine;

public class BossMeteor : MonoBehaviour
{
    // =========================================================
    // CẤU HÌNH RƠI
    // =========================================================

    [Header("===== CẤU HÌNH RƠI =====")]
    [Tooltip("Tốc độ rơi của thiên thạch")]
    [SerializeField] private float fallSpeed = 10f;

    [Tooltip("Hướng rơi (Mặc định: rơi thẳng xuống)")]
    [SerializeField] private Vector2 fallDirection = new Vector2(0f, -1f);

    [Tooltip("Thời gian sống tối đa nếu không trúng mục tiêu")]
    [SerializeField] private float lifeTime = 3f;


    // =========================================================
    // VA CHẠM & HIỆU ỨNG
    // =========================================================

    [Header("===== VA CHẠM & HIỆU ỨNG =====")]
    [Tooltip("LayerMask của tường (Wall) để thiên thạch chạm vào và nổ")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Prefab hiệu ứng nổ sinh ra khi chạm Wall, Player hoặc hết lifeTime")]
    [SerializeField] private GameObject explosionEffect;


    // =========================================================
    // ÂM THANH NỔ
    // =========================================================

    [Header("===== ÂM THANH NỔ =====")]
    [Tooltip("File âm thanh khi thiên thạch phát nổ")]
    [SerializeField] private AudioClip explosionSound;

    [Tooltip("Âm lượng âm thanh nổ (0 đến 1)")]
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;


    // =========================================================
    // RUNG CAM (BASIC MULTI CHANNEL PERLIN)
    // =========================================================

    [Header("===== TÙY CHỈNH RUNG CAMERA (PERLIN) =====")]
    [Tooltip("Độ mạnh của lực rung (Amplitude Gain)")]
    [SerializeField] private float shakeAmplitude = 2f;

    [Tooltip("Tốc độ giật/tần số rung (Frequency Gain)")]
    [SerializeField] private float shakeFrequency = 2f;

    [Tooltip("Thời gian rung màn hình (giây)")]
    [SerializeField] private float shakeDuration = 0.3f;


    // =========================================================
    // BIẾN PRIVATE
    // =========================================================

    private float timer = 0f;
    private bool isExploded = false;


    private void Update()
    {
        if (isExploded) return;

        // Di chuyển thiên thạch rơi theo hướng chỉ định[cite: 7]
        transform.Translate(fallDirection.normalized * fallSpeed * Time.deltaTime);

        // Đếm ngược tự nổ nếu không trúng ai[cite: 7]
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Explode();
        }
    }

    // Xử lý va chạm nếu Collider dùng kiểu TRIGGER
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckAndExplode(collision.gameObject, collision.gameObject.layer);
    }

    // Xử lý va chạm nếu Collider dùng kiểu SOLID (Non-Trigger / Rigidbody2D Collision)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndExplode(collision.gameObject, collision.gameObject.layer);
    }

    // Hàm chung kiểm tra điều kiện nổ khi chạm Wall hoặc Player
    private void CheckAndExplode(GameObject hitObj, int hitLayer)
    {
        if (isExploded) return;

        // 1. Kiểm tra va chạm với Player (dựa vào Tag hoặc Component CharacterStats nếu có)
        bool isPlayer = hitObj.CompareTag("Player") ||
                        hitObj.GetComponentInParent<Rigidbody2D>()?.CompareTag("Player") == true ||
                        hitObj.GetComponent<CharacterStats>() != null;

        // 2. Kiểm tra va chạm với Wall (dựa vào LayerMask Wall đã chọn trên Inspector)
        bool isWall = ((wallLayer.value & (1 << hitLayer)) > 0) || hitObj.CompareTag("Wall");

        if (isPlayer || isWall)
        {
            Explode();
        }
    }

    private void Explode()
    {
        isExploded = true;

        // Phát âm thanh phát nổ 1 lần duy nhất tại vị trí nổ (không bị ngắt khi Destroy)[cite: 7]
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // Kích hoạt rung màn hình qua Basic Multi Channel Perlin[cite: 7]
        TriggerPerlinShake();

        // Sinh ra hiệu ứng nổ tại vị trí hiện tại[cite: 7]
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Xóa GameObject thiên thạch[cite: 7]
        Destroy(gameObject);
    }

    private void TriggerPerlinShake()
    {
        // Tự động tìm CinemachineCamera duy nhất trong Scene[cite: 7]
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();

        if (vcam != null)
        {
            // Lấy component CinemachineBasicMultiChannelPerlin gắn trên Camera[cite: 7]
            CinemachineBasicMultiChannelPerlin perlin = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (perlin != null)
            {
                // Vì Meteor bị Destroy ngay nên cần mượn 1 Runner trong Scene để chạy Coroutine giảm rung[cite: 7]
                vcam.StartCoroutine(ProcessShake(perlin));
            }
        }
    }

    private IEnumerator ProcessShake(CinemachineBasicMultiChannelPerlin perlin)
    {
        // Bật lực rung[cite: 7]
        perlin.AmplitudeGain = shakeAmplitude;
        perlin.FrequencyGain = shakeFrequency;

        // Chờ hết thời gian rung[cite: 7]
        yield return new WaitForSeconds(shakeDuration);

        // Tắt rung (Trả về 0)[cite: 7]
        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }
}