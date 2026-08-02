using StatsSystem.Components;
using UnityEngine;

public class TamLienTramKiem : MonoBehaviour
{
    [Header("Cấu Hình Rung Camera")]
    [SerializeField] private float shakeIntensity = 3f; // Cường độ rung
    [SerializeField] private float shakeDuration = 0.2f; // Thời gian rung (giây)

    private CharacterStats skillstat;

    public void Execute(SkillData skillData, Transform firePoint, Vector2 direction)
    {
        if (skillData == null || skillData.skillPrefab == null || firePoint == null) return;

        Debug.Log($"<color=cyan>[SKILL RIÊNG]</color> {skillData.skillName} bộc phát 3 đường dẻ quạt!");

        // 1. XỬ LÝ CỐT LÕI: GỌI RUNG CAM 1 LẦN DUY NHẤT VỚI BẢO HIỂM LỰC RUNG
        if (CameraShake.Instance != null)
        {
            // Kiểm tra nếu ngoài Inspector vô tình để bằng 0, ép về 3f và 0.2f
            float forceIntensity = shakeIntensity > 0f ? shakeIntensity : 3f;
            float forceDuration = shakeDuration > 0f ? shakeDuration : 0.2f;

            CameraShake.Instance.Shake(forceIntensity, forceDuration);
        }

        // 2. TÍNH TOÁN BẮN 3 VIÊN ĐẠN DẺ QUẠT
        float spreadAngle = 15f;
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            Quaternion rotationOffset = Quaternion.Euler(0, 0, angle);
            Vector2 spreadDirection = rotationOffset * direction;

            float swordAngle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;
            Quaternion swordRotation = Quaternion.Euler(0, 0, swordAngle);

            GameObject skillObj = Instantiate(skillData.skillPrefab, firePoint.position, swordRotation);

            // Khởi tạo các Component trên đạn (Không chứa bất kỳ lệnh Shake() nào nữa)
            BaseSkillEffect effect = skillObj.GetComponent<BaseSkillEffect>();
            if (effect != null)
            {
                effect.Initialize(spreadDirection);
            }

            PhiKiem scriptKiem = skillObj.GetComponent<PhiKiem>();
            if (scriptKiem != null)
            {
                scriptKiem.Setup(spreadDirection, skillstat);
            }

            Rigidbody2D rb = skillObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = spreadDirection * 12f;
            }

            Destroy(skillObj, 3f);
        }
    }
}