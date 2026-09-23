using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineSceneChanger : MonoBehaviour
{
    // =========================================================
    // CẤU HÌNH CUTSCENE MỞ ĐẦU (TÍNH NĂNG MỚI)
    // =========================================================

    [Header("--- MODE CUTSCENE MỞ ĐẦU ---")]
    [Tooltip("Tích vào đây để bật Cutscene Mở Đầu (Vô hiệu hóa hoàn toàn tính năng cũ)")]
    public bool isCutsceneMoDau = false;

    [Tooltip("Tên Scene cố định sẽ chuyển tới sau khi Timeline Mở Đầu chạy hết")]
    public string tenSceneCoDinh = "KinhThanh";


    // =========================================================
    // CẤU HÌNH TIMELINE & MODE CỦ
    // =========================================================

    [Header("--- CẤU HÌNH TIMELINE ---")]
    [Tooltip("PlayableDirector chứa Timeline đang chạy")]
    public PlayableDirector timelineDirector;

    [Tooltip("Mốc thời gian (tính bằng giây) để thực hiện chuyển Scene (Chỉ dùng khi KHÔNG tích Cutscene Mở Đầu)")]
    public float mocThoiGianChuyenScene = 5.0f;

    [Header("--- LOẠI CUTSCENE CỦ ---")]
    [Tooltip("TÍCH VÀO NẾU ĐÂY LÀ CUTSCENE THÀNH CÔNG (Sẽ chuyển đến Map Mới Tiếp Theo). KHÔNG TÍCH NẾU LÀ CUTSCENE THẤT BẠI (Sẽ quay lại Map Cũ).")]
    public bool isCutsceneThanhCong = false;

    private bool daChuyenScene = false;

    private void Start()
    {
        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }

        // Xử lý đăng ký sự kiện cho Mode Cutscene Mở Đầu
        if (isCutsceneMoDau && timelineDirector != null)
        {
            // Bắt sự kiện khi PlayableDirector chạy hết thời lượng hoàn toàn
            timelineDirector.stopped += OnTimelineEnded;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi leak bộ nhớ
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimelineEnded;
        }
    }

    private void Update()
    {
        // NẾU TÍCH CUTSCENE MỞ ĐẦU -> BỎ QUA HOÀN TOÀN TÍNH NĂNG CỦ BÊN DƯỚI
        if (isCutsceneMoDau) return;

        KiemTraThoiGianTimeline();
    }

    // 🎯 SỰ KIỆN TỰ ĐỘNG GỌI KHI TIMELINE MỞ ĐẦU CHẠY HẾT CẢNH
    private void OnTimelineEnded(PlayableDirector director)
    {
        if (daChuyenScene) return;

        if (!string.IsNullOrEmpty(tenSceneCoDinh))
        {
            daChuyenScene = true;
            Debug.Log("<color=cyan>[Timeline Changer]</color> Cutscene mở đầu kết thúc! Đang chuyển tới Scene cố định: " + tenSceneCoDinh);
            ThucHienChuyenSceneAnToan(tenSceneCoDinh);
        }
        else
        {
            Debug.LogError("[Timeline Changer] Chưa điền tên 'tenSceneCoDinh' trên Inspector!");
        }
    }

    // 🎯 TÍNH NĂNG CỦ: ĐẾM THỜI GIAN THEO MỐC VÀ ĐỌC MAP TỪ QUESTSAVESYSTEM
    private void KiemTraThoiGianTimeline()
    {
        if (daChuyenScene || timelineDirector == null) return;

        if (timelineDirector.time >= mocThoiGianChuyenScene)
        {
            daChuyenScene = true;
            XuLyChuyenSceneSauCutscene();
        }
    }

    // 🎯 HÀM ĐIỀU HƯỚNG CHUYỂN SCENE CỦ (TÙY THEO THẮNG/THUA)
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
                ThucHienChuyenSceneAnToan(tenMapMoi);
            }
        }
        // CẢNH THẤT BẠI -> ĐỌC VÀ QUAY LẠI MAP CỦ
        else
        {
            string tenMapCu = QuestSaveSystem.Instance.LayMapTruocDo();

            if (!string.IsNullOrEmpty(tenMapCu))
            {
                Debug.Log("<color=yellow>[Timeline Changer]</color> Thất bại! Đang đưa người chơi quay về Map Cũ: " + tenMapCu);
                ThucHienChuyenSceneAnToan(tenMapCu);
            }
        }
    }

    // 🎯 THUẬT TOÁN MỚI: KIỂM TRA SCENE TRƯỚC KHI LOAD TRÁNH CRASH
    private void ThucHienChuyenSceneAnToan(string tenScene)
    {
        if (KiemTraSceneCoTrongBuild(tenScene))
        {
            SceneManager.LoadScene(tenScene);
        }
        else
        {
            Debug.LogError($"[Timeline Changer] LỖI: Scene tên '{tenScene}' KHÔNG TÌM THẤY trong Build Profiles! " +
                           $"Hãy kiểm tra lại tên file Scene hoặc gõ đúng chuỗi tên trong QuestSaveSystem.");
        }
    }

    // Thuật toán duyệt danh sách Build Profile
    private bool KiemTraSceneCoTrongBuild(string tenScene)
    {
        int soLuongScene = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < soLuongScene; i++)
        {
            string duongDanScene = SceneUtility.GetScenePathByBuildIndex(i);
            string tenSceneTrongBuild = System.IO.Path.GetFileNameWithoutExtension(duongDanScene);

            if (tenSceneTrongBuild.Equals(tenScene, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}