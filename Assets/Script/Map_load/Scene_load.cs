using UnityEngine;
using UnityEngine.UI;
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

    [Header("--- CẤU HÌNH UI & BUTTON ---")]
    [SerializeField] private GameObject MainUi;

    [Tooltip("Button Start Game (Chỉ cấu hình tự động khi TICK isMainMenu)")]
    [SerializeField] private Button btnStartGame;

    [Tooltip("Button Tiếp Tục Game (Sẽ bị mờ nếu không có file save)")]
    [SerializeField] private Button btnTiepTucGame;

    [Tooltip("Game Object UI Xoahaykhong (Bật/Tắt qua Button)")]
    [SerializeField] private GameObject xoahaykhong;

    [SerializeField] private GameObject CheDo;

    [Tooltip("Game Object UI Khởi Đầu Lần Nữa (Bật khi bấm Start mà đã có Save)")]
    [SerializeField] private GameObject khoiDauLanNua;

    private void Start()
    {
        Sfx = GetComponent<AudioSource>();

        // Mặc định cho game chạy bình thường khi vừa vào Scene
        Time.timeScale = 1f;

        // Mặc định ẩn các UI khi bắt đầu game
        if (MainUi != null) MainUi.SetActive(false);
        if (xoahaykhong != null) xoahaykhong.SetActive(false);
        if (CheDo != null) CheDo.SetActive(false);
        if (khoiDauLanNua != null) khoiDauLanNua.SetActive(false);

        // 🎯 CHỈ CẤU HÌNH BUTTON START KHI ĐÂY LÀ MAIN MENU
        if (isMainMenu)
        {
            CauHinhButtonStartGame();
        }

        // 🎯 KIỂM TRA VÀ CẬP NHẬT TRẠNG THÁI NÚT TIẾP TỤC GAME NGAY KHI VÀO GAME
        CapNhatTrangThaiButtonTiepTuc();
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

    /// <summary>
    /// Thuật toán kiểm tra và làm mờ/kích hoạt Nút Tiếp Tục Game dựa vào File Save
    /// </summary>
    public void CapNhatTrangThaiButtonTiepTuc()
    {
        if (btnTiepTucGame != null)
        {
            // Kiểm tra tiến trình
            bool coSaveValid = KiemTraCoSaveFileTienTrinh();

            // Gán interactable = true (sáng/click được) hoặc false (mờ/không click được)
            btnTiepTucGame.interactable = coSaveValid;
        }
    }

    /// <summary>
    /// Thuật toán cấu hình Button Start Game ở Main Menu dựa vào file save
    /// </summary>
    private void CauHinhButtonStartGame()
    {
        if (btnStartGame != null)
        {
            // Xóa sạch sự kiện gán thủ công từ Inspector để tránh bị gọi 2 lần
            btnStartGame.onClick.RemoveAllListeners();

            // Đăng ký lại sự kiện bấm nút StartGame
            btnStartGame.onClick.AddListener(StartGame);
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

        // Cập nhật lại nút Tiếp tục phòng trường hợp dữ liệu vừa thay đổi
        CapNhatTrangThaiButtonTiepTuc();

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

    public void Bat_UiCheDo()
    {
        PlayClickSound();
        if (CheDo != null)
        {
            CheDo.SetActive(true);
        }
    }

    // 🎯 HÀM BẬT / TẮT UI KHỞI ĐẦU LẦN NỮA
    public void Mo_KhoiDauLanNua()
    {
        PlayClickSound();
        if (khoiDauLanNua != null)
        {
            khoiDauLanNua.SetActive(true);
        }
    }

    public void Dong_KhoiDauLanNua()
    {
        PlayClickSound();
        if (khoiDauLanNua != null)
        {
            khoiDauLanNua.SetActive(false);
        }
    }

    // =========================================================
    // 🎯 CHỨC NĂNG: XỬ LÝ NÚT START GAME (DÙNG Ở MAIN MENU)
    // =========================================================

    /// <summary>
    /// Hàm xử lý khi bấm nút Start Game ở Main Menu
    /// </summary>
    public void StartGame()
    {
        PlayClickSound();

        // Kiểm tra xem người chơi đã có dữ liệu tiến trình thực sự hay chưa
        if (KiemTraCoSaveFileTienTrinh())
        {
            // Có save cũ đã chơi -> Bật UI "Khởi Đầu Lần Nữa"
            Mo_KhoiDauLanNua();
        }
        else
        {
            // Chưa chơi hoặc Save trắng -> Vào thẳng Main Scene / Map khởi đầu
            ThucHienResetSaveVaStartGame();
        }
    }

    /// <summary>
    /// Thuật toán kiểm tra người chơi đã có TIẾN TRÌNH THỰC SỰ trong game hay chưa
    /// </summary>
    private bool KiemTraCoSaveFileTienTrinh()
    {
        if (QuestSaveSystem.Instance != null)
        {
            // Tải dữ liệu từ file TXT vào bộ nhớ
            QuestSaveSystem.Instance.LoadDuLieuQuestFromTxt();

            var saveClass = QuestSaveSystem.Instance.duLieuSaveHienTai;
            if (saveClass != null)
            {
                // 1. Kiểm tra danh sách Quest: Nếu đã từng nhận/ghi nhận bất kỳ Quest nào
                if (saveClass.danhSachProgress != null && saveClass.danhSachProgress.Count > 0)
                {
                    return true;
                }

                // 2. Kiểm tra chỉ số người chơi PlayerStats: Đã có EXP, Level > 1 hoặc Đột phá cảnh giới
                if (saveClass.playerStats != null)
                {
                    if (saveClass.playerStats.currentExp > 0f || saveClass.playerStats.level > 1f)
                    {
                        return true;
                    }

                    if (saveClass.playerStats.danhSachCanhGioiDaDotPha != null && saveClass.playerStats.danhSachCanhGioiDaDotPha.Count > 0)
                    {
                        return true;
                    }
                }

                // 3. Kiểm tra Kho Đồ Item: Đã có vật phẩm lưu chưa
                if (saveClass.danhSachItemSave != null && saveClass.danhSachItemSave.Count > 0)
                {
                    return true;
                }

                // 4. Kiểm tra Kỹ Năng Skill: Đã học skill chưa
                if (saveClass.danhSachSkillSave != null && saveClass.danhSachSkillSave.Count > 0)
                {
                    return true;
                }

                // 5. Kiểm tra điểm đạo đức
                if (saveClass.moralStats != null)
                {
                    if (saveClass.moralStats.diemThien > 0 || saveClass.moralStats.diemAc > 0 || saveClass.moralStats.diemDanhVong > 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false; // Nếu chỉ là Save trắng chưa có tiến trình -> Trả về false
    }

    /// <summary>
    /// Thực hiện chuyển tới Main Scene / Scene khởi đầu và XÓA sạch file save cũ
    /// </summary>
    public void ThucHienResetSaveVaStartGame()
    {
        Time.timeScale = 1f;

        // Nếu xác nhận bắt đầu lại từ đầu -> Xóa toàn bộ file Save cũ
        if (QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.XoaToanBoSaveData();
        }

        // Đóng các UI cảnh báo nếu đang mở
        Dong_Xoahaykhong();
        Dong_KhoiDauLanNua();

        // Cập nhật lại nút Tiếp tục thành mờ
        CapNhatTrangThaiButtonTiepTuc();

        // Chuyển tới Scene khởi đầu đã gán tên
        string targetScene = !string.IsNullOrEmpty(map1Name) ? map1Name : mainMapName;
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("[Scene_load] Chưa gán tên Scene khởi đầu trong Inspector (map1Name hoặc mainMapName)!");
        }
    }

    // 🎯 HÀM NÚT TIẾP TỤC (CONTINUE GAME TỪ FILE SAVE)
    public void TiepTucGame()
    {
        // Kiểm tra an toàn: Nếu nút đang mờ thì không phản hồi
        if (btnTiepTucGame != null && !btnTiepTucGame.interactable) return;

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