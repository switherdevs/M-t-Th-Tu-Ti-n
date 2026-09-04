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

        [Tooltip("Tick vào đây nếu đây là trận cuối! Thắng trận này mới được trao thưởng Item và hiện UI WinGame/Button.")]
        public bool isFinalStage = false;

        [Header("Script Vùng Map (Chứa BoxCollider2D)")]
        [Tooltip("Kéo GameObject đại diện vùng map có gắn script MapZoneChecker vào đây")]
        public MapZoneChecker mapZone;

        [Header("Liên Kết Quản Lý Phần Thưởng")]
        [Tooltip("Kéo GameObject có gắn script StageRewardManager vào đây")]
        public StageRewardManager rewardManager;

        [Header("NPC Giải Cứu (Nếu có trong ải này)")]
        [Tooltip("Kéo GameObject/Script NPCGiaiCuu trong map này vào đây (Nếu không có NPC thì để trống)")]
        public NPCGiaiCuu npcGiaiCuuMap;

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

    [Header("--- NÚT BẤM KHI THẮNG TRẬN CUỐI ---")]
    [Tooltip("Kéo Button hoặc GameObject nút bấm (VD: Nút Chuyển Map / Về Thành) vào đây. Nút này sẽ bị ẩn và chỉ hiện khi thắng trận cuối!")]
    [SerializeField] private GameObject buttonChuyenMap;

    void Start()
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].nextTeleportPortal != null) stages[i].nextTeleportPortal.SetActive(false);
            if (stages[i].winUIObject != null) stages[i].winUIObject.SetActive(false);
        }

        if (buttonChuyenMap != null)
        {
            buttonChuyenMap.SetActive(false);
        }
    }

    void Update()
    {
        if (currentStageIndex >= stages.Count) return;

        StageInfo currentStage = stages[currentStageIndex];

        if (currentStage.isCompleted) return;

        CheckCurrentStage(currentStage);
    }

    public void KiemTraHoanThanhTran()
    {
        if (currentStageIndex < stages.Count)
        {
            CheckCurrentStage(stages[currentStageIndex]);
        }
    }

    private void CheckCurrentStage(StageInfo stage)
    {
        if (stage.mapZone == null) return;

        int aliveCount = stage.mapZone.GetRemainingEnemiesCount();
        bool coNPCGiaiCuuChuaXong = stage.npcGiaiCuuMap != null && stage.npcGiaiCuuMap.gameObject.activeInHierarchy;

        if (statusText != null)
        {
            if (aliveCount > 0)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = $"{stage.stageName} - Quái còn lại: {aliveCount}";
            }
            else if (coNPCGiaiCuuChuaXong)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = $"{stage.stageName} - Hãy trò chuyện giải cứu NPC!";
            }
            else
            {
                statusText.text = $"{stage.stageName} đã dọn sạch!";
            }
        }

        // KHI DIỆT SẠCH QUÁI
        if (aliveCount == 0)
        {
            // Nếu NPC vẫn active (chưa tương tác xong thoại câu cuối) -> Chưa xong ải!
            if (coNPCGiaiCuuChuaXong)
            {
                return;
            }

            stage.isCompleted = true;
            Debug.Log($"<color=green>Đã vượt qua {stage.stageName}!</color>");

            if (stage.isFinalStage)
            {
                if (!stage.rewardClaimed)
                {
                    stage.rewardClaimed = true;

                    if (stage.rewardManager != null)
                    {
                        stage.rewardManager.TraoPhanThuongThangTran();
                    }
                }

                if (stage.winUIObject != null)
                {
                    stage.winUIObject.SetActive(true);
                }

                if (buttonChuyenMap != null)
                {
                    buttonChuyenMap.SetActive(true);
                }
            }
            else
            {
                if (stage.nextTeleportPortal != null)
                {
                    stage.nextTeleportPortal.SetActive(true);
                }
            }

            currentStageIndex++;
        }
    }

    public void ProceedToNextStage()
    {
        Debug.Log($"[StageManager] Người chơi đã dịch chuyển qua cổng ải index: {currentStageIndex}");
    }
}