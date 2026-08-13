using System.Collections;
using UnityEngine;

public class BuildingInteraction : MonoBehaviour
{
    [Header("--- CẤU HÌNH GIAO DIỆN (UI) ---")]
    [Tooltip("Không cần kéo thả! Script sẽ tự tìm UI_quest khi chuyển Scene.")]
    [SerializeField] private GameObject uiGameObject;

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
        // Tự động liên kết với UI trong Scene
        TimVaCapNhatUI();
    }

    // 🎯 TƯƠNG TÁC CLICK CHUỘT VÀO CÔNG TRÌNH
    private void OnMouseDown()
    {
        // Kiểm tra và tìm lại UI phòng trường hợp vừa chuyển Scene sang
        TimVaCapNhatUI();

        if (uiGameObject != null)
        {
            // Đảo trạng thái Bật / Tắt UI
            bool trangThaiMoi = !uiGameObject.activeSelf;
            uiGameObject.SetActive(trangThaiMoi);

            // Nếu Bật UI -> Load dữ liệu Quest mới nhất từ file Save
            if (trangThaiMoi && QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
                QuestUIManager.Instance.DongBangThoai();
            }

            Debug.Log("<color=green>[Building]</color> Mở UI thành công trên Scene mới!");
        }
        else
        {
            Debug.LogError("<color=red>[Building Error]</color> Không tìm thấy UI_quest hoặc QuestUIManager trong Scene KinhThanh!");
        }
    }

    // 🎯 TỰ ĐỘNG TÌM GAMEOBJECT UI DÙ CHUYỂN SCENE NÀO
    private void TimVaCapNhatUI()
    {
        // 1. Ưu tiên lấy GameObject từ QuestUIManager Instance của Scene hiện tại
        if (QuestUIManager.Instance != null)
        {
            uiGameObject = QuestUIManager.Instance.gameObject;
        }
        // 2. Nếu Instance chưa sẵn sàng, tìm trực tiếp tên 'UI_quest' trên Hierarchy
        else if (uiGameObject == null)
        {
            uiGameObject = GameObject.Find("UI_quest");
        }
    }

    private void OnMouseEnter()
    {
        if (spriteRenderer != null) spriteRenderer.color = hoverColor;
        StartSmoothScale(targetScale);
    }

    private void OnMouseExit()
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