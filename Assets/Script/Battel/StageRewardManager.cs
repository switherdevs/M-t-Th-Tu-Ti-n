using System;
using System.Collections.Generic;
using UnityEngine;

public class StageRewardManager : MonoBehaviour
{
    [System.Serializable]
    public class DropItemConfig
    {
        public string tenPhanThuong = "Vật phẩm";
        public ItemData itemData;

        [Range(0f, 100f)]
        [Tooltip("Tỉ lệ rớt (%)")]
        public float dropChance = 50f;

        public int count = 1;

        [Tooltip("Prefab của vật phẩm sẽ xuất hiện dưới đất khi thắng")]
        public GameObject itemWorldPrefab;
    }

    [Header("--- 1. DANH SÁCH ITEM CÓ THỂ NHẬN ---")]
    [Tooltip("Mảng chứa thông tin Item và Tỉ lệ % tương ứng")]
    public List<DropItemConfig> rewardItemList = new List<DropItemConfig>();

    [Header("--- 2. DANH SÁCH VỊ TRÍ SPAWN TƯƠNG ỨNG ---")]
    [Tooltip("Mảng các vị trí spawn. Phần tử [0] ở đây tương ứng với Item [0] ở mảng trên")]
    public Transform[] spawnPositionList;

    /// <summary>
    /// Hàm này CHỈ ĐƯỢC GỌI từ StageManager khi người chơi dọn sạch quái (Win trận)
    /// </summary>
    public void TraoPhanThuongThangTran()
    {
        if (rewardItemList == null || rewardItemList.Count == 0) return;

        for (int i = 0; i < rewardItemList.Count; i++)
        {
            DropItemConfig config = rewardItemList[i];
            if (config == null || config.itemData == null) continue;

            // 1. Tính toán tỉ lệ % ngẫu nhiên từ 0 -> 100
            float randomRoll = UnityEngine.Random.Range(0f, 100f);

            if (randomRoll <= config.dropChance)
            {
                Debug.Log($"<color=cyan>[Reward]</color> Thắng trận! Trúng Item: {config.itemData.tenItem} (Tỉ lệ: {config.dropChance}%)");

                // 2. Lưu thông tin vật phẩm vào File Save
                if (QuestSaveSystem.Instance != null)
                {
                    QuestSaveSystem.Instance.LuuItemVaoSaveGame(config.itemData.idItem, config.count);
                }

                // 3. Xác định vị trí xuất hiện dựa theo mảng vị trí (Index i)
                Vector3 targetSpawnPos = transform.position;
                if (spawnPositionList != null && i < spawnPositionList.Length && spawnPositionList[i] != null)
                {
                    targetSpawnPos = spawnPositionList[i].position;
                }

                // 4. CHỈ KHI WIN MỚI SINH RA (SPAWN) ITEM TRÊN MÀN HÌNH
                if (config.itemWorldPrefab != null)
                {
                    Instantiate(config.itemWorldPrefab, targetSpawnPos, Quaternion.identity);
                }
            }
        }
    }
}