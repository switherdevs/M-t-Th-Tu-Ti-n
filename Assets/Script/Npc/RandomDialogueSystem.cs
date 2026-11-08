using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameCore.Settings; // Bổ sung Namespace để đọc SettingsManager

public class RandomDialogueSystem : MonoBehaviour
{
    [Header("--- THÀNH PHẦN UI & ÂM THANH ---")]
    [SerializeField] private TextMeshProUGUI txtTenNPC;
    [SerializeField] private TextMeshProUGUI txtNoiDungThoai;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amThanhGoChu;

    [Header("--- CẤU HÌNH NPC & THỜI GIAN ---")]
    [SerializeField] private string tenNPC = "Cao Nhân Ẩn Danh";
    [SerializeField] private string tenNPC_EN = "Anonymous Master"; // Tên NPC Tiếng Anh
    [SerializeField] private float tocDoGoChu = 0.04f;
    [SerializeField] private float thoiGianDoiThoai = 30f;

    [Header("--- DANH SÁCH THOẠI RANDOM (TIẾNG VIỆT - MẶC ĐỊNH) ---")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> danhSachThoaiRandom = new List<string>();

    [Header("--- DANH SÁCH THOẠI RANDOM (TIẾNG ANH - OVERRIDE) ---")]
    [Tooltip("Danh sách thoại bằng Tiếng Anh tương ứng. Nếu để trống sẽ tự động dùng lại Tiếng Việt")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> danhSachThoaiRandomEN = new List<string>();

    private Coroutine coroutineGoChu;
    private Coroutine coroutineDemThoiGian;
    private int indexThoaiVuaChay = -1;
    private string cauThoaiHienTaiDangChay = ""; // Lưu lại câu thoại hiện tại để phục vụ đổi ngôn ngữ

    private void OnEnable()
    {
        // 🎯 Lắng nghe sự kiện đổi ngôn ngữ toàn cục để cập nhật giao diện ngay lập tức
        SettingsManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        SettingsManager.OnLanguageChanged -= HandleLanguageChanged;

        // Dọn dẹp Coroutine khi GameObject bị ẩn/tắt
        if (coroutineGoChu != null) StopCoroutine(coroutineGoChu);
        if (coroutineDemThoiGian != null) StopCoroutine(coroutineDemThoiGian);
    }

    private void Start()
    {
        CapNhatTenNPC();

        // Bắt đầu luồng chạy thoại tự động
        coroutineDemThoiGian = StartCoroutine(DemThoiGianDoiThoaiRoutine());
    }

    /// <summary>
    /// Xử lý khi người chơi thay đổi ngôn ngữ trong Settings
    /// </summary>
    private void HandleLanguageChanged(string newLang)
    {
        CapNhatTenNPC();

        // Nếu đang hiển thị thoại, ta có thể đổi sang danh sách tương ứng hoặc giữ nguyên câu dịch
        // Ở đây ta gọi làm mới lại câu thoại hiện tại để chuyển ngữ ngay lập tức
        if (!string.IsNullOrEmpty(cauThoaiHienTaiDangChay))
        {
            ChayThoaiRandomMoi();
        }
    }

    /// <summary>
    /// Kiểm tra Settings và cập nhật tên NPC hiển thị theo ngôn ngữ
    /// </summary>
    private void CapNhatTenNPC()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        if (txtTenNPC != null)
        {
            if (isEN && !string.IsNullOrEmpty(tenNPC_EN))
            {
                txtTenNPC.text = tenNPC_EN;
            }
            else
            {
                txtTenNPC.text = tenNPC;
            }
        }
    }

    private IEnumerator DemThoiGianDoiThoaiRoutine()
    {
        while (true)
        {
            ChayThoaiRandomMoi();
            yield return new WaitForSeconds(thoiGianDoiThoai);
        }
    }

    public void ChayThoaiRandomMoi()
    {
        // 1. Kiểm tra ngôn ngữ từ SettingsManager
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        // 2. Cập nhật lại tên NPC
        CapNhatTenNPC();

        // 3. Chọn danh sách thoại tương ứng dựa vào ngôn ngữ (nếu tiếng Anh trống thì fallback về tiếng Việt)
        List<string> danhSachHienTai = danhSachThoaiRandom;

        if (isEN && danhSachThoaiRandomEN != null && danhSachThoaiRandomEN.Count > 0)
        {
            danhSachHienTai = danhSachThoaiRandomEN;
        }

        if (danhSachHienTai == null || danhSachHienTai.Count == 0) return;

        // 4. Chọn index ngẫu nhiên (tránh lặp lại câu vừa nói nếu mảng có từ 2 câu trở lên)
        int indexMoi = Random.Range(0, danhSachHienTai.Count);
        if (danhSachHienTai.Count > 1)
        {
            while (indexMoi == indexThoaiVuaChay)
            {
                indexMoi = Random.Range(0, danhSachHienTai.Count);
            }
        }
        indexThoaiVuaChay = indexMoi;

        cauThoaiHienTaiDangChay = danhSachHienTai[indexMoi];

        // 5. Dừng hiệu ứng gõ chữ cũ nếu đang chạy dở
        if (coroutineGoChu != null)
        {
            StopCoroutine(coroutineGoChu);
        }

        coroutineGoChu = StartCoroutine(GoChuKemAmThanhRoutine(cauThoaiHienTaiDangChay));
    }

    private IEnumerator GoChuKemAmThanhRoutine(string chuoiVanBan)
    {
        if (txtNoiDungThoai == null) yield break;

        txtNoiDungThoai.text = "";

        foreach (char c in chuoiVanBan.ToCharArray())
        {
            txtNoiDungThoai.text += c;

            // Phát âm thanh gõ chữ theo từng ký tự (bỏ qua khoảng trắng)
            if (c != ' ' && audioSource != null && amThanhGoChu != null)
            {
                audioSource.PlayOneShot(amThanhGoChu);
            }

            yield return new WaitForSeconds(tocDoGoChu);
        }
    }
}