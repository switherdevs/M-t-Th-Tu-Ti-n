using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChuyenMapManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH NÚT QUA MAP ---")]
    [Tooltip("Nút bấm dùng để chuyển sang Map mới")]
    public Button nutQuaMap;

    [Tooltip("Tên Scene map mới cần load khi bấm nút (có thể để trống nếu chỉ muốn ẩn nút)")]
    public string tenSceneMapMoi = "Map2";

    [Header("--- TÙY CHỌN ẨN NÚT KHI HOÀN THÀNH ---")]
    [Tooltip("Nếu tích chọn, khi hoàn thành nhiệm vụ nút sẽ biến mất thay vì chuyển map")]
    public bool anNutKhiXong = false;

    [Header("--- ĐIỀU KIỆN QUA MAP (NHIỆM VỤ YÊU CẦU) ---")]
    [Tooltip("Danh sách các Quest BẮT BUỘC phải hoàn thành riêng cho cổng/map này")]
    public List<QuestData> danhSachQuestYeuCau = new List<QuestData>();

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

    // 🎯 HÀM KIỂM TRA XEM TẤT CẢ QUEST YÊU CẦU CHO MAP NÀY ĐÃ HOÀN THÀNH CHƯA
    public bool KiemTraKichHoatQuaMap()
    {
        // Nếu không cài đặt nhiệm vụ yêu cầu nào -> Mặc định đã đủ điều kiện
        if (danhSachQuestYeuCau == null || danhSachQuestYeuCau.Count == 0)
        {
            return true;
        }

        foreach (QuestData quest in danhSachQuestYeuCau)
        {
            if (quest != null)
            {
                // Tra cứu tiến trình quest trực tiếp từ QuestSaveSystem
                ProgressQuest progress = QuestSaveSystem.Instance != null
                    ? QuestSaveSystem.Instance.LayTienTrinhQuest(quest.idQuest)
                    : null;

                // Nếu có dù chỉ 1 quest chưa hoàn thành -> Chưa đủ điều kiện
                if (progress == null || progress.trangThai != TrangThaiQuest.HoanThanh)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 🎯 HÀM CẬP NHẬT TRẠNG THÁI NÚT (KHÓA / MỜ / ẨN NÚT)
    private void CapNhatTrangThaiNutQuaMap()
    {
        if (nutQuaMap == null) return;

        bool duDieuKienQuaMap = KiemTraKichHoatQuaMap();

        // 🎯 XỬ LÝ KHI TÍCH BIẾN "ẨN NÚT KHI XONG"
        if (anNutKhiXong && duDieuKienQuaMap)
        {
            // Nếu đã hoàn thành nhiệm vụ và biến anNutKhiXong = true -> Biến mất nút
            if (nutQuaMap.gameObject.activeSelf)
            {
                nutQuaMap.gameObject.SetActive(false);
                Debug.Log("<color=yellow>[Map Manager]</color> Đã hoàn thành nhiệm vụ, nút đã tự động biến mất!");
            }
            return;
        }

        // Đảm bảo nút được hiện nếu chưa đủ điều kiện hoặc không bật anNutKhiXong
        if (!nutQuaMap.gameObject.activeSelf)
        {
            nutQuaMap.gameObject.SetActive(true);
        }

        // Bật / Tắt khả năng tương tác của Button
        nutQuaMap.interactable = duDieuKienQuaMap;

        // Chỉnh độ mờ (Alpha) của Button
        if (canvasGroupNut != null)
        {
            canvasGroupNut.alpha = duDieuKienQuaMap ? 1f : doMoKhiKhoa;
        }
    }

    // 🎯 HÀM BẤM NÚT CHUYỂN SCENE
    public void OnClickQuaMap()
    {
        if (KiemTraKichHoatQuaMap())
        {
            // Nếu biến anNutKhiXong được tích thì ưu tiên ẩn nút
            if (anNutKhiXong)
            {
                nutQuaMap.gameObject.SetActive(false);
                Debug.Log("<color=yellow>[Map Manager]</color> Đã nhấn nút! Nút biến mất thay vì chuyển map.");
                return;
            }

            // Nếu không chọn ẩn nút và có điền tên Scene thì mới chuyển Map
            if (!string.IsNullOrEmpty(tenSceneMapMoi))
            {
                Debug.Log("<color=green>[Map Manager]</color> Đã hoàn thành tất cả nhiệm vụ yêu cầu! Đang chuyển sang Scene: " + tenSceneMapMoi);
                SceneManager.LoadScene(tenSceneMapMoi);
            }
            else
            {
                Debug.LogWarning("[Map Manager] Chưa thiết lập tên Scene cần chuyển!");
            }
        }
        else
        {
            Debug.LogWarning("[Map Manager] Chưa hoàn thành các nhiệm vụ yêu cầu của map này, không thể qua map!");
        }
    }
}