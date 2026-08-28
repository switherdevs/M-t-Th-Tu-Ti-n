using UnityEngine;

[CreateAssetMenu(fileName = "Skill3Data", menuName = "Kỹ Năng/Ciu Thien Tram Dao Data")]
public class CiuThienTramDaoData : SkillData
{
    [Header("Cấu Hình Cửu Thiên Trảm Đao")]
    [Tooltip("Sát thương diện rộng của cự kiếm")]
    public float skillDamage = 80f;

    [Tooltip("Bán kính vùng gây sát thương AoE khi kiếm chạm đất")]
    public float aoeRadius = 2.5f;

    [Tooltip("Thời gian làm choáng quái/boss (giây)")]
    public float stunDuration = 1.5f;

    [Header("Cấu Hình Bán Kính Tìm Kẻ Địch")]
    [Tooltip("Bán kính quét tìm kẻ địch xung quanh vị trí thi triển (Player)")]
    public float searchRadius = 8f;

    [Tooltip("LayerMask chứa kẻ địch (Đảm bảo chọn Layer Enemy)")]
    public LayerMask enemyLayer;

    public override void UseSkill(Transform firePoint, Vector2 direction)
    {
        if (skillPrefab == null || firePoint == null) return;

        // 1. Quét tìm tất cả kẻ địch trong bán kính searchRadius tính từ firePoint
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, searchRadius, enemyLayer);

        Transform selectedEnemy = null;

        if (hitEnemies.Length > 0)
        {
            // Chọn kẻ địch ở gần vị trí thi triển nhất
            float minDistance = float.MaxValue;
            foreach (var col in hitEnemies)
            {
                if (col.CompareTag("Enemy"))
                {
                    float dist = Vector2.Distance(firePoint.position, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        selectedEnemy = col.transform;
                    }
                }
            }
        }

        // 2. Kiểm tra nếu có kẻ địch trong bán kính thì triệu hồi rơi lên đầu, không có thì rơi tại điểm chỉ định trước mặt
        Vector3 targetPos = selectedEnemy != null ? selectedEnemy.position : (Vector3)(firePoint.position + (Vector3)direction * 3f);

        Debug.Log($"<color=magenta>[CỬU THIÊN TRẢM ĐAO]</color> Triệu hồi cự kiếm giáng xuống vị trí: {targetPos}!");

        // Sinh ra Prefab Cự Kiếm tại vị trí mục tiêu
        GameObject skillObj = Instantiate(skillPrefab, targetPos, Quaternion.identity);

        // Truyền thông số vào script điều khiển Cự Kiếm
        CuuThienTramDao effect = skillObj.GetComponent<CuuThienTramDao>();
        if (effect != null)
        {
            effect.Setup(selectedEnemy, targetPos, skillDamage, stunDuration, aoeRadius);
        }
    }
}