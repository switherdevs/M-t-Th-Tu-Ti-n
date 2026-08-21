using System;
using UnityEngine;

namespace StatsSystem.Components
{
    public class LevelSystem : MonoBehaviour
    {
        public static LevelSystem Instance { get; private set; }

        [Header("=== LEVEL & EXP CONFIG ===")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentExp = 0f;
        [SerializeField] private float maxExp = 5f;

        [Header("=== CẤU HÌNH RANDOM ĐỘT PHÁ ===")]
        [SerializeField] private float expMultiplier = 1.5f;
        [SerializeField] private float minRandomNgoTinh = 0.8f;
        [SerializeField] private float maxRandomNgoTinh = 1.2f;

        public event Action<int, float, float> OnExpChanged;
        public event Action<int> OnLevelUp;

        public int CurrentLevel => currentLevel;
        public float CurrentExp => currentExp;
        public float MaxExp => maxExp;

        private bool isLoaded = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LoadExpFromSave();
        }

        /// <summary>
        /// Nạp dữ liệu Level & EXP từ QuestSaveSystem
        /// </summary>
        public void LoadExpFromSave()
        {
            if (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null)
            {
                var stats = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

                if (stats != null)
                {
                    currentLevel = Mathf.Max(1, (int)stats.level);
                    currentExp = stats.currentExp;
                    maxExp = stats.maxExp > 0 ? stats.maxExp : 5f;

                    isLoaded = true;
                    Debug.Log($"<color=green>[LevelSystem]</color> Load thành công: Level {currentLevel} | EXP: {currentExp}/{maxExp}");
                }
            }
            else
            {
                Debug.LogWarning("[LevelSystem] Chưa tìm thấy QuestSaveSystem.Instance. Đang giữ nguyên thông số mặc định.");
            }

            // Báo UI cập nhật lại thông số ngay lập tức
            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);
        }

        /// <summary>
        /// Đồng bộ dữ liệu hiện tại vào QuestSaveSystem
        /// </summary>
        public void SaveExpToSaveSystem()
        {
            if (!isLoaded) return; // Bảo vệ: Không lưu khi chưa load dữ liệu cũ xong

            if (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null)
            {
                var stats = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;
                if (stats != null)
                {
                    stats.level = currentLevel;
                    stats.currentExp = currentExp;
                    stats.maxExp = maxExp;

                    QuestSaveSystem.Instance.SaveDuLieuQuestToTxt();
                }
            }
        }

        public void AddExp(float amount = 1f)
        {
            if (amount <= 0) return;

            currentExp += amount;

            while (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                currentLevel++;

                CalculateNextMaxExp();

                Debug.Log($"<color=cyan>[LEVEL UP!]</color> Bạn đạt Level {currentLevel}! MaxEXP mới: {maxExp}");
                OnLevelUp?.Invoke(currentLevel);
            }

            // Cập nhật UI ngay lập tức
            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);

            // Ghi dữ liệu vừa cập nhật vào File Save
            SaveExpToSaveSystem();
        }

        private void CalculateNextMaxExp()
        {
            float baseNextExp = maxExp * expMultiplier;
            float randomNgoTinh = UnityEngine.Random.Range(minRandomNgoTinh, maxRandomNgoTinh);
            maxExp = Mathf.Round(baseNextExp * randomNgoTinh);
        }
    }
}