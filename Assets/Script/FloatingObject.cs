using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("--- CẤU HÌNH LƠ LỬNG ---")]
    [Tooltip("Độ cao lên xuống (Biên độ dao động)")]
    [SerializeField] private float floatAmplitude = 0.5f;

    [Tooltip("Tốc độ lên xuống của vật thể")]
    [SerializeField] private float floatSpeed = 2f;

    // Lưu lại vị trí gốc ban đầu của GameObject
    private Vector3 startPosition;

    private void Start()
    {
        // Ghi nhận tọa độ X, Y, Z ban đầu khi game vừa chạy
        startPosition = transform.position;
    }

    private void Update()
    {
        // 🎯 THUẬT TOÁN CHÍNH: Sử dụng hàm sóng hình Sin để tạo chuyển động mượt mà
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // Cập nhật vị trí mới, giữ nguyên trục X và Z so với vị trí gốc
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}