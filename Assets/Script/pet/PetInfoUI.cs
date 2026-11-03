using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetInfoUI : MonoBehaviour
{
    [Header("--- CẤU HÌNH THÔNG SỐ CƠ BẢN ---")]
    [SerializeField] private string tenPet = "Hỏa Hồ Tinh";
    [SerializeField] private string heNguHanh = "Hỏa";
    [SerializeField] private float satThuongGốc = 30f;
    [SerializeField] private float banKinhQuetGốc = 6f;
    [SerializeField] private int capDoToiDa = 10;

    [Header("--- HỆ SỐ NHÂN NGUYÊN LIỆU (TÙY CHỈNH TRÊN UI) ---")]
    [SerializeField] private float heSoItem1 = 5f;
    [SerializeField] private float heSoItem2 = 2f;
    [SerializeField] private float heSoItem3 = 1f;

    [Header("--- UI TEXT MESH PRO (THÔNG SỐ) ---")]
    [SerializeField] private TextMeshProUGUI txtTenPet;
    [SerializeField] private TextMeshProUGUI txtCapDo;
    [SerializeField] private TextMeshProUGUI txtSatThuong;
    [SerializeField] private TextMeshProUGUI txtHeNguHanh;
    [SerializeField] private TextMeshProUGUI txtBanKinhDanh;

    [Header("--- UI NÂNG CẤP (NGUYÊN LIỆU & NÚT) ---")]
    [SerializeField] private Button btnNangCap;
    [SerializeField] private TextMeshProUGUI txtThongTinNguyenLieu1;
    [SerializeField] private TextMeshProUGUI txtThongTinNguyenLieu2;
    [SerializeField] private TextMeshProUGUI txtThongTinNguyenLieu3;

    [Header("--- SPRITE ICON 3 ITEM (LUÔN HIỆN) ---")]
    [SerializeField] private GameObject iconItem1;
    [SerializeField] private GameObject iconItem2;
    [SerializeField] private GameObject iconItem3;

    [Header("--- ÂM THANH NÂNG CẤP ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amThanhNangCapThanhCong;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (btnNangCap != null)
        {
            btnNangCap.onClick.RemoveAllListeners();
            btnNangCap.onClick.AddListener(OnClickNangCapPet);
        }
    }

    private void OnEnable()
    {
        CapNhatGiaoDienThongTinPet();
    }

    private void Start()
    {
        CapNhatGiaoDienThongTinPet();
    }

    public void CapNhatGiaoDienThongTinPet()
    {
        int capDoHienTai = (QuestSaveSystem.Instance != null) ? QuestSaveSystem.Instance.LayCapDoPet() : 1;

        // Tính toán chỉ số theo cấp số nhân 1.5 giống hệt logic của Pet
        float heSo = Mathf.Pow(1.5f, capDoHienTai - 1);
        float satThuongHienTai = Mathf.Round(satThuongGốc * heSo);
        float banKinhHienTai = Mathf.Round(banKinhQuetGốc * heSo * 10f) / 10f;

        if (txtTenPet != null) txtTenPet.text = $"Linh Thú: {tenPet}";
        if (txtCapDo != null) txtCapDo.text = $"Cấp: {capDoHienTai} / {capDoToiDa}";
        if (txtSatThuong != null) txtSatThuong.text = $"Sát Thương: {satThuongHienTai}";
        if (txtHeNguHanh != null) txtHeNguHanh.text = $"Hệ: {heNguHanh}";
        if (txtBanKinhDanh != null) txtBanKinhDanh.text = $"Tầm Quét: {banKinhHienTai}m";

        if (capDoHienTai >= capDoToiDa)
        {
            AnHienTatCaNguyenLieu(false, "Đã đạt cấp tối đa!");
            AnHienNutNangCap(false);
            return;
        }

        int can1 = Mathf.RoundToInt(10 + (capDoHienTai * heSoItem1));
        int can2 = Mathf.RoundToInt(5 + (capDoHienTai * heSoItem2));
        int can3 = Mathf.RoundToInt(1 + (capDoHienTai * heSoItem3));

        int kho1 = QuestSaveSystem.Instance != null ? QuestSaveSystem.Instance.LaySoLuongItemTrongKho("1") : 0;
        int kho2 = QuestSaveSystem.Instance != null ? QuestSaveSystem.Instance.LaySoLuongItemTrongKho("2") : 0;
        int kho3 = QuestSaveSystem.Instance != null ? QuestSaveSystem.Instance.LaySoLuongItemTrongKho("3") : 0;

        if (txtThongTinNguyenLieu1 != null) txtThongTinNguyenLieu1.text = $"{kho1} / {can1}";
        if (txtThongTinNguyenLieu2 != null) txtThongTinNguyenLieu2.text = $"{kho2} / {can2}";
        if (txtThongTinNguyenLieu3 != null) txtThongTinNguyenLieu3.text = $"{kho3} / {can3}";

        if (iconItem1 != null) iconItem1.SetActive(true);
        if (iconItem2 != null) iconItem2.SetActive(true);
        if (iconItem3 != null) iconItem3.SetActive(true);

        bool duTatCa = (QuestSaveSystem.Instance != null && kho1 >= can1 && kho2 >= can2 && kho3 >= can3);
        AnHienNutNangCap(duTatCa);
    }

    private void AnHienTatCaNguyenLieu(bool hienThi, string thongBaoMax = "")
    {
        if (iconItem1 != null) iconItem1.SetActive(hienThi);
        if (iconItem2 != null) iconItem2.SetActive(hienThi);
        if (iconItem3 != null) iconItem3.SetActive(hienThi);

        if (txtThongTinNguyenLieu1 != null) txtThongTinNguyenLieu1.text = hienThi ? "" : thongBaoMax;
        if (txtThongTinNguyenLieu2 != null) txtThongTinNguyenLieu2.text = "";
        if (txtThongTinNguyenLieu3 != null) txtThongTinNguyenLieu3.text = "";
    }

    private void AnHienNutNangCap(bool duocPhep)
    {
        if (btnNangCap != null)
        {
            btnNangCap.interactable = duocPhep;
        }
    }

    private void OnClickNangCapPet()
    {
        if (QuestSaveSystem.Instance == null) return;

        int capDoHienTai = QuestSaveSystem.Instance.LayCapDoPet();
        if (capDoHienTai >= capDoToiDa) return;

        int can1 = Mathf.RoundToInt(10 + (capDoHienTai * heSoItem1));
        int can2 = Mathf.RoundToInt(5 + (capDoHienTai * heSoItem2));
        int can3 = Mathf.RoundToInt(1 + (capDoHienTai * heSoItem3));

        int kho1 = QuestSaveSystem.Instance.LaySoLuongItemTrongKho("1");
        int kho2 = QuestSaveSystem.Instance.LaySoLuongItemTrongKho("2");
        int kho3 = QuestSaveSystem.Instance.LaySoLuongItemTrongKho("3");

        if (kho1 >= can1 && kho2 >= can2 && kho3 >= can3)
        {
            // Trừ nguyên liệu trực tiếp vào file save
            QuestSaveSystem.Instance.LuuItemVaoSaveGame("1", -can1);
            QuestSaveSystem.Instance.LuuItemVaoSaveGame("2", -can2);
            QuestSaveSystem.Instance.LuuItemVaoSaveGame("3", -can3);

            // Tăng cấp độ và lưu vào file save
            int capDoMoi = capDoHienTai + 1;
            QuestSaveSystem.Instance.LuuCapDoPet(capDoMoi);

            // Phát âm thanh thành công
            if (audioSource != null && amThanhNangCapThanhCong != null)
            {
                audioSource.PlayOneShot(amThanhNangCapThanhCong);
            }

            // Làm mới giao diện ngay lập tức
            CapNhatGiaoDienThongTinPet();
        }
    }
}