using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    [Header("Cấu hình Mờ Theo Khung Va Chạm (Collider Fade)")]
    [Tooltip("Độ mờ ngay tại vị trí người chơi (0 = trong suốt hoàn toàn, 0.5 = mờ 50%)")]
    [Range(0f, 1f)]
    [SerializeField] private float fadedAlpha = 0.3f;

    [Tooltip("Mở rộng/Thu nhỏ vùng mờ xung quanh Collider của Player (đơn vị Unity)")]
    [SerializeField] private float padding = 0.2f;

    [Tooltip("Độ làm mượt/làm nhòe phần viền xung quanh hình dáng Player")]
    [SerializeField] private float smoothness = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Material instanceMaterial;
    private Collider2D playerCollider;
    private bool isPlayerInside = false;

    // Các biến đặt tên Property trong Shader
    private static readonly int PlayerMinID = Shader.PropertyToID("_PlayerMin");
    private static readonly int PlayerMaxID = Shader.PropertyToID("_PlayerMax");
    private static readonly int MinAlphaID = Shader.PropertyToID("_MinAlpha");
    private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // Tạo bản sao Material riêng để không ảnh hưởng các vật thể khác
            instanceMaterial = spriteRenderer.material;
        }
    }

    private void Update()
    {
        // Khi người chơi ở trong vùng che khuất, liên tục cập nhật kích thước Collider của Player vào Shader
        if (isPlayerInside && playerCollider != null && instanceMaterial != null)
        {
            // Lấy tọa độ góc dưới-trái (Min) và góc trên-phải (Max) của Collider Player
            Bounds bounds = playerCollider.bounds;
            Vector4 minPos = bounds.min - new Vector3(padding, padding, 0);
            Vector4 maxPos = bounds.max + new Vector3(padding, padding, 0);

            instanceMaterial.SetVector(PlayerMinID, minPos);
            instanceMaterial.SetVector(PlayerMaxID, maxPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCollider = other;
            isPlayerInside = true;

            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(MinAlphaID, fadedAlpha);
                instanceMaterial.SetFloat(SmoothnessID, smoothness);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Đưa vùng mờ về 0 khi Player đi ra ngoài
            if (instanceMaterial != null)
            {
                instanceMaterial.SetVector(PlayerMinID, Vector4.zero);
                instanceMaterial.SetVector(PlayerMaxID, Vector4.zero);
            }
        }
    }
}