using UnityEngine;

public class MouseTargetFollower : MonoBehaviour
{
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Kiểm tra an toàn: nếu chưa có Main Camera thì bỏ qua, không tính toán để tránh lỗi NaN
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // Lấy vị trí chuột từ màn hình
        Vector3 mouseScreenPos = Input.mousePosition;

        // Ép vị trí Z của chuột bằng khoảng cách Z của Camera để hàm ScreenToWorldPoint không bị ra NaN
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z);

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // Khóa trục Z cố định ở mặt phẳng 2D

        transform.position = mouseWorldPos;
    }
}