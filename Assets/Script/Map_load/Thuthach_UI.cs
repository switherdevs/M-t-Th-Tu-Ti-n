using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có thư viện này để chuyển Map

public class MenuChonAi : MonoBehaviour
{
    [Header("Cấu Hình Giao Diện UI")]
    [SerializeField] private GameObject menuChaPanel; // Kéo Object UI Cha vào đây để bật/tắt

    [Header("Cấu Hình Bản Đồ (Map)")]
    // Dùng kiểu Object đúng ý bạn để kéo thẳng file Scene từ cửa sổ Project vào
    [SerializeField] private Object mapThuChachScene;

    // Hàm xử lý khi người chơi nhấn nút Resume (Hủy/Quay lại)
    public void OnClickResume()
    {
        if (menuChaPanel != null)
        {
            // Tắt Object UI cha đi để ẩn menu lựa chọn
            menuChaPanel.SetActive(false);
        }
    }

    // Hàm xử lý khi người chơi nhấn nút Thử Thách
    public void OnClickThuChach()
    {
        // Kiểm tra an toàn: Nếu bạn quên chưa kéo file Scene vào thì bỏ qua không báo lỗi đỏ
        if (mapThuChachScene != null)
        {
            // Thuật toán lấy chính xác TÊN của file Scene mà bạn đã kéo vào ô Object
            string tenMap = mapThuChachScene.name;

            // Kích hoạt lệnh của Unity để chuyển ngay sang Map mới dựa theo tên vừa lấy được
            SceneManager.LoadScene(tenMap);
        }
        else
        {
            Debug.LogWarning("Bạn ơi, bạn chưa kéo file Scene của Map mới vào ô 'Map Thu Chach Scene' rồi!");
        }
    }
}