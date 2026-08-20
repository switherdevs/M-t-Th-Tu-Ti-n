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

        [Tooltip("Prefab của vật phẩm UI (Có chứa RawImage/Image)")]
        public GameObject itemWorldPrefab;
    }

    [Header("--- 1. DANH SÁCH ITEM CÓ THỂ NHẬN ---")]
    [Tooltip("Mảng chứa thông tin Item và Tỉ lệ % tương ứng")]
    public List<DropItemConfig> rewardItemList = new List<DropItemConfig>();

    [Header("--- 2. DANH SÁCH VỊ TRÍ SPAWN TƯƠNG ỨNG ---")]
    [Tooltip("Kéo các GameObject Vị Trí (RectTransform trên Canvas) vào đây. Element [0] ứng với Item [0], Element [1] ứng với Item [1]")]
    public Transform[] spawnPositionList;

    /// <summary>
    /// Hàm xử lý sinh Item đúng theo từng vị trí Element trong mảng
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
                Debug.Log($"<color=cyan>[Reward]</color> Thắng trận! Trúng Item index [{i}]: {config.itemData.tenItem}");

                // 2. Lưu thông tin vật phẩm vào File Save
                if (QuestSaveSystem.Instance != null)
                {
                    QuestSaveSystem.Instance.LuuItemVaoSaveGame(config.itemData.idItem, config.count);
                }

                // 3. Kiểm tra vị trí Spawn theo chỉ số i
                Transform targetParent = null;
                if (spawnPositionList != null && i < spawnPositionList.Length && spawnPositionList[i] != null)
                {
                    targetParent = spawnPositionList[i]; // Lấy đúng ô vị trí thứ i
                }

                // 4. Sinh ra Item đúng vị trí cha (Target Parent)
                if (config.itemWorldPrefab != null)
                {
                    GameObject spawnedItem;

                    if (targetParent != null)
                    {
                        // Sinh ra làm con của vị trí spawnPositionList[i]
                        spawnedItem = Instantiate(config.itemWorldPrefab, targetParent);

                        // Đặt lại RectTransform để nằm vừa khít vị trí ô thứ i
                        RectTransform rect = spawnedItem.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = Vector2.zero; // Nằm chính giữa ô vị trí i
                            rect.localPosition = Vector3.zero;
                        }
                    }
                    else
                    {
                        // Trường hợp không gán vị trí -> Spawn tại điểm mặc định của script
                        spawnedItem = Instantiate(config.itemWorldPrefab, transform.position, Quaternion.identity);
                    }

                    // Đẩy Item lên trên cùng để không bị các lớp UI khác che
                    spawnedItem.transform.SetAsLastSibling();
                }
            }
        }
    }
}