using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameCore.Settings
{
    public class SettingsUIController : MonoBehaviour
    {
        // Struct chứa dữ liệu của 1 phần tử đổi nút bấm trong mảng
        [Serializable]
        public struct RebindItem
        {
            public Button rebindButton;          // Button kích hoạt đổi phím
            public string targetActionName;      // Tên của Action trong Input System (vd: "Attack", "Move")
            public TextMeshProUGUI bindingText;  // Text hiển thị tên phím gán hiện tại
        }

        [Header("--- AUDIO SLIDERS & TEXTS ---")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [Space(5)]
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [Space(5)]
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;

        [Header("--- SENSITIVITY ---")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityValueText;

        [Header("--- REBIND KEYS ARRAY ---")]
        [SerializeField] private RebindItem[] rebindItems; // Mảng chứa thông tin các nút đổi phím

        [Header("--- ACTION BUTTONS ---")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;

        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

        private void OnEnable()
        {
            InitializeUIValues();
            RegisterListeners();
        }

        private void OnDisable()
        {
            UnregisterListeners();
            _rebindingOperation?.Dispose();
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

            // 2. Cập nhật độ nhạy chuột
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = 0.1f;
                mouseSensitivitySlider.maxValue = 5.0f;
                mouseSensitivitySlider.value = data.mouseSensitivity;
                UpdateSensitivityText(data.mouseSensitivity);
            }

            // 3. Cập nhật Text tên phím cho toàn bộ mảng rebindItems
            UpdateAllRebindTexts();
        }

        private void RegisterListeners()
        {
            // Xóa bớt listener cũ trước khi Add để đảm bảo không bị trùng lặp sự kiện
            UnregisterListeners();

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            // Đăng ký sự kiện Click cho mảng Rebind Buttons
            if (rebindItems != null)
            {
                for (int i = 0; i < rebindItems.Length; i++)
                {
                    int index = i; // Tạo biến cục bộ để tránh lỗi closure trong lambda loop
                    if (rebindItems[index].rebindButton != null)
                    {
                        rebindItems[index].rebindButton.onClick.AddListener(() => StartRebinding(index));
                    }
                }
            }

            if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        }

        private void UnregisterListeners()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

            // Gỡ bỏ Listener cho mảng Rebind Buttons một cách an toàn
            if (rebindItems != null)
            {
                for (int i = 0; i < rebindItems.Length; i++)
                {
                    if (rebindItems[i].rebindButton != null)
                    {
                        rebindItems[i].rebindButton.onClick.RemoveAllListeners();
                    }
                }
            }

            if (applyButton != null) applyButton.onClick.RemoveListener(OnApplyClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
        }

        // --- HÀM XỬ LÝ ÂM LƯỢNG ---
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

        // --- HÀM XỬ LÝ ĐỘ NHẠY CHUỘT ---
        private void OnMouseSensitivityChanged(float val)
        {
            SettingsManager.Instance?.SetMouseSensitivity(val);
            UpdateSensitivityText(val);
        }

        private void UpdateSensitivityText(float val)
        {
            if (sensitivityValueText != null) sensitivityValueText.text = val.ToString("F1");
        }

        // --- HÀM XỬ LÝ REBIND PHÍM BẰNG INDEX MẢNG ---
        private void StartRebinding(int itemIndex)
        {
            if (SettingsManager.Instance == null) return;
            InputActionAsset actions = SettingsManager.Instance.GetInputActions();
            if (actions == null) return;

            if (itemIndex < 0 || itemIndex >= rebindItems.Length) return;

            RebindItem item = rebindItems[itemIndex];
            InputAction action = actions.FindAction(item.targetActionName);
            if (action == null) return;

            action.Disable();
            if (item.bindingText != null) item.bindingText.text = "...";

            _rebindingOperation?.Dispose();
            _rebindingOperation = action.PerformInteractiveRebinding()
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    action.Enable();
                    _rebindingOperation.Dispose();
                    _rebindingOperation = null;
                    UpdateRebindTextAt(itemIndex);
                })
                .OnCancel(operation =>
                {
                    action.Enable();
                    _rebindingOperation.Dispose();
                    _rebindingOperation = null;
                    UpdateRebindTextAt(itemIndex);
                });

            _rebindingOperation.Start();
        }

        private void UpdateRebindTextAt(int index)
        {
            if (SettingsManager.Instance == null) return;
            if (index < 0 || index >= rebindItems.Length) return;

            InputActionAsset actions = SettingsManager.Instance.GetInputActions();
            if (actions == null) return;

            RebindItem item = rebindItems[index];
            if (item.bindingText == null) return;

            InputAction action = actions.FindAction(item.targetActionName);
            if (action != null)
            {
                item.bindingText.text = action.GetBindingDisplayString(0);
            }
        }

        private void UpdateAllRebindTexts()
        {
            if (rebindItems == null) return;
            for (int i = 0; i < rebindItems.Length; i++)
            {
                UpdateRebindTextAt(i);
            }
        }

        // --- ACTION BUTTONS ---
        private void OnApplyClicked()
        {
            SettingsManager.Instance?.SaveSettings();
        }

        private void OnCloseClicked()
        {
            SettingsManager.Instance?.SaveSettings();
            gameObject.SetActive(false);
        }

        private void OnResetClicked()
        {
            SettingsManager.Instance?.ResetToDefaults();
            InitializeUIValues();
        }
    }
}