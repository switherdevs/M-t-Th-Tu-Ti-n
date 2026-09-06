using UnityEngine;

public class DontDestroyUI : MonoBehaviour
{
    private static DontDestroyUI _instance;

    private void Awake()
    {
        // Kiểm tra xem đã có Canvas UI nào từ Scene trước chuyển sang chưa
        if (_instance != null && _instance != this)
        {
            // Nếu đã có UI gốc rồi, xóa UI trùng lặp vừa tạo ở Scene mới
            Destroy(gameObject);
            return;
        }

        // Đánh dấu đây là Canvas UI gốc và giữ lại khi chuyển Scene
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}