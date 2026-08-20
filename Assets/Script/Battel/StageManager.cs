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

        [Tooltip("Tick vào đây nếu đây là trận cuối! Thắng trận này mới được trao thưởng Item và hiện UI WinGame.")]
        public bool isFinalStage = false;

        [Header("Script Vùng Map (Chứa BoxCollider2D)")]
        [Tooltip("Kéo GameObject đại diện vùng map có gắn script MapZoneChecker vào đây")]
        public MapZoneChecker mapZone;

        [Header("Liên Kết Quản Lý Phần Thưởng")]
        [Tooltip("Kéo GameObject có gắn script StageRewardManager vào đây")]
        public StageRewardManager rewardManager;

        [Header("Phần Thưởng / Lối Đi (Mở khi thắng)")]
        [Tooltip("Tế đàn / Cổng dịch chuyển sang ải tiếp (Hiện khi KHÔNG tick isFinalStage)")]
        public GameObject nextTeleportPortal;

        [Tooltip("UI Win Game (Hiện khi CÓ TICK isFinalStage)")]
        public GameObject winUIObject;

        [Header("Trạng Thái")]
        [HideInInspector] public bool isCompleted = false;
        [HideInInspector] public bool rewardClaimed = false;
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
            if (stages[i].nextTeleportPortal != null) stages[i].nextTeleportPortal.SetActive(false); // Hide portal
            if (stages[i].winUIObject != null) stages[i].winUIObject.SetActive(false); // Hide Win UI
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

        // Đếm số lượng quái sống trong vùng map
        int aliveCount = stage.mapZone.GetRemainingEnemiesCount();

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

        // KHI DIỆT SẠCH QUÁI (WIN TRẬN)
        if (aliveCount == 0)
        {
            stage.isCompleted = true;
            Debug.Log($"<color=green>Đã vượt qua {stage.stageName}!</color>");

            // 🎯 ĐÃ SỬA: CHỈ CHO PHÉP TRAO THƯỞNG VÀ BẬT UI KHI Ô isFinalStage ĐƯỢC TICK
            if (stage.isFinalStage)
            {
                // 1. Trao thưởng Item (Chỉ cho trận Final)
                if (!stage.rewardClaimed)
                {
                    stage.rewardClaimed = true;

                    if (stage.rewardManager != null)
                    {
                        stage.rewardManager.TraoPhanThuongThangTran(); // Gọi script thưởng
                    }
                }

                // 2. Hiện UI Win Game
                if (stage.winUIObject != null)
                {
                    stage.winUIObject.SetActive(true);
                }
            }
            else
            {
                // Nếu KHÔNG PHẢI ẢI CUỐI -> Chỉ hiện Tế Đàn dịch chuyển
                if (stage.nextTeleportPortal != null)
                {
                    stage.nextTeleportPortal.SetActive(true);
                }
            }

            currentStageIndex++;
            Debug.Log($"[StageManager] Đã tự động tăng chỉ số sang ải index: {currentStageIndex}");
        }
    }

    public void ProceedToNextStage()
    {
        Debug.Log($"[StageManager] Người chơi đã dịch chuyển qua cổng ải index: {currentStageIndex}");
    }
}