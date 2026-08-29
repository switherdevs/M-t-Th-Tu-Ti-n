using UnityEngine;
using StatsSystem.Components; // Gọi namespace chứa CharacterStats

public class EnemyDifficultyManager : MonoBehaviour
{
    [System.Serializable]
    public class DifficultyConfig
    {
        [Tooltip("Tên độ khó (VD: Dễ, Thường, Khó, Ác Mộng)")]
        public string doKhoName = "Thường";

        [Tooltip("Tích vào đây để áp dụng cấu hình độ khó này cho Scene")]
        public bool isUsed = false;

        [Header("--- THÔNG SỐ CỘNG THÊM ---")]
        [Tooltip("Lượng Máu cộng thêm vào CharacterStats của Quái")]
        public float bonusHealth = 50f;

        [Tooltip("Lượng Giáp cộng thêm vào CharacterStats của Quái")]
        public float bonusDefense = 0.05f;

        [Tooltip("Lượng Sát thương cộng thêm vào DamageDealer của Quái (áp dụng cả cho Đạn spawn sau này)")]
        public float bonusDamage = 15f;
    }

    [Header("=== CẤU HÌNH ĐỘ KHÓ ===")]
    [Tooltip("Danh sách các cấp độ khó")]
    public DifficultyConfig[] difficultyConfigs;

    [Header("=== THIẾT LẬP PHÁT HIỆN QUÁI ===")]
    [Tooltip("Tag dùng để nhận diện Quái vật trong Scene")]
    [SerializeField] private string enemyTag = "Enemy";

    // Biến static lưu sát thương cộng thêm để mọi DamageDealer đạn sinh ra sau này tự truy cập
    public static float GlobalBonusDamage { get; private set; } = 0f;

    private void Awake()
    {
        // Reset lại sát thương cộng thêm về 0 mỗi khi load Scene mới
        GlobalBonusDamage = 0f;
    }

    private void Start()
    {
        ApplyDifficultyToScene();
    }

    /// <summary>
    /// Hàm áp dụng Máu/Giáp 1 lần duy nhất lúc đầu game và lưu Sát thương toàn cục
    /// </summary>
    public void ApplyDifficultyToScene()
    {
        if (difficultyConfigs == null || difficultyConfigs.Length == 0) return;

        // 1. Tìm cấu hình độ khó được tích `isUsed`
        DifficultyConfig activeConfig = null;
        foreach (var config in difficultyConfigs)
        {
            if (config != null && config.isUsed)
            {
                activeConfig = config;
                break; // Lấy độ khó đầu tiên được tích
            }
        }

        if (activeConfig == null)
        {
            Debug.LogWarning("[EnemyDifficultyManager] Không có cấp độ khó nào được tích 'isUsed'!");
            return;
        }

        // 2. Lưu lại chỉ số Sát thương vào biến Static cho toàn bộ Đạn/Vũ khí spawn sau này
        GlobalBonusDamage = activeConfig.bonusDamage;

        // 3. TĂNG MÁU VÀ GIÁP 1 LẦN DUY NHẤT cho các Quái vật đang có sẵn trong Scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        foreach (GameObject enemyObj in enemies)
        {
            CharacterStats enemyStats = enemyObj.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                // Cộng thêm Máu tối đa và cập nhật lại Máu hiện tại
                enemyStats.MaxHealth.Value += activeConfig.bonusHealth;
                enemyStats.Heal(activeConfig.bonusHealth);

                // Cộng thêm Giáp
                enemyStats.Defense.Value += activeConfig.bonusDefense;
            }
        }

        Debug.Log($"<color=cyan>[EnemyDifficultyManager]</color> Đã áp dụng độ khó: <b>{activeConfig.doKhoName}</b>! Máu/Giáp đã cộng 1 lần cho {enemies.Length} quái, Sát thương đạn (+{GlobalBonusDamage}) sẽ tự động cộng cho mọi viên đạn spawn ra.");
    }
}