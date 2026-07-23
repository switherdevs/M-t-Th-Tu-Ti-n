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


    [SerializeField] private GameObject MainUi;

    private void Start()
    {
        if(MainUi != null) MainUi.SetActive(false);
    }
    // Hàm chuyển đến Scene MainMap
    public void MainMaps()
    {
        SceneManager.LoadScene(mainMapName);
    }

    // Hàm chuyển đến Scene Map1
    public void Map1s()
    {

        SceneManager.LoadScene(map1Name);
    }
    public void Map2()
    {

        SceneManager.LoadScene(map2Name);
    }
    public void Kinhthanh()
    {

        SceneManager.LoadScene(Kinhthanhs);
    }

    public void VeMenues()
    {

        SceneManager.LoadScene(VeMenu);
    }

    public void Resume()
    {

        MainUi.SetActive(false);
    }

    public void Bat_MainMenu()
    {

        if(MainUi != null)
        {
            MainUi.SetActive(true);

        }
    }

    // Hàm chuyển đến Scene Map2
    public void Map2s()
    {
        SceneManager.LoadScene(map2Name);
    }
}