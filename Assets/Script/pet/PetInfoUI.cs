using UnityEngine;
using TMPro;

public class PetInfoUI : MonoBehaviour
{
    [Header("--- THAM CHIẾU PET ---")]
    [SerializeField] private PetController petTarget;

    [Header("--- UI TEXT MESH PRO ---")]
    [SerializeField] private TextMeshProUGUI txtTenPet;
    [SerializeField] private TextMeshProUGUI txtCapDo;
    [SerializeField] private TextMeshProUGUI txtSatThuong;
    [SerializeField] private TextMeshProUGUI txtHeNguHanh;
    [SerializeField] private TextMeshProUGUI txtBanKinhDanh;

    private void Start()
    {
        CapNhatGiaoDienThongTinPet();
    }

    public void CapNhatGiaoDienThongTinPet()
    {
        if (petTarget == null) return;

        if (txtTenPet != null) txtTenPet.text = $"Linh Thú: {petTarget.TenPet}";
        if (txtCapDo != null) txtCapDo.text = $"Cấp: {petTarget.CấpĐộ}";
        if (txtSatThuong != null) txtSatThuong.text = $"Sát Thương: {petTarget.SátThương}";
        if (txtHeNguHanh != null) txtHeNguHanh.text = $"Hệ: {petTarget.HệNguHanh}";
        if (txtBanKinhDanh != null) txtBanKinhDanh.text = $"Tầm Đánh: {petTarget.BánKínhTấnCông}m";
    }
}