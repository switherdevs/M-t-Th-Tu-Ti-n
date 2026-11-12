using System;
using System.Collections;
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

        // Sự kiện toàn cục đổi ngôn ngữ
        public static event Action<string> OnLanguageChanged;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private string TempSaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName + ".tmp");

        private void Awake()
        {
            _instance = this;

            // 1. Chỉ đọc file save vào bộ nhớ trong Awake (không tác động vội vào AudioMixer)
            LoadDataFromFileOnly();
        }

        private void Start()
        {
            // 2. Đợi AudioMixer của Unity khởi tạo xong hoàn toàn (sau 1 frame) rồi mới ép áp dụng âm lượng
            StartCoroutine(ApplyAudioDelayed());
        }

        private IEnumerator ApplyAudioDelayed()
        {
            yield return null; // Chờ 1 frame để Audio Engine sẵn sàng 100%
            ApplyAllSettings();
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
            catch (Exception) { }
        }

        private void LoadDataFromFileOnly()
        {
            if (!File.Exists(SaveFilePath))
            {
                CurrentData = new SettingsData();
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
        }

        public void LoadSettings()
        {
            LoadDataFromFileOnly();
            ApplyAllSettings();
        }

        public void ApplyAllSettings()
        {
            SetMasterVolume(CurrentData.masterVolume);
            SetBGMVolume(CurrentData.bgmVolume);
            SetSFXVolume(CurrentData.sfxVolume);
        }

        public bool IsEnglish() => CurrentData != null && CurrentData.language == "EN";
        public string GetCurrentLanguage() => CurrentData != null ? CurrentData.language : "VN";

        public void SetLanguage(string langCode)
        {
            if (CurrentData != null && CurrentData.language != langCode)
            {
                CurrentData.language = langCode;
                SaveSettings();
                OnLanguageChanged?.Invoke(langCode);
            }
        }

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
            ApplyAllSettings();
            SaveSettings();
            OnLanguageChanged?.Invoke(CurrentData.language);
        }
    }
}