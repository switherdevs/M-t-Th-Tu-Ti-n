using UnityEngine;

// Script quản lý tự hủy & va chạm tường tương thích Object Pool
public class AutoDestroy : MonoBehaviour
{
    [Header("THỜI GIAN TỰ HỦY")]
    [SerializeField] private float time = 3f; // Khoảng thời gian tự hủy tối đa

    [Header("MIỄN TRỪ VA CHẠM BAN ĐẦU")]
    [SerializeField] private float ignoreWallTime = 0.2f; // Thời gian miễn dịch va chạm tường khi vừa spawn

    private float spawnTimer;

    // Sử dụng OnEnable thay vì Start để mỗi lần lấy từ Object Pool ra timer đều chạy lại
    private void OnEnable()
    {
        spawnTimer = 0f;
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
        // Chỉ phá hủy nếu đạn đã xuất hiện vượt qua khoảng thời gian ignoreWallTime
        if (spawnTimer >= ignoreWallTime && other.gameObject.CompareTag("Wall"))
        {
            TuHuyDirect();
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