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
            ChuyenVeMapTruocDo();
        }
    }

    // 🎯 HÀM LẤY TÊN MAP TRƯỚC ĐÓ TỪ FILE SAVE VÀ CHUYỂN VỀ
    private void ChuyenVeMapTruocDo()
    {
        if (QuestSaveSystem.Instance != null)
        {
            string tenMapTruocDo = QuestSaveSystem.Instance.LayMapTruocDo();

            if (!string.IsNullOrEmpty(tenMapTruocDo))
            {
                Debug.Log("<color=green>[Timeline Changer]</color> Hết Cutscene! Đang chuyển trở về Map cũ từ Save File: " + tenMapTruocDo);
                SceneManager.LoadScene(tenMapTruocDo);
            }
        }
        else
        {
            Debug.LogError("[Timeline Changer] Không tìm thấy QuestSaveSystem Instance!");
        }
    }
}