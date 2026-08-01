using UnityEngine;

// Đổi tên Class thành AutoDestroy để tránh trùng tên với hàm Destroy gốc của Unity
public class AutoDestroy : MonoBehaviour
{
    [Header("Thời gian tự hủy")]
    [SerializeField] private float time = 3f; // Khoảng thời gian (tính bằng giây) trước khi object bị xóa

    void Start()
    {
        // Gọi hàm tự hủy ngay khi đối tượng được khởi tạo vào Game
        TuHuy();
    }

    // Hàm thực hiện việc xóa GameObject
    public void TuHuy()
    {
        // Hàm Destroy có 2 đối số: 
        // 1. gameObject: Đối tượng sẽ bị xóa (chính là đối tượng gắn script này)
        // 2. time: Thời gian đếm ngược (tính bằng giây) trước khi thực sự xóa
        Destroy(gameObject, time);
    }
}