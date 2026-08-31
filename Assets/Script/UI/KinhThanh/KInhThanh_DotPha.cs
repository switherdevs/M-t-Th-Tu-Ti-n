using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    }

    private void Start()
    {
        TimVaCapNhatUI();
    }

    // 🎯 TƯƠNG TÁC CLICK CHUỘT VÀO CÔNG TRÌNH
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Kiểm tra nếu người chơi đang click TRỰC TIẾP vào chính BẢNG UI CÔNG TRÌNH đang mở thì không bật/tắt lại
        if (KiemTraChuotDangDeLenChinhBangUI(eventData))
        {
            return;
        }

        TimVaCapNhatUI();

        if (danhSachUIGameObject != null && danhSachUIGameObject.Count > 0)
        {
            // 2. Kiểm tra xem Bảng UI hiện tại đang BẬT hay TẮT
            bool dangMo = danhSachUIGameObject[0].activeSelf;

            if (!dangMo)
            {
                // Nếu đang TẮT -> BẬT UI LÊN
                foreach (GameObject uiItem in danhSachUIGameObject)
                {
                    if (uiItem != null) uiItem.SetActive(true);
                }

                if (QuestUIManager.Instance != null)
                {
                    QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
                    QuestUIManager.Instance.DongBangThoai();
                }

                Debug.Log($"<color=green>[Building]</color> Đã MỞ {danhSachUIGameObject.Count} UI!");
            }
            else
            {
                // Nếu đang MỞ mà click TRỰC TIẾP VÀO CÔNG TRÌNH (World Object) -> TẮT UI
                foreach (GameObject uiItem in danhSachUIGameObject)
                {
                    if (uiItem != null) uiItem.SetActive(false);
                }

                Debug.Log($"<color=yellow>[Building]</color> Đã TẮT {danhSachUIGameObject.Count} UI!");
            }
        }
        else
        {
            Debug.LogError("<color=red>[Building Error]</color> Danh sách UI đang trống!");
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
            // Duyệt từng UI trúng Raycast, nếu trúng 1 trong các UI trong danh bạ Bảng UI công trình -> Trả về true
            foreach (GameObject uiTarget in danhSachUIGameObject)
            {
                if (uiTarget != null && uiTarget.activeSelf)
                {
                    if (result.gameObject == uiTarget || result.gameObject.transform.IsChildOf(uiTarget.transform))
                    {
                        return true; // Chuột đang click vào bên trong Bảng UI
                    }
                }
            }
        }

        return false; // Chuột click vào Công Trình World 2D
    }

    // 🎯 TỰ ĐỘNG TÌM LẠI NẾU DANH SÁCH TRỐNG
    private void TimVaCapNhatUI()
    {
        if (danhSachUIGameObject.Count == 0)
        {
            if (QuestUIManager.Instance != null)
            {
                danhSachUIGameObject.Add(QuestUIManager.Instance.gameObject);
            }
            else
            {
                GameObject uiQuest = GameObject.Find("UI_quest");
                if (uiQuest != null) danhSachUIGameObject.Add(uiQuest);
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
            transform.localScale = Vector3.Lerp(transform.localScale, destinationScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = destinationScale;
    }
}