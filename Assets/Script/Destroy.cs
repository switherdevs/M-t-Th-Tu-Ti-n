using UnityEngine;

// Script quản lý tự hủy & va chạm tường/người chơi tương thích Object Pool
public class AutoDestroy : MonoBehaviour
{
    [Header("CẤU HÌNH ĐẠN (IS ĐẠN)")]
    [Tooltip("Tick chọn nếu đây là viên đạn cần va chạm biến mất và tạo hiệu ứng")]
    [SerializeField] private bool isDan = false;

    [Header("HIỆU ỨNG & ÂM THANH KHI VA CHẠM / HỦY")]
    [SerializeField] private GameObject hitVFX;    // Prefab Hiệu ứng nổ/trúng đích có sẵn trong script[cite: 13]
    [SerializeField] private float vfxDestroyTime = 1f; // Thời gian tự hủy của Hiệu ứng[cite: 13]
    [SerializeField] private AudioClip sfxHit;     // Âm thanh trúng đích/hủy (Phát 1 lần)[cite: 13]
    [SerializeField] private float sfxVolume = 1f; // Âm lượng âm thanh[cite: 13]

    [Header("THỜI GIAN TỰ HỦY")]
    [SerializeField] private float time = 3f; // Khoảng thời gian tự hủy tối đa[cite: 13]

    [Header("MIỄN TRỪ VA CHẠM BAN ĐẦU")]
    [SerializeField] private float ignoreWallTime = 0.2f; // Thời gian miễn dịch va chạm tường khi vừa spawn[cite: 13]

    private float spawnTimer;
    private bool hasHit = false; // Cờ chống kích hoạt va chạm nhiều lần trong 1 frame[cite: 13]
    private bool hasEffectSpawned = false; // Cờ chống tạo hiệu ứng nhiều lần khi hủy

    // Sử dụng OnEnable thay vì Start để mỗi lần lấy từ Object Pool ra timer đều chạy lại[cite: 13]
    private void OnEnable()
    {
        spawnTimer = 0f;
        hasHit = false;
        hasEffectSpawned = false; // Reset lại cờ hiệu ứng mỗi khi tái sử dụng từ Pool
        CancelInvoke(nameof(TuHuyDirect)); // Hủy các lệnh đếm ngược cũ nếu có[cite: 13]
        Invoke(nameof(TuHuyDirect), time);  // Đếm ngược thời gian tự hủy mới[cite: 13]
    }

    private void Update()
    {
        // Tính thời gian đã trôi qua kể từ khi đạn xuất hiện[cite: 13]
        if (spawnTimer < ignoreWallTime)
        {
            spawnTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu không phải là đạn hoặc đã xử lý va chạm rồi thì bỏ qua[cite: 13]
        if (!isDan || hasHit) return;

        // Chỉ phá hủy nếu đạn đã xuất hiện vượt qua khoảng thời gian ignoreWallTime[cite: 13]
        if (spawnTimer >= ignoreWallTime)
        {
            // Kiểm tra va chạm với Tag "Wall" hoặc "Player"[cite: 13]
            if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Player"))
            {
                hasHit = true; // Đánh dấu đã va chạm để tránh gọi lặp lại[cite: 13]

                // Gọi trực tiếp hàm hủy (hàm này sẽ tự động sinh hiệu ứng và âm thanh)
                TuHuyDirect();
            }
        }
    }

    // Hàm gọi hủy/trả về Pool (Nơi tập trung xử lý hiệu ứng khi đối tượng bị tiêu diệt)
    public void TuHuyDirect()
    {
        // Kiểm tra nếu hiệu ứng chưa được sinh ra thì mới thực hiện để tránh gọi nhiều lần
        if (!hasEffectSpawned)
        {
            hasEffectSpawned = true;

            // 1. Tạo GameObject hiệu ứng tại vị trí hiện tại (sử dụng biến hitVFX có sẵn)[cite: 13]
            if (hitVFX != null)
            {
                GameObject vfxInstance = Instantiate(hitVFX, transform.position, Quaternion.identity);
                Destroy(vfxInstance, vfxDestroyTime);
            }

            // 2. Phát âm thanh 1 lần độc lập tại vị trí hiện tại (sử dụng biến sfxHit có sẵn)[cite: 13]
            if (sfxHit != null)
            {
                AudioSource.PlayClipAtPoint(sfxHit, transform.position, sfxVolume);
            }
        }

        // Nếu bạn dùng SetActive(false) cho Object Pool thì thay bằng line dưới:
        // gameObject.SetActive(false);

        // Mặc định phá hủy GameObject[cite: 13]:
        Destroy(gameObject);
    }
}