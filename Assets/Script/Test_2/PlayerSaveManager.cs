using UnityEngine;

namespace PersistenceSystem
{
    public static class PlayerSaveManager
    {
        private static PlayerData _cachedData;

        public static bool HasSavedData => _cachedData != null;

        public static void SaveData(PlayerData data)
        {
            if (data == null) return;
            _cachedData = data;
            Debug.Log("<color=green>[SaveManager] Đã lưu dữ liệu tạm vào RAM!</color>");
        }

        public static PlayerData LoadData()
        {
            return _cachedData;
        }

        public static void ClearCache()
        {
            _cachedData = null;
        }
    }
}