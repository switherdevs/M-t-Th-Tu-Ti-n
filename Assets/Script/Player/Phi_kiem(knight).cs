using UnityEngine;

public class PhiKiem : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Hàm này giúp phi kiếm xoay mũi theo hướng nó đang bay
    public void Setup(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            // Có thể thêm hiệu ứng nổ/chạm kiếm ở đây
            Destroy(gameObject);
        }
    }
}