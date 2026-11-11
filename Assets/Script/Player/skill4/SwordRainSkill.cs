using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SwordRainSkill : MonoBehaviour
{
    public static SwordRainSkill Instance;

    [Header("--- CẤU HÌNH TEST GAME ---")]
    [Tooltip("Tick chọn để dùng Skill tự do không cần tích đủ năng lượng")]
    public bool testGame = false;

    [Header("--- CẤU HÌNH HIỂN THỊ KHI ĐẦY NĂNG LƯỢNG ---")]
    [Tooltip("GameObject sẽ hiện lên khi năng lượng tích đầy và ẩn đi khi chưa đầy")]
    [SerializeField] private GameObject notifyGameObject;

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
    [Tooltip("Tổng thời gian mưa kiếm kéo dài (giây)")]
    public float thoiGianDuyTriMuaKiem = 5f;

    [Tooltip("Số lượng kiếm khi thi triển skill (Mặc định: 1000)")]
    [SerializeField] private int soLuongKiem = 1000;

    [Header("--- CẤU HÌNH NĂNG LƯỢNG SKILL ---")]
    [Tooltip("Mức năng lượng tối đa cần để thi triển Skill")]
    public float nangLuongToiDa = 100f;

    [Tooltip("Mức năng lượng hiện tại của kỹ năng Mưa Kiếm (Có thể theo dõi trực tiếp trên Inspector)")]
    [SerializeField] private float nangLuongHienTai = 0f;

    public float NangLuongHienTai => nangLuongHienTai;

    [Tooltip("Thanh Slider UI hiển thị năng lượng Skill")]
    public Slider thanhNangLuongUI;

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
        if (Input.GetKeyDown(KeyCode.H))
        {
            KiemTraVaKichHoatSkill();
        }
    }

    public void CongNangLuong(float luongCong)
    {
        nangLuongHienTai += luongCong;
        nangLuongHienTai = Mathf.Clamp(nangLuongHienTai, 0f, nangLuongToiDa);
        CapNhatGiaoDienUI();

        Debug.Log($"<color=cyan>[SwordRainSkill]</color> +{luongCong} Năng lượng! Hiện tại: {nangLuongHienTai}/{nangLuongToiDa}");
    }

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

    private IEnumerator Routine_ThucHienMuaKiem()
    {
        if (prefabKiem == null)
        {
            Debug.LogError("Chưa gán Prefab Kiếm vào Script SwordRainSkill!");
            yield break;
        }

        int tongSoKiem = Mathf.Max(1, soLuongKiem);
        float khoangCachCachNhauRaiKiem = thoiGianDuyTriMuaKiem / tongSoKiem;

        // 🔄 VÒNG LẶP THẢ KIẾM:
        // - Lặp qua từng lượt thả kiếm dựa trên tổng số lượng cấu hình.
        // - Tạo bản sao (Instantiate) thanh kiếm tại vị trí ngẫu nhiên trong vùng Box Overlap.
        // - Chờ một khoảng thời gian ngắn (khoangCachCachNhauRaiKiem) trước khi tiếp tục thả thanh kế tiếp.
        for (int i = 0; i < tongSoKiem; i++)
        {
            Transform diemGoc = (tamVungMuaKiem != null) ? tamVungMuaKiem : transform;

            Vector3 viTriTamThucTe = diemGoc.TransformPoint(viTriLechVungBox);

            float offsetX = Random.Range(-kichThuocVungBox.x / 2f, kichThuocVungBox.x / 2f);
            float offsetY = Random.Range(-kichThuocVungBox.y / 2f, kichThuocVungBox.y / 2f);
            float offsetZ = Random.Range(-kichThuocVungBox.z / 2f, kichThuocVungBox.z / 2f);

            Vector3 offsetNgauNhien = new Vector3(offsetX, offsetY, offsetZ);
            Vector3 viTriXuatHien = viTriTamThucTe + (diemGoc.rotation * offsetNgauNhien);

            Quaternion gocXoayKiem = prefabKiem.transform.rotation;

            Instantiate(prefabKiem, viTriXuatHien, gocXoayKiem);

            yield return new WaitForSeconds(khoangCachCachNhauRaiKiem);
        }

        coroutineMuaKiem = null;
    }

    private void CapNhatGiaoDienUI()
    {
        if (thanhNangLuongUI != null)
        {
            thanhNangLuongUI.maxValue = nangLuongToiDa;
            thanhNangLuongUI.value = nangLuongHienTai;
        }

        // 🎯 KIỂM TRA TRẠNG THÁI HIỂN THỊ GAMEOBJECT KHI ĐẦY/CHƯA ĐẦY NĂNG LƯỢNG
        if (notifyGameObject != null)
        {
            bool daDayNangLuong = (nangLuongHienTai >= nangLuongToiDa);
            notifyGameObject.SetActive(daDayNangLuong);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform diemGoc = (tamVungMuaKiem != null) ? tamVungMuaKiem : transform;

        Matrix4x4 maTranCu = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(diemGoc.position, diemGoc.rotation, Vector3.one);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(viTriLechVungBox, kichThuocVungBox);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(viTriLechVungBox, kichThuocVungBox);

        Gizmos.matrix = maTranCu;
    }
}