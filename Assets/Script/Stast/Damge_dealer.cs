using UnityEngine;

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

    // Getter công khai để các script khác đọc baseDamage chuẩn xác
    public float BaseDamage => baseDamage;

    /// <summary>
    /// Hàm nhận lượng sát thương được cộng thêm từ EnemyDifficultyManager
    /// </summary>
    public void AddBonusDamage(float amount)
    {
        bonusDamage += amount;
    }

    // Hàm mặc định của Unity, kích hoạt khi có 1 Collider2D khác chạm vào
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra xem thứ vừa chạm có đúng là mục tiêu mình muốn đánh không
        if (!collision.CompareTag(targetTag)) return;

        // 2. Tìm script CharacterStats trên đối tượng bị đánh trúng
        CharacterStats targetStats = collision.GetComponentInParent<CharacterStats>();

        // 3. Nếu tìm thấy script (Nghĩa là mục tiêu có máu)
        if (targetStats != null)
        {
            // SÁT THƯƠNG GỐC CỦA ĐẠN/VŨ KHÍ
            float totalDamage = baseDamage + bonusDamage;

            // 🎯 SỬA LỖI 1: CHỈ TÌM OWNER LÀ PLAYER NẾU ĐÂY LÀ ĐẠN DÀNH ĐỂ ĐÁNH ENEMY
            CharacterStats ownerStats = GetComponentInParent<CharacterStats>();

            if (isTargetEnemy && ownerStats == null)
            {
                CharacterStats[] allStats = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
                foreach (var stat in allStats)
                {
                    if (stat.IsPlayer)
                    {
                        ownerStats = stat;
                        break;
                    }
                }
            }

            // CHỈ CỘNG THÊM ATTACK CỦA PLAYER NẾU ĐÂY LÀ ĐẠN CỦA PLAYER
            if (isTargetEnemy && ownerStats != null && ownerStats.IsPlayer)
            {
                totalDamage += ownerStats.Attack.Value;
            }

            float finalDamage = totalDamage;

            // 🎯 SỬA LỖI 2: CHỈ ÁP DỤNG CHEAT DAMAGE CHO ĐẠN CỦA PLAYER ĐÁNH QUÁI
            if (isTargetEnemy && CheatItemSystem.IsDamageCheatActive)
            {
                finalDamage *= CheatItemSystem.DamageHeSoNhan;
            }

            targetStats.TakeDamage(finalDamage);

            // Tính điểm va chạm thực tế trên bề mặt Collider
            Vector3 hitPoint = collision.ClosestPoint(transform.position);

            // XÁC ĐỊNH MÀU SẮC DỰA VÀO CHECKBOX BOOL TRÊN INSPECTOR
            Color popupColor = isTargetPlayer ? Color.red : Color.white;

            // Hiển thị Popup ngay tại vị trí tiếp xúc
            SpawnDamagePopup(finalDamage, hitPoint, popupColor);
        }
    }

    /// <summary>
    /// Hàm sinh ra Popup sát thương và truyền màu đã chọn sang DamagePopup
    /// </summary>
    private void SpawnDamagePopup(float damageAmount, Vector3 spawnPosition, Color textColor)
    {
        if (damagePopupPrefab == null) return;

        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);

        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            popupScript.Setup(damageAmount, textColor);
        }
    }

    /// <summary>
    /// Hàm công khai để PhiKiemGoiVe.cs kích hoạt Popup với sát thương đã x2
    /// </summary>
    public void HienThiPopupGoiVe(float satThuongGoiVe, Vector3 viTriVaCham)
    {
        Color mauPopup = isTargetPlayer ? Color.red : Color.white;

        // Nếu bật Cheat Damage và đạn này là của Player
        if (isTargetEnemy && CheatItemSystem.IsDamageCheatActive)
        {
            satThuongGoiVe *= CheatItemSystem.DamageHeSoNhan;
        }

        SpawnDamagePopup(satThuongGoiVe, viTriVaCham, mauPopup);
    }
}