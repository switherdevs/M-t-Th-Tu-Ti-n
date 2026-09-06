using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using GameCore.Settings;

public class SmartCursorController : MonoBehaviour
{
    [Header("--- ASSETS ---")]
    [SerializeField] private Sprite cursorSprite; // Kéo file ảnh chuột (.png) vào đây
    [SerializeField] private Vector2 cursorSize = new Vector2(32, 32); // Kích thước con trỏ chuột (Pixel)

    private RectTransform _cursorRect;
    private Canvas _cursorCanvas;
    private Vector2 _virtualPosition;

    private void Awake()
    {
        // TỰ ĐỘNG DỰNG CANVAS VÀ UI IMAGE TRONG CODE (KHÔNG CẦN SETUP HAND)
        GameObject canvasObj = new GameObject("[Auto_Cursor_Canvas]");
        _cursorCanvas = canvasObj.AddComponent<Canvas>();
        _cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _cursorCanvas.sortingOrder = 999; // Luôn hiển thị trên cùng mọi UI khác

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject cursorObj = new GameObject("VirtualCursorImage");
        cursorObj.transform.SetParent(canvasObj.transform, false);

        Image img = cursorObj.AddComponent<Image>();
        img.sprite = cursorSprite;
        img.raycastTarget = false; // Tránh cản trở việc click nút Button phía dưới

        _cursorRect = cursorObj.GetComponent<RectTransform>();
        _cursorRect.sizeDelta = cursorSize;
        _cursorRect.anchorMin = new Vector2(0, 1); // Đặt gốc Anchor góc Trên - Trái
        _cursorRect.anchorMax = new Vector2(0, 1);
        _cursorRect.pivot = new Vector2(0, 1);     // Pivot nằm ở chính đầu mũi tên
    }

    private void Start()
    {
        // Khóa không cho Unity tự giấu chuột khi click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (Mouse.current != null)
        {
            _virtualPosition = Mouse.current.position.ReadValue();
        }
    }

    private void Update()
    {
        // 1. Luôn giữ trạng thái chuột không bị Unity tự ẩn khi click vào Game
        if (Cursor.visible) Cursor.visible = false;
        if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;

        if (Mouse.current == null || _cursorRect == null) return;

        // 2. Đọc độ nhích chuột trong khung hình
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 3. Lấy độ nhạy từ SettingsManager
        float sensitivity = 1.0f;
        if (SettingsManager.Instance != null)
        {
            sensitivity = SettingsManager.Instance.CurrentData.mouseSensitivity;
        }

        // 4. Cộng dồn tọa độ chuột ảo theo độ nhạy
        _virtualPosition += mouseDelta * sensitivity;
        _virtualPosition.x = Mathf.Clamp(_virtualPosition.x, 0, Screen.width);
        _virtualPosition.y = Mathf.Clamp(_virtualPosition.y, 0, Screen.height);

        // 5. Cập nhật vị trí chuột ảo chính xác lên màn hình
        RectTransform UtilityRect = _cursorCanvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(UtilityRect, _virtualPosition, null, out Vector2 localPoint))
        {
            _cursorRect.anchoredPosition = localPoint;
        }
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }
}