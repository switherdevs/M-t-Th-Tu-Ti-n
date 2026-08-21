using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StatsSystem.Components;

namespace StatsSystem.UI
{
    public class LevelUI : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private Slider levelProgressSlider;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI expText;

        private void Start()
        {
            // 🎯 Đã sửa: Dùng Start() đảm bảo LevelSystem đã được khởi tạo Instance an toàn
            if (LevelSystem.Instance != null)
            {
                // Hủy đăng ký Event trước một lần để chống lỗi bị nhân đôi Event
                LevelSystem.Instance.OnExpChanged -= UpdateExpDetailUI;

                // Đăng ký Event lấy EXP chuẩn
                LevelSystem.Instance.OnExpChanged += UpdateExpDetailUI;

                // Cập nhật UI ngay lập tức bằng thông số hiện tại
                UpdateExpDetailUI(
                    LevelSystem.Instance.CurrentLevel,
                    LevelSystem.Instance.CurrentExp,
                    LevelSystem.Instance.MaxExp
                );
            }
            else
            {
                Debug.LogError("[LevelUI] LỖI: Không tìm thấy LevelSystem! Hãy đảm bảo GameObject chứa LevelSystem đã được bật.");
            }
        }

        private void OnDestroy()
        {
            // Giải phóng bộ nhớ khi Player/UI bị xóa khỏi Scene
            if (LevelSystem.Instance != null)
            {
                LevelSystem.Instance.OnExpChanged -= UpdateExpDetailUI;
            }
        }

        private void UpdateExpDetailUI(int level, float currentExp, float maxExp)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv. {level}";
            }

            if (expText != null)
            {
                expText.text = $"{Mathf.RoundToInt(currentExp)} / {Mathf.RoundToInt(maxExp)}";
            }

            if (levelProgressSlider != null && maxExp > 0)
            {
                // Ép trực tiếp giá trị giới hạn của Slider theo maxExp, miễn nhiễm lỗi kéo sai trong Inspector
                levelProgressSlider.minValue = 0f;
                levelProgressSlider.maxValue = maxExp;
                levelProgressSlider.value = currentExp;
            }
        }
    }
}