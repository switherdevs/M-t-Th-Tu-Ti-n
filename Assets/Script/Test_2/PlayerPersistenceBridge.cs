using System.Collections.Generic;
using UnityEngine;

namespace PersistenceSystem
{
    public class PlayerPersistenceBridge : MonoBehaviour
    {
        public static PlayerPersistenceBridge Instance { get; private set; }

        private PlayerData currentData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ Cầu nối này không bị xóa khi đổi Scene

            // 1. NẠP DỮ LIỆU TỪ Ổ CỨNG LÊN KHI MỞ GAME
            LoadAllFromDisk();
        }

        private void Start()
        {
            // 2. ÉP CÁC COMPONENT TRONG SCENE ĐỌC DỮ LIỆU ĐÃ LOAD
            ApplyDataToAllSaveables();
        }

        /// <summary>
        /// Tải dữ liệu từ File JSON ổ cứng vào RAM
        /// </summary>
        public void LoadAllFromDisk()
        {
            currentData = PlayerSaveManager.LoadGame();
        }

        /// <summary>
        /// Thu thập tất cả dữ liệu hiện tại trong game và LƯU XUỐNG Ổ CỨNG
        /// </summary>
        public void SaveAllToDisk()
        {
            if (currentData == null) currentData = new PlayerData();

            // Tìm tất cả các MonoBehavior có triển khai IPlayerSaveable trong Scene (InventoryManager, LevelSystem...)
            var saveables = FindObjectsOfType<MonoBehaviour>();
            foreach (var mono in saveables)
            {
                if (mono is IPlayerSaveable saveable)
                {
                    saveable.SaveToData(currentData);
                }
            }

            // Ghi trực tiếp xuống ổ cứng
            PlayerSaveManager.SaveGame(currentData);
        }

        /// <summary>
        /// Đẩy dữ liệu từ RAM áp dụng lại cho các Component trong Scene
        /// </summary>
        public void ApplyDataToAllSaveables()
        {
            if (currentData == null) return;

            var saveables = FindObjectsOfType<MonoBehaviour>();
            foreach (var mono in saveables)
            {
                if (mono is IPlayerSaveable saveable)
                {
                    saveable.LoadFromData(currentData);
                }
            }
        }

        // Tự động LƯU GAME khi Người chơi tắt Game / Thoát ứng dụng
        private void OnApplicationQuit()
        {
            SaveAllToDisk();
        }

        // Tự động LƯU GAME khi Tạm dừng trên Mobile
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveAllToDisk();
            }
        }
    }
}