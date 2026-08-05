using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Lấy trực tiếp Component Noise trên cùng GameObject này
        GetNoiseComponent();
    }

    private void GetNoiseComponent()
    {
        // Tự động tìm component Noise gắn cùng chỗ với Script
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        // Nếu không thấy, tìm trong các component con/mở rộng
        if (noise == null)
        {
            noise = GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Shake(float intensity, float time)
    {
        if (noise == null)
        {
            GetNoiseComponent();

            if (noise == null)
            {
                Debug.LogWarning("[CameraShake] Không tìm thấy CinemachineBasicMultiChannelPerlin trên GameObject này!");
                return;
            }
        }

        // Tắt Coroutine cũ nếu đang rung dở để đè rung mới lên
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ProcessShake(intensity, time));
    }

    private IEnumerator ProcessShake(float intensity, float time)
    {
        // Tăng lực rung
        noise.AmplitudeGain = intensity;

        float timer = time;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            // Giảm dần lực rung theo thời gian để tạo cảm giác mượt
            noise.AmplitudeGain = Mathf.Lerp(intensity, 0f, 1f - (timer / time));
            yield return null;
        }

        // Trả về 0 khi kết thúc
        noise.AmplitudeGain = 0f;
    }
}