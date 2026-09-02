using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleFadeAndDestroy : MonoBehaviour
{
    [Header("--- CẤU HÌNH THỜI GIAN ---")]
    [Tooltip("Thời gian chờ (giây) trước khi bắt đầu làm mờ. Nếu = 0 sẽ tự lấy theo độ dài của Particle")]
    [SerializeField] private float waitTimeBeforeFade = 0f;

    [Tooltip("Thời gian (giây) để hiệu ứng mờ dần hoàn toàn về 0")]
    [SerializeField] private float fadeDuration = 1.0f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        // Nếu không cài đặt thời gian chờ, tự động lấy theo thời gian sống tối đa của Particle
        if (waitTimeBeforeFade <= 0f)
        {
            var mainModule = ps.main;
            waitTimeBeforeFade = mainModule.duration;
        }

        StartCoroutine(Routine_FadeAndDestroy());
    }

    private IEnumerator Routine_FadeAndDestroy()
    {
        // 1. Chờ Particle chạy xong khoảng thời gian quy định
        yield return new WaitForSeconds(waitTimeBeforeFade);

        // Ngừng phát thêm hạt mới, chỉ làm mờ các hạt hiện có
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        float timer = 0f;

        // 2. Vòng lặp giảm dần độ mờ (Alpha) theo thời gian fadeDuration
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration; // Giá trị từ 0.0 -> 1.0
            float alphaMultiplier = Mathf.Lerp(1f, 0f, normalizedTime);

            CapNhatDoMoCuaCacHat(alphaMultiplier);

            yield return null; // Chờ sang khung hình tiếp theo
        }

        // 3. Phá hủy vật thể khi độ mờ đã về 0 hoàn toàn
        Destroy(gameObject);
    }

    /// <summary>
    /// Thuật toán duyệt qua toàn bộ hạt Particle đang tồn tại và giảm Alpha của từng hạt
    /// </summary>
    private void CapNhatDoMoCuaCacHat(float alphaMultiplier)
    {
        int maxParticles = ps.main.maxParticles;

        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        // Lấy danh sách các hạt đang sống
        int numParticlesAlive = ps.GetParticles(particles);

        // Duyệt vòng lặp cập nhật màu cho từng hạt
        for (int i = 0; i < numParticlesAlive; i++)
        {
            Color32 currentColor = particles[i].startColor;
            
            // Giảm chỉ số Alpha dựa trên tỷ lệ alphaMultiplier
            currentColor.a = (byte)(currentColor.a * alphaMultiplier);
            particles[i].startColor = currentColor;
        }

        // Gán ngược lại danh sách hạt đã sửa đổi vào ParticleSystem
        ps.SetParticles(particles, numParticlesAlive);
    }
}