using UnityEngine;
using TMPro;

public class PlayerStatsDisplayUI : MonoBehaviour
{
    [Header("--- UI TEXT ELEMENTS ---")]
    [SerializeField] private TextMeshProUGUI textTenCanhGioi;
    [SerializeField] private TextMeshProUGUI textCapDo;
    [SerializeField] private TextMeshProUGUI textKinhNghiem;
    [SerializeField] private TextMeshProUGUI textMau;
    [SerializeField] private TextMeshProUGUI textSatThuong;
    [SerializeField] private TextMeshProUGUI textPhongThu;
    [SerializeField] private TextMeshProUGUI textNangLuong;

    private void OnEnable()
    {
        // 🎯 Đăng ký lắng nghe sự kiện đổi ngôn ngữ toàn cục để cập nhật ngay lập tức
        GameCore.Settings.SettingsManager.OnLanguageChanged += HandleLanguageChanged;
        CapNhatGiaoDienThongSo();
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi object bị ẩn/hủy để tránh rò rỉ bộ nhớ
        GameCore.Settings.SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(string newLang)
    {
        CapNhatGiaoDienThongSo();
    }

    // Kiểm tra ngôn ngữ từ hệ thống save / settings
    private bool KiemTraNgonNguTiengAnh()
    {
        if (QuestSaveSystem.Instance != null)
        {
            return QuestSaveSystem.Instance.IsEnglish();
        }
        return false;
    }

    public void CapNhatGiaoDienThongSo()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            return;
        }

        PlayerStatsSaveData data = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;
        bool isEnglish = KiemTraNgonNguTiengAnh();

        // Dịch tên cảnh giới sang tiếng Anh nếu cần
        string canhGioiHienThi = TranslateCanhGioi(data.tenCanhGioi, isEnglish);

        // Các nhãn dịch thuật tương ứng giữa 2 ngôn ngữ
        string nhanCanhGioi = isEnglish ? "Realm: " : "Cảnh Giới: ";
        string nhanCapDo = isEnglish ? "Level: " : "Cấp Độ: ";
        string nhanExp = isEnglish ? "EXP: " : "EXP: ";
        string nhanMau = isEnglish ? "Health (HP): " : "Sinh Lực (HP): ";
        string nhanSatThuong = isEnglish ? "Damage: " : "Sát Thương: ";
        string nhanGiap = isEnglish ? "Armor: " : "Giáp: ";
        string nhanNangLuong = isEnglish ? "Energy: " : "Năng Lượng: ";

        if (textTenCanhGioi != null)
            textTenCanhGioi.text = $"{nhanCanhGioi}{canhGioiHienThi}";

        if (textCapDo != null)
            textCapDo.text = $"{nhanCapDo}{data.level}";

        if (textKinhNghiem != null)
            textKinhNghiem.text = $"{nhanExp}{data.currentExp} / {data.maxExp}";

        if (textMau != null)
            textMau.text = $"{nhanMau}{data.maxHP}";

        if (textSatThuong != null)
            textSatThuong.text = $"{nhanSatThuong}{data.damage}";

        if (textPhongThu != null)
            textPhongThu.text = $"{nhanGiap}{data.armor * 100f:F0}%";

        if (textNangLuong != null)
            textNangLuong.text = $"{nhanNangLuong}{data.maxEnergy}";
    }

    /// <summary>
    /// Hàm hỗ trợ tự động dịch tên cảnh giới sang tiếng Anh
    /// </summary>
    private string TranslateCanhGioi(string vnRealm, bool isEnglish)
    {
        if (!isEnglish || string.IsNullOrEmpty(vnRealm)) return vnRealm;

        // Dịch các mốc cảnh giới phổ biến trong game
        if (vnRealm.Contains("Luyện Khí Tầng 1")) return "Qi Condensation Level 1";
        if (vnRealm.Contains("Luyện Khí Tầng 2")) return "Qi Condensation Level 2";
        if (vnRealm.Contains("Luyện Khí Tầng 3")) return "Qi Condensation Level 3";
        if (vnRealm.Contains("Luyện Khí")) return "Qi Condensation";
        if (vnRealm.Contains("Trúc Cơ")) return "Foundation Establishment";
        if (vnRealm.Contains("Kim Đan")) return "Golden Core";
        if (vnRealm.Contains("Phàm Nhân")) return "Mortal";

        return vnRealm; // Mặc định giữ nguyên nếu là tên custom khác
    }
}