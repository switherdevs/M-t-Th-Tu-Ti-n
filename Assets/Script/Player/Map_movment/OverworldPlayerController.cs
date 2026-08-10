using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // BỔ SUNG: Thư viện bắt buộc để dùng TextMeshPro

/// <summary>
/// Xử lý logic Click-to-Move, đếm ngày/tháng/năm realtime bằng TextMeshPro,
/// hiển thị đường đi, lật mặt bằng Rotation Y và khóa click trong khi di chuyển cho Player trên Overworld.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class OverworldPlayerController : MonoBehaviour
{
    [Header("=== Tham chiếu (References) ===")]
    [Tooltip("Kéo MapManager có chứa script OverworldGrid vào đây")]
    public OverworldGrid mapGrid;

    [Tooltip("GameObject dùng để hiển thị đánh dấu vị trí đích (Ví dụ: Sprite vuông màu vàng)")]
    public GameObject targetHighlight;

    [Header("=== Cài đặt TextMeshPro hiển thị Thời gian ===")]
    [Tooltip("Kéo Component TextMeshProUGUI (trên UI Canvas) hoặc TextMeshPro (trong 3D World) vào đây")]
    public TMP_Text dateText;

    [Header("=== Cài đặt Di chuyển ===")]
    [Tooltip("Tốc độ di chuyển giữa các ô")]
    public float moveSpeed = 5f;

    [Header("=== Cài đặt Animation ===")]
    [Tooltip("Tên tham số Bool duy nhất trong Animator kiểm tra trạng thái di chuyển")]
    public string isMovingAnimBool = "IsMoving";

    [Header("=== Cài đặt Thời gian (In-game Time) ===")]
    [Tooltip("Ngày bắt đầu")]
    public int startDay = 27;

    [Tooltip("Tháng bắt đầu")]
    public int startMonth = 5;

    [Tooltip("Năm bắt đầu")]
    public int startYear = 1113;

    [Tooltip("Cứ đi bao nhiêu ô thì tăng 1 ngày?")]
    public int tilesPerDay = 2;

    // Các biến nội bộ để xử lý thời gian
    private int currentDay;
    private int currentMonth;
    private int currentYear;

    // Mảng lưu số ngày cố định của 12 tháng (Tháng 2 mặc định 28 ngày)
    private readonly int[] daysInMonths = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    // Các biến nội bộ để xử lý luồng
    private LineRenderer lineRenderer;
    private Animator animator;
    private Coroutine movementCoroutine;
    private int accumulatedTiles = 0; // Số ô đã đi được (tích lũy để tính ngày)
    private Vector2Int currentGridPos; // Vị trí hiện tại của Player trên Lưới
    private bool isMoving = false; // BỔ SUNG: Khóa kiểm tra trạng thái đang di chuyển

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0; // Tắt đường vẽ ban đầu

        // Lấy Animator nằm ở GameObject này hoặc con của nó
        animator = GetComponentInChildren<Animator>();

        if (targetHighlight != null)
            targetHighlight.SetActive(false);

        // Khởi tạo thời gian ban đầu
        currentDay = startDay;
        currentMonth = startMonth;
        currentYear = startYear;

        // Cập nhật lên màn hình ngay khi bắt đầu
        UpdateDateUI();

        // Khởi tạo vị trí ban đầu của Player đồng bộ với Lưới
        currentGridPos = mapGrid.WorldToGrid(transform.position);
        transform.position = mapGrid.GridToWorld(currentGridPos);
    }

    private void Update()
    {
        // Nhận diện Click Chuột Trái (0)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    /// <summary>
    /// Xử lý khi chuột click: Lấy vị trí, kiểm tra hợp lệ, tạo đường đi.
    /// </summary>
    private void HandleMouseClick()
    {
        // BỔ SUNG: Nếu đang di chuyển thì KHÔNG CHO phép click chọn ô khác (Bắt buộc đến nơi mới được đi tiếp)
        if (isMoving) return;

        // 1. Lấy tọa độ thế giới từ vị trí chuột trên màn hình
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. Chuyển đổi thành tọa độ Ô trên lưới
        Vector2Int targetGridPos = mapGrid.WorldToGrid(mouseWorldPos);

        // 3. Kiểm tra: Nếu ô được click nằm ngoài giới hạn bản đồ -> Bỏ qua
        if (!mapGrid.IsValidGridPosition(targetGridPos))
        {
            Debug.Log("Vị trí click nằm ngoài vùng di chuyển!");
            return;
        }

        // 4. Kiểm tra: Nếu click lại đúng vị trí đang đứng -> Bỏ qua
        if (targetGridPos == currentGridPos) return;

        // 5. Lấy đường đi từ vị trí hiện tại đến đích
        List<Vector2Int> newPath = mapGrid.GetPath(currentGridPos, targetGridPos);

        // 6. Bật tiến trình di chuyển mới
        movementCoroutine = StartCoroutine(Routine_MoveAlongPath(newPath, targetGridPos));
    }

    /// <summary>
    /// Coroutine: Xử lý di chuyển từng ô một, xoay mặt, cập nhật đường vẽ, animation và tính toán ngày tháng.
    /// </summary>
    /// <param name="path">Danh sách các ô cần đi qua</param>
    /// <param name="finalTarget">Tọa độ ô đích cuối cùng</param>
    private IEnumerator Routine_MoveAlongPath(List<Vector2Int> path, Vector2Int finalTarget)
    {
        // BỔ SUNG: Bật cờ khóa di chuyển
        isMoving = true;

        // --- CHUẨN BỊ ---
        // Đặt Highlight tại vị trí đích
        if (targetHighlight != null)
        {
            targetHighlight.transform.position = mapGrid.GridToWorld(finalTarget);
            targetHighlight.SetActive(true);
        }

        // BỔ SUNG: Tự động xoay mặt ngay lập tức về hướng ô đầu tiên trong đường đi
        if (path.Count > 0)
        {
            Vector3 firstStepWorldPos = mapGrid.GridToWorld(path[0]);
            UpdateFacingRotation(firstStepWorldPos.x);
        }

        // Vẽ đường đi dự kiến (LineRenderer)
        UpdatePathVisuals(path);

        // Bật Animation di chuyển (Bool)
        SetAnimBool(isMovingAnimBool, true);

        // --- BẮT ĐẦU DI CHUYỂN ---
        // Duyệt qua từng ô trong đường đi
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int nextTile = path[i];
            Vector3 targetWorldPos = mapGrid.GridToWorld(nextTile);

            // BỔ SUNG: Cập nhật hướng xoay mặt Trái/Phải trước khi tiến sang ô tiếp theo
            UpdateFacingRotation(targetWorldPos.x);

            // Vòng lặp nhỏ: Di chuyển mượt mà từ điểm hiện tại đến targetWorldPos
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                // Vector3.MoveTowards giúp di chuyển tuyến tính ổn định không bị giật
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

                // Cập nhật điểm đầu tiên của đường vẽ dính vào Player để đường vẽ ngắn dần
                if (lineRenderer.positionCount > 0)
                {
                    lineRenderer.SetPosition(0, transform.position);
                }

                yield return null; // Đợi đến frame tiếp theo
            }

            // --- KHI VỪA ĐẾN GIỮA Ô MỚI ---
            transform.position = targetWorldPos; // Ép tọa độ chuẩn xác để chống sai số
            currentGridPos = nextTile;           // Cập nhật vị trí hiện tại trên lưới

            // Xóa điểm của ô vừa đi qua khỏi LineRenderer
            RemoveFirstPointFromPathVisuals();

            // Tính toán Cơ chế Thời gian (Đi 1 ô -> tích lũy 1)
            accumulatedTiles++;
            if (accumulatedTiles >= tilesPerDay)
            {
                AdvanceOneDay(); // Cộng thêm 1 ngày và xử lý nhảy tháng/năm
                accumulatedTiles = 0;
            }
        }

        // --- KẾT THÚC ĐƯỜNG ĐI ---
        if (targetHighlight != null)
        {
            targetHighlight.SetActive(false);
        }

        // Tắt Animation di chuyển khi đã tới nơi (Trở về Idle)
        SetAnimBool(isMovingAnimBool, false);

        lineRenderer.positionCount = 0; // Tắt hẳn đường vẽ
        movementCoroutine = null;       // Đưa tiến trình về rỗng

        // BỔ SUNG: Mở khóa di chuyển, người chơi được phép click ô mới
        isMoving = false;
    }

    /// <summary>
    /// Cộng thêm 1 ngày vào lịch và cập nhật UI.
    /// </summary>
    private void AdvanceOneDay()
    {
        currentDay++;

        // Lấy tổng số ngày tối đa của tháng hiện tại (Có tính năm nhuận)
        int maxDaysInCurrentMonth = GetDaysInMonth(currentMonth, currentYear);

        // Nếu ngày vượt quá số ngày của tháng hiện tại -> Sang tháng mới
        if (currentDay > maxDaysInCurrentMonth)
        {
            currentDay = 1;
            currentMonth++;

            // Nếu tháng vượt quá 12 -> Sang năm mới
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
        }

        // Cập nhật hiển thị lên màn hình
        UpdateDateUI();
    }

    /// <summary>
    /// Thuật toán kiểm tra số ngày trong tháng (Bao gồm kiểm tra Năm Nhuận).
    /// </summary>
    private int GetDaysInMonth(int month, int year)
    {
        // Nếu là tháng 2, kiểm tra xem có phải năm nhuận không
        if (month == 2)
        {
            bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            return isLeapYear ? 29 : 28;
        }

        // Lấy số ngày từ mảng (Trừ 1 vì mảng bắt đầu từ chỉ số 0)
        return daysInMonths[month - 1];
    }

    /// <summary>
    /// Định dạng chuỗi ngày tháng năm và gán vào TextMeshPro.
    /// </summary>
    private void UpdateDateUI()
    {
        if (dateText != null)
        {
            dateText.text = $"Ngày {currentDay} tháng {currentMonth} năm {currentYear}";
        }
    }

    /// <summary>
    /// BỔ SUNG: Cơ chế xoay mặt bằng Rotation (Trục Y) tương thích với 2D Bone/Sprite
    /// </summary>
    /// <param name="targetX">Tọa độ X của điểm muốn di chuyển tới</param>
    private void UpdateFacingRotation(float targetX)
    {
        // Điểm đến nằm bên Trái so với Player -> Xoay Y = 180 độ
        if (targetX < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        // Điểm đến nằm bên Phải so với Player -> Xoay Y = 0 độ
        else if (targetX > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Vẽ đường đi bằng LineRenderer.
    /// </summary>
    private void UpdatePathVisuals(List<Vector2Int> path)
    {
        // Số điểm = Vị trí Player hiện tại + Các ô trong đường đi
        lineRenderer.positionCount = path.Count + 1;
        lineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, mapGrid.GridToWorld(path[i]));
        }
    }

    /// <summary>
    /// Xóa điểm đầu tiên của LineRenderer khi Player vừa đi qua để đường line ngắn lại.
    /// </summary>
    private void RemoveFirstPointFromPathVisuals()
    {
        if (lineRenderer.positionCount <= 1) return;

        Vector3[] remainingPoints = new Vector3[lineRenderer.positionCount - 1];

        // Copy các điểm từ vị trí 1 trở đi vào mảng mới
        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            remainingPoints[i - 1] = lineRenderer.GetPosition(i);
        }

        lineRenderer.positionCount = remainingPoints.Length;
        lineRenderer.SetPositions(remainingPoints);
    }

    // ==========================================
    // HELPER FUNCTION XỬ LÝ ANIMATION SAFE
    // ==========================================
    private void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }
}