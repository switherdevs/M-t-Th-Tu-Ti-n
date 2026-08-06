using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StageManager : MonoBehaviour
{
    [System.Serializable]
    public class StageInfo
    {
        [Header("Thông Tin Màn Chơi")]
        public string stageName = "Ải 1";

        [Header("Script Vùng Map (Chứa BoxCollider2D)")]
        [Tooltip("Kéo cái GameObject đại diện vùng map có gắn script MapZoneChecker vào đây")]
        public MapZoneChecker mapZone;

        [Header("Phần Thưởng / Lối Đi (Mở khi thắng)")]
        public GameObject nextTeleportPortal;
        public GameObject winUIObject;

        [Header("Trạng Thái")]
        [HideInInspector] public bool isCompleted = false;
    }

    [Header("Danh Sách Các Ải Trong Scene")]
    [SerializeField] private List<StageInfo> stages = new List<StageInfo>();
    [SerializeField] private int currentStageIndex = 0;

    [Header("UI Hiển Thị Chung")]
    [SerializeField] private TextMeshProUGUI statusText;

    void Start()
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].nextTeleportPortal != null) stages[i].nextTeleportPortal.SetActive(false);
            if (stages[i].winUIObject != null) stages[i].winUIObject.SetActive(false);
        }
    }

    void Update()
    {
        if (currentStageIndex >= stages.Count) return;

        StageInfo currentStage = stages[currentStageIndex];

        if (currentStage.isCompleted) return;

        CheckCurrentStage(currentStage);
    }

    private void CheckCurrentStage(StageInfo stage)
    {
        if (stage.mapZone == null) return;

        // Lấy số lượng quái thông qua vùng quét riêng của map đó
        int aliveCount = stage.mapZone.GetRemainingEnemiesCount();

        // Cập nhật giao diện chữ
        if (statusText != null)
        {
            if (aliveCount > 0)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = $"{stage.stageName} - Quái còn lại: {aliveCount}";
            }
            else
            {
                statusText.text = $"{stage.stageName} đã dọn sạch!";
            }
        }

        // Nếu dọn sạch quái trong vùng map hiện tại
        if (aliveCount == 0)
        {
            stage.isCompleted = true;
            Debug.Log($"<color=green>Đã vượt qua {stage.stageName}!</color>");

            if (stage.nextTeleportPortal != null)
            {
                stage.nextTeleportPortal.SetActive(true);
            }

            if (stage.winUIObject != null)
            {
                stage.winUIObject.SetActive(true);
            }
        }
    }

    // Hàm gọi khi bước qua cổng tele
    public void ProceedToNextStage()
    {
        currentStageIndex++;
        Debug.Log($"[StageManager] Đã chuyển sang ải index: {currentStageIndex}");
    }
}