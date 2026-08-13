using UnityEngine;
using StatsSystem.Components; // BẮT BUỘC: Gọi namespace chứa CharacterStats

public class DamageDealer : MonoBehaviour
{
    [Header("=== THÔNG SỐ SÁT THƯƠNG ===")]
    [Tooltip("Lượng sát thương cơ bản vũ khí này gây ra")]
    [SerializeField] private float baseDamage = 30f;

    [Tooltip("Tag của đối tượng mà đạn/vũ khí này được phép gây sát thương (Vd: Enemy, Player)")]
    [SerializeField] private string targetTag = "Enemy";

    // Hàm mặc định của Unity, kích hoạt khi có 1 Collider2D khác chạm vào (Cần tick isTrigger ở Collider)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra xem thứ vừa chạm có đúng là mục tiêu mình muốn đánh không
        if (!collision.CompareTag(targetTag)) return;

        // 2. Tìm script CharacterStats trên đối tượng bị đánh trúng
        // Dùng GetComponentInParent phòng trường hợp Hitbox nằm ở Object con, còn Script Máu nằm ở Object cha
        CharacterStats targetStats = collision.GetComponentInParent<CharacterStats>();

        // 3. Nếu tìm thấy script (Nghĩa là cục này có máu, có thể nhận sát thương)
        if (targetStats != null)
        {
            // Truyền lượng sát thương vào hàm TakeDamage
            targetStats.TakeDamage(baseDamage);
            
            // Xóa đạn sau khi gây sát thương (Nếu đây là đạn bắn ra)
            // Destroy(gameObject); 
        }
    }
}