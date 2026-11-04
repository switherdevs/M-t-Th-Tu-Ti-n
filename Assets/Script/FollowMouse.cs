using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private void Update()
    {
        // 1. Lấy vị trí pixel của con chuột trên màn hình
        Vector3 mouseScreenPos = Input.mousePosition;

        // 2. Cố định trục Z để không bị lệch độ sâu trong không gian 2D
        mouseScreenPos.z = 0f;

        // 3. Chuyển đổi tọa độ màn hình sang tọa độ thế giới game
        Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        targetWorldPos.z = 0f;

        // 4. Gán thẳng vị trí đối tượng trùng khớp với vị trí chuột ngay lập tức
        transform.position = targetWorldPos;
    }
}