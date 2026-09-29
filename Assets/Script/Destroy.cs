using UnityEngine;

// Script quản lý tự hủy & va chạm tường/người chơi tương thích Object Pool
public class AutoDestroy : MonoBehaviour
{
    [Header("CẤU HÌNH ĐẠN (IS ĐẠN)")]
    [Tooltip("Tick chọn nếu đây là viên đạn cần va chạm biến mất và tạo hiệu ứng")]
    [SerializeField] private bool isDan = false;

    [Header("HIỆU ỨNG & ÂM THANH KHI VA CHẠM")]
    [SerializeField] private GameObject hitVFX;    // Prefab Hiệu ứng nổ/trúng đích
    [SerializeField] private float vfxDestroyTime = 1f; // Thời gian tự hủy của Hiệu ứng
    [SerializeField] private AudioClip sfxHit;     // Âm thanh trúng đích (Phát 1 lần)
    [SerializeField] private float sfxVolume = 1f; // Âm lượng âm thanh

    [Header("THỜI GIAN TỰ HỦY")]
    [SerializeField] private float time = 3f; // Khoảng thời gian tự hủy tối đa

    [Header("MIỄN TRỪ VA CHẠM BAN ĐẦU")]
    [SerializeField] private float ignoreWallTime = 0.2f; // Thời gian miễn dịch va chạm tường khi vừa spawn

    private float spawnTimer;
    private bool hasHit = false; // Cờ chống kích hoạt hiệu ứng/âm thanh nhiều lần trong 1 frame

    // Sử dụng OnEnable thay vì Start để mỗi lần lấy từ Object Pool ra timer đều chạy lại
    private void OnEnable()
    {
        spawnTimer = 0f;
        hasHit = false; // Reset lại cờ va chạm mỗi khi tái sử dụng từ Pool
        CancelInvoke(nameof(TuHuyDirect)); // Hủy các lệnh đếm ngược cũ nếu có
        Invoke(nameof(TuHuyDirect), time);  // Đếm ngược thời gian tự hủy mới
    }

    private void Update()
    {
        // Tính thời gian đã trôi qua kể từ khi đạn xuất hiện
        if (spawnTimer < ignoreWallTime)
        {
            spawnTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu không phải là đạn hoặc đã xử lý va chạm rồi thì bỏ qua
        if (!isDan || hasHit) return;

        // Chỉ phá hủy nếu đạn đã xuất hiện vượt qua khoảng thời gian ignoreWallTime
        if (spawnTimer >= ignoreWallTime)
        {
            // Kiểm tra va chạm với Tag "Wall" hoặc "Player"
            if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Player"))
            {
                hasHit = true; // Đánh dấu đã va chạm để tránh gọi lặp lại

                // 1. Tạo GameObject hiệu ứng tại vị trí va chạm (nếu có gán VFX)
                if (hitVFX != null)
                {
                    GameObject vfxInstance = Instantiate(hitVFX, transform.position, Quaternion.identity);
                    Destroy(vfxInstance, vfxDestroyTime);
                }

                // 2. Phát âm thanh 1 lần độc lập tại vị trí va chạm (nếu có gán Sound)
                if (sfxHit != null)
                {
                    AudioSource.PlayClipAtPoint(sfxHit, transform.position, sfxVolume);
                }

                // 3. Cho viên đạn biến mất / tự hủy
                TuHuyDirect();
            }
        }
    }

    // Hàm gọi hủy/trả về Pool
    public void TuHuyDirect()
    {
        // Nếu bạn dùng SetActive(false) cho Object Pool thì thay bằng line dưới:
        // gameObject.SetActive(false);

        // Mặc định phá hủy GameObject nếu không dùng Pooling trực tiếp trong đạn:
        Destroy(gameObject);
    }
}