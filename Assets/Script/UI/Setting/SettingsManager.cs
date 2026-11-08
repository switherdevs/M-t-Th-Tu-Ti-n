using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

namespace GameCore.Settings
{
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
                    }
                }

                // 🛡️ BẢO VỆ TUYỆT ĐỐI: Ép load file ngay lập tức nếu chưa được load (Khắc phục triệt để lỗi chậm Awake)
                if (_instance != null && !_instance._isLoaded)
                {
                    _instance.LoadSettings();
                }

                return _instance;
            }
        }

        [Header("--- AUDIO SETTINGS ---")]
        [SerializeField] private AudioMixer mainAudioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string bgmVolumeParam = "BGMVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        [Header("--- SAVE CONFIG ---")]
        [SerializeField] private string saveFileName = "settings.txt";

        public SettingsData CurrentData { get; private set; } = new SettingsData();

        private bool _isLoaded = false; // Cờ kiểm tra trạng thái đã load hay chưa

        // Sự kiện toàn cục để các UI tự đổi chữ ngay lập tức khi đổi ngôn ngữ
        public static event Action<string> OnLanguageChanged;

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
            if (!_isLoaded)
            {
                LoadSettings();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentData, true);
                File.WriteAllText(TempSaveFilePath, json);

                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                File.Move(TempSaveFilePath, SaveFilePath);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi ghi file
            }
        }

        public void LoadSettings()
        {
            if (_isLoaded) return; // Tránh load trùng lặp

            if (!File.Exists(SaveFilePath))
            {
                CurrentData = new SettingsData();
                _isLoaded = true;
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
            catch (Exception)
            {
                CurrentData = new SettingsData();
            }

            _isLoaded = true;
            ApplyAllSettings();
        }

        public void ApplyAllSettings()
        {
            SetMasterVolume(CurrentData.masterVolume);
            SetBGMVolume(CurrentData.bgmVolume);
            SetSFXVolume(CurrentData.sfxVolume);
        }

        // --- LANGUAGE HELPERS ---
        public bool IsEnglish()
        {
            return CurrentData != null && CurrentData.language == "EN";
        }

        public string GetCurrentLanguage()
        {
            return CurrentData != null ? CurrentData.language : "VN";
        }

        public void SetLanguage(string langCode)
        {
            if (CurrentData != null && CurrentData.language != langCode)
            {
                CurrentData.language = langCode;
                SaveSettings(); // Lưu trực tiếp vào file ngay lập tức

                OnLanguageChanged?.Invoke(langCode);
            }
        }

        // --- AUDIO HELPERS ---
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

        public void ResetToDefaults()
        {
            CurrentData = new SettingsData();
            _isLoaded = true;
            ApplyAllSettings();
            SaveSettings();
            OnLanguageChanged?.Invoke(CurrentData.language);
        }
    }
}