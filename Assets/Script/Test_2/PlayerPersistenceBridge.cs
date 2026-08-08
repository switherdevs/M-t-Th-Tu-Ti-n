using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersistenceSystem
{
    [DisallowMultipleComponent]
    public class PlayerPersistenceBridge : MonoBehaviour
    {
        private readonly List<IPlayerSaveable> _saveables = new List<IPlayerSaveable>();
        private bool _isSaved = false;

        private void Awake()
        {
            // Tự động tìm CharacterStats, InventoryManager... trên Player
            GetComponentsInChildren(true, _saveables);
        }

        private IEnumerator Start()
        {
            // Chờ 1 frame để đảm bảo Awake/Initialize của các hệ thống cũ chạy xong
            yield return null;
            LoadToCurrentPlayer();
        }

        /// <summary>
        /// Unity TỰ ĐỘNG gọi hàm này khi Scene cũ bị Unload (Player cũ bị Destroy)
        /// </summary>
        private void OnDestroy()
        {
            // Đảm bảo chỉ Save khi Game đang chạy (tránh Save khi bấm Stop Play mode trong Editor)
            if (Application.isPlaying && !_isSaved)
            {
                SaveFromCurrentPlayer();
            }
        }

        public void SaveFromCurrentPlayer()
        {
            if (_isSaved) return;

            PlayerData data = new PlayerData();
            foreach (var saveable in _saveables)
            {
                saveable?.SaveToData(data);
            }

            PlayerSaveManager.SaveData(data);
            _isSaved = true;
            Debug.Log($"<color=green>[Bridge] ĐÃ SAVE THÀNH CÔNG từ Scene '{gameObject.scene.name}'!</color>");
        }

        public void LoadToCurrentPlayer()
        {
            if (!PlayerSaveManager.HasSavedData)
            {
                Debug.Log("[Bridge] Scene đầu tiên (chưa có Save Data) -> Giữ chỉ số mặc định.");
                return;
            }

            PlayerData data = PlayerSaveManager.LoadData();
            foreach (var saveable in _saveables)
            {
                saveable?.LoadFromData(data);
            }
            Debug.Log($"<color=cyan>[Bridge] ĐÃ LOAD THÀNH CÔNG vào Player ở Scene '{gameObject.scene.name}'!</color>");
        }
    }
}