using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StatsSystem.UI
{
    public class LevelUI : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private Slider levelProgressSlider;
        [SerializeField] private TextMeshProUGUI levelText; // Hiển thị "Lv. 1.5"

        private void Start()
        {
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.OnLevelChanged += UpdateLevelUI;

                if (QuestSaveSystem.Instance.duLieuSaveHienTai != null)
                {
                    UpdateLevelUI(QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.level);
                }
            }
        }

        private void OnDestroy()
        {
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.OnLevelChanged -= UpdateLevelUI;
            }
        }

        private void UpdateLevelUI(float currentLevel)
        {
            if (levelText != null)
            {
                // Hiển thị 1 chữ số thập phân (VD: Lv. 1.5)
                levelText.text = $"Lv. {currentLevel:F1}";
            }

            if (levelProgressSlider != null)
            {
                // Slider lấy phần thập phân lẻ làm tiến trình (VD: 1.5 -> value = 0.5)
                levelProgressSlider.value = currentLevel % 1f;
            }
        }
    }
}