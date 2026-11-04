using UnityEngine;

public class TriggerPusher2D : MonoBehaviour
{
    [Header("--- CẤU HÌNH LỰC ĐẨY ---")]
    [Tooltip("Sức mạnh của lực đẩy tác dụng lên đối tượng khác")]
    [SerializeField] private float pushForce = 10f;
    
    [Tooltip("Chỉ định Tag của đối tượng cần đẩy (Để trống nếu muốn đẩy mọi thứ có Rigidbody2D)")]
    [SerializeField] private string targetTag = "Player";

    private void OnTriggerStay2D(Collider2D other)
    {
        // 1. Kiểm tra xem đối tượng chạm vào có component Rigidbody2D hay không
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
        if (otherRb != null)
        {
            // 2. Nếu có cài đặt Tag, kiểm tra xem đối tượng có khớp Tag không
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            {
                return;
            }

            // 3. Tính toán hướng đẩy: Lấy vị trí đối tượng kia trừ đi vị trí hiện tại để ra hướng văng ra ngoài
            Vector2 pushDirection = (other.transform.position - transform.position).normalized;

            // 4. Tác dụng một lực đột ngột (Impulse) theo hướng tính được để đẩy văng đi
            otherRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }
    }
}