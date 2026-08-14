using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChuyenMapManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH NÚT QUA MAP ---")]
    [Tooltip("Nút bấm dùng để chuyển sang Map mới")]
    public Button nutQuaMap;

    [Tooltip("Tên Scene map mới cần load khi bấm nút")]
    public string tenSceneMapMoi = "Map2";

    [Header("--- TÙY CHỌN MÀU SẮC KHI MỜ (TÙY CHỌN) ---")]
    [Tooltip("Độ trong suốt khi nút bị khóa (mờ)")]
    [Range(0.1f, 1f)]
    public float doMoKhiKhoa = 0.4f;

    private CanvasGroup canvasGroupNut;

    private void Start()
    {
        if (nutQuaMap != null)
        {
            // Lấy hoặc tự thêm CanvasGroup để điều khiển độ mờ
            canvasGroupNut = nutQuaMap.GetComponent<CanvasGroup>();
            if (canvasGroupNut == null)
            {
                canvasGroupNut = nutQuaMap.gameObject.AddComponent<CanvasGroup>();
            }

            // Đăng ký sự kiện Click cho nút qua map
            nutQuaMap.onClick.RemoveAllListeners();
            nutQuaMap.onClick.AddListener(OnClickQuaMap);
        }
    }

    private void Update()
    {
        CapNhatTrangThaiNutQuaMap();
    }

    // 🎯 HÀM CẬP NHẬT TRẠNG THÁI NÚT DỰA VÀO BIẾN Complete BÊN QUESTUIMANAGER
    private void CapNhatTrangThaiNutQuaMap()
    {
        if (nutQuaMap == null) return;

        bool isComplete = QuestUIManager.Instance != null && QuestUIManager.Instance.Complete;

        // Bật / Tắt khả năng tương tác của Button
        nutQuaMap.interactable = isComplete;

        // Chỉnh độ mờ (Alpha) của Button
        if (canvasGroupNut != null)
        {
            canvasGroupNut.alpha = isComplete ? 1f : doMoKhiKhoa;
        }
    }

    // 🎯 HÀM BẤM NÚT CHUYỂN SCENE
    public void OnClickQuaMap()
    {
        if (QuestUIManager.Instance != null && QuestUIManager.Instance.Complete)
        {
            Debug.Log("<color=green>[Map Manager]</color> Đã hoàn thành tất cả nhiệm vụ! Đang chuyển sang Scene: " + tenSceneMapMoi);
            SceneManager.LoadScene(tenSceneMapMoi);
        }
        else
        {
            Debug.LogWarning("[Map Manager] Chưa hoàn thành tất cả nhiệm vụ, không thể qua map!");
        }
    }
}