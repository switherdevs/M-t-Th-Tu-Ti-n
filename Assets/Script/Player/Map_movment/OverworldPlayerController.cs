using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // BẮT BỘC: Thêm thư viện EventSystems để kiểm tra click UI
using UnityEngine.UI;          // BẮT BỘC: Thêm thư viện UI để check GraphicRaycaster
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class OverworldPlayerController : MonoBehaviour
{
    [Header("=== Tham chiếu (References) ===")]
    [Tooltip("Kéo MapManager có chứa script OverworldGrid vào đây")]
    public OverworldGrid mapGrid;

    [Tooltip("GameObject dùng để hiển thị đánh dấu vị trí đích")]
    public GameObject targetHighlight;

    [Tooltip("Vị trí cố định để dịch chuyển về khi nhấn phím R")]
    public Transform checkpointPoint;

    [Header("=== Cài đặt TextMeshPro hiển thị Thời gian ===")]
    public TMP_Text dateText;

    [Header("=== Cài đặt Di chuyển ===")]
    [Tooltip("Tốc độ di chuyển giữa các ô")]
    public float moveSpeed = 5f;

    [Tooltip("Chỉ có Collider thuộc Layer Wall mới ngăn cản di chuyển, các Layer khác đi qua bình thường")]
    public LayerMask wallLayer;

    [Tooltip("Số ô cách xa Wall khi phát hiện Wall ở đích đến (Mặc định 2 ô)")]
    public int wallSafetyOffset = 2;

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
    }

    private void Update()
    {
        // 🎯 BẤM R ĐỂ DỊCH CHUYỂN VỀ VỊ TRÍ CỐ ĐỊNH (CHECKPOINT)
        if (Input.GetKeyDown(KeyCode.R))
        {
            TeleportToCheckpoint();
            return;
        }

        // 🎯 BẤM SPACE ĐỂ DÙNG NGAY Ở Ô HIỆN TẠI (NẾU LỆCH SẼ TỰ ĐỘNG LÙI/CĂN VỀ TÂM Ô)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceAction();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // 🎯 CHẶN DI CHUYỂN KHI CLICK TRÚNG UI (BẤT KỂ OBJECT NÀO CÓ RAYCAST TARGET HOẶC BỊ UI CHE CHẮN)
            if (IsPointerOverUIObject())
            {
                return;
            }

            HandleMouseClick();
        }
    }

    /// <summary>
    /// Xử lý hành động khi nhấn phím Space tại ô hiện tại
    /// </summary>
    private void HandleSpaceAction()
    {
        // Cập nhật lại vị trí lưới hiện tại dựa trên tọa độ thực tế
        Vector2Int calculatedGridPos = mapGrid.WorldToGrid(transform.position);
        Vector3 exactCenterPos = mapGrid.GridToWorld(calculatedGridPos);

        // Kiểm tra xem vị trí hiện tại có bị lệch so với tâm ô chuẩn hay không
        if (Vector3.Distance(transform.position, exactCenterPos) > 0.001f)
        {
            Debug.Log("<color=yellow>[Player]</color> Vị trí bị lệch, đang lùi/căn chỉnh về tâm ô hiện tại!");

            // Nếu đang di chuyển dở dang thì dừng Coroutine cũ lại
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
                isMoving = false;
            }

            // Đưa nhân vật về chính xác tâm ô hiện tại
            transform.position = exactCenterPos;
        }

        currentGridPos = calculatedGridPos;

        // --- VIẾT LOGIC SỬ DỤNG / TƯƠNG TÁC TẠI Ô HIỆN TẠI Ở ĐÂY ---
        Debug.Log($"<color=green>[Player]</color> Đã dùng kỹ năng/tương tác tại ô hiện tại: {currentGridPos}");
    }

    /// <summary>
    /// Hàm thông minh kiểm tra xem vị trí chuột hiện tại có đang chạm vào bất kỳ UI Element nào có bật Raycast Target hay không
    /// </summary>
    private bool IsPointerOverUIObject()
    {
        // 1. Kiểm tra nhanh cơ bản bằng EventSystem hiện tại
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // 2. Kiểm tra sâu hơn bằng GraphicRaycaster (Phòng trường hợp UI Image/Panel chặn nhưng EventSystem bỏ sót)
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();

        // Lấy tất cả các Canvas trong Scene đang quản lý Raycast
        GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        foreach (var raycaster in raycasters)
        {
            raycaster.Raycast(eventDataCurrentPosition, results);
            if (results.Count > 0)
            {
                return true; // Có UI chặn dưới chuột
            }
        }

        return false;
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

    /// <summary>
    /// Xử lý dịch chuyển tức thời Player về vị trí Checkpoint khi nhấn R
    /// </summary>
    private void TeleportToCheckpoint()
    {
        if (checkpointPoint == null)
        {
            Debug.LogWarning("<color=yellow>[Player]</color> Chưa gán Checkpoint Point vào Inspector!");
            return;
        }

        // Hủy quá trình di chuyển đang diễn ra nếu có
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        // Đặt lại các trạng thái
        isMoving = false;
        isCollidedWithWall = false;
        SetAnimBool(isMovingAnimBool, false);

        if (lineRenderer != null) lineRenderer.positionCount = 0;
        if (targetHighlight != null) targetHighlight.SetActive(false);

        // Dịch chuyển đến Checkpoint và căn chỉnh tọa độ Grid chuẩn
        Vector2Int checkpointGridPos = mapGrid.WorldToGrid(checkpointPoint.position);
        Vector3 targetWorldPos = mapGrid.GridToWorld(checkpointGridPos);

        transform.position = targetWorldPos;
        currentGridPos = checkpointGridPos;

        Debug.Log("<color=green>[Player]</color> Đã dịch chuyển về vị trí cố định (R)!");
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

        // KIỂM TRA NẾU ĐÍCH ĐẾN HOẶC ĐƯỜNG ĐÍ CÓ WALL -> GIỮ KHỎANG CÁCH 2 Ô
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

            // Nếu đụng Wall bất ngờ trong quá trình đi -> Dừng ngay tại vị trí hiện tại
            if (isCollidedWithWall)
            {
                break;
            }

            // --- ĐẾN Ô MỚI AN TOÀN ---
            transform.position = targetWorldPos;
            currentGridPos = nextTile;

            RemoveFirstPointFromPathVisuals();

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