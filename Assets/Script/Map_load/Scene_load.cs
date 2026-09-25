using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_load : MonoBehaviour
{
    [Header("--- CẤU HÌNH LOẠI SCENE ---")]
    [Tooltip("Tick chọn nếu Scene này là Main Menu (Mở UI sẽ KHÔNG pause game)")]
    [SerializeField] private bool isMainMenu = false;

    [Header("--- CẤU HÌNH TÊN SCENE ---")]
    [SerializeField] private string mainMapName;
    [SerializeField] private string map1Name;
    [SerializeField] private string map2Name;
    [SerializeField] private string Kinhthanhs;
    [SerializeField] private string VeMenu;

    [Header("--- CẤU HÌNH ÂM THANH ---")]
    [SerializeField] private AudioClip Click;
    private AudioSource Sfx;

    [Header("--- CẤU HÌNH UI ---")]
    [SerializeField] private GameObject MainUi;
    [Tooltip("Game Object UI Xoahaykhong (Bật/Tắt qua Button)")]
    [SerializeField] private GameObject xoahaykhong;

    private void Start()
    {
        Sfx = GetComponent<AudioSource>();

        // Mặc định cho game chạy bình thường khi vừa vào Scene
        Time.timeScale = 1f;

        // Mặc định ẩn các UI khi bắt đầu game
        if (MainUi != null) MainUi.SetActive(false);
        if (xoahaykhong != null) xoahaykhong.SetActive(false);
    }

    private void Update()
    {
        // Phím tắt ESC để bật/tắt Main UI nhanh
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MainUi != null)
            {
                if (MainUi.activeSelf)
                {
                    Resume();
                }
                else
                {
                    Bat_MainMenu();
                }
            }
        }
    }

    // Phát âm thanh Click mượt mà khi chuyển Scene
    public void PlayClickSound()
    {
        if (Click != null)
        {
            AudioSource.PlayClipAtPoint(Click, Camera.main.transform.position);
        }
    }

    // 🎯 HÀM BẬT MENU (Xử lý theo biến isMainMenu)
    public void Bat_MainMenu()
    {
        PlayClickSound();
        if (MainUi != null)
        {
            MainUi.SetActive(true);

            // Kiểm tra: Nếu là Main Menu thì không pause game (timeScale = 1), ngược lại pause game (timeScale = 0)
            if (isMainMenu)
            {
                Time.timeScale = 1f;
            }
            else
            {
                Time.timeScale = 0f;
            }
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

    // 🎯 HÀM TẮT/BẬT GAME OBJECT "XOA HAY KHONG" (Gán vào Button)
    public void Bat_Tat_Xoahaykhong()
    {
        PlayClickSound();
        if (xoahaykhong != null)
        {
            // Tự động đảo ngược trạng thái (Đang bật -> Tắt, Đang tắt -> Bật)
            bool trangThaiHienTai = xoahaykhong.activeSelf;
            xoahaykhong.SetActive(!trangThaiHienTai);
        }
    }

    // Hàm mở trực tiếp UI Xoahaykhong
    public void Mo_Xoahaykhong()
    {
        PlayClickSound();
        if (xoahaykhong != null)
        {
            xoahaykhong.SetActive(true);
        }
    }

    // Hàm đóng trực tiếp UI Xoahaykhong
    public void Dong_Xoahaykhong()
    {
        PlayClickSound();
        if (xoahaykhong != null)
        {
            xoahaykhong.SetActive(false);
        }
    }

    // 🎯 HÀM NÚT TIẾP TỤC (CONTINUE GAME TỪ FILE SAVE)
    public void TiepTucGame()
    {
        PlayClickSound();
        Time.timeScale = 1f;

        if (QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.LoadDuLieuQuestFromTxt();

            string mapCanDen = QuestSaveSystem.Instance.LayMapMoiTiepTheo();

            if (!string.IsNullOrEmpty(mapCanDen))
            {
                SceneManager.LoadScene(mapCanDen);
            }
            else
            {
                SceneManager.LoadScene(mainMapName);
            }
        }
        else
        {
            Debug.LogWarning("[Scene_load] Không tìm thấy QuestSaveSystem Singleton! Load map mặc định.");
            SceneManager.LoadScene(mainMapName);
        }
    }

    // Hàm chuyển đến Scene MainMap
    public void MainMaps()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMapName);
    }

    // Hàm chuyển đến Scene Map1
    public void Map1s()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(map1Name);
    }

    public void Map2()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(map2Name);
    }

    public void Kinhthanh()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(Kinhthanhs);
    }

    public void VeMenues()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(VeMenu);
    }

    // Hàm chuyển đến Scene Map2
    public void Map2s()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(map2Name);
    }
}