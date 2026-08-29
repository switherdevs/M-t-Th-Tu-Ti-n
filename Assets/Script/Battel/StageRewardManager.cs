using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Đã thêm namespace TextMeshPro

public class StageRewardManager : MonoBehaviour
{
    [System.Serializable]
    public class DropItemConfig
    {
        public string tenPhanThuong = "Vật phẩm";
        public ItemData itemData;

        [Range(0f, 100f)]
        public float dropChance = 50f;

        public int count = 1;
        public GameObject itemWorldPrefab;
    }

    [Header("--- 1. DANH SÁCH ITEM CÓ THỂ NHẬN ---")]
    public List<DropItemConfig> rewardItemList = new List<DropItemConfig>();

    [Header("--- 2. DANH SÁCH VỊ TRÍ SPAWN TƯƠNG ỨNG ---")]
    public Transform[] spawnPositionList;

    [Header("--- 3. DANH SÁCH TEXT HIỂN THỊ SỐ LƯỢNG TƯƠNG ỨNG ---")]
    [Tooltip("Kéo các UI TextMeshProUGUI hiển thị số lượng tương ứng với từng Index vào đây")]
    public TextMeshProUGUI[] rewardCountTextList;

    [Header("--- 4. HIỆU ỨNG VÀ ÂM THANH ---")]
    [Tooltip("Prefab hiệu ứng UI (Đã chuyển sang dùng UI Image thay vì SpriteRenderer)")]
    public GameObject[] effectPrefabList;

    public AudioClip rewardSpawnSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 1f;

    public float delayBetweenRewards = 0.3f;

    private void Start()
    {
        // Tự động ẩn toàn bộ UI phần thưởng khi vừa vào game
        AnToanBoUIPhanThuong();
    }

    /// <summary>
    /// Ẩn toàn bộ ô Icon và Text hiển thị số lượng khi vừa vào Game
    /// </summary>
    private void AnToanBoUIPhanThuong()
    {
        // 1. Ẩn toàn bộ vị trí/khung hiển thị Icon
        if (spawnPositionList != null)
        {
            foreach (Transform spawnPoint in spawnPositionList)
            {
                if (spawnPoint != null)
                {
                    spawnPoint.gameObject.SetActive(false);
                }
            }
        }

        // 2. Ẩn toàn bộ Text Mesh Pro hiển thị số lượng
        if (rewardCountTextList != null)
        {
            foreach (TextMeshProUGUI countText in rewardCountTextList)
            {
                if (countText != null)
                {
                    countText.gameObject.SetActive(false);
                }
            }
        }
    }

    public void TraoPhanThuongThangTran()
    {
        if (rewardItemList == null || rewardItemList.Count == 0) return;
        StartCoroutine(RoutineTraoPhanThuong());
    }

    private IEnumerator RoutineTraoPhanThuong()
    {
        for (int i = 0; i < rewardItemList.Count; i++)
        {
            DropItemConfig config = rewardItemList[i];
            if (config == null || config.itemData == null) continue;

            float randomRoll = UnityEngine.Random.Range(0f, 100f);

            if (randomRoll <= config.dropChance)
            {
                int soLuongThucTeNhan = UnityEngine.Random.Range(1, Mathf.Max(1, config.count) + 1);

                if (QuestSaveSystem.Instance != null)
                {
                    QuestSaveSystem.Instance.LuuItemVaoSaveGame(config.itemData.idItem, soLuongThucTeNhan);
                }

                Transform targetParent = null;
                if (spawnPositionList != null && i < spawnPositionList.Length && spawnPositionList[i] != null)
                {
                    targetParent = spawnPositionList[i];
                    // BẬT ACTIVE Ô VỊ TRÍ PHẦN THƯỞNG KHI ĐƯỢC NHẬN
                    targetParent.gameObject.SetActive(true);
                }

                // 1. SPAWN ITEM UI VÀO CANVAS
                if (config.itemWorldPrefab != null)
                {
                    GameObject spawnedItem;
                    if (targetParent != null)
                    {
                        spawnedItem = Instantiate(config.itemWorldPrefab, targetParent);
                        RectTransform rect = spawnedItem.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.anchoredPosition = Vector2.zero;
                            rect.localPosition = Vector3.zero;
                        }
                    }
                    else
                    {
                        spawnedItem = Instantiate(config.itemWorldPrefab, transform.position, Quaternion.identity);
                    }
                    spawnedItem.transform.SetAsLastSibling();
                }

                // 2. HIỆN VÀ GÁN SỐ LƯỢNG VÀO TEXT MESH PRO
                if (rewardCountTextList != null && i < rewardCountTextList.Length && rewardCountTextList[i] != null)
                {
                    // BẬT ACTIVE CẢ VÙNG CHỨA TEXT ĐỂ ĐẢM BẢO KHÔNG BỊ KHỦNG ẢNH HƯỞNG BỞI OBJECT CHA
                    rewardCountTextList[i].gameObject.SetActive(true);
                    rewardCountTextList[i].text = "x" + soLuongThucTeNhan.ToString();

                    // Đưa Text lên trên cùng để tránh bị Prefab đè mất
                    rewardCountTextList[i].transform.SetAsLastSibling();
                }

                // 3. SPAWN HIỆU ỨNG UI VÀO DÚNG TÂM Ô PHẦN THƯỞNG
                if (effectPrefabList != null && i < effectPrefabList.Length && effectPrefabList[i] != null)
                {
                    Transform effectParent = targetParent != null ? targetParent : transform;
                    GameObject effectObj = Instantiate(effectPrefabList[i], effectParent);

                    RectTransform effectRect = effectObj.GetComponent<RectTransform>();
                    if (effectRect != null)
                    {
                        effectRect.anchoredPosition = Vector2.zero;
                        effectRect.localPosition = Vector3.zero;
                        effectRect.localScale = Vector3.one; // Đảm bảo Scale đúng chuẩn 1, 1, 1
                    }

                    // Đẩy hiệu ứng xuống cuối danh sách con của targetParent để nó nằm hiển thị ĐÈ LÊN TRÊN ITEM
                    effectObj.transform.SetAsLastSibling();
                }

                // 4. ÂM THANH PHÁT 1 LẦN
                if (rewardSpawnSound != null)
                {
                    Vector3 soundPos = targetParent != null ? targetParent.position : transform.position;
                    AudioSource.PlayClipAtPoint(rewardSpawnSound, soundPos, soundVolume);
                }

                yield return new WaitForSeconds(delayBetweenRewards);
            }
        }
    }
}