using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SwordRainSkill : MonoBehaviour
{
    public static SwordRainSkill Instance;

    [Header("--- CẤU HÌNH TEST GAME ---")]
    [Tooltip("Tick chọn để dùng Skill tự do không cần tích đủ năng lượng")]
    public bool testGame = false;

    [Header("--- CẤU HÌNH VÙNG MƯA KIẾM (BOX OVERLAP) ---")]
    [Tooltip("Prefab của thanh kiếm thả từ trên trời xuống")]
    public GameObject prefabKiem;

    [Tooltip("Tâm của vùng mưa kiếm (Kéo Player hoặc vị trí mục tiêu vào đây)")]
    public Transform tamVungMuaKiem;

    [Tooltip("Kích thước vùng mưa kiếm dạng hình hộp (Chiều rộng X, Chiều cao Y, Chiều dài Z)")]
    public Vector3 kichThuocVungBox = new Vector3(10f, 5f, 10f);

    [Tooltip("Độ lệch vị trí tâm Box so với mục tiêu (X: Trái/Phải, Y: Lên/Xuống, Z: Trước/Sau)")]
    public Vector3 viTriLechVungBox = new Vector3(0f, 5f, 5f);

    [Header("--- CẤU HÌNH THỜI GIAN & SỐ LƯỢNG ---")]
    [Tooltip("Tổng thời gian mưa kiếm kéo dài (giây). Hết thời gian này sẽ dừng thả kiếm")]
    public float thoiGianDuyTriMuaKiem = 5f;

    [Tooltip("Khoảng thời gian nghỉ (giây) giữa mỗi lần thả 1 thanh kiếm")]
    public float khoangCachCachNhauRaiKiem = 0.1f;

    [Header("--- CẤU HÌNH NĂNG LƯỢNG SKILL ---")]
    [Tooltip("Mức năng lượng tối đa cần để thi triển Skill")]
    public float nangLuongToiDa = 100f;

    [Tooltip("Thanh Slider UI hiển thị năng lượng Skill")]
    public Slider thanhNangLuongUI;

    private float nangLuongHienTai = 0f;
    private Coroutine coroutineMuaKiem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        CapNhatGiaoDienUI();
    }

    private void Update()
    {
        // Bấm phím H (Legacy Input System) để kiểm tra dùng skill
        if (Input.GetKeyDown(KeyCode.H))
        {
            KiemTraVaKichHoatSkill();
        }
    }

    // 🎯 HÀM CỘNG NĂNG LƯỢNG (Đã được sửa lỗi để CharacterStats có thể gọi)
    public void CongNangLuong(float luongCong)
    {
        nangLuongHienTai += luongCong;
        nangLuongHienTai = Mathf.Clamp(nangLuongHienTai, 0f, nangLuongToiDa);
        CapNhatGiaoDienUI();

        Debug.Log($"<color=cyan>[SwordRainSkill]</color> +{luongCong} Năng lượng! Hiện tại: {nangLuongHienTai}/{nangLuongToiDa}");
    }

    // 🎯 Logic kiểm tra điều kiện kích hoạt Skill
    private void KiemTraVaKichHoatSkill()
    {
        if (testGame || nangLuongHienTai >= nangLuongToiDa)
        {
            if (coroutineMuaKiem != null)
            {
                StopCoroutine(coroutineMuaKiem);
            }

            coroutineMuaKiem = StartCoroutine(Routine_ThucHienMuaKiem());

            if (!testGame)
            {
                nangLuongHienTai = 0f;
                CapNhatGiaoDienUI();
            }
        }
        else
        {
            Debug.Log("<color=yellow>[Skill Mưa Kiếm]</color> Chưa đủ năng lượng! Đang có: " + nangLuongHienTai + "/" + nangLuongToiDa);
        }
    }

    // 🎯 Coroutine thả kiếm ngẫu nhiên trong vùng Box đã căn chỉnh vị trí theo thời gian
    private IEnumerator Routine_ThucHienMuaKiem()
    {
        if (prefabKiem == null)
        {
            Debug.LogError("Chưa gán Prefab Kiếm vào Script SwordRainSkill!");
            yield break;
        }

        float thoiGianDaTroiQua = 0f;

        while (thoiGianDaTroiQua < thoiGianDuyTriMuaKiem)
        {
            Transform diemGoc = (tamVungMuaKiem != null) ? tamVungMuaKiem : transform;

            // Tính toán vị trí tâm thực tế của Box có cộng thêm độ lệch viTriLechVungBox và xoay theo hướng mặt của Player
            Vector3 viTriTamThucTe = diemGoc.TransformPoint(viTriLechVungBox);

            // Lấy độ lệch ngẫu nhiên X, Y, Z bên trong kích thước Box Overlap
            float offsetX = Random.Range(-kichThuocVungBox.x / 2f, kichThuocVungBox.x / 2f);
            float offsetY = Random.Range(-kichThuocVungBox.y / 2f, kichThuocVungBox.y / 2f);
            float offsetZ = Random.Range(-kichThuocVungBox.z / 2f, kichThuocVungBox.z / 2f);

            // Tọa độ ngẫu nhiên tính theo không gian địa phương của tâm Box
            Vector3 offsetNgauNhien = new Vector3(offsetX, offsetY, offsetZ);
            Vector3 viTriXuatHien = viTriTamThucTe + (diemGoc.rotation * offsetNgauNhien);

            // Mặc định lấy góc quay chuẩn của Prefab Kiếm
            Quaternion gocXoayKiem = prefabKiem.transform.rotation;

            // Sinh ra thanh kiếm
            Instantiate(prefabKiem, viTriXuatHien, gocXoayKiem);

            thoiGianDaTroiQua += khoangCachCachNhauRaiKiem;

            yield return new WaitForSeconds(khoangCachCachNhauRaiKiem);
        }

        coroutineMuaKiem = null;
    }

    // 🎯 Cập nhật UI Slider
    private void CapNhatGiaoDienUI()
    {
        if (thanhNangLuongUI != null)
        {
            thanhNangLuongUI.maxValue = nangLuongToiDa;
            thanhNangLuongUI.value = nangLuongHienTai;
        }
    }

    // 🎯 Vẽ khung hiển thị Vùng Box Overlap trực quan trong cửa sổ Scene (Xoay & Dịch chuyển theo Player)
    private void OnDrawGizmosSelected()
    {
        Transform diemGoc = (tamVungMuaKiem != null) ? tamVungMuaKiem : transform;

        // Lưu ma trận Gizmos cũ
        Matrix4x4 maTranCu = Gizmos.matrix;

        // Thiết lập ma trận Gizmos mới đồng bộ vị trí và góc xoay theo Player
        Gizmos.matrix = Matrix4x4.TRS(diemGoc.position, diemGoc.rotation, Vector3.one);

        // Vẽ khung dây dạng Box màu đỏ theo vị trí lệch viTriLechVungBox
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(viTriLechVungBox, kichThuocVungBox);

        // Vẽ khối mờ màu đỏ bên trong
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(viTriLechVungBox, kichThuocVungBox);

        // Khôi phục ma trận Gizmos cũ
        Gizmos.matrix = maTranCu;
    }
}