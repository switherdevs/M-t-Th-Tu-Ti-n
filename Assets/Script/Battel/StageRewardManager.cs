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

        [Tooltip("Số lượng tối đa có thể rớt (Hệ thống sẽ random ngẫu nhiên từ 1 đến số này)")]
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

            // 1. Tính toán tỉ lệ % ngẫu nhiên từ 0 -> 100[cite: 10]
            float randomRoll = UnityEngine.Random.Range(0f, 100f);

            if (randomRoll <= config.dropChance)
            {
                // 🎯 ĐÃ SỬA: Random số lượng thực tế nhận được từ 1 đến giá trị tối đa (config.count)
                // Lưu ý: Random.Range với kiểu int lấy cận dưới (bao gồm) và cận trên (không bao gồm), nên dùng config.count + 1
                int soLuongThucTeNhan = UnityEngine.Random.Range(1, Mathf.Max(1, config.count) + 1);

                Debug.Log($"<color=cyan>[Reward]</color> Thắng trận! Trúng Item index [{i}]: {config.itemData.tenItem} | Số lượng nhận: {soLuongThucTeNhan}");

                // 2. Lưu thông tin vật phẩm vào File Save với số lượng đã random[cite: 10]
                if (QuestSaveSystem.Instance != null)
                {
                    QuestSaveSystem.Instance.LuuItemVaoSaveGame(config.itemData.idItem, soLuongThucTeNhan);
                }

                // 3. Kiểm tra vị trí Spawn theo chỉ số i[cite: 10]
                Transform targetParent = null;
                if (spawnPositionList != null && i < spawnPositionList.Length && spawnPositionList[i] != null)
                {
                    targetParent = spawnPositionList[i]; // Lấy đúng ô vị trí thứ i[cite: 10]
                }

                // 4. Sinh ra Item đúng vị trí cha (Target Parent)[cite: 10]
                if (config.itemWorldPrefab != null)
                {
                    GameObject spawnedItem;

                    if (targetParent != null)
                    {
                        // Sinh ra làm con của vị trí spawnPositionList[i][cite: 10]
                        spawnedItem = Instantiate(config.itemWorldPrefab, targetParent);

                        // Đặt lại RectTransform để nằm vừa khít vị trí ô thứ i[cite: 10]
                        RectTransform rect = spawnedItem.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = Vector2.zero; // Nằm chính giữa ô vị trí i[cite: 10]
                            rect.localPosition = Vector3.zero;
                        }
                    }
                    else
                    {
                        // Trường hợp không gán vị trí -> Spawn tại điểm mặc định của script[cite: 10]
                        spawnedItem = Instantiate(config.itemWorldPrefab, transform.position, Quaternion.identity);
                    }

                    // Đẩy Item lên trên cùng để không bị các lớp UI khác che[cite: 10]
                    spawnedItem.transform.SetAsLastSibling();
                }
            }
        }
    }
}