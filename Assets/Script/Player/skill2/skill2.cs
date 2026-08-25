using UnityEngine;

[CreateAssetMenu(fileName = "VanKiemBatPhuongData", menuName = "Kỹ Năng/Van Kiem Bat Phuong Data")]
public class VanKiemBatPhuongData : SkillData
{
    [Header("Cấu Hình Vạn Kiếm Bát Phương")]
    [Tooltip("Số lượng kiếm tỏa ra 360 độ")]
    public int numberOfSwords = 12;

    [Tooltip("Tốc độ bay của sóng kiếm tỏa ra")]
    public float swordSpeed = 10f;

    [Tooltip("Thời gian quái bị đứng yên (Stun)")]
    public float stunDuration = 2f;

    [Tooltip("Sát thương của mỗi tia kiếm")]
    public float skillDamage = 25f;

    // Ghi đè phương thức UseSkill để thực thi logic 360 độ đặc thù của skill này
    public override void UseSkill(Transform firePoint, Vector2 direction)
    {
        if (skillPrefab == null || firePoint == null) return;

        Debug.Log($"<color=yellow>[SKILL 360 ĐỘ]</color> {skillName} bùng nổ {numberOfSwords} hướng xung quanh!");

        // Chia đều 360 độ dựa trên số lượng kiếm
        float angleStep = 360f / numberOfSwords;

        for (int i = 0; i < numberOfSwords; i++)
        {
            float currentAngle = i * angleStep;
            float rad = currentAngle * Mathf.Deg2Rad;

            // Tính toán hướng tỏa ra 360 độ từ vị trí player
            Vector2 spreadDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            // Tính góc xoay trên trục Z theo hướng bay spreadDirection
            float angle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // Sinh ra Prefab kỹ năng tại vị trí nhân vật kèm góc xoay 360 độ chuẩn
            GameObject skillObj = Instantiate(skillPrefab, firePoint.position, rotation);

            // Gán thông số và hướng cho script hiệu ứng (Script VanKiemBatPhuong sẽ tự lo phần tỏa ra và tụ lao đi)
            VanKiemBatPhuong effect = skillObj.GetComponent<VanKiemBatPhuong>();
            if (effect != null)
            {
                effect.Setup(spreadDirection, skillDamage, stunDuration, null);
            }
        }
    }
}