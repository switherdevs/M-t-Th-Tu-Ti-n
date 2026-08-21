using UnityEngine;
using UnityEngine.EventSystems;

public class UISlideController : MonoBehaviour, IPointerClickHandler
{
    [Header("=== CẤU HÌNH TRẠNG THÁI BAN ĐẦU ===")]
    [SerializeField, Tooltip("Tích chọn nếu muốn UI mặc định ở vị trí ẨN ngay khi vào game để không chắn màn hình")]
    private bool macDinhAn = false;

    [Header("=== CẤU HÌNH KHOẢNG CÁCH TRƯỢT ===")]
    [SerializeField, Tooltip("Khoảng cách trượt theo X và Y khi bấm toggle (Ví dụ: X = -500 để trượt sang trái)")]
    private Vector2 doDichChuyen = new Vector2(-500f, 0f);

    [Header("=== CẤU HÌNH TỐC ĐỘ ===")]
    [SerializeField, Tooltip("Tốc độ trượt mượt")]
    private float tocDoTruot = 10f;

    private RectTransform rectTransform;
    private Vector2 viTriBanDau;
    private Vector2 viTriAn;
    private Vector2 viTriMucTieu;
    private bool dangAn = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // 1. Lưu vị trí hiển thị chuẩn bạn đã đặt trên Editor
        viTriBanDau = rectTransform.anchoredPosition;

        // 2. Tính vị trí ẩn dựa trên khoảng dịch chuyển
        viTriAn = viTriBanDau + doDichChuyen;

        // 3. Kiểm tra biến tích chọn Mac Dinh An để thiết lập vị trí khởi đầu
        if (macDinhAn)
        {
            dangAn = true;
            rectTransform.anchoredPosition = viTriAn;
            viTriMucTieu = viTriAn;
        }
        else
        {
            dangAn = false;
            rectTransform.anchoredPosition = viTriBanDau;
            viTriMucTieu = viTriBanDau;
        }
    }

    private void Update()
    {
        // Trượt mượt mà về vị trí mục tiêu
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            viTriMucTieu,
            Time.deltaTime * tocDoTruot
        );
    }

    // Xử lý khi bấm vào Bảng UI
    public void OnPointerClick(PointerEventData eventData)
    {
        dangAn = !dangAn;
        viTriMucTieu = dangAn ? viTriAn : viTriBanDau;
    }
}