using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineSceneChanger : MonoBehaviour
{
    [Header("--- CẤU HÌNH TIMELINE ---")]
    [Tooltip("PlayableDirector chứa Timeline đang chạy")]
    public PlayableDirector timelineDirector;

    [Tooltip("Mốc thời gian (tính bằng giây) để thực hiện chuyển Scene")]
    public float mocThoiGianChuyenScene = 5.0f;

    [Header("--- LOẠI CUTSCENE ---")]
    [Tooltip("TÍCH VÀO NẾU ĐÂY LÀ CUTSCENE THÀNH CÔNG (Sẽ chuyển đến Map Mới Tiếp Theo). KHÔNG TÍCH NẾU LÀ CUTSCENE THẤT BẠI (Sẽ quay lại Map Cũ).")]
    public bool isCutsceneThanhCong = false;

    private bool daChuyenScene = false;

    private void Start()
    {
        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }
    }

    private void Update()
    {
        KiemTraThoiGianTimeline();
    }

    private void KiemTraThoiGianTimeline()
    {
        if (daChuyenScene || timelineDirector == null) return;

        if (timelineDirector.time >= mocThoiGianChuyenScene)
        {
            daChuyenScene = true;
            XuLyChuyenSceneSauCutscene();
        }
    }

    // 🎯 HÀM ĐIỀU HƯỚNG CHUYỂN SCENE TÙY THEO LOẠI CUTSCENE
    private void XuLyChuyenSceneSauCutscene()
    {
        if (QuestSaveSystem.Instance == null)
        {
            Debug.LogError("[Timeline Changer] Không tìm thấy QuestSaveSystem Instance!");
            return;
        }

        // CẢNH THÀNH CÔNG -> ĐỌC VÀ CHUYỂN SANG MAP MỚI
        if (isCutsceneThanhCong)
        {
            string tenMapMoi = QuestSaveSystem.Instance.LayMapMoiTiepTheo();

            if (!string.IsNullOrEmpty(tenMapMoi))
            {
                Debug.Log("<color=green>[Timeline Changer]</color> Thăng cấp thành công! Đang tiến vào Map Mới: " + tenMapMoi);
                SceneManager.LoadScene(tenMapMoi);
            }
        }
        // CẢNH THẤT BẠI -> ĐỌC VÀ QUAY LẠI MAP CỦ
        else
        {
            string tenMapCu = QuestSaveSystem.Instance.LayMapTruocDo();

            if (!string.IsNullOrEmpty(tenMapCu))
            {
                Debug.Log("<color=yellow>[Timeline Changer]</color> Thất bại! Đang đưa người chơi quay về Map Cũ: " + tenMapCu);
                SceneManager.LoadScene(tenMapCu);
            }
        }
    }
}