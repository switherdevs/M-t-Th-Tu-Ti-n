using System.IO;
using UnityEngine;
using StatsSystem.UI;

public class ResetSaveGameButton : MonoBehaviour
{
    /// <summary>
    /// Gán hàm này vào Event OnClick() của Button Reset
    /// </summary>
    public void XoaVaResetSaveGame()
    {
        if (QuestSaveSystem.Instance == null)
        {
            Debug.LogWarning("[Reset Save] Không tìm thấy QuestSaveSystem.Instance trong Scene!");
            return;
        }

        // 1. LẤY ĐƯỜNG DẪN FILE SAVE VÀ TIẾN HÀNH XÓA FILE
        string duongDanSave = Path.Combine(Application.persistentDataPath, QuestSaveSystem.Instance.tenFileSave);

        if (File.Exists(duongDanSave))
        {
            File.Delete(duongDanSave);
            Debug.Log("<color=red>[Reset Save]</color> Đã xóa thành công file save tại: " + duongDanSave);
        }
        else
        {
            Debug.LogWarning("[Reset Save] File save không tồn tại để xóa.");
        }

        // 2. TẢI LẠI DỮ LIỆU TỪ HỆ THỐNG (QuestSaveSystem sẽ tự tạo lại file mới chuẩn ban đầu)
        QuestSaveSystem.Instance.LoadDuLieuQuestFromTxt();

        // 3. CẬP NHẬT LẠI CHỈ SỐ NHÂN VẬT THỰC TẾ TRÊN SCENE
        CharacterStats[] allStats = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var stat in allStats)
        {
            stat.TaiThongSoTuSaveFile();
        }

        // 4. LÀM MỚI TẤT CẢ UI TRÊN SCENE
        // Cập nhật BangDotPhaSingleUI nếu đang mở
        BangDotPhaSingleUI[] allBangDotPha = FindObjectsByType<BangDotPhaSingleUI>(FindObjectsSortMode.None);
        foreach (var bang in allBangDotPha)
        {
            bang.CapNhatGiaoDienBang();
        }

        // Cập nhật Quest UI / HUD
        if (QuestUIManager.Instance != null)
        {
            QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
        }
        QuestHUDTracker.ThongBaoCapNhatHUD();

        Debug.Log("<color=green>[Reset Save Success]</color> Game đã được làm mới toàn bộ về dữ liệu ban đầu!");
    }
}