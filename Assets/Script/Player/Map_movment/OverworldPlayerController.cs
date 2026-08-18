using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class OverworldPlayerController : MonoBehaviour
{
    [Header("=== Tham chiếu (References) ===")]
    [Tooltip("Kéo MapManager có chứa script OverworldGrid vào đây")]
    public OverworldGrid mapGrid; //[cite: 9]

    [Tooltip("GameObject dùng để hiển thị đánh dấu vị trí đích")]
    public GameObject targetHighlight; //[cite: 9]

    [Header("=== Cài đặt TextMeshPro hiển thị Thời gian ===")]
    public TMP_Text dateText; //[cite: 9]

    [Header("=== Cài đặt Di chuyển ===")]
    [Tooltip("Tốc độ di chuyển giữa các ô")]
    public float moveSpeed = 5f; //[cite: 9]

    [Tooltip("Chỉ có Collider thuộc Layer Wall mới ngăn cản di chuyển, các Layer khác đi qua bình thường")]
    public LayerMask wallLayer;

    [Tooltip("Số ô cách xa Wall khi phát hiện Wall ở đích đến (Mặc định 2 ô)")]
    public int wallSafetyOffset = 2;

    [Tooltip("Số lần đi lặp lại cùng 1 ô trước khi xác định bị kẹt và tự lùi 2 ô")]
    public int stuckThreshold = 3;

    [Header("=== Cài đặt Animation ===")]
    public string isMovingAnimBool = "IsMoving"; //[cite: 9]

    [Header("=== Cài đặt Thời gian (In-game Time) ===")]
    public int startDay = 27; //[cite: 9]
    public int startMonth = 5; //[cite: 9]
    public int startYear = 1113; //[cite: 9]
    public int tilesPerDay = 2; //[cite: 9]

    private int currentDay; //[cite: 9]
    private int currentMonth; //[cite: 9]
    private int currentYear; //[cite: 9]
    private readonly int[] daysInMonths = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }; //[cite: 9]

    private LineRenderer lineRenderer; //[cite: 9]
    private Animator animator; //[cite: 9]
    private Coroutine movementCoroutine; //[cite: 9]
    private int accumulatedTiles = 0; //[cite: 9]
    private Vector2Int currentGridPos; //[cite: 9]
    private bool isMoving = false; //[cite: 9]
    private bool isCollidedWithWall = false;

    // Quản lý phát hiện kẹt vị trí và lịch sử lùi 2 ô
    private List<Vector2Int> positionHistory = new List<Vector2Int>();
    private Dictionary<Vector2Int, int> visitCounts = new Dictionary<Vector2Int, int>();

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>(); //[cite: 9]
        lineRenderer.positionCount = 0; //[cite: 9]

        animator = GetComponentInChildren<Animator>(); //[cite: 9]

        if (targetHighlight != null) //[cite: 9]
            targetHighlight.SetActive(false); //[cite: 9]

        currentDay = startDay; //[cite: 9]
        currentMonth = startMonth; //[cite: 9]
        currentYear = startYear; //[cite: 9]

        UpdateDateUI(); //[cite: 9]

        currentGridPos = mapGrid.WorldToGrid(transform.position); //[cite: 9]
        transform.position = mapGrid.GridToWorld(currentGridPos); //[cite: 9]

        RecordPositionHistory(currentGridPos);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) //[cite: 9]
        {
            HandleMouseClick(); //[cite: 9]
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
        if (isMoving) return; //[cite: 9]

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //[cite: 9]
        Vector2Int targetGridPos = mapGrid.WorldToGrid(mouseWorldPos); //[cite: 9]

        if (!mapGrid.IsValidGridPosition(targetGridPos)) return; //[cite: 9]
        if (targetGridPos == currentGridPos) return; //[cite: 9]

        List<Vector2Int> newPath = mapGrid.GetPath(currentGridPos, targetGridPos); //[cite: 9]

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
        isMoving = true; //[cite: 9]
        isCollidedWithWall = false;

        if (targetHighlight != null) //[cite: 9]
        {
            targetHighlight.transform.position = mapGrid.GridToWorld(finalTarget); //[cite: 9]
            targetHighlight.SetActive(true); //[cite: 9]
        }

        if (path.Count > 0) //[cite: 9]
        {
            Vector3 firstStepWorldPos = mapGrid.GridToWorld(path[0]); //[cite: 9]
            UpdateFacingRotation(firstStepWorldPos.x); //[cite: 9]
        }

        UpdatePathVisuals(path); //[cite: 9]
        SetAnimBool(isMovingAnimBool, true); //[cite: 9]

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int nextTile = path[i];
            Vector3 targetWorldPos = mapGrid.GridToWorld(nextTile); //[cite: 9]

            UpdateFacingRotation(targetWorldPos.x); //[cite: 9]

            // Di chuyển mượt sang ô tiếp theo
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f) //[cite: 9]
            {
                if (isCollidedWithWall) break;

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime); //[cite: 9]

                if (lineRenderer.positionCount > 0) //[cite: 9]
                {
                    lineRenderer.SetPosition(0, transform.position); //[cite: 9]
                }

                yield return null; //[cite: 9]
            }

            // Nếu đụng Wall bất ngờ trong quá trình đi
            if (isCollidedWithWall)
            {
                break;
            }

            // --- ĐẾN Ô MỚI AN TOÀN ---
            transform.position = targetWorldPos; //[cite: 9]
            currentGridPos = nextTile; //[cite: 9]

            RemoveFirstPointFromPathVisuals(); //[cite: 9]

            // 🎯 GHI NHẬN VỊ TRÍ VÀ KIỂM TRA BỊ KẸT LẶP ĐI LẶP LẠI
            RecordPositionHistory(currentGridPos);
            if (CheckIfStuck(currentGridPos))
            {
                Debug.LogWarning("<color=orange>[Player]</color> Phát hiện bị kẹt do lặp vị trí! Đang tự động lùi 2 ô.");
                yield return StartCoroutine(Routine_StepBackTwoTiles());
                break;
            }

            accumulatedTiles++; //[cite: 9]
            if (accumulatedTiles >= tilesPerDay) //[cite: 9]
            {
                AdvanceOneDay(); //[cite: 9]
                accumulatedTiles = 0; //[cite: 9]
            }
        }

        // --- KẾT THÚC DI CHUYỂN ---
        if (targetHighlight != null) //[cite: 9]
        {
            targetHighlight.SetActive(false); //[cite: 9]
        }

        SetAnimBool(isMovingAnimBool, false); //[cite: 9]
        lineRenderer.positionCount = 0; //[cite: 9]
        movementCoroutine = null; //[cite: 9]
        isMoving = false; //[cite: 9]
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

    private void AdvanceOneDay() //[cite: 9]
    {
        currentDay++; //[cite: 9]
        int maxDaysInCurrentMonth = GetDaysInMonth(currentMonth, currentYear); //[cite: 9]

        if (currentDay > maxDaysInCurrentMonth) //[cite: 9]
        {
            currentDay = 1; //[cite: 9]
            currentMonth++; //[cite: 9]

            if (currentMonth > 12) //[cite: 9]
            {
                currentMonth = 1; //[cite: 9]
                currentYear++; //[cite: 9]
            }
        }

        UpdateDateUI(); //[cite: 9]
    }

    private int GetDaysInMonth(int month, int year) //[cite: 9]
    {
        if (month == 2) //[cite: 9]
        {
            bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0); //[cite: 9]
            return isLeapYear ? 29 : 28; //[cite: 9]
        }

        return daysInMonths[month - 1]; //[cite: 9]
    }

    private void UpdateDateUI() //[cite: 9]
    {
        if (dateText != null) //[cite: 9]
        {
            dateText.text = $"Ngày {currentDay} tháng {currentMonth} năm {currentYear}"; //[cite: 9]
        }
    }

    private void UpdateFacingRotation(float targetX) //[cite: 9]
    {
        if (targetX < transform.position.x) //[cite: 9]
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f); //[cite: 9]
        }
        else if (targetX > transform.position.x) //[cite: 9]
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f); //[cite: 9]
        }
    }

    private void UpdatePathVisuals(List<Vector2Int> path) //[cite: 9]
    {
        lineRenderer.positionCount = path.Count + 1; //[cite: 9]
        lineRenderer.SetPosition(0, transform.position); //[cite: 9]

        for (int i = 0; i < path.Count; i++) //[cite: 9]
        {
            lineRenderer.SetPosition(i + 1, mapGrid.GridToWorld(path[i])); //[cite: 9]
        }
    }

    private void RemoveFirstPointFromPathVisuals() //[cite: 9]
    {
        if (lineRenderer.positionCount <= 1) return; //[cite: 9]

        Vector3[] remainingPoints = new Vector3[lineRenderer.positionCount - 1]; //[cite: 9]

        for (int i = 1; i < lineRenderer.positionCount; i++) //[cite: 9]
        {
            remainingPoints[i - 1] = lineRenderer.GetPosition(i); //[cite: 9]
        }

        lineRenderer.positionCount = remainingPoints.Length; //[cite: 9]
        lineRenderer.SetPositions(remainingPoints); //[cite: 9]
    }

    private void SetAnimBool(string paramName, bool value) //[cite: 9]
    {
        if (animator != null && !string.IsNullOrEmpty(paramName)) //[cite: 9]
        {
            animator.SetBool(paramName, value); //[cite: 9]
        }
    }
}