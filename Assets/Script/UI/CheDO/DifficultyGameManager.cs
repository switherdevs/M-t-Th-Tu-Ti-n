using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyGameManager : MonoBehaviour
{
    public enum DifficultyMode
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Asian = 3
    }

    [System.Serializable]
    public struct DifficultyModifier
    {
        [Tooltip("Số máu cộng/trừ thêm vào MaxHealth của quái (Ví dụ: -20 hoặc 50)")]
        public float bonusHealth;

        [Tooltip("Số phòng thủ cộng/trừ thêm vào Defense của quái (Ví dụ: -0.05 hoặc 0.1)")]
        public float bonusDefense;

        [Tooltip("Số sát thương cộng/trừ thêm vào Attack của quái trong CharacterStats")]
        public float bonusAttack;

        [Tooltip("Số sát thương cộng/trừ thêm trực tiếp vào vũ khí DamageDealer của quái")]
        public float bonusDamageDealer;
    }

    [Header("=== CẤU HÌNH THÔNG SỐ CHO 4 CHẾ ĐỘ ===")]
    [SerializeField] private DifficultyModifier easyMode = new DifficultyModifier { bonusHealth = -30f, bonusDefense = -0.05f, bonusAttack = -5f, bonusDamageDealer = -5f };
    [SerializeField] private DifficultyModifier normalMode = new DifficultyModifier { bonusHealth = 0f, bonusDefense = 0f, bonusAttack = 0f, bonusDamageDealer = 0f };
    [SerializeField] private DifficultyModifier hardMode = new DifficultyModifier { bonusHealth = 50f, bonusDefense = 0.1f, bonusAttack = 10f, bonusDamageDealer = 10f };
    [SerializeField] private DifficultyModifier asianMode = new DifficultyModifier { bonusHealth = 150f, bonusDefense = 0.25f, bonusAttack = 35f, bonusDamageDealer = 30f };

    [Header("=== CẤU HÌNH QUẢN LÝ QUÁI ===")]
    [SerializeField, Tooltip("Tag dùng để nhận diện quái vật")]
    private string enemyTag = "Enemy";

    [SerializeField, Tooltip("Tần suất (giây) quét và cập nhật DamageDealer trong Update")]
    private float updateInterval = 0.5f;

    private DifficultyModifier currentModifier;
    private HashSet<GameObject> processedEnemies = new HashSet<GameObject>();
    private float timer = 0f;

    private void Awake()
    {
        // 1. Đọc Chế Độ Khó Đã Lưu
        int savedModeIndex = PlayerPrefs.GetInt("GameDifficulty", (int)DifficultyMode.Normal);
        DifficultyMode mode = (DifficultyMode)savedModeIndex;

        // 2. Lấy Cấu Hình Thông Số Tương Ứng
        currentModifier = GetModifierByMode(mode);
    }

    private void Start()
    {
        // 3. Vừa Vào Game -> Áp Dụng Ngay Cho Các Quái Đang Có Mặt
        ApplyStatsToAllEnemies();
    }

    private void Update()
    {
        // 4. Dùng Timer Đếm Tần Suất Để Liên Tục Kiểm Tra & Cập Nhật Các DamageDealer Mới Sinh Ra (Quái Spawns, Đạn Bắn Ra...)
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateAllDamageDealers();
        }
    }

    /// <summary>
    /// Áp dụng chỉ số Máu, Giáp, Công cho toàn bộ GameObject có Tag Enemy khi vừa vào Game
    /// </summary>
    private void ApplyStatsToAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        foreach (GameObject enemy in enemies)
        {
            if (processedEnemies.Contains(enemy)) continue;

            CharacterStats stats = enemy.GetComponent<CharacterStats>();
            if (stats != null && !stats.IsPlayer)
            {
                // Cập nhật Máu
                float newMaxHP = Mathf.Max(1f, stats.MaxHealth.Value + currentModifier.bonusHealth);
                stats.MaxHealth.Value = newMaxHP;
                stats.SetCurrentHealth(newMaxHP);

                // Cập nhật Phòng thủ
                stats.Defense.Value = Mathf.Clamp(stats.Defense.Value + currentModifier.bonusDefense, 0f, 0.9f);

                // Cập nhật Tấn công cơ bản
                stats.Attack.Value = Mathf.Max(0f, stats.Attack.Value + currentModifier.bonusAttack);

                processedEnemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Liên tục kiểm tra các DamageDealer trên Scene để cộng/trừ sát thương theo độ khó
    /// </summary>
    private void UpdateAllDamageDealers()
    {
        DamageDealer[] dealers = FindObjectsByType<DamageDealer>(FindObjectsSortMode.None);

        foreach (DamageDealer dealer in dealers)
        {
            // Kiểm tra xem DamageDealer này có thuộc về Quái vật hay không (hoặc bắn vào Player)
            if (dealer.CompareTag(enemyTag) || dealer.transform.root.CompareTag(enemyTag))
            {
                // Gọi hàm AddBonusDamage có sẵn trong script DamageDealer của bạn
                dealer.AddBonusDamage(currentModifier.bonusDamageDealer);
            }
        }
    }

    private DifficultyModifier GetModifierByMode(DifficultyMode mode)
    {
        return mode switch
        {
            DifficultyMode.Easy => easyMode,
            DifficultyMode.Normal => normalMode,
            DifficultyMode.Hard => hardMode,
            DifficultyMode.Asian => asianMode,
            _ => normalMode
        };
    }
}