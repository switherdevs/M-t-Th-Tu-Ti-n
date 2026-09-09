using UnityEngine;

public class ShieldBlocker : MonoBehaviour
{
    private string targetTag = "PlayerSword";
    private Elite_TongQuan owner;

    public void Init(string swordTag)
    {
        if (!string.IsNullOrEmpty(swordTag))
        {
            targetTag = swordTag;
        }

        // Tìm Elite_TongQuan ở cha hoặc xung quanh
        owner = GetComponentInParent<Elite_TongQuan>();
        if (owner == null)
        {
            owner = FindFirstObjectByType<Elite_TongQuan>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu va chạm đúng với Kiếm của Player
        if (collision.CompareTag(targetTag) || collision.gameObject.name.Contains(targetTag))
        {
            if (owner != null)
            {
                // Gọi Tống Quân bị Choáng khi Player đánh trúng khiên
                owner.ApplyStun(2f);
            }
        }
    }
}