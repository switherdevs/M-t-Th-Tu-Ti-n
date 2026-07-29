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
        [SerializeField] private float maxExp = 5f; // Ban đầu cần 5 EXP để lên cấp (chỉnh được trong Inspector)
        [SerializeField] private float expGrowthPerLevel = 1f; // Tăng thêm 1 MAX EXP sau mỗi cấp

        // Event thông báo cho UI hoặc các hệ thống khác khi EXP hoặc Level thay đổi
        public event Action<int, float, float> OnExpChanged; // (level, currentExp, maxExp)
        public event Action<int> OnLevelUp; // (newLevel)

        public int CurrentLevel => currentLevel;
        public float CurrentExp => currentExp;
        public float MaxExp => maxExp;

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
            // Khởi tạo UI ban đầu
            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);
        }

        /// <summary>
        /// Gọi hàm này mỗi khi hạ gục 1 kẻ địch
        /// </summary>
        public void AddExp(float amount = 1f)
        {
            currentExp += amount;

            // Kiểm tra lên cấp (dùng while phòng trường hợp nhận lượng EXP cực lớn vượt nhiều cấp)
            while (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                currentLevel++;
                maxExp += expGrowthPerLevel; // +1 maxExp sau khi lên cấp (ví dụ: 5 -> 6 -> 7...)

                Debug.Log($"<color=cyan>[LEVEL UP!]</color> Chúc mừng! Bạn đã đạt Level {currentLevel}!");
                OnLevelUp?.Invoke(currentLevel);
            }

            // Cập nhật lại UI Slider / Text
            OnExpChanged?.Invoke(currentLevel, currentExp, maxExp);
        }
    }
}