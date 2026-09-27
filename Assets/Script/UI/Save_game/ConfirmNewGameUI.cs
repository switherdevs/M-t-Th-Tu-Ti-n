using UnityEngine;
using UnityEngine.UI;

public class ConfirmNewGameUI : MonoBehaviour
{
    [Header("=== CẤU HÌNH BUTTON UI ===")]
    [SerializeField, Tooltip("Button Đồng Ý Chơi Mới (Xóa Save Cũ)")]
    private Button btnXacNhan;

    [SerializeField, Tooltip("Button Hủy Bỏ (Không Xóa Save)")]
    private Button btnHuyBo;

    [Header("=== REFERENCE TỚI SCENE LOAD ===")]
    [SerializeField] private Scene_load sceneLoader;

    private void Start()
    {
        // Tự động tìm Scene_load nếu chưa gán trong Inspector (Cập nhật API Unity mới)
        if (sceneLoader == null)
        {
            sceneLoader = FindFirstObjectByType<Scene_load>();
        }

        // Đăng ký sự kiện Click cho 2 nút bấm
        if (btnXacNhan != null)
        {
            btnXacNhan.onClick.RemoveAllListeners();
            btnXacNhan.onClick.AddListener(OnXacNhanNewGame);
        }

        if (btnHuyBo != null)
        {
            btnHuyBo.onClick.RemoveAllListeners();
            btnHuyBo.onClick.AddListener(OnHuyBo);
        }
    }

    /// <summary>
    /// Xử lý khi người chơi bấm nút "Xác Nhận / Chắc Chắn"
    /// </summary>
    private void OnXacNhanNewGame()
    {
        if (sceneLoader != null)
        {
            sceneLoader.PlayClickSound();
            sceneLoader.ThucHienResetSaveVaStartGame();
        }
    }

    /// <summary>
    /// Xử lý khi người chơi bấm nút "Hủy Bỏ"
    /// </summary>
    private void OnHuyBo()
    {
        if (sceneLoader != null)
        {
            sceneLoader.PlayClickSound();
            sceneLoader.Dong_Xoahaykhong();
        }
    }
}