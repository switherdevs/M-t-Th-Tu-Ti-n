using UnityEngine;

public class BasicEnemy : Enemy
{
    // Khi quái va chạm vào vùng Trigger của Player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu đối tượng va chạm mang Tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Bạn chỉ gọi hàm báo hiệu Player bị dính đòn.
            // Thành viên quản lý Player sẽ chịu trách nhiệm trừ máu bên trong hàm TakeDamage() của họ.
            if (player != null)
            {
                //player.TakeDamage();
            }
        }
    }
}