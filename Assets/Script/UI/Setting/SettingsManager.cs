using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace GameCore.Settings
{
    /// <summary>
    /// Manager singleton quản lý Load/Save cài đặt (Audio, Mouse Sensitivity, Rebind Key)
    /// ĐÃ LOẠI BỎ DontDestroyOnLoad để quản lý theo LifeCycle thông thường của Scene.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SettingsManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("[SettingsManager]");
                        _instance = container.AddComponent<SettingsManager>();
                        // ĐÃ BỎ: DontDestroyOnLoad(container);
                    }
                }
                return _instance;
            }
        }

        [Header("--- AUDIO SETTINGS ---")]
        [SerializeField] private AudioMixer mainAudioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string bgmVolumeParam = "BGMVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        [Header("--- INPUT SETTINGS (Unity 6 Input System) ---")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("--- SAVE CONFIG ---")]
        [SerializeField] private string saveFileName = "settings.txt";

        public SettingsData CurrentData { get; private set; } = new SettingsData();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private string TempSaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName + ".tmp");

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            // ĐÃ BỎ: DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        public void SaveSettings()
        {
            try
            {
                // Lưu rebind overrides của Input System nếu có
                if (inputActions != null)
                {
                    CurrentData.inputOverridesJson = inputActions.SaveBindingOverridesAsJson();
                }

                string json = JsonUtility.ToJson(CurrentData, true);
                File.WriteAllText(TempSaveFilePath, json);

                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                File.Move(TempSaveFilePath, SaveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] Lỗi khi lưu file settings: {ex.Message}");
            }
        }

        public void LoadSettings()
        {
            if (!File.Exists(SaveFilePath))
            {
                CurrentData = new SettingsData();
                ApplyAllSettings();
                SaveSettings();
                return;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    CurrentData = new SettingsData();
                }
                else
                {
                    SettingsData loaded = JsonUtility.FromJson<SettingsData>(json);
                    CurrentData = loaded ?? new SettingsData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SettingsManager] Lỗi khi đọc file settings, dùng mặc định: {ex.Message}");
                CurrentData = new SettingsData();
            }

            ApplyAllSettings();
        }

        public void ApplyAllSettings()
        {
            SetMasterVolume(CurrentData.masterVolume);
            SetBGMVolume(CurrentData.bgmVolume);
            SetSFXVolume(CurrentData.sfxVolume);

            if (inputActions != null && !string.IsNullOrEmpty(CurrentData.inputOverridesJson))
            {
                inputActions.LoadBindingOverridesFromJson(CurrentData.inputOverridesJson);
            }
        }

        // --- AUDIO HELPERS ---
        // Chuẩn hóa công thức Log10 tiêu chuẩn tránh hiện tượng méo âm thanh thanh do vượt ngưỡng 0dB
        private float LinearToDecibel(float linear)
        {
            linear = Mathf.Clamp(linear, 0.0001f, 1f);
            return Mathf.Log10(linear) * 20f;
        }

        public void SetMasterVolume(float value)
        {
            CurrentData.masterVolume = Mathf.Clamp01(value);
            if (mainAudioMixer != null)
            {
                mainAudioMixer.SetFloat(masterVolumeParam, LinearToDecibel(CurrentData.masterVolume));
            }
        }

        public void SetBGMVolume(float value)
        {
            CurrentData.bgmVolume = Mathf.Clamp01(value);
            if (mainAudioMixer != null)
            {
                mainAudioMixer.SetFloat(bgmVolumeParam, LinearToDecibel(CurrentData.bgmVolume));
            }
        }

        public void SetSFXVolume(float value)
        {
            CurrentData.sfxVolume = Mathf.Clamp01(value);
            if (mainAudioMixer != null)
            {
                mainAudioMixer.SetFloat(sfxVolumeParam, LinearToDecibel(CurrentData.sfxVolume));
            }
        }

        public void SetMouseSensitivity(float value)
        {
            CurrentData.mouseSensitivity = Mathf.Clamp(value, 0.1f, 5.0f);
        }

        public InputActionAsset GetInputActions() => inputActions;

        public void ResetToDefaults()
        {
            CurrentData = new SettingsData();
            if (inputActions != null)
            {
                inputActions.RemoveAllBindingOverrides();
            }
            ApplyAllSettings();
            SaveSettings();
        }
    }
}