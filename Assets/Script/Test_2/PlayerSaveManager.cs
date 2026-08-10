using System.IO;
using UnityEngine;

namespace PersistenceSystem
{
    public static class PlayerSaveManager
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        /// <summary>
        /// Lưu dữ liệu xuống File JSON trên ổ cứng
        /// </summary>
        public static void SaveGame(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"<color=green>[SaveManager] Đã LƯU thành công dữ liệu vào: {SaveFilePath}</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Lỗi khi lưu file: {e.Message}");
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ File JSON trên ổ cứng
        /// </summary>
        public static PlayerData LoadGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("[SaveManager] Chưa có file save cũ! Tạo dữ liệu mặc định mới.");
                return new PlayerData();
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log($"<color=yellow>[SaveManager] Đã LOAD dữ liệu thành công từ: {SaveFilePath}</color>");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Lỗi khi đọc file save: {e.Message}");
                return new PlayerData();
            }
        }

        /// <summary>
        /// Xóa file Save (Dùng khi muốn Reset Game / New Game)
        /// </summary>
        public static void DeleteSaveFile()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[SaveManager] Đã xóa file save thành công.");
            }
        }
    }
}