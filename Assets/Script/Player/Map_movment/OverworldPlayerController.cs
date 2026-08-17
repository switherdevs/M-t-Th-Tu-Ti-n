using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Tooltip("Layer của các vật cản/công trình cần tính khoảng cách va chạm")]
    public LayerMask obstacleLayer;

    [Header("=== Cài đặt Khoảng cách Tương tác khi Va chạm ===")]
    [Tooltip("Tỉ lệ dừng lại so với ô trước đó (Từ 0.1 đến 0.9). Giá trị càng lớn càng tiến sát Collider hơn")]
    [Range(0.1f, 0.9f)]
    public float distanceThreshold = 0.7f;

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
    private Vector2Int previousGridPos;
    private bool isMoving = false;
    private bool isCollided = false;

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
        previousGridPos = currentGridPos;
        transform.position = mapGrid.GridToWorld(currentGridPos);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    // 🎯 PHÁT HIỆN VA CHẠM VỚI COLLIDER CỦA OBSTACLE
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isMoving && !isCollided)
        {
            // Kiểm tra Collider có thuộc Layer obstacleLayer hay không
            if ((1 << collision.gameObject.layer & obstacleLayer) != 0 || obstacleLayer.value == 0)
            {
                Debug.Log("<color=yellow>[Player]</color> Đã chạm Collider công trình/vật cản! Đang hãm phanh giữ khoảng cách.");
                isCollided = true;
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
        movementCoroutine = StartCoroutine(Routine_MoveAlongPath(newPath, targetGridPos));
    }

    private IEnumerator Routine_MoveAlongPath(List<Vector2Int> path, Vector2Int finalTarget)
    {
        isMoving = true;
        isCollided = false;

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
            previousGridPos = currentGridPos;
            Vector2Int nextTile = path[i];
            Vector3 targetWorldPos = mapGrid.GridToWorld(nextTile);

            UpdateFacingRotation(targetWorldPos.x);

            // Vòng lặp di chuyển tiến từng ô
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                if (isCollided) break;

                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

                if (lineRenderer.positionCount > 0)
                {
                    lineRenderer.SetPosition(0, transform.position);
                }

                yield return null;
            }

            // 🎯 XỬ LÝ GIỮ KHỎANG CÁCH KHI CHẠM COLLIDER
            if (isCollided)
            {
                Vector3 previousWorldPos = mapGrid.GridToWorld(previousGridPos);

                // Tính điểm dừng trung gian dựa trên distanceThreshold (% tiến sát tới ô vật cản)
                Vector3 stopPointPosition = Vector3.Lerp(previousWorldPos, targetWorldPos, distanceThreshold);

                // Di chuyển mượt đến điểm dừng khoảng cách sát Collider
                while (Vector3.Distance(transform.position, stopPointPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, stopPointPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Gán vị trí chính xác tại điểm dừng
                transform.position = stopPointPosition;
                currentGridPos = previousGridPos; // Vẫn tính thuộc về ô trước đó trên Grid

                break; // Hủy toàn bộ hành trình đi tiếp
            }

            // --- ĐẾN Ô AN TOÀN ---
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
        isCollided = false;
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