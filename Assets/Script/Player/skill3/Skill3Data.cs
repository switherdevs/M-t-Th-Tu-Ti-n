using UnityEngine;

[CreateAssetMenu(fileName = "Skill3Data", menuName = "Kỹ Năng/Ciu Thien Tram Dao Data")]
public class CiuThienTramDaoData : SkillData
{
    [Header("Cấu Hình Cửu Thiên Trảm Đao")]
    [Tooltip("Sát thương diện rộng của cự kiếm")]
    public float skillDamage = 80f;

    [Tooltip("Bán kính vùng gây sát thương AoE")]
    public float aoeRadius = 2.5f;

    [Tooltip("Thời gian làm choáng quái/boss (giây)")]
    public float stunDuration = 1.5f;

    public override void UseSkill(Transform firePoint, Vector2 direction)
    {
        if (skillPrefab == null) return;

        // Tìm tất cả kẻ địch xung quanh người chơi để chọn mục tiêu rơi cự kiếm lên đầu
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        // Chọn ngẫu nhiên hoặc chọn kẻ địch ở gần nhất
        Transform selectedEnemy = enemies[Random.Range(0, enemies.Length)].transform;

        if (selectedEnemy != null)
        {
            Debug.Log($"<color=magenta>[CỬU THIÊN TRẢM ĐAO]</color> Triệu hồi cự kiếm giáng xuống đầu {selectedEnemy.name}!");

            // Sinh ra Prefab Cự Kiếm
            GameObject skillObj = Instantiate(skillPrefab, selectedEnemy.position, Quaternion.identity);

            // Truyền thông số vào hiệu ứng
            CuuThienTramDao effect = skillObj.GetComponent<CuuThienTramDao>();
            if (effect != null)
            {
                effect.Setup(selectedEnemy, skillDamage, stunDuration, aoeRadius);
            }
        }
    }
}