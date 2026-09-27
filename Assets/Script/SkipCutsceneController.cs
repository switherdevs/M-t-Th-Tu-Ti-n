using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipCutsceneController : MonoBehaviour
{
    [Header("--- CẤU HÌNH TÊN MAP CHUYỂN ĐẾN ---")]
    [Tooltip("Tên Scene/Map sẽ chuyển tới sau khi skip Cutscene")]
    [SerializeField] private string targetSceneName;

    [Header("--- CẤU HÌNH PHÍM VÀ THỜI GIAN ---")]
    [Tooltip("Phím cần giữ để skip (Mặc định là Enter / Return)")]
    [SerializeField] private KeyCode skipKey = KeyCode.Return;

    [Tooltip("Thời gian giữ phím (giây) để kích hoạt skip")]
    [SerializeField] private float holdDuration = 3f;

    [Tooltip("Tốc độ mờ dần của Icon khi thả phím")]
    [SerializeField] private float fadeOutSpeed = 5f;

    [Header("--- CẤU HÌNH UI ICON ---")]
    [Tooltip("Gán Image Icon Skip (UI) vào đây")]
    [SerializeField] private Image skipIcon;

    // Biến lưu thời gian đã giữ phím
    private float holdTimer = 0f;
    // Cờ đánh dấu để tránh chuyển scene nhiều lần trong 1 khung hình
    private bool isSkipping = false;

    private void Start()
    {
        // 🎯 Khởi tạo trạng thái ban đầu của Icon (Đưa độ trong suốt Alpha về 0 - ẩn hoàn toàn)
        if (skipIcon != null)
        {
            Color initialColor = skipIcon.color;
            initialColor.a = 0f;
            skipIcon.color = initialColor;
        }
    }

    private void Update()
    {
        // Nếu đã kích hoạt skip thì không xử lý thêm logic nữa
        if (isSkipping) return;

        // 🎯 KIỂM TRA NGƯỜI CHƠI CÓ ĐANG GIỮ PHÍM ENTER KHÔNG
        if (Input.GetKey(skipKey))
        {
            // Tăng thời gian đếm dựa trên thời gian khung hình thực tế
            holdTimer += Time.deltaTime;

            // Kiểm tra nếu thời gian giữ đã đủ 3 giây
            if (holdTimer >= holdDuration)
            {
                TriggerSkipScene();
            }
        }
        else
        {
            // Nếu người chơi thả phím, giảm dần thời gian đếm để Icon mờ đi mượt mà
            if (holdTimer > 0f)
            {
                holdTimer -= Time.deltaTime * fadeOutSpeed;
                if (holdTimer < 0f) holdTimer = 0f;
            }
        }

        // 🎯 CẬP NHẬT ĐỘ MỜ (ALPHA) CHO ICON UI
        CapNhatDoMoIcon();
    }

    /// <summary>
    /// Thuật toán quy đổi thời gian giữ thành độ mượt Alpha của Icon (từ 0.0 đến 1.0)
    /// </summary>
    private void CapNhatDoMoIcon()
    {
        if (skipIcon == null) return;

        // Tính tỉ lệ phần trăm từ 0.0 -> 1.0 dựa trên thời gian đã giữ
        float alphaProgress = Mathf.Clamp01(holdTimer / holdDuration);

        // Lấy màu hiện tại của Icon và gán lại giá trị Alpha mới
        Color currentColor = skipIcon.color;
        currentColor.a = alphaProgress;
        skipIcon.color = currentColor;
    }

    /// <summary>
    /// Hàm thực hiện chuyển Scene khi đã giữ đủ 3 giây
    /// </summary>
    private void TriggerSkipScene()
    {
        isSkipping = true;
        holdTimer = holdDuration;
        CapNhatDoMoIcon(); // Đảm bảo Icon hiện 100% rõ trước khi load

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"<color=cyan>[Skip Cutscene]</color> Đã skip Cutscene thành công! Đang chuyển sang Map: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("[Skip Cutscene] Chưa gán tên Map (targetSceneName) trong Inspector!");
            isSkipping = false; // Reset lại cờ nếu quên chưa gán tên Scene
        }
    }
}