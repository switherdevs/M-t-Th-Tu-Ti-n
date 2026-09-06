using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursorController : MonoBehaviour
{
    [Header("--- COMPONENTS ---")]
    [SerializeField] private RectTransform cursorTransform; // UI Image của con trỏ chuột (Object 'Mouse')
    [SerializeField] private Canvas cursorCanvas;           // Canvas chứa con trỏ chuột ('Canvas_chuot')

    private void OnEnable()
    {
        // Ẩn con trỏ chuột mặc định của hệ điều hành Windows
        Cursor.visible = false;
    }

    private void Update()
    {
        // Guard Clause: Kiểm tra nếu thiếu phần cứng hoặc UI thì dừng ngay để tránh crash
        if (Mouse.current == null || cursorTransform == null || cursorCanvas == null) return;

        // 1. ĐỌC TRỰC TIẾP TỌA ĐỘ THẬT CỦA CHUỘT TRÊN MÀN HÌNH (Screen Space)
        Vector2 screenPosition = Mouse.current.position.ReadValue();

        // 2. CHUYỂN TỌA ĐỘ MÀN HÌNH SANG TỌA ĐỘ LOCAL CỦA CANVAS
        RectTransform canvasRect = cursorCanvas.transform as RectTransform;
        Camera uiCamera = (cursorCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : cursorCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint))
        {
            // 3. ĐƯA CHUỘT ẢO ĐẾN THẲNG VỊ TRÍ CHUỘT THẬT
            cursorTransform.localPosition = localPoint;
        }
    }

    private void OnDisable()
    {
        // Hiện lại con trỏ chuột mặc định khi tắt UI này
        Cursor.visible = true;
    }
}