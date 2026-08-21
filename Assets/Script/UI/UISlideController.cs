using UnityEngine;
using UnityEngine.EventSystems; // 🎯 BẮT BỘC để dùng IPointerClickHandler cho UI Canvas

public class UISlideController : MonoBehaviour, IPointerClickHandler
{
    [Header("=== CẤU HÌNH VỊ TRÍ (RectTransform) ===")]
    [SerializeField, Tooltip("Kéo RectTransform vị trí ban đầu vào đây")]
    private RectTransform viTri1;

    [SerializeField, Tooltip("Kéo RectTransform vị trí trượt tới vào đây")]
    private RectTransform viTri2;

    [Header("=== CẤU HÌNH TỐC ĐỘ TRƯỢT ===")]
    [SerializeField, Tooltip("Tốc độ trượt mượt giữa 2 vị trí")]
    private float tocDoTruot = 5f;

    private RectTransform rectTransform;
    private Vector3 viTriMucTieu;
    private bool dangOViTri2 = false;

    private void Awake()
    {
        // Lấy RectTransform của chính GameObject UI này
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (viTri1 == null || viTri2 == null)
        {
            Debug.LogError($"[UISlideController] Chưa gán viTri1 hoặc viTri2 trên UI: {gameObject.name}");
            return;
        }

        // Mặc định đặt UI ở vị trí 1
        rectTransform.position = viTri1.position;
        viTriMucTieu = viTri1.position;
    }

    private void Update()
    {
        if (viTri1 == null || viTri2 == null) return;

        // Trượt mượt mà bằng Vector3.Lerp chuẩn cho UI Canvas
        rectTransform.position = Vector3.Lerp(
            rectTransform.position,
            viTriMucTieu,
            Time.deltaTime * tocDoTruot
        );
    }

    /// <summary>
    /// Hàm chuẩn của Unity UI kích hoạt khi người chơi CLICK vào UI này
    /// (Đảm bảo ô 'Raycast Target' trên Image đã được TICK chọn!)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (viTri1 == null || viTri2 == null) return;

        // Đổi trạng thái qua lại giữa 2 vị trí
        dangOViTri2 = !dangOViTri2;

        if (dangOViTri2)
        {
            viTriMucTieu = viTri2.position;
            Debug.Log($"[UISlideController] UI đang trượt sang Vị trí 2.");
        }
        else
        {
            viTriMucTieu = viTri1.position;
            Debug.Log($"[UISlideController] UI đang trượt về Vị trí 1.");
        }
    }
}