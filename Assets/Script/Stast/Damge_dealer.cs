using UnityEngine;
using StatsSystem.Components; // BẮT BULOG: Gọi namespace chứa CharacterStats

public class DamageDealer : MonoBehaviour
{
    [Header("=== THÔNG SỐ SÁT THƯƠNG ===")]
    [Tooltip("Lượng sát thương cơ bản vũ khí này gây ra")]
    [SerializeField] private float baseDamage = 30f;

    [Tooltip("Tag của đối tượng mà đạn/vũ khí này được phép gây sát thương (Vd: Enemy, Player)")]
    [SerializeField] private string targetTag = "Enemy";

    [Header("=== LOẠI MỤC TIÊU CẦN TÍCH (QUYẾT ĐỊNH MÀU TEXT) ===")]
    [Tooltip("Tích chọn nếu vũ khí này gây sát thương lên Player -> Text hiển thị màu ĐỎ")]
    [SerializeField] private bool isTargetPlayer = false;

    [Tooltip("Tích chọn nếu vũ khí này gây sát thương lên Enemy -> Text hiển thị màu TRẮNG")]
    [SerializeField] private bool isTargetEnemy = true;

    [Header("=== POPUP SÁT THƯƠNG ===")]
    [Tooltip("Prefab TextMeshPro Popup sát thương")]
    [SerializeField] private GameObject damagePopupPrefab;

    // Biến lưu lượng sát thương cộng thêm từ độ khó
    private float bonusDamage = 0f;

    /// <summary>
    /// Hàm nhận lượng sát thương được cộng thêm từ EnemyDifficultyManager
    /// </summary>
    public void AddBonusDamage(float amount)
    {
        bonusDamage += amount;
    }

    // Hàm mặc định của Unity, kích hoạt khi có 1 Collider2D khác chạm vào (Cần tick isTrigger ở Collider)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra xem thứ vừa chạm có đúng là mục tiêu mình muốn đánh không
        if (!collision.CompareTag(targetTag)) return;

        // 2. Tìm script CharacterStats trên đối tượng bị đánh trúng
        CharacterStats targetStats = collision.GetComponentInParent<CharacterStats>();

        // 3. Nếu tìm thấy script (Nghĩa là cục này có máu, có thể nhận sát thương)
        if (targetStats != null)
        {
            // Truyền tổng sát thương (Gốc + Cộng thêm từ độ khó) vào hàm TakeDamage
            float finalDamage = baseDamage + bonusDamage;
            targetStats.TakeDamage(finalDamage);

            // Tính điểm va chạm thực tế trên bề mặt Collider
            Vector3 hitPoint = collision.ClosestPoint(transform.position);

            // XÁC ĐỊNH MÀU SẮC DỰA VÀO CHECKBOX BOOL TRÊN INSPECTOR:
            // Sát thương lên Player = MÀU ĐỎ | Sát thương lên Enemy = MÀU TRẮNG
            Color popupColor = Color.white; // Màu mặc định

            if (isTargetPlayer)
            {
                popupColor = Color.red;    // Đánh Player -> Màu Đỏ
            }
            else if (isTargetEnemy)
            {
                popupColor = Color.white;  // Đánh Enemy -> Màu Trắng
            }

            // Hiển thị Popup ngay tại vị trí tiếp xúc
            SpawnDamagePopup(finalDamage, hitPoint, popupColor);

            // Xóa đạn sau khi gây sát thương (Nếu đây là đạn bắn ra)
            // Destroy(gameObject); 
        }
    }

    /// <summary>
    /// Hàm sinh ra Popup sát thương và truyền màu đã chọn sang DamagePopup
    /// </summary>
    private void SpawnDamagePopup(float damageAmount, Vector3 spawnPosition, Color textColor)
    {
        if (damagePopupPrefab == null) return;

        // Sinh ra Prefab Popup ngay tại vị trí va chạm tiếp xúc
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);

        // Lấy script DamagePopup và truyền thông số
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            popupScript.Setup(damageAmount, textColor);
        }
    }
}