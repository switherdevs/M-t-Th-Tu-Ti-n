using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_load : MonoBehaviour
{
    // Đổi từ kiểu Scene sang string để nhập tên Scene ngoài Inspector
    [SerializeField] private string mainMapName;
    [SerializeField] private string map1Name;
    [SerializeField] private string map2Name;
    [SerializeField] private string Kinhthanhs;
    [SerializeField] private string VeMenu;
    [SerializeField] private AudioClip Click;
    private AudioSource Sfx;
    [SerializeField] private GameObject MainUi;

    private void Start()
    {
        Sfx = GetComponent<AudioSource>();

        // Mặc định cho game chạy bình thường khi vừa vào Scene
        Time.timeScale = 1f;

        if (MainUi != null) MainUi.SetActive(false);
    }

    // Hàm mới: Gọi hàm này để phát âm thanh mà không bị ngắt khi load Scene ngay lập tức
    public void PlayClickSound()
    {
        if (Click != null)
        {
            // Sử dụng PlayClipAtPoint để âm thanh tiếp tục phát mượt mà ngay cả khi chuyển Scene
            AudioSource.PlayClipAtPoint(Click, Camera.main.transform.position);
        }
    }

    // 🎯 HÀM BẬT MENU & TẠM DỪNG GAME
    public void Bat_MainMenu()
    {
        PlayClickSound();
        if (MainUi != null)
        {
            MainUi.SetActive(true);

            // Dừng toàn bộ thời gian trong game
            Time.timeScale = 0f;
        }
    }

    // 🎯 HÀM TẮT MENU & TIẾP TỤC GAME
    public void Resume()
    {
        PlayClickSound();
        if (MainUi != null)
        {
            MainUi.SetActive(false);
        }

        // Khôi phục lại thời gian bình thường cho game
        Time.timeScale = 1f;
    }

    // Hàm chuyển đến Scene MainMap
    public void MainMaps()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(mainMapName);
    }

    // Hàm chuyển đến Scene Map1
    public void Map1s()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(map1Name);
    }

    public void Map2()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(map2Name);
    }

    public void Kinhthanh()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(Kinhthanhs);
    }

    public void VeMenues()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(VeMenu);
    }

    // Hàm chuyển đến Scene Map2
    public void Map2s()
    {
        PlayClickSound();
        Time.timeScale = 1f; // Trả thời gian về 1 trước khi load scene mới
        SceneManager.LoadScene(map2Name);
    }
}