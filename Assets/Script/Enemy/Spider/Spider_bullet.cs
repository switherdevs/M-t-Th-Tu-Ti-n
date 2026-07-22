using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Đạn độc")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage = 10;

    private Vector2 direction;


    // Nhận hướng bay từ Spider
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }


    private void Update()
    {
        transform.Translate(
            direction * speed * Time.deltaTime
        );
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Khi đạn chạm Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player bị trúng độc: " + damage + " damage");


            // Nếu chưa làm máu thì chỉ test xóa
            Destroy(gameObject);
        }
    }
}