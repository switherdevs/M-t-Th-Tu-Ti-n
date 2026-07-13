using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_load : MonoBehaviour
{
    // Đổi từ kiểu Scene sang string để nhập tên Scene ngoài Inspector
    [SerializeField] private string mainMapName;
    [SerializeField] private string map1Name;
    [SerializeField] private string map2Name;

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

    // Hàm chuyển đến Scene Map2
    public void Map2s()
    {
        SceneManager.LoadScene(map2Name);
    }
}