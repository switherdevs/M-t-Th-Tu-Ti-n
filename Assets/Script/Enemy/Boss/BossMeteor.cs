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

    [Tooltip("Thời gian sống tối đa nếu không trúng Player")]
    [SerializeField] private float lifeTime = 3f;


    // =========================================================
    // VA CHẠM & HIỆU ỨNG
    // =========================================================

    [Header("===== VA CHẠM & HIỆU ỨNG =====")]
    [Tooltip("Prefab hiệu ứng nổ sinh ra khi chạm Player hoặc hết lifeTime")]
    [SerializeField] private GameObject explosionEffect;


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

        // Di chuyển thiên thạch rơi theo hướng chỉ định
        transform.Translate(fallDirection.normalized * fallSpeed * Time.deltaTime);

        // Đếm ngược tự nổ nếu không trúng ai
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded) return;

        // Kiểm tra đúng Tag "Player" để không nổ oan vào Box Camera
        if (collision.CompareTag("Player") || collision.GetComponentInParent<Rigidbody2D>()?.CompareTag("Player") == true)
        {
            Explode();
        }
    }

    private void Explode()
    {
        isExploded = true;

        // Kích hoạt rung màn hình qua Basic Multi Channel Perlin
        TriggerPerlinShake();

        // Sinh ra hiệu ứng nổ tại vị trí hiện tại
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Xóa GameObject thiên thạch
        Destroy(gameObject);
    }

    private void TriggerPerlinShake()
    {
        // Tự động tìm CinemachineCamera duy nhất trong Scene
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();

        if (vcam != null)
        {
            // Lấy component CinemachineBasicMultiChannelPerlin gắn trên Camera
            CinemachineBasicMultiChannelPerlin perlin = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();

            if (perlin != null)
            {
                // Vì Meteor bị Destroy ngay nên cần mượn 1 Runner trong Scene (hoặc chính Vcam) để chạy Coroutine giảm rung
                vcam.StartCoroutine(ProcessShake(perlin));
            }
        }
    }

    private IEnumerator ProcessShake(CinemachineBasicMultiChannelPerlin perlin)
    {
        // Bật lực rung
        perlin.AmplitudeGain = shakeAmplitude;
        perlin.FrequencyGain = shakeFrequency;

        // Chờ hết thời gian rung
        yield return new WaitForSeconds(shakeDuration);

        // Tắt rung (Trả về 0)
        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }
}