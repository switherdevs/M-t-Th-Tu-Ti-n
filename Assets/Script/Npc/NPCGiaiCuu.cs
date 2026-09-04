using UnityEngine;

public class NPCGiaiCuu : MonoBehaviour
{
    [Header("--- CẤU HÌNH NPC GIẢI CỨU ---")]
    [Tooltip("Mã ID NPC giải cứu (Phải trùng khớp với idDoiTuongCanGiaiCuu trong QuestData)")]
    public int idNpcGiaiCuu;

    [Header("--- LIÊN KẾT SCRIPT THOẠI TUYẾN TÍNH ---")]
    [Tooltip("Kéo Script LinearVictimDialogue đính kèm trên NPC hoặc UI thoại vào đây")]
    public LinearVictimDialogue linearDialogueScript;

    private bool daGiaiCuu = false;

    private void Start()
    {
        KiemTraTrangThaiKichHoat();
    }

    public void KiemTraTrangThaiKichHoat()
    {
        if (QuestSaveSystem.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool duocPhepKichHoat = QuestSaveSystem.Instance.KiemTraNPCGiaiCuuCoDuocPhepXuatHien(idNpcGiaiCuu);
        gameObject.SetActive(duocPhepKichHoat);
    }

    // Hàm này CHỈ ĐƯỢC GỌI khi đã nói chuyện xong câu thoại cuối cùng!
    public void ThucHienGiaiCuuHoanThanh()
    {
        if (daGiaiCuu) return;

        daGiaiCuu = true;

        if (QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.GhiNhanGiaiCuu(idNpcGiaiCuu, 1);
            Debug.Log($"<color=cyan>[Giải Cứu]</color> Đã hoàn thành thoại & giải cứu NPC ID: {idNpcGiaiCuu}");
        }

        // Tắt NPC sau khi thoại hoàn tất
        gameObject.SetActive(false);

        // Báo cho StageManager kiểm tra để hiện UI Win Game (Đã sửa hàm FindFirstObjectByType chuẩn Unity 6)
        StageManager stageManager = FindFirstObjectByType<StageManager>();
        if (stageManager != null)
        {
            stageManager.KiemTraHoanThanhTran();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Khi chạm vào chỉ MỞ UI THOẠI chứ KHÔNG kết thúc game ngay
        if (collision.CompareTag("Player") && !daGiaiCuu)
        {
            if (linearDialogueScript != null)
            {
                linearDialogueScript.MoHoiThoaiTuyenTinh(this);
            }
        }
    }
}