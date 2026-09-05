using System;

namespace GameCore.Settings
{
    /// <summary>
    /// Dữ liệu cài đặt người dùng, chuẩn hóa để serialize ra file JSON.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public float mouseSensitivity = 1.0f;
        public string inputOverridesJson = string.Empty; // Lưu override keybinding của Unity Input System
        public int saveVersion = 1;
    }
}