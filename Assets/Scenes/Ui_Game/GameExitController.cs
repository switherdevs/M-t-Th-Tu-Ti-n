using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameExitController : MonoBehaviour
{
    [Header("--- TÙY CHỌN PHÍM TẮT ---")]
    [SerializeField] private bool suDungPhimTat = false;
    [SerializeField] private KeyCode phimThoatGame = KeyCode.Escape;

    private void Update()
    {
        if (suDungPhimTat && Input.GetKeyDown(phimThoatGame))
        {
            ThoatGame();
        }
    }

    // Gán hàm này vào sự kiện OnClick() của Button Thoát Game trên UI
    public void ThoatGame()
    {
        // Đảm bảo thời gian game trở về bình thường trước khi thoát
        Time.timeScale = 1f;

        Debug.Log("<color=yellow>[Game Exit]</color> Đang thoát game...");

#if UNITY_EDITOR
        // Dừng Play Mode khi đang chạy thử trong Unity Editor
        EditorApplication.isPlaying = false;
#else
        // Thoát ứng dụng khi đã build thành game hoàn chỉnh
        Application.Quit();
#endif
    }
}