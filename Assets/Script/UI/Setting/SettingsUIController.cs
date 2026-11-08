using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Settings
{
    public class SettingsUIController : MonoBehaviour
    {
        [Header("--- LANGUAGE BUTTONS ---")]
        [SerializeField] private Button vietnameseButton;
        [SerializeField] private Button englishButton;
        [SerializeField] private Color selectedButtonColor = Color.green;
        [SerializeField] private Color normalButtonColor = Color.white;

        [Header("--- AUDIO SLIDERS & TEXTS ---")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [Space(5)]
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [Space(5)]
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;

        [Header("--- ACTION BUTTONS ---")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;

        private void Awake()
        {
            // Đăng ký lắng nghe sự kiện ngay khi script được nạp vào bộ nhớ
            RegisterListeners();
        }

        private void OnEnable()
        {
            InitializeUIValues();
            SettingsManager.OnLanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSettingsUI();
            }
        }

        /// <summary>
        /// Hàm đóng bảng Settings chuẩn xác, lưu dữ liệu và ẩn GameObject
        /// </summary>
        public void CloseSettingsUI()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SaveSettings();
            }
            gameObject.SetActive(false);
        }

        private void InitializeUIValues()
        {
            if (SettingsManager.Instance == null) return;

            SettingsData data = SettingsManager.Instance.CurrentData;

            // 1. Cập nhật thanh trượt và Text âm lượng
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = data.masterVolume;
                UpdateAudioText(masterVolumeText, data.masterVolume);
            }
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = data.bgmVolume;
                UpdateAudioText(bgmVolumeText, data.bgmVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = data.sfxVolume;
                UpdateAudioText(sfxVolumeText, data.sfxVolume);
            }

            // 2. Cập nhật trạng thái màu sắc 2 nút ngôn ngữ dựa theo dữ liệu đã lưu
            UpdateLanguageButtonsUI(data.language);
        }

        private void RegisterListeners()
        {
            UnregisterListeners();

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (vietnameseButton != null) vietnameseButton.onClick.AddListener(() => ChangeLanguage("VN"));
            if (englishButton != null) englishButton.onClick.AddListener(() => ChangeLanguage("EN"));

            if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        }

        private void UnregisterListeners()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

            if (vietnameseButton != null) vietnameseButton.onClick.RemoveAllListeners();
            if (englishButton != null) englishButton.onClick.RemoveAllListeners();

            if (applyButton != null) applyButton.onClick.RemoveListener(OnApplyClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
        }

        private void ChangeLanguage(string langCode)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetLanguage(langCode);
                UpdateLanguageButtonsUI(langCode);
            }
        }

        private void HandleLanguageChanged(string newLang)
        {
            UpdateLanguageButtonsUI(newLang);
        }

        private void UpdateLanguageButtonsUI(string langCode)
        {
            bool isVN = (langCode == "VN");

            if (vietnameseButton != null)
            {
                Image img = vietnameseButton.GetComponent<Image>();
                if (img != null) img.color = isVN ? selectedButtonColor : normalButtonColor;
            }

            if (englishButton != null)
            {
                Image img = englishButton.GetComponent<Image>();
                if (img != null) img.color = !isVN ? selectedButtonColor : normalButtonColor;
            }
        }

        private void OnMasterVolumeChanged(float val)
        {
            SettingsManager.Instance?.SetMasterVolume(val);
            UpdateAudioText(masterVolumeText, val);
        }

        private void OnBGMVolumeChanged(float val)
        {
            SettingsManager.Instance?.SetBGMVolume(val);
            UpdateAudioText(bgmVolumeText, val);
        }

        private void OnSFXVolumeChanged(float val)
        {
            SettingsManager.Instance?.SetSFXVolume(val);
            UpdateAudioText(sfxVolumeText, val);
        }

        private void UpdateAudioText(TextMeshProUGUI targetText, float val)
        {
            if (targetText != null)
            {
                targetText.text = Mathf.RoundToInt(val * 100f) + "%";
            }
        }

        private void OnApplyClicked()
        {
            SettingsManager.Instance?.SaveSettings();
        }

        private void OnCloseClicked()
        {
            CloseSettingsUI();
        }

        private void OnResetClicked()
        {
            SettingsManager.Instance?.ResetToDefaults();
            InitializeUIValues();
        }
    }
}