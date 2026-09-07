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
    [Tooltip("Tên Scene map mới cần load khi ĐỦ điều kiện qua map")]
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
        // 🎯 LƯU LẠI TÊN SCENE MAP HIỆN TẠI VÀO SAVE SYSTEM TRƯỚC KHI CHUYỂN
        if (QuestSaveSystem.Instance != null)
        {
            string mapHienTai = SceneManager.GetActiveScene().name;
            QuestSaveSystem.Instance.LuuMapTruocDo(mapHienTai);
        }

        // TRƯỜNG HỢP 1: ĐỦ ĐIỀU KIỆN -> CHUYỂN QUA MAP MỚI
        if (KiemTraKichHoatQuaMap())
        {
            if (!string.IsNullOrEmpty(tenSceneMapMoi))
            {
                Debug.Log("<color=green>[Map Manager]</color> Đã đủ điều kiện! Đang chuyển sang Scene Map mới: " + tenSceneMapMoi);
                SceneManager.LoadScene(tenSceneMapMoi);
            }
        }
        // TRƯỜNG HỢP 2: CHƯA ĐỦ ĐIỀU KIỆN -> CHUYỂN QUA SCENE PHÁ ĐÁ THẤT BẠI
        else
        {
            if (!string.IsNullOrEmpty(tenScenePhaDaThatBai))
            {
                Debug.Log("<color=red>[Map Manager]</color> Chưa đủ điều kiện! Đang chuyển sang Scene Phá Đá Thất Bại: " + tenScenePhaDaThatBai);
                SceneManager.LoadScene(tenScenePhaDaThatBai);
            }
        }
    }
}