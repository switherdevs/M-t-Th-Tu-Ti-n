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

    [Header("--- CẤU HÌNH COLLIDER ---")]
    [Tooltip("Tick chọn nếu bắt buộc phải có Collider hợp lệ mới cho tương tác; bỏ tick để chạy như script gốc")]
    [SerializeField] private bool dungCollider = false;

    [Header("--- CẤU HÌNH BUTTON ---")]
    [Tooltip("Tick chọn nếu muốn kích hoạt bật/tắt UI thông qua các Button; bỏ tick để chạy như script gốc")]
    [SerializeField] private bool dungButton = false;
    [Tooltip("Danh sách Button dùng để mở/đóng UI (Nếu để trống khi tick Dung Button, script sẽ tự tìm Button liên quan)")]
    [SerializeField] private List<Button> danhSachButtonDieuKhien = new List<Button>();

    private SpriteRenderer spriteRenderer;
    private Collider2D collider2DComponent;
    private Collider collider3DComponent;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;
    private readonly List<Button> buttonDaDangKy = new List<Button>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2DComponent = GetComponent<Collider2D>();
        collider3DComponent = GetComponent<Collider>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        originalScale = transform.localScale;
        targetScale = originalScale * targetScaleMultiplier;

        // ❌ ĐÃ XÓA BỎ HÀM TỰ ĐỘNG ÉP ẨN (AnTatCaUIGameObject) TẠI ĐÂY 
        // ĐỂ TRÁNH XUNG ĐỘT VỚI SCRIPT RESUME / ĐÓNG UI.
    }

    private void OnEnable()
    {
        if (dungButton)
        {
            DangKySuKienButton();
            CapNhatTrangThaiButton();
        }
    }

    private void OnDisable()
    {
        HuyDangKySuKienButton();
    }

    private void Start()
    {
        // Chỉ gán danh sách nếu chưa có, hoàn toàn không tự động setactive ép buộc
        KiemTraVaBoSungUI();

        if (dungButton)
        {
            DangKySuKienButton();
            CapNhatTrangThaiButton();
        }
    }

    private void DangKySuKienButton()
    {
        HuyDangKySuKienButton();
        KiemTraVaBoSungUI();

        List<Button> danhSachCanDung = new List<Button>();

        if (danhSachButtonDieuKhien != null && danhSachButtonDieuKhien.Count > 0)
        {
            for (int i = 0; i < danhSachButtonDieuKhien.Count; i++)
            {
                Button btn = danhSachButtonDieuKhien[i];
                if (btn != null && !danhSachCanDung.Contains(btn))
                {
                    danhSachCanDung.Add(btn);
                }
            }
        }
        else
        {
            GetComponentsInChildren<Button>(true, danhSachCanDung);

            if (danhSachUIGameObject != null)
            {
                for (int i = 0; i < danhSachUIGameObject.Count; i++)
                {
                    GameObject uiObj = danhSachUIGameObject[i];
                    if (uiObj == null) continue;

                    Button[] buttonsTrongUI = uiObj.GetComponentsInChildren<Button>(true);
                    for (int j = 0; j < buttonsTrongUI.Length; j++)
                    {
                        if (buttonsTrongUI[j] != null && !danhSachCanDung.Contains(buttonsTrongUI[j]))
                        {
                            danhSachCanDung.Add(buttonsTrongUI[j]);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < danhSachCanDung.Count; i++)
        {
            Button btn = danhSachCanDung[i];
            if (btn != null)
            {
                btn.onClick.AddListener(OnButtonClick);
                buttonDaDangKy.Add(btn);
            }
        }
    }

    private void HuyDangKySuKienButton()
    {
        for (int i = 0; i < buttonDaDangKy.Count; i++)
        {
            if (buttonDaDangKy[i] != null)
            {
                buttonDaDangKy[i].onClick.RemoveListener(OnButtonClick);
            }
        }
        buttonDaDangKy.Clear();
    }

    private void CapNhatTrangThaiButton()
    {
        if (!dungButton) return;

        bool choPhepTuongTac = CoColliderHopLe();
        for (int i = 0; i < buttonDaDangKy.Count; i++)
        {
            if (buttonDaDangKy[i] != null)
            {
                buttonDaDangKy[i].interactable = choPhepTuongTac;
            }
        }
    }

    public void OnButtonClick()
    {
        if (!dungButton) return;
        if (!CoColliderHopLe()) return;

        ChuyenDoiTrangThaiDanhSachUI();
        CapNhatTrangThaiButton();
    }

    private bool CoColliderHopLe()
    {
        if (!dungCollider) return true;

        if (collider2DComponent == null)
        {
            collider2DComponent = GetComponent<Collider2D>();
        }
        if (collider3DComponent == null)
        {
            collider3DComponent = GetComponent<Collider>();
        }

        bool valid2D = collider2DComponent != null && collider2DComponent.enabled && collider2DComponent.gameObject.activeInHierarchy;
        bool valid3D = collider3DComponent != null && collider3DComponent.enabled && collider3DComponent.gameObject.activeInHierarchy;

        return valid2D || valid3D;
    }

    private void ChuyenDoiTrangThaiDanhSachUI()
    {
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

    // 🎯 TƯƠNG TÁC CLICK CHUỘT VÀO CÔNG TRÌNH
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CoColliderHopLe())
        {
            return;
        }

        // 1. Nếu bị bất kỳ UI nào có Raycast chặn lại thì không cho click xuyên qua
        if (BiUIRaycastChanLai(eventData))
        {
            return;
        }

        // 2. Nếu chuột đang click trực tiếp lên chính bảng UI thì không can thiệp
        if (KiemTraChuotDangDeLenChinhBangUI(eventData))
        {
            return;
        }

        ChuyenDoiTrangThaiDanhSachUI();
        CapNhatTrangThaiButton();
    }

    /// <summary>
    /// Kiểm tra xem có bất kỳ UI nào đang chặn Raycast tại vị trí con trỏ hay không
    /// </summary>
    private bool BiUIRaycastChanLai(PointerEventData eventData)
    {
        if (EventSystem.current == null || eventData == null)
            return false;

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Chỉ kiểm tra Raycast đến từ UI
            if (result.module is GraphicRaycaster)
            {
                return true;
            }
        }

        return false;
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
        if (!CoColliderHopLe()) return;
        if (BiUIRaycastChanLai(eventData)) return;

        if (spriteRenderer != null) spriteRenderer.color = hoverColor;
        StartSmoothScale(targetScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CoColliderHopLe()) return;

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