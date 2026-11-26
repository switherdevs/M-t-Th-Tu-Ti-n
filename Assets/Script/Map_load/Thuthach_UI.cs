
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuChonAi : MonoBehaviour
{
    [Header("Cấu Hình Giao Diện UI")]
    [SerializeField] private GameObject menuChaPanel;

    [Header("Cấu Hình Bản Đồ (Map)")]
    [SerializeField] private string tenScene = "Map_Doantrai";

    // Hàm xử lý khi người chơi nhấn nút Resume
    public void OnClickResume()
    {
        if (menuChaPanel != null)
        {
            menuChaPanel.SetActive(false);
        }
    }

    // Hàm xử lý khi người chơi nhấn nút Thử Thách
    public void OnClickThuChach()
    {
        if (string.IsNullOrWhiteSpace(tenScene))
        {
            Debug.LogError("[MenuChonAi] Chưa nhập tên Scene!");
            return;
        }

        // Kiểm tra Scene có tồn tại trong Build hay không
        if (!Application.CanStreamedLevelBeLoaded(tenScene))
        {
            Debug.LogError("[MenuChonAi] Không tìm thấy Scene trong Build: " + tenScene);
            return;
        }

        Debug.Log("[MenuChonAi] Đang chuyển Scene: " + tenScene);

        // Chuyển sang bản đồ thử thách
        SceneManager.LoadScene(tenScene);
    }
}
