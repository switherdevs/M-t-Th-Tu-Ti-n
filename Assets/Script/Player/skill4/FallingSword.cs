using UnityEngine;

public class FallingSword : MonoBehaviour
{
    [Header("--- CẤU HÌNH DI CHUYỂN ---")]
    [Tooltip("Tốc độ kiếm rơi xuống theo phương thẳng đứng")]
    public float tocDoRoi = 20f;

    [Header("--- CẤU HÌNH THỜI GIAN BIẾN MẤT ---")]
    [Tooltip("Thời gian biến mất ngắn nhất (giây)")]
    public float thoiGianBienMatMin = 1.5f;

    [Tooltip("Thời gian biến mất dài nhất (giây)")]
    public float thoiGianBienMatMax = 3.5f;

    [Header("--- HIỆU ỨNG VÀ ÂM THANH ---")]
    [Tooltip("Prefab hiệu ứng VFX xuất hiện khi kiếm biến mất")]
    public GameObject prefabHieuUng;

    [Tooltip("File âm thanh SFX phát ra 1 lần khi kiếm biến mất")]
    public AudioClip amThanhBienMat;

    [Tooltip("Độ rộng/Âm lượng âm thanh phát ra (0.0 đến 1.0)")]
    [Range(0f, 1f)] public float amLuong = 1f;

    // Biến cờ đảm bảo hiệu ứng và âm thanh chỉ tạo đúng 1 lần
    private bool daBienMat = false;

    private void Start()
    {
        // 1. Tính toán ngẫu nhiên thời gian tồn tại của kiếm
        float thoiGianTonTai = Random.Range(thoiGianBienMatMin, thoiGianBienMatMax);

        // 2. Hẹn giờ gọi hàm BienMat() sau khoảng thời gian ngẫu nhiên
        Invoke(nameof(BienMat), thoiGianTonTai);
    }

    private void Update()
    {
        // Cho kiếm rơi xuống theo chiều âm trục Y thế giới (Vector3.down)
        transform.Translate(Vector3.down * tocDoRoi * Time.deltaTime, Space.World);
    }

    // 🎯 Hàm xử lý khi kiếm biến mất
    private void BienMat()
    {
        // Kiểm tra nếu đã biến mất rồi thì dừng lại, tránh chạy lặp lại
        if (daBienMat) return;
        daBienMat = true;

        // 1. Sinh ra Prefab hiệu ứng VFX tại đúng vị trí của kiếm
        if (prefabHieuUng != null)
        {
            Instantiate(prefabHieuUng, transform.position, transform.rotation);
        }

        // 2. Phát âm thanh SFX 1 lần tại tọa độ 3D của kiếm
        if (amThanhBienMat != null)
        {
            AudioSource.PlayClipAtPoint(amThanhBienMat, transform.position, amLuong);
        }

        // 3. Xóa GameObject kiếm khỏi Scene
        Destroy(gameObject);
    }
}