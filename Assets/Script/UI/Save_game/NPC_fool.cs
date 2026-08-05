using UnityEngine;

public class NPC_QuestGiver : MonoBehaviour
{
    [Header("--- NỐI DATA QUEST CỦA NPC NÀY ---")]
    public QuestData questDataCuaNPC;

    // Bắt sự kiện Click chuột trực tiếp vào NPC (Cần có Collider2D trên NPC)
    private void OnMouseDown()
    {
        if (questDataCuaNPC != null && QuestUIManager.Instance != null)
        {
            // Bật UI Bảng thoại lên
            QuestUIManager.Instance.MoBangThoaiQuest(questDataCuaNPC, this);
        }
    }
}