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
        [SerializeField] private float expMultiplier = 1.2f;
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

            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);
        }

        public void SaveExpToSaveSystem()
        {
            if (!isLoaded) return;

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

            // Đảm bảo maxExp luôn lớn hơn 0 để tránh chia cho 0 hoặc lặp vô tận
            if (maxExp <= 0) maxExp = 5f;

            currentExp += amount;

            // 🎯 CHỐT BẢO VỆ CHỐNG ĐÓNG BĂNG UNITY: Đếm số vòng lặp tối đa
            int loopSafetyCounter = 0;
            int maxAllowedLoops = 100; // Giới hạn tối đa 100 level/lần cộng exp

            while (currentExp >= maxExp && loopSafetyCounter < maxAllowedLoops)
            {
                loopSafetyCounter++;
                currentExp -= maxExp;
                currentLevel++;

                CalculateNextMaxExp();

                Debug.Log($"<color=cyan>[LEVEL UP!]</color> Bạn đạt Level {currentLevel}! MaxEXP mới: {maxExp}");
                OnLevelUp?.Invoke(currentLevel);
            }

 

            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);
            SaveExpToSaveSystem();
        }

        private void CalculateNextMaxExp()
        {
            float baseNextExp = maxExp * expMultiplier;
            float randomNgoTinh = UnityEngine.Random.Range(minRandomNgoTinh, maxRandomNgoTinh);
            maxExp = Mathf.Round(baseNextExp * randomNgoTinh);

            // 🎯 Ép maxExp tối thiểu phải là 1 để tránh maxExp = 0 gây treo loop
            if (maxExp <= 0) maxExp = 1f;
        }
    }
}