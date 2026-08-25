using UnityEngine;

[CreateAssetMenu(fileName = "TamLienTramData", menuName = "Skills/Tam Lien Tram Skill")]
public class TamLienTramSkillData : SkillData
{
    [Header("=== CẤU HÌNH TẢN ĐẠN & TỐC ĐỘ ===")]
    [Tooltip("Góc xòe của 2 viên đạn bên cạnh (Độ)")]
    [SerializeField] private float spreadAngle = 15f;

    [Tooltip("Tốc độ di chuyển của viên đạn / kiếm")]
    [SerializeField] private float projectileSpeed = 18f;

    [Tooltip("Thời gian tồn tại tối đa trước khi tự hủy (giây)")]
    [SerializeField] private float projectileLifeTime = 3f;

    [Header("=== HIỆU ỨNG TẠI VỊ TRÍ BẮN (MUZZLE EFFECT) ===")]
    [Tooltip("Prefab hiệu ứng bùng nổ/lóe sáng xuất hiện cùng lúc khi bắn (Tùy chọn)")]
    [SerializeField] private GameObject spawnVfxPrefab;

    [Header("=== CẤU HÌNH RUNG CAMERA ===")]
    [Tooltip("Cường độ rung (Intensity)")]
    [SerializeField] private float shakeAmplitude = 2.5f;
    [Tooltip("Thời gian rung camera (giây)")]
    [SerializeField] private float shakeDuration = 0.15f;

    /// <summary>
    /// Override hàm UseSkill từ class cha SkillData
    /// </summary>
    public override void UseSkill(Transform firePoint, Vector2 direction)
    {
        if (skillPrefab == null)
        {
            Debug.LogError($"[TamLienTramSkillData] Thiếu skillPrefab trên ScriptableObject: {skillName}");
            return;
        }

        if (firePoint == null) return;

        // 1. TẠO HIỆU ỨNG TẠI VỊ TRÍ BẮN (SPAWN VFX)
        if (spawnVfxPrefab != null)
        {
            float fireAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Instantiate(spawnVfxPrefab, firePoint.position, Quaternion.Euler(0, 0, fireAngle));
        }

        // 2. KÍCH HOẠT RUNG CAMERA BẰNG SINGLETON CAMERASHAKE
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeAmplitude, shakeDuration);
        }

        // 3. TÍNH TOÁN VÀ SINH RA 3 VIÊN ĐẠN / KIẾM
        float[] angles = { 0f, -spreadAngle, spreadAngle };

        foreach (float angle in angles)
        {
            // Tính hướng bay tỏa ra
            Quaternion rotationOffset = Quaternion.Euler(0, 0, angle);
            Vector2 finalDirection = (rotationOffset * direction).normalized;

            // Tính góc xoay Z cho Sprite
            float swordAngle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
            Quaternion swordRotation = Quaternion.Euler(0, 0, swordAngle);

            // Sinh ra Prefab
            GameObject projObj = Instantiate(skillPrefab, firePoint.position, swordRotation);

            // Gán tốc độ và hướng cho viên đạn
            SkillProjectile projScript = projObj.GetComponent<SkillProjectile>();
            if (projScript != null)
            {
                projScript.Setup(finalDirection, projectileSpeed, projectileLifeTime);
            }
            else
            {
                Debug.LogWarning($"[TamLienTramSkillData] Prefab '{skillPrefab.name}' chưa được gắn script SkillProjectile!");
            }
        }
    }
}