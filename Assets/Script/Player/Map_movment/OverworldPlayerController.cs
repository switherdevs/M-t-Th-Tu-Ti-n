using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 🎯 BẮT BỘC: Thêm thư viện EventSystems để kiểm tra click UI
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class OverworldPlayerController : MonoBehaviour
{
    [Header("=== Tham chiếu (References) ===")]
    [Tooltip("Kéo MapManager có chứa script OverworldGrid vào đây")]
    public OverworldGrid mapGrid;

    [Tooltip("GameObject dùng để hiển thị đánh dấu vị trí đích")]
    public GameObject targetHighlight;

    [Header("=== Cài đặt TextMeshPro hiển thị Thời gian ===")]
    public TMP_Text dateText;

    [Header("=== Cài đặt Di chuyển ===")]
    [Tooltip("Tốc độ di chuyển giữa các ô")]
    public float moveSpeed = 5f;

    [Tooltip("Chỉ có Collider thuộc Layer Wall mới ngăn cản di chuyển, các Layer khác đi qua bình thường")]
    public LayerMask wallLayer;

    [Tooltip("Số ô cách xa Wall khi phát hiện Wall ở đích đến (Mặc định 2 ô)")]
    public int wallSafetyOffset = 2;

    [Tooltip("Số lần đi lặp lại cùng 1 ô trước khi xác định bị kẹt và tự lùi 2 ô")]
    public int stuckThreshold = 3;

    [Header("=== Cài đặt Animation ===")]
    public string isMovingAnimBool = "IsMoving";

    [Header("=== Cài đặt Thời gian (In-game Time) ===")]
    public int startDay = 27;
    public int startMonth = 5;
    public int startYear = 1113;
    public int tilesPerDay = 2;

    private int currentDay;
    private int currentMonth;
    private int currentYear;
    private readonly int[] daysInMonths = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    private LineRenderer lineRenderer;
    private Animator animator;
    private Coroutine movementCoroutine;
    private int accumulatedTiles = 0;
    private Vector2Int currentGridPos;
    private bool isMoving = false;
    private bool isCollidedWithWall = false;

    // Quản lý phát hiện kẹt vị trí và lịch sử lùi 2 ô
    private List<Vector2Int> positionHistory = new List<Vector2Int>();
    private Dictionary<Vector2Int, int> visitCounts = new Dictionary<Vector2Int, int>();

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        animator = GetComponentInChildren<Animator>();

        if (targetHighlight != null)
            targetHighlight.SetActive(false);

        currentDay = startDay;
        currentMonth = startMonth;
        currentYear = startYear;

        UpdateDateUI();

        currentGridPos = mapGrid.WorldToGrid(transform.position);
        transform.position = mapGrid.GridToWorld(currentGridPos);

        RecordPositionHistory(currentGridPos);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 🎯 CHẶN DI CHUYỂN KHI CLICK TRÊN UI:
            // Nếu con trỏ chuột đang đè lên bất kỳ UI nào có Raycast Target -> Bỏ qua không xử lý click map
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            HandleMouseClick();
        }
    }

    // 🎯 CHỈ BẮT VA CHẠM KHI CHẠM VÀO LAYER WALL
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isMoving && !isCollidedWithWall)
        {
            // Kiểm tra Collider có thuộc Layer Wall hay không
            if ((1 << collision.gameObject.layer & wallLayer) != 0)
            {
                Debug.Log("<color=red>[Player]</color> Va chạm với WALL! Dừng di chuyển.");
                isCollidedWithWall = true;
            }
        }
    }

    private void HandleMouseClick()
    {
        if (isMoving) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int targetGridPos = mapGrid.WorldToGrid(mouseWorldPos);

        if (!mapGrid.IsValidGridPosition(targetGridPos)) return;
        if (targetGridPos == currentGridPos) return;

        List<Vector2Int> newPath = mapGrid.GetPath(currentGridPos, targetGridPos);

        if (newPath == null || newPath.Count == 0) return;

        // 🎯 KIỂM TRA NẾU ĐÍCH ĐẾN HOẶC ĐƯỜNG ĐÍ CÓ WALL -> GIỮ KHỎANG CÁCH 2 Ô
        newPath = AdjustPathForWallSafety(newPath);

        if (newPath.Count > 0)
        {
            movementCoroutine = StartCoroutine(Routine_MoveAlongPath(newPath, newPath[newPath.Count - 1]));
        }
    }

    /// <summary>
    /// Hàm kiểm tra nếu đường đi chạm Layer Wall thì cắt bớt 2 ô cuối để tránh bị kẹt
    /// </summary>
    private List<Vector2Int> AdjustPathForWallSafety(List<Vector2Int> originalPath)
    {
        int wallIndex = -1;

        for (int i = 0; i < originalPath.Count; i++)
        {
            Vector3 worldPos = mapGrid.GridToWorld(originalPath[i]);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.2f, wallLayer);
            if (hit != null)
            {
                wallIndex = i;
                break;
            }
        }

        // Nếu tìm thấy Wall trên đường đi hoặc ở điểm đích
        if (wallIndex != -1)
        {
            int safeIndex = Mathf.Max(0, wallIndex - wallSafetyOffset);
            List<Vector2Int> safePath = new List<Vector2Int>();
            for (int i = 0; i < safeIndex; i++)
            {
                safePath.Add(originalPath[i]);
            }
            return safePath;
        }

        return originalPath;
    }

    private IEnumerator Routine_MoveAlongPath(List<Vector2Int> path, Vector2Int finalTarget)
    {
        isMoving = true;
        isCollidedWithWall = false;

        if (targetHighlight != null)
        {
            targetHighlight.transform.position = mapGrid.GridToWorld(finalTarget);
            targetHighlight.SetActive(true);
        }

        if (path.Count > 0)
        {
            Vector3 firstStepWorldPos = mapGrid.GridToWorld(path[0]);
            UpdateFacingRotation(firstStepWorldPos.x);
        }

        UpdatePathVisuals(path);
        SetAnimBool(isMovingAnimBool, true);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int nextTile = path[i];
            Vector3 targetWorldPos = mapGrid.GridToWorld(nextTile);

            UpdateFacingRotation(targetWorldPos.x);

            // Di chuyển mượt sang ô tiếp theo
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                if (isCollidedWithWall) break;

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

                if (lineRenderer.positionCount > 0)
                {
                    lineRenderer.SetPosition(0, transform.position);
                }

                yield return null;
            }

            // Nếu đụng Wall bất ngờ trong quá trình đi
            if (isCollidedWithWall)
            {
                break;
            }

            // --- ĐẾN Ô MỚI AN TOÀN ---
            transform.position = targetWorldPos;
            currentGridPos = nextTile;

            RemoveFirstPointFromPathVisuals();

            // 🎯 GHI NHẬN VỊ TRÍ VÀ KIỂM TRA BỊ KẸT LẶP ĐI LẶP LẠI
            RecordPositionHistory(currentGridPos);
            if (CheckIfStuck(currentGridPos))
            {
                Debug.LogWarning("<color=orange>[Player]</color> Phát hiện bị kẹt do lặp vị trí! Đang tự động lùi 2 ô.");
                yield return StartCoroutine(Routine_StepBackTwoTiles());
                break;
            }

            accumulatedTiles++;
            if (accumulatedTiles >= tilesPerDay)
            {
                AdvanceOneDay();
                accumulatedTiles = 0;
            }
        }

        // --- KẾT THÚC DI CHUYỂN ---
        if (targetHighlight != null)
        {
            targetHighlight.SetActive(false);
        }

        SetAnimBool(isMovingAnimBool, false);
        lineRenderer.positionCount = 0;
        movementCoroutine = null;
        isMoving = false;
        isCollidedWithWall = false;
    }

    /// <summary>
    /// Ghi nhớ lịch sử các ô đã đi qua để tính số lần đi lặp lại
    /// </summary>
    private void RecordPositionHistory(Vector2Int pos)
    {
        positionHistory.Add(pos);

        if (visitCounts.ContainsKey(pos))
            visitCounts[pos]++;
        else
            visitCounts[pos] = 1;
    }

    /// <summary>
    /// Thuật toán kiểm tra nếu ô hiện tại bị lặp đi lặp lại quá số lần quy định
    /// </summary>
    private bool CheckIfStuck(Vector2Int currentPos)
    {
        return visitCounts.ContainsKey(currentPos) && visitCounts[currentPos] >= stuckThreshold;
    }

    /// <summary>
    /// Coroutine lùi lại 2 ô trong lịch sử khi phát hiện bị kẹt
    /// </summary>
    private IEnumerator Routine_StepBackTwoTiles()
    {
        int targetHistoryIndex = Mathf.Max(0, positionHistory.Count - 3);
        Vector2Int stepBackGridPos = positionHistory[targetHistoryIndex];
        Vector3 stepBackWorldPos = mapGrid.GridToWorld(stepBackGridPos);

        UpdateFacingRotation(stepBackWorldPos.x);

        while (Vector3.Distance(transform.position, stepBackWorldPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, stepBackWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = stepBackWorldPos;
        currentGridPos = stepBackGridPos;

        // Reset lại dữ liệu kẹt sau khi đã lùi an toàn
        positionHistory.Clear();
        visitCounts.Clear();
        RecordPositionHistory(currentGridPos);
    }

    private void AdvanceOneDay()
    {
        currentDay++;
        int maxDaysInCurrentMonth = GetDaysInMonth(currentMonth, currentYear);

        if (currentDay > maxDaysInCurrentMonth)
        {
            currentDay = 1;
            currentMonth++;

            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }
        }

        UpdateDateUI();
    }

    private int GetDaysInMonth(int month, int year)
    {
        if (month == 2)
        {
            bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            return isLeapYear ? 29 : 28;
        }

        return daysInMonths[month - 1];
    }

    private void UpdateDateUI()
    {
        if (dateText != null)
        {
            dateText.text = $"Ngày {currentDay} tháng {currentMonth} năm {currentYear}";
        }
    }

    private void UpdateFacingRotation(float targetX)
    {
        if (targetX < transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (targetX > transform.position.x)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    private void UpdatePathVisuals(List<Vector2Int> path)
    {
        lineRenderer.positionCount = path.Count + 1;
        lineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, mapGrid.GridToWorld(path[i]));
        }
    }

    private void RemoveFirstPointFromPathVisuals()
    {
        if (lineRenderer.positionCount <= 1) return;

        Vector3[] remainingPoints = new Vector3[lineRenderer.positionCount - 1];

        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            remainingPoints[i - 1] = lineRenderer.GetPosition(i);
        }

        lineRenderer.positionCount = remainingPoints.Length;
        lineRenderer.SetPositions(remainingPoints);
    }

    private void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }
}