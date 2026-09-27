using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossSceneTransition : MonoBehaviour
{
    [Header("--- CAU HINH BOSS ---")]
    [SerializeField, Tooltip("Tham chieu toi CharacterStats cua Boss (Can bat tick isBoss)")]
    private CharacterStats bossStats;

    [Header("--- CAU HINH DIEU KIEN DAU DUC ---")]
    [SerializeField, Tooltip("Diem dao duc (Diem Thien) toi thieu de vao Scene Good")]
    private int diemThienToiThieu = 3;

    [Header("--- CAU HINH SCENE ---")]
    [SerializeField, Tooltip("Ten Scene Good (Ket thuc co gia tri/Thien)")]
    private string tenSceneGood = "GoodEndingScene";

    [SerializeField, Tooltip("Ten Scene Bad (Ket thuc te/Ac)")]
    private string tenSceneBad = "BadEndingScene";

    [Header("--- CAU HINH TIMING & UI ---")]
    [SerializeField, Tooltip("Thoi gian delay (giay) truoc khi chuyen Scene")]
    private float thoiGianDelay = 2.0f;

    [SerializeField, Tooltip("GameObject Canvas/Image Fade Out UI se hien len khi Boss chet")]
    private GameObject fadeOutUI;

    private bool daKichHoatChuyenScene = false;

    private void Awake()
    {
        // Dam bao Fade Out UI an luc bat dau
        if (fadeOutUI != null)
        {
            fadeOutUI.SetActive(false);
        }

        // Tu dong tim CharacterStats tren cung GameObject neu chua gán
        if (bossStats == null)
        {
            bossStats = GetComponent<CharacterStats>();
        }
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện nghe khi Boss chết
        if (bossStats != null)
        {
            bossStats.OnDeath += OnBossDie;
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
        if (bossStats != null)
        {
            bossStats.OnDeath -= OnBossDie;
        }
    }

    /// <summary>
    /// Hàm xử lý khi nhận thông báo Boss đã chết từ CharacterStats
    /// </summary>
    private void OnBossDie()
    {
        // Kiểm tra đúng Boss và chưa từng kích hoạt chuyển scene
        if (bossStats != null && bossStats.IsBoss && !daKichHoatChuyenScene)
        {
            daKichHoatChuyenScene = true;
            StartCoroutine(Routine_XuLyChuyenScene());
        }
    }

    /// <summary>
    /// Luồng Coroutine xử lý hiện Fade Out, delay 2 giây và đọc Save File chuyển Scene
    /// </summary>
    private IEnumerator Routine_XuLyChuyenScene()
    {
        // 1. Hiển thị Game Object Fade Out UI
        if (fadeOutUI != null)
        {
            fadeOutUI.SetActive(true);
        }

        // 2. Chờ delay 2 giây
        yield return new WaitForSeconds(thoiGianDelay);

        // 3. Đọc dữ liệu điểm đạo đức từ Save File qua QuestSaveSystem
        int diemThienHienTai = 0;
        if (QuestSaveSystem.Instance != null)
        {
            // Tải/Cập nhật dữ liệu từ file Save txt mới nhất
            QuestSaveSystem.Instance.LoadDuLieuQuestFromTxt();
            diemThienHienTai = QuestSaveSystem.Instance.LayDiemThien();
        }

        // 4. Kiểm tra điều kiện điểm đạo đức để quyết định Scene
        string tenSceneChuyenToi = (diemThienHienTai >= diemThienToiThieu) ? tenSceneGood : tenSceneBad;

        // 5. Chuyển sang Scene tương ứng
        Debug.Log($"<color=yellow>[BossSceneTransition]</color> Boss chết! Điểm Thiện: {diemThienHienTai} -> Chuyển sang Scene: {tenSceneChuyenToi}");
        SceneManager.LoadScene(tenSceneChuyenToi);
    }
}