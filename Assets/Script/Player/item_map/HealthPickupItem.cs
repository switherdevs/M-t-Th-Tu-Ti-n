using UnityEngine;

public class HealthPickupItem : MonoBehaviour
{
    [Header("--- THÔNG SỐ HỒI MÁU ---")]
    [Tooltip("Số lượng máu hồi lại khi nhặt")]
    [SerializeField] private float luongMauHoi = 25f;

    [Tooltip("Hiệu ứng khi nhặt item (Option)")]
    [SerializeField] private GameObject vfxNhatItem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật thể va chạm có chứa CharacterStats hay không
        CharacterStats stats = collision.GetComponent<CharacterStats>();

        if (stats != null && stats.IsPlayer && !stats.IsDead)
        {
            // Nếu máu chưa đầy thì mới nhặt được
            if (stats.CurrentHealth < stats.MaxHealth.Value)
            {
                stats.Heal(luongMauHoi);

                if (vfxNhatItem != null)
                {
                    GameObject vfx = Instantiate(vfxNhatItem, transform.position, Quaternion.identity);
                    Destroy(vfx, 1.5f);
                }

                Debug.Log($"<color=green>[Pickup]</color> Đã hồi {luongMauHoi} HP cho người chơi.");
                Destroy(gameObject);
            }
        }
    }
}