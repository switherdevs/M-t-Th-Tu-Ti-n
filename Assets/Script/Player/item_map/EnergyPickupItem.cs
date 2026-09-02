using UnityEngine;

public class EnergyPickupItem : MonoBehaviour
{
    [Header("--- THÔNG SỐ HỒI NĂNG LƯỢNG ---")]
    [Tooltip("Số lượng năng lượng hồi lại khi nhặt")]
    [SerializeField] private float luongNangLuongHoi = 30f;

    [Tooltip("Hiệu ứng khi nhặt item (Option)")]
    [SerializeField] private GameObject vfxNhatItem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerSkillManager skillManager = collision.GetComponent<PlayerSkillManager>();

        if (skillManager != null)
        {
            // Thực hiện hồi năng lượng trực tiếp
            bool hoanthanh = HoiNangLuongChoPlayer(skillManager, luongNangLuongHoi);

            if (hoanthanh)
            {
                if (vfxNhatItem != null)
                {
                    GameObject vfx = Instantiate(vfxNhatItem, transform.position, Quaternion.identity);
                    Destroy(vfx, 1.5f);
                }

                Debug.Log($"<color=cyan>[Pickup]</color> Đã hồi {luongNangLuongHoi} Năng Lượng cho người chơi.");
                Destroy(gameObject);
            }
        }
    }

    private bool HoiNangLuongChoPlayer(PlayerSkillManager manager, float amount)
    {
        // Sử dụng Reflection hoặc gọi hàm public nếu có. 
        // Ở đây ta thêm trực tiếp hàm hồi năng lượng thông minh
        return manager.HoiNangLuongTrucTiep(amount);
    }
}