using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    // Độ mờ mong muốn khi người chơi đi vào (Ví dụ: 0.5f nghĩa là mờ đi 50%)
    [Range(0f, 1f)]
    [SerializeField] private float fadedAlpha = 0.5f;

    // Thành phần điều khiển hình ảnh của vật thể
    private SpriteRenderer spriteRenderer;
    // Lưu lại màu gốc ban đầu của vật thể (để sau này khôi phục lại rõ nét)
    private Color originalColor;

    private void Awake()
    {
        // TỰ ĐỘNG lấy thành phần SpriteRenderer trên chính vật thể này
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Khi người chơi đi xuyên vào vùng bán kính (Trigger) của vật thể
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có đúng là Người chơi chạm vào không
        if (other.CompareTag("Player"))
        {
            // Làm mờ vật thể đi bằng cách đổi thông số Alpha (độ trong suốt)
            SetSpriteAlpha(fadedAlpha);
        }
    }

    // Khi người chơi đi ra khỏi vùng bán kính (Trigger) của vật thể
    private void OnTriggerExit2D(Collider2D other)
    {
        // Nếu người chơi rời đi
        if (other.CompareTag("Player"))
        {
            // Khôi phục lại màu sắc rõ nét ban đầu
            SetSpriteAlpha(originalColor.a);
        }
    }

    // Hàm bổ trợ dùng để thay đổi độ trong suốt (Alpha) của Sprite
    private void SetSpriteAlpha(float alphaValue)
    {
        if (spriteRenderer != null)
        {
            Color newColor = spriteRenderer.color;
            newColor.a = alphaValue; // Gán giá trị alpha mới truyền vào
            spriteRenderer.color = newColor; // Cập nhật lại màu cho Sprite
        }
    }
}