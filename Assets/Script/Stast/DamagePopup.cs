using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("=== CẤU HÌNH HIỆU ỨNG ===")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveSpeed = 1.5f;   // Tốc độ bay lên
    [SerializeField] private float disappearSpeed = 3f; // Tốc độ mờ dần (Fade out)
    [SerializeField] private float lifeTime = 0.8f;     // Thời gian tồn tại trước khi mờ

    private Color _textColor;
    private float _disappearTimer;

    private void Awake()
    {
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// Hàm nhận số sát thương và màu sắc do DamageDealer chỉ định
    /// </summary>
    public void Setup(float damageAmount, Color textColor)
    {
        if (damageText == null) return;

        // 1. Gán màu chữ trực tiếp theo màu được truyền sang
        damageText.color = textColor;

        // 2. Hiển thị số sát thương và lưu màu gốc
        damageText.text = Mathf.RoundToInt(damageAmount).ToString();
        _textColor = damageText.color;
        _disappearTimer = lifeTime;
    }

    private void Update()
    {
        // 1. Cho chữ bay lên từ từ theo trục Y
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. Đếm ngược thời gian tồn tại
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer <= 0)
        {
            // 3. Giảm độ trong suốt (Alpha) để tạo hiệu ứng mờ dần
            _textColor.a -= disappearSpeed * Time.deltaTime;
            damageText.color = _textColor;

            // 4. Khi mờ hoàn toàn thì xóa Object khỏi Scene
            if (_textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}