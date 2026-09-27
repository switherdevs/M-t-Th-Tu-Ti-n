using UnityEngine;

public class ExplodeOnWall : MonoBehaviour
{
    [Header("=== CẤU HÌNH VA CHẠM ===")]
    [Tooltip("Chọn Layer đại diện cho Tường (Wall)")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Prefab hiệu ứng phát nổ (VFX) sẽ sinh ra khi chạm tường")]
    [SerializeField] private GameObject explosionPrefab;


    [Header("=== CẤU HÌNH ÂM THANH NỔ ===")]
    [Tooltip("File âm thanh khi va chạm vào tường phát nổ")]
    [SerializeField] private AudioClip explosionSound;

    [Tooltip("Âm lượng âm thanh nổ (0 đến 1)")]
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem Layer của vật thể va chạm có thuộc wallLayer hay không
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            ExplodeAndDestroy();
        }
    }

    // Nếu game của bạn dùng Collider dạng 3D / Vật lý 3D thì dùng hàm này:
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            ExplodeAndDestroy();
        }
    }
    */

    private void ExplodeAndDestroy()
    {
        // 1. Phát âm thanh phát nổ 1 lần duy nhất tại vị trí va chạm
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // 2. Sinh ra Prefab phát nổ tại vị trí hiện tại
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }

        // 3. Biến mất (Xóa GameObject gốc khỏi Hierarchy)
        Destroy(gameObject);
    }
}