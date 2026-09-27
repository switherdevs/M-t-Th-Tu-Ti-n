using System.Collections;
using UnityEngine;

public class ParticleFadeController : MonoBehaviour
{
    [Header("--- CẤU HÌNH PARTICLE SYSTEM ---")]
    [Tooltip("Kéo Particle System cần điều khiển vào đây")]
    [SerializeField] private ParticleSystem targetParticleSystem;

    [Header("--- CẤU HÌNH THỜI GIAN (GIÂY) ---")]
    [Tooltip("Thời điểm bắt đầu chạy hiệu ứng Fade In (Hạt sáng dần lên)")]
    [SerializeField] private float thoiDiemBatDauFadeIn = 0f;

    [Tooltip("Thời gian để hoàn tất Fade In")]
    [SerializeField] private float thoiGianFadeIn = 1.0f;

    [Tooltip("Thời điểm bắt đầu chạy Fade Out (Phải lớn hơn thời điểm Fade In + Thời gian Fade In)")]
    [SerializeField] private float thoiDiemBatDauFadeOut = 5.0f;

    [Tooltip("Thời gian để hoàn tất Fade Out")]
    [SerializeField] private float thoiGianFadeOut = 1.0f;

    // Các biến cờ (flags) quản lý trạng thái
    private bool daHoanThanhFadeIn = false;
    private bool dangChayFadeIn = false;
    private bool dangChayFadeOut = false;

    private void Start()
    {
        // Kiểm tra nếu đã gán Particle System thì thiết lập ẩn hoàn toàn ở mốc ban đầu
        if (targetParticleSystem != null)
        {
            var mainModule = targetParticleSystem.main;
            Color mauBanDau = mainModule.startColor.color;
            mauBanDau.a = 0f; // Đặt độ mờ ban đầu về 0 (Trong suốt)
            mainModule.startColor = new ParticleSystem.MinMaxGradient(mauBanDau);
            
            targetParticleSystem.Stop(); // Dừng không cho hạt phát ra lúc đầu
        }
    }

    private void Update()
    {
        float timeHienTai = Time.time;

        // 1. Kiểm tra và kích hoạt Fade In
        if (timeHienTai >= thoiDiemBatDauFadeIn && !dangChayFadeIn && !daHoanThanhFadeIn)
        {
            if (targetParticleSystem != null && !targetParticleSystem.isPlaying)
            {
                targetParticleSystem.Play(); // Bắt đầu phát hạt
            }
            StartCoroutine(ChayFadeParticle(0f, 1f, thoiGianFadeIn, () => {
                daHoanThanhFadeIn = true;
                dangChayFadeIn = false;
            }));
        }

        // 2. Kiểm tra và kích hoạt Fade Out (ĐẢM BẢO CHỈ CHẠY KHI FADE IN ĐÃ HOÀN THÀNH)
        if (daHoanThanhFadeIn && timeHienTai >= thoiDiemBatDauFadeOut && !dangChayFadeOut)
        {
            StartCoroutine(ChayFadeParticle(1f, 0f, thoiGianFadeOut, () => {
                dangChayFadeOut = true;
                if (targetParticleSystem != null)
                {
                    targetParticleSystem.Stop(); // Tắt hẳn hệ thống hạt khi mờ hết
                }
            }));
        }
    }

    /// <summary>
    /// Thuật toán chính thay đổi độ mờ của hạt mượt mà theo thời gian thực
    /// </summary>
    private IEnumerator ChayFadeParticle(float alphaBatDau, float alphaKetThuc, float thoiGianKeoDai, System.Action khiHoanThanh)
    {
        if (alphaBatDau < alphaKetThuc) dangChayFadeIn = true;
        else dangChayFadeOut = true;

        float thoiGianTroQua = 0f;
        var mainModule = targetParticleSystem.main;

        while (thoiGianTroQua < thoiGianKeoDai)
        {
            thoiGianTroQua += Time.deltaTime;
            float tyLe = thoiGianTroQua / thoiGianKeoDai;

            // Lấy màu hiện tại của Particle System
            Color mauHienTai = mainModule.startColor.color;
            
            // Tính toán giá trị alpha tuyến tính giữa điểm đầu và điểm cuối
            mauHienTai.a = Mathf.Lerp(alphaBatDau, alphaKetThuc, tyLe);
            
            // Gán ngược lại vào Particle System
            mainModule.startColor = new ParticleSystem.MinMaxGradient(mauHienTai);

            yield return null; // Chờ sang frame tiếp theo
        }

        // Đảm bảo giá trị alpha cuối cùng chuẩn xác tuyệt đối
        Color mauCuoi = mainModule.startColor.color;
        mauCuoi.a = alphaKetThuc;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(mauCuoi);

        khiHoanThanh?.Invoke(); // Gọi hành động hoàn thành
    }
}