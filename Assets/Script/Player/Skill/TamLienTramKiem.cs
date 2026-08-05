using System.Collections;
using StatsSystem.Components;
using UnityEngine;

public class TamLienTramKiem : MonoBehaviour
{
    [Header("Cấu Hình Rung Camera")]
    [SerializeField] private float shakeIntensity = 3f; // Cường độ rung
    [SerializeField] private float shakeDuration = 0.2f; // Thời gian rung (giây)

    [Header("Cấu Hình Kỹ Năng")]
    [SerializeField] private CharacterStats skillstat;

    public void Execute(SkillData skillData, Transform firePoint, Vector2 direction)
    {
        if (skillData == null || skillData.skillPrefab == null || firePoint == null) return;

        Debug.Log($"<color=cyan>[SKILL RIÊNG]</color> {skillData.skillName} bộc phát 3 đường dẻ quạt!");

        // 1. RUNG CAMERA - GỌI THẲNG QUA SINGLETON CameraShake, KHÔNG TỰ VIẾT LOGIC RIÊNG
        float forceIntensity = shakeIntensity > 0f ? shakeIntensity : 3f;
        float forceDuration = shakeDuration > 0f ? shakeDuration : 0.2f;

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(forceIntensity, forceDuration);
        }
        else
        {
            Debug.LogWarning("[TamLienTramKiem] Không tìm thấy CameraShake.Instance trong Scene! " +
                              "Hãy đảm bảo GameObject chứa script CameraShake (và component CinemachineBasicMultiChannelPerlin) đã tồn tại trong Scene.");
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