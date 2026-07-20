using UnityEngine;
using TMPro;

namespace InventorySystem.UI
{
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance { get; private set; }

        [Header("=== UI COMPONENTS ===")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("=== SETTINGS ===")]
        [SerializeField] private Vector2 offset = new Vector2(15f, -15f);

        private Canvas parentCanvas;

        private void Awake()
        {
            Instance = this;
            parentCanvas = GetComponentInParent<Canvas>();
            HideTooltip();
        }

        private void LateUpdate()
        {
            // Nếu Tooltip đang hiển thị thì liên tục di chuyển theo vị trí chuột
            if (canvasGroup.alpha > 0)
            {
                FollowMousePosition();
            }
        }

        public void ShowTooltip(string itemName, string description)
        {
            nameText.text = itemName;
            // Phần description đã được chuẩn bị sẵn cấu trúc ở đây, chỉ cần bật lên khi cần mở rộng sau này
            // descriptionText.text = description; 

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false; // Đảm bảo tooltip không cản tia chuột bấm xuống slot bên dưới
            FollowMousePosition();
        }

        public void HideTooltip()
        {
            canvasGroup.alpha = 0f;
        }

        private void FollowMousePosition()
        {
            Vector2 mousePos = Input.mousePosition;

            // Tính toán chuyển đổi tọa độ chuột sang tọa độ UI Canvas tương thích mọi độ phân giải
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = mousePos + offset;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    mousePos,
                    parentCanvas.worldCamera,
                    out Vector2 localPoint);
                rectTransform.anchoredPosition = localPoint + offset;
            }
        }
    }
}