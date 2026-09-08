using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChuyenMapManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH NÚT QUA MAP ---")]
    [Tooltip("Nút bấm dùng để chuyển sang Map mới")]
    public Button nutQuaMap;

    [Header("--- CẤU HÌNH SCENE ---")]
    [Tooltip("Tên Scene Cutscene Thành Công (hoặc Scene Map mới cần load khi ĐỦ điều kiện)")]
    public string tenScenePhaDaThanhCong = "PhaDaThanhCongScene";

    [Tooltip("Tên Scene Map Mới sẽ chuyển đến sau khi xem xong Cutscene Thành Công")]
    public string tenSceneMapMoi = "Map2";

    [Tooltip("Tên Scene chuyển sang khi CHƯA ĐỦ điều kiện qua map (Phá đá thất bại)")]
    public string tenScenePhaDaThatBai = "PhaDaThatBaiScene";

    [Header("--- ĐIỀU KIỆN QUA MAP (NHIỆM VỤ & CẢNH GIỚI) ---")]
    [Tooltip("Danh sách các Quest BẮT BUỘC phải hoàn thành riêng cho cổng/map này")]
    public List<QuestData> danhSachQuestYeuCau = new List<QuestData>();

    [Tooltip("Danh sách ID các Cảnh Giới BẮT BUỘC người chơi phải đột phá để mở chuyển map")]
    public List<string> danhSachCanhGioiYeuCau = new List<string>();

    private void Start()
    {
        if (nutQuaMap != null)
        {
            nutQuaMap.onClick.RemoveAllListeners();
            nutQuaMap.onClick.AddListener(OnClickQuaMap);
            nutQuaMap.interactable = true;
        }
    }

    public bool KiemTraKichHoatQuaMap()
    {
        if (QuestSaveSystem.Instance == null) return false;

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

    public void OnClickQuaMap()
    {
        // 🎯 1. BẤM CHUYỂN MAP THÌ LUÔN LƯU TÊN MAP HIỆN TẠI VÀO 'tenMapTruocDo'
        if (QuestSaveSystem.Instance != null)
        {
            string mapHienTai = SceneManager.GetActiveScene().name;
            QuestSaveSystem.Instance.LuuMapTruocDo(mapHienTai);
        }

        // TRƯỜNG HỢP 1: ĐỦ ĐIỀU KIỆN -> LƯU MAP MỚI VÀ CHUYỂN SANG CUTSCENE THÀNH CÔNG
        if (KiemTraKichHoatQuaMap())
        {
            if (QuestSaveSystem.Instance != null && !string.IsNullOrEmpty(tenSceneMapMoi))
            {
                // Lưu sẵn tên Map mới vào file save để Timeline Cutscene đọc sau
                QuestSaveSystem.Instance.LuuMapMoiTiepTheo(tenSceneMapMoi);
            }

            if (!string.IsNullOrEmpty(tenScenePhaDaThanhCong))
            {
                Debug.Log("<color=green>[Map Manager]</color> Đã đủ điều kiện! Đang chuyển sang Cutscene Thành Công: " + tenScenePhaDaThanhCong);
                SceneManager.LoadScene(tenScenePhaDaThanhCong);
            }
        }
        // TRƯỜNG HỢP 2: CHƯA ĐỦ ĐIỀU KIỆN -> CHUYỂN QUA CUTSCENE THẤT BẠI
        else
        {
            if (!string.IsNullOrEmpty(tenScenePhaDaThatBai))
            {
                Debug.Log("<color=red>[Map Manager]</color> Chưa đủ điều kiện! Đang chuyển sang Cutscene Thất Bại: " + tenScenePhaDaThatBai);
                SceneManager.LoadScene(tenScenePhaDaThatBai);
            }
        }
    }
}