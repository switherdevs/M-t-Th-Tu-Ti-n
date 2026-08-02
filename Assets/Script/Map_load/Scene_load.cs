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

    // Hàm chuyển đến Scene MainMap
    public void MainMaps()
    {
        PlayClickSound();
        SceneManager.LoadScene(mainMapName);
    }

    // Hàm chuyển đến Scene Map1
    public void Map1s()
    {
        PlayClickSound();
        SceneManager.LoadScene(map1Name);
    }

    public void Map2()
    {
        PlayClickSound();
        SceneManager.LoadScene(map2Name);
    }

    public void Kinhthanh()
    {
        PlayClickSound();
        SceneManager.LoadScene(Kinhthanhs);
    }

    public void VeMenues()
    {
        PlayClickSound();
        SceneManager.LoadScene(VeMenu);
    }

    public void Resume()
    {
        PlayClickSound();
        MainUi.SetActive(false);
    }

    public void Bat_MainMenu()
    {
        PlayClickSound();
        if (MainUi != null)
        {
            MainUi.SetActive(true);
        }
    }

    // Hàm chuyển đến Scene Map2
    public void Map2s()
    {
        PlayClickSound();
        SceneManager.LoadScene(map2Name);
    }
}