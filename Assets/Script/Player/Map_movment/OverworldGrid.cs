using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý lưới tọa độ, giới hạn di chuyển và hiển thị Grid trên Overworld.
/// Hỗ trợ kích thước ô tùy chỉnh cả chiều X và Y (Vector2).
/// </summary>
public class OverworldGrid : MonoBehaviour
{
    [Header("=== Cấu hình Lưới (Grid) ===")]
    [Tooltip("Kích thước của 1 ô trên bản đồ theo chiều X (ngang) và Y (dọc)")]
    public Vector2 cellSize = new Vector2(1f, 1f);

    [Header("=== Giới hạn vùng di chuyển (Map Bounds) ===")]
    [Tooltip("Tọa độ X nhỏ nhất cho phép")]
    public int minGridX = -50;
    [Tooltip("Tọa độ X lớn nhất cho phép")]
    public int maxGridX = 50;
    [Tooltip("Tọa độ Y nhỏ nhất cho phép")]
    public int minGridY = -50;
    [Tooltip("Tọa độ Y lớn nhất cho phép")]
    public int maxGridY = 50;

    [Header("=== Cài đặt Hiển thị Grid (Gizmos) ===")]
    [Tooltip("Bật/Tắt hiển thị các đường kẻ ô lưới trong cửa sổ Scene")]
    public bool showGrid = true;
    [Tooltip("Màu sắc của lưới")]
    public Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    [Tooltip("Màu sắc của khung biên bản đồ")]
    public Color boundsColor = Color.cyan;

    /// <summary>
    /// Chuyển đổi tọa độ thực trong Unity (World Space) sang tọa độ Ô (Grid/Cell).
    /// </summary>
    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        float sizeX = cellSize.x > 0 ? cellSize.x : 1f;
        float sizeY = cellSize.y > 0 ? cellSize.y : 1f;

        int x = Mathf.RoundToInt(worldPosition.x / sizeX);
        int y = Mathf.RoundToInt(worldPosition.y / sizeY);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Chuyển đổi tọa độ Ô (Grid) ngược lại thành tọa độ thực để Player di chuyển tới.
    /// </summary>
    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector2(gridPosition.x * cellSize.x, gridPosition.y * cellSize.y);
    }

    /// <summary>
    /// Kiểm tra xem một Ô có nằm trong vùng cho phép di chuyển không.
    /// </summary>
    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= minGridX && gridPosition.x <= maxGridX &&
               gridPosition.y >= minGridY && gridPosition.y <= maxGridY;
    }

    /// <summary>
    /// Thuật toán tạo đường đi (Pathfinding) tối ưu khi KHÔNG CÓ VẬT CẢN.
    /// </summary>
    public List<Vector2Int> GetPath(Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        while (current != target)
        {
            int stepX = System.Math.Sign(target.x - current.x);
            int stepY = System.Math.Sign(target.y - current.y);

            current += new Vector2Int(stepX, stepY);
            path.Add(current);
        }

        return path;
    }

    // Hiển thị đường lưới Grid và Khung biên bản đồ trong Scene view
    private void OnDrawGizmos()
    {
        float sizeX = cellSize.x > 0 ? cellSize.x : 1f;
        float sizeY = cellSize.y > 0 ? cellSize.y : 1f;

        // 1. Vẽ toàn bộ các đường lưới ô (Grid lines)
        if (showGrid)
        {
            Gizmos.color = gridColor;

            // Tính góc mép dưới-trái và mép trên-phải của toàn bộ vùng lưới
            float minXWorld = minGridX * sizeX - sizeX * 0.5f;
            float maxXWorld = maxGridX * sizeX + sizeX * 0.5f;
            float minYWorld = minGridY * sizeY - sizeY * 0.5f;
            float maxYWorld = maxGridY * sizeY + sizeY * 0.5f;

            // Vẽ các đường dọc (chạy từ bottom lên top)
            for (int x = minGridX; x <= maxGridX + 1; x++)
            {
                float xPos = (x - 0.5f) * sizeX;
                Vector3 start = new Vector3(xPos, minYWorld, 0f);
                Vector3 end = new Vector3(xPos, maxYWorld, 0f);
                Gizmos.DrawLine(start, end);
            }

            // Vẽ các đường ngang (chạy từ left sang right)
            for (int y = minGridY; y <= maxGridY + 1; y++)
            {
                float yPos = (y - 0.5f) * sizeY;
                Vector3 start = new Vector3(minXWorld, yPos, 0f);
                Vector3 end = new Vector3(maxXWorld, yPos, 0f);
                Gizmos.DrawLine(start, end);
            }
        }

        // 2. Vẽ 4 đường biên khung ngoài bản đồ (Bounds)
        Gizmos.color = boundsColor;
        Vector2 bottomLeft = GridToWorld(new Vector2Int(minGridX, minGridY));
        Vector2 topRight = GridToWorld(new Vector2Int(maxGridX, maxGridY));
        Vector2 topLeft = new Vector2(bottomLeft.x, topRight.y);
        Vector2 bottomRight = new Vector2(topRight.x, bottomLeft.y);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}