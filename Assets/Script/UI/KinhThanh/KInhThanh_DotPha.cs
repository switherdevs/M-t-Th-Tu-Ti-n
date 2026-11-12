using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameCore.Quests;

public class BuildingInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("--- CẤU HÌNH GIAO DIỆN / OBJECT (UI) ---")]
    [Tooltip("Kéo thả các UI cần MỞ khi click vào công trình này")]
    [SerializeField] private List<GameObject> danhSachUIGameObject = new List<GameObject>();

    [Header("--- CẤU HÌNH PHÓNG TO (SCALE) ---")]
    [SerializeField] private float targetScaleMultiplier = 1.2f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("--- CẤU HÌNH ĐỔI MÀU (HIGHLIGHT) ---")]
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        originalScale = transform.localScale;
        targetScale = originalScale * targetScaleMultiplier;

        // ❌ ĐÃ XÓA BỎ HÀM TỰ ĐỘNG ÉP ẨN (AnTatCaUIGameObject) TẠI ĐÂY 
        // ĐỂ TRÁNH XUNG ĐỘT VỚI SCRIPT RESUME / ĐÓNG UI.
    }

    private void Start()
    {
        // Chỉ gán danh sách nếu chưa có, hoàn toàn không tự động setactive ép buộc
        KiemTraVaBoSungUI();
    }

    // 🎯 TƯƠNG TÁC CLICK CHUỘT VÀO CÔNG TRÌNH
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Nếu chuột đang click trực tiếp lên chính bảng UI thì không can thiệp
        if (KiemTraChuotDangDeLenChinhBangUI(eventData))
        {
            return;
        }

        KiemTraVaBoSungUI();

        if (danhSachUIGameObject != null && danhSachUIGameObject.Count > 0)
        {
            GameObject firstValidUI = null;
            for (int i = 0; i < danhSachUIGameObject.Count; i++)
            {
                if (danhSachUIGameObject[i] != null)
                {
                    firstValidUI = danhSachUIGameObject[i];
                    break;
                }
            }

            if (firstValidUI == null) return;

            // 2. Kiểm tra trạng thái hiện tại của UI
            bool dangMo = firstValidUI.activeSelf;

            if (!dangMo)
            {
                // Nếu đang TẮT -> BẬT LÊN
                foreach (GameObject uiItem in danhSachUIGameObject)
                {
                    if (uiItem != null) uiItem.SetActive(true);
                }
            }
            else
            {
                // Nếu đang MỞ -> TẮT ĐI
                foreach (GameObject uiItem in danhSachUIGameObject)
                {
                    if (uiItem != null) uiItem.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Bắn tia Raycast UI thủ công để kiểm tra xem chuột có đang click TRÚNG vào chính Bảng UI đang bật không
    /// </summary>
    private bool KiemTraChuotDangDeLenChinhBangUI(PointerEventData eventData)
    {
        if (EventSystem.current == null) return false;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            foreach (GameObject uiTarget in danhSachUIGameObject)
            {
                if (uiTarget != null && uiTarget.activeSelf)
                {
                    if (result.gameObject == uiTarget || result.gameObject.transform.IsChildOf(uiTarget.transform))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Chỉ hỗ trợ tìm lại reference nếu danh sách bị để trống trên Inspector, không ép trạng thái Active.
    /// </summary>
    private void KiemTraVaBoSungUI()
    {
        if (danhSachUIGameObject == null)
        {
            danhSachUIGameObject = new List<GameObject>();
        }

        if (danhSachUIGameObject.Count == 0)
        {
            if (QuestManager.Instance != null)
            {
                danhSachUIGameObject.Add(QuestManager.Instance.gameObject);
            }
            else
            {
                GameObject uiQuest = GameObject.Find("UI_quest");
                if (uiQuest != null)
                {
                    danhSachUIGameObject.Add(uiQuest);
                }
            }
        }
    }

    // 🎯 TƯƠNG TÁC RÊ CHUỘT VÀO / RA
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spriteRenderer != null) spriteRenderer.color = hoverColor;
        StartSmoothScale(targetScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        StartSmoothScale(originalScale);
    }

    private void StartSmoothScale(Vector3 endScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(endScale));
    }

    private IEnumerator ScaleRoutine(Vector3 destinationScale)
    {
        while (Vector3.Distance(transform.localScale, destinationScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, destinationScale, scaleSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        transform.localScale = destinationScale;
    }
}