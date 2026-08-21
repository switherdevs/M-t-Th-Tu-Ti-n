using System.Collections;
using System.Collections.Generic; // 🎯 Thêm thư viện để dùng List
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("--- CẤU HÌNH GIAO DIỆN / OBJECT (UI) ---")]
    [Tooltip("Kéo thả bao nhiêu UI / GameObjects vào đây tùy ý. Click vào nhà sẽ Bật/Tắt tất cả!")]
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
        TimVaCapNhatUI();

        if (danhSachUIGameObject != null && danhSachUIGameObject.Count > 0)
        {
            // Lấy trạng thái ngược lại của Object đầu tiên để đảo trạng thái (Toggle)
            bool trangThaiMoi = !danhSachUIGameObject[0].activeSelf;

            // Vòng lặp duyệt qua tất cả các Object trong mảng để Ẩn / Hiện đồng loạt
            foreach (GameObject uiItem in danhSachUIGameObject)
            {
                if (uiItem != null)
                {
                    uiItem.SetActive(trangThaiMoi);
                }
            }

            // Nếu trạng thái mới là Bật -> Tải lại dữ liệu Quest
            if (trangThaiMoi && QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
                QuestUIManager.Instance.DongBangThoai();
            }

            Debug.Log($"<color=green>[Building]</color> Đã {(trangThaiMoi ? "MỞ" : "TẮT")} thành công {danhSachUIGameObject.Count} UI!");
        }
        else
        {
            Debug.LogError("<color=red>[Building Error]</color> Danh sách UI đang trống, không tìm thấy GameObject nào!");
        }
    }

    // 🎯 TỰ ĐỘNG TÌM LẠI NẾU DANH SÁCH TRỐNG
    private void TimVaCapNhatUI()
    {
        // Nếu danh sách chưa được kéo tay trong Inspector
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