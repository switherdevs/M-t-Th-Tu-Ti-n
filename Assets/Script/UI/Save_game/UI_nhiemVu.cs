using UnityEngine;
using UnityEngine.UI;
using TMPro; // Sử dụng TextMeshProUGUI
using System.Collections;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("--- THÀNH PHẦN UI CẦN KÉO VÀO ---")]
    public GameObject bangThoaiUI;              // Panel chứa toàn bộ UI giao tiếp Quest
    public TextMeshProUGUI textTenNhiemVu;      // Text (TMP) hiển thị Tên Quest
    public TextMeshProUGUI textLoiThoaiNPC;     // Text (TMP) hiển thị Lời thoại
    public TextMeshProUGUI textTienTrinh;       // Text (TMP) hiển thị Tiến trình

    [Header("--- HÌNH CẢNH NHÂN VẬT (GAMEOBJECT) ---")]
    [Tooltip("GameObject hình ảnh NPC chính trên UI (Kéo GameObject UI NPC vào đây)")]
    public GameObject npcMainQuestObject;

    [Tooltip("GameObject hình ảnh NPC phụ trên UI (Để trống nếu chưa test tới)")]
    public GameObject npcSideQuestObject;

    [Header("--- CẤU HÌNH TRƯỢT NPC (SLIDE SETTINGS) ---")]
    [Tooltip("Vị trí dừng (Anchored Position X, Y) của NPC trên Canvas")]
    public Vector2 npcStopPosition = new Vector2(300f, 0f);

    [Tooltip("Khoảng cách đẩy NPC sang Phải ngoài màn hình để chuẩn bị trượt vào")]
    public float npcSlideOffsetDistance = 500f;

    [Tooltip("Thời gian hiệu ứng trượt hoàn thành (Giây)")]
    public float npcSlideDuration = 0.4f;

    [Header("--- CÁC NÚT BẤM (BUTTONS) ---")]
    public Button nutMoQuestMain;   // Nút bấm Quest chính ngoài màn hình
    public Button nutDongY;
    public Button nutTuChoi;
    public Button nutTraNhiemVu;
    public Button nutDongBang;

    [Header("--- CẤU HÌNH TEST NHIỆM VỤ CHÍNH ---")]
    public QuestData mainQuestData;  // Dữ liệu Quest chính (Có thể để trống khi chưa test)
    public NPC_QuestGiver mainNPC;   // NPC QuestGiver chính trong Scene (Có thể để trống khi chưa test)

    private QuestData questDangXem;
    private NPC_QuestGiver npcHienTai;
    private Coroutine npcSlideCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (bangThoaiUI != null) bangThoaiUI.SetActive(false);
    }

    private void Start()
    {
        if (nutMoQuestMain != null)
        {
            nutMoQuestMain.onClick.AddListener(OnClickMoMainQuest);
        }
    }

    // 🎯 SỰ KIỆN NÚT BẤM TEST QUEST CHÍNH
    public void OnClickMoMainQuest()
    {
        if (mainQuestData != null)
        {
            MoBangThoaiQuest(mainQuestData, mainNPC, isMainQuest: true);
        }
        else
        {
            Debug.LogWarning("<color=yellow>[Test Warning]</color> Bạn chưa kéo mainQuestData vào QuestUIManager! Hãy kéo vào để test.");
        }
    }

    // 🎯 HÀM MỞ BẢNG UI THOẠI (Hỗ trợ phân biệt Quest Chính hay Phụ)
    public void MoBangThoaiQuest(QuestData questData, NPC_QuestGiver npc, bool isMainQuest = true)
    {
        if (questData == null)
        {
            Debug.LogWarning("<color=yellow>[Test Warning]</color> Dữ liệu QuestData bị Null, không thể mở bảng thoại.");
            return;
        }

        questDangXem = questData;
        npcHienTai = npc;

        if (bangThoaiUI != null) bangThoaiUI.SetActive(true);

        // KÍCH HOẠT HÌNH ẢNH GAMEOBJECT NPC TƯƠNG ỨNG
        HandleNPCGameObjectDisplay(isMainQuest);

        // Lấy tiến trình từ hệ thống Save TXT
        ProgressQuest progress = QuestSaveSystem.Instance.LayTiencTrinhQuest(questData.idQuest);

        if (textTenNhiemVu != null) textTenNhiemVu.text = questData.tenNhiemVu;

        // Ẩn an toàn tất cả nút bấm
        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        // Xử lý giao diện theo từng trạng thái
        switch (progress.trangThai)
        {
            case TrangThaiQuest.ChuaNhan:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiNhanQuest;
                if (textTienTrinh != null) textTienTrinh.text = $"Mục tiêu: Tiêu diệt {questData.soLuongBoXuongCanDiet} Bộ Xương.";
                if (nutDongY != null) nutDongY.gameObject.SetActive(true);
                if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DangLam:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = questData.loiThoaiDangLam;
                if (textTienTrinh != null) textTienTrinh.text = $"Tiến trình: {progress.soBoXuongDaDiet}/{questData.soLuongBoXuongCanDiet} Bộ Xương.";
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.DaXongChuaTra:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Tốt lắm! Ngươi đã tiêu diệt đủ số lượng bộ xương. Đây là phần thưởng!";
                if (textTienTrinh != null) textTienTrinh.text = $"Tiến trình: Hoàn thành ({questData.soLuongBoXuongCanDiet}/{questData.soLuongBoXuongCanDiet})";
                if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(true);
                break;

            case TrangThaiQuest.HoanThanh:
                if (textLoiThoaiNPC != null) textLoiThoaiNPC.text = "Cảm ơn đại hiệp đã giúp đỡ dân lành!";
                if (textTienTrinh != null) textTienTrinh.text = "Nhiệm vụ đã hoàn thành.";
                if (nutDongBang != null) nutDongBang.gameObject.SetActive(true);
                break;
        }
    }

    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);
        DongBangThoai();
        Debug.Log("<color=yellow>[QuestUI]</color> Đã nhận nhiệm vụ ID: " + questDangXem.idQuest);
    }

    public void DongBangThoai()
    {
        if (npcSlideCoroutine != null) StopCoroutine(npcSlideCoroutine);
        if (bangThoaiUI != null) bangThoaiUI.SetActive(false);
    }

    public void OnClickTraNhiemVu()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.HoanThanh);

        if (questDangXem.prefabItemPhanThuong != null && npcHienTai != null)
        {
            for (int i = 0; i < questDangXem.soLuongItemThuong; i++)
            {
                Instantiate(questDangXem.prefabItemPhanThuong, npcHienTai.transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
            }
            Debug.Log($"<color=green>[Reward]</color> Đã trao {questDangXem.soLuongItemThuong}x {questDangXem.prefabItemPhanThuong.name}");
        }

        DongBangThoai();
    }

    // ==========================================
    // XỬ LÝ HIỂN THỊ VÀ TRƯỢT GAMEOBJECT NPC
    // ==========================================
    private void HandleNPCGameObjectDisplay(bool isMainQuest)
    {
        // Ẩn cả 2 GameObject NPC trước
        if (npcMainQuestObject != null) npcMainQuestObject.SetActive(false);
        if (npcSideQuestObject != null) npcSideQuestObject.SetActive(false);

        // Xác định GameObject NPC nào sẽ được dùng
        GameObject targetNPCObject = isMainQuest ? npcMainQuestObject : npcSideQuestObject;

        // Bỏ qua nếu GameObject NPC đó chưa được gán trong Inspector (An toàn khi Test)
        if (targetNPCObject == null) return;

        targetNPCObject.SetActive(true);

        // Lấy RectTransform để tính toán tọa độ trượt UI
        RectTransform npcRect = targetNPCObject.GetComponent<RectTransform>();
        if (npcRect != null)
        {
            if (npcSlideCoroutine != null) StopCoroutine(npcSlideCoroutine);
            npcSlideCoroutine = StartCoroutine(Routine_SlideNPCObject(npcRect));
        }
    }

    private IEnumerator Routine_SlideNPCObject(RectTransform npcRect)
    {
        Vector2 startPos = npcStopPosition + new Vector2(npcSlideOffsetDistance, 0f);
        Vector2 targetPos = npcStopPosition;

        npcRect.anchoredPosition = startPos;

        float elapsedTime = 0f;

        while (elapsedTime < npcSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / npcSlideDuration;

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            npcRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothProgress);
            yield return null;
        }

        npcRect.anchoredPosition = targetPos;
    }
}