using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("--- THÀNH PHẦN UI TEXTPRO ---")]
    public TextMeshProUGUI textCanhGioi;
    public TextMeshProUGUI textLevel;
    public TextMeshProUGUI textDamage;
    public TextMeshProUGUI textArmor;
    public TextMeshProUGUI textHP;

    private void OnEnable()
    {
        CapNhatGiaoDienChiSo();
    }

    /// <summary>
    /// Đọc dữ liệu trực tiếp từ Save JSON và đổ lên UI
    /// </summary>
    public void CapNhatGiaoDienChiSo()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null) return;

        PlayerStatsSaveData stats = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

        if (textCanhGioi != null) textCanhGioi.text = $"Cảnh Giới: <color=yellow>{stats.tenCanhGioi}</color>";
        if (textLevel != null) textLevel.text = $"Level: {stats.level}";
        if (textDamage != null) textDamage.text = $"Sát Thương: {stats.damage}";
        if (textArmor != null) textArmor.text = $"Giáp: {stats.armor}";
        if (textHP != null) textHP.text = $"Máu: {stats.maxHP}";
    }
}