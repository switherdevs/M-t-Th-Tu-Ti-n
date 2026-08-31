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
        CapNhatGiaoDienThongSo();
    }

    public void CapNhatGiaoDienThongSo()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogWarning("[Stats Display] Chưa tìm thấy QuestSaveSystem!");
            return;
        }

        PlayerStatsSaveData data = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

        if (textTenCanhGioi != null) 
            textTenCanhGioi.text = $"Cảnh Giới: {data.tenCanhGioi}";

        if (textCapDo != null) 
            textCapDo.text = $"Cấp Độ: {data.level}";

        if (textKinhNghiem != null) 
            textKinhNghiem.text = $"EXP: {data.currentExp} / {data.maxExp}";

        if (textMau != null) 
            textMau.text = $"Sinh Lực (HP): {data.maxHP}";

        if (textSatThuong != null) 
            textSatThuong.text = $"Sát Thương: {data.damage}";

        if (textPhongThu != null) 
            textPhongThu.text = $"Giáp: {data.armor * 100f:F0}%";

        if (textNangLuong != null) 
            textNangLuong.text = $"Năng Lượng: {data.maxEnergy}";
    }
}