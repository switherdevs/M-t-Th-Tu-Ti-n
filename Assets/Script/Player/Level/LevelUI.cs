using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StatsSystem.Components;

namespace StatsSystem.UI
{
    public class LevelUI : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private Slider expSlider;
        [SerializeField] private TextMeshProUGUI levelText; // Hiển thị "Lv. 1"
        [SerializeField] private TextMeshProUGUI expText;   // Hiển thị "0 / 5"

        private void Start()
        {
            // Kết nối Event ở Start() để đảm bảo LevelSystem.Instance đã khởi tạo xong!
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.OnExpChanged += UpdateLevelUI;

                // Cập nhật giao diện lần đầu tiên ngay khi vào game
                UpdateLevelUI(
                    LevelSystem.Instance.CurrentLevel,
                    LevelSystem.Instance.CurrentExp,
                    LevelSystem.Instance.MaxExp
                );
            }
            else
            {
                Debug.LogError("[LevelUI] Vẫn không tìm thấy LevelSystem.Instance! Kiểm tra lại LevelManager trong Scene.");
            }
        }

        private void OnDestroy()
        {
            // Hủy đăng ký Event khi bị Destroy để tránh rò rỉ bộ nhớ
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.OnExpChanged -= UpdateLevelUI;
            }
        }

        private void UpdateLevelUI(int level, float currentExp, float maxExp)
        {
            if (expSlider != null)
            {
                expSlider.maxValue = maxExp;
                expSlider.value = currentExp;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv. {level}";
            }

            if (expText != null)
            {
                expText.text = $"{currentExp} / {maxExp}";
            }
        }
    }
}