using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChuyenMapManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH NÚT QUA MAP ---")]
    [Tooltip("Nút bấm dùng để chuyển sang Map mới")]
    public Button nutQuaMap;

    [Tooltip("GameObject của nút bấm (hoặc Panel/UI chứa nút) sẽ bị ẩn biến mất hoàn toàn")]
    [SerializeField] private GameObject objectNutCanAn;

    [Tooltip("Tên Scene map mới cần load khi bấm nút (có thể để trống nếu chỉ muốn ẩn nút)")]
    public string tenSceneMapMoi = "Map2";

    [Header("--- TÙY CHỌN ẨN NÚT KHI HOÀN THÀNH ---")]
    [Tooltip("Nếu tích chọn, khi hoàn thành điều kiện GameObject nút sẽ biến mất hoàn toàn")]
    public bool anNutKhiXong = false;

    [Header("--- ĐIỀU KIỆN QUA MAP (NHIỆM VỤ & CẢNH GIỚI) ---")]
    [Tooltip("Danh sách các Quest BẮT BUỘC phải hoàn thành riêng cho cổng/map này")]
    public List<QuestData> danhSachQuestYeuCau = new List<QuestData>();

    [Tooltip("Danh sách ID các Cảnh Giới BẮT BUỘC người chơi phải đột phá để mở chuyển map")]
    public List<string> danhSachCanhGioiYeuCau = new List<string>();

    [Header("--- TÙY CHỌN MÀU SẮC KHI MỜ (TÙY CHỌN) ---")]
    [Tooltip("Độ trong suốt khi nút bị khóa (mờ)")]
    [Range(0.1f, 1f)]
    public float doMoKhiKhoa = 0.4f;

    private CanvasGroup canvasGroupNut;

    private void Start()
    {
        if (objectNutCanAn == null && nutQuaMap != null)
        {
            objectNutCanAn = nutQuaMap.gameObject;
        }

        if (nutQuaMap != null)
        {
            canvasGroupNut = nutQuaMap.GetComponent<CanvasGroup>();
            if (canvasGroupNut == null)
            {
                canvasGroupNut = nutQuaMap.gameObject.AddComponent<CanvasGroup>();
            }

            nutQuaMap.onClick.RemoveAllListeners();
            nutQuaMap.onClick.AddListener(OnClickQuaMap);
        }
    }

    private void Update()
    {
        CapNhatTrangThaiNutQuaMap();
    }

    // 🎯 HÀM KIỂM TRA ĐIỀU KIỆN QUA MAP (QUEST & CẢNH GIỚI)
    public bool KiemTraKichHoatQuaMap()
    {
        if (QuestSaveSystem.Instance == null) return false;

        // 1. Kiểm tra toàn bộ Quest yêu cầu
        if (danhSachQuestYeuCau != null && danhSachQuestYeuCau.Count > 0)
        {
            foreach (QuestData quest in danhSachQuestYeuCau)
            {
                if (quest != null)
                {
                    ProgressQuest progress = QuestSaveSystem.Instance.LayTienTrinhQuest(quest.idQuest);
                    if (progress == null || progress.trangThai != TrangThaiQuest.HoanThanh)
                    {
                        return false;
                    }
                }
            }
        }

        // 2. Kiểm tra toàn bộ Cảnh Giới ID yêu cầu
        if (danhSachCanhGioiYeuCau != null && danhSachCanhGioiYeuCau.Count > 0)
        {
            foreach (string idCanhGioi in danhSachCanhGioiYeuCau)
            {
                if (!string.IsNullOrEmpty(idCanhGioi))
                {
                    bool daDat = QuestSaveSystem.Instance.KiemTraDaDatCanhGioi(idCanhGioi);
                    if (!daDat)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    // 🎯 HÀM CẬP NHẬT TRẠNG THÁI NÚT
    private void CapNhatTrangThaiNutQuaMap()
    {
        bool duDieuKienQuaMap = KiemTraKichHoatQuaMap();

        if (anNutKhiXong && duDieuKienQuaMap)
        {
            if (objectNutCanAn != null && objectNutCanAn.activeSelf)
            {
                objectNutCanAn.SetActive(false);
                Debug.Log("<color=yellow>[Map Manager]</color> Đã đủ điều kiện, GameObject nút đã biến mất!");
            }
            return;
        }

        if (objectNutCanAn != null && !objectNutCanAn.activeSelf)
        {
            objectNutCanAn.SetActive(true);
        }

        if (nutQuaMap != null)
        {
            nutQuaMap.interactable = duDieuKienQuaMap;

            if (canvasGroupNut != null)
            {
                canvasGroupNut.alpha = duDieuKienQuaMap ? 1f : doMoKhiKhoa;
            }
        }
    }

    // 🎯 HÀM BẤM NÚT CHUYỂN SCENE
    public void OnClickQuaMap()
    {
        if (KiemTraKichHoatQuaMap())
        {
            if (anNutKhiXong)
            {
                if (objectNutCanAn != null)
                {
                    objectNutCanAn.SetActive(false);
                }
                Debug.Log("<color=yellow>[Map Manager]</color> Đã nhấn nút! GameObject đã biến mất hoàn toàn.");
                return;
            }

            if (!string.IsNullOrEmpty(tenSceneMapMoi))
            {
                Debug.Log("<color=green>[Map Manager]</color> Đã đủ điều kiện cảnh giới & quest! Đang chuyển sang Scene: " + tenSceneMapMoi);
                SceneManager.LoadScene(tenSceneMapMoi);
            }
            else
            {
                Debug.LogWarning("[Map Manager] Chưa thiết lập tên Scene cần chuyển!");
            }
        }
        else
        {
            Debug.LogWarning("[Map Manager] Chưa đạt đủ Cảnh Giới hoặc chưa hoàn thành Quest yêu cầu!");
        }
    }
}