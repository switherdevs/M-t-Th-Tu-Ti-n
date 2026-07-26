using UnityEngine;

public class BaseSkillEffect : MonoBehaviour
{
    [SerializeField] private float skillSpeed = 12f;
    [SerializeField] private float lifeTime = 3f;
    private Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * skillSpeed * Time.deltaTime);
    }

    // --- THÊM ĐOẠN CODE NÀY ĐỂ ĐẠN TỰ BIẾN MẤT KHI TRÚNG ĐÍCH ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu va chạm với đối tượng có Tag là "Enemy" (Địch) hoặc "Tuong" (Tường)
        if (collision.CompareTag("Enemy") /*|| collision.CompareTag("Tuong") || collision.CompareTag("Obstacle")*/)
        {
            // (Tùy chọn) Gọi hàm trừ máu quái ở đây nếu có...

            // Hủy ngay lập tức viên đạn/skill khi chạm mục tiêu
            Destroy(gameObject);
        }
    }
}