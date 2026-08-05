using UnityEngine;
using UnityEngine.UI;
using TMPro; // Sử dụng TextMeshProUGUI theo chuẩn
using System.Collections;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("--- THÀNH PHẦN UI CẦN KÉO VÀO ---")]
    public GameObject bangThoaiUI;              // Panel chứa toàn bộ UI giao tiếp Quest
    public TextMeshProUGUI textTenNhiemVu;      // Text (TMP) hiển thị Tên Quest
    public TextMeshProUGUI textLoiThoaiNPC;     // Text (TMP) hiển thị Lời thoại
    public TextMeshProUGUI textTienTrinh;       // Text (TMP) hiển thị Tiến trình

    [Header("--- HIỂN THỊ & HIỆU ỨNG NPC NHIỆM VỤ CHÍNH ---")]
    [Tooltip("GameObject / RectTransform hình ảnh NPC chính trên UI")]
    public RectTransform npcMainQuestImage;

    [Tooltip("Vị trí dừng (Anchored Position X, Y) của NPC trên Canvas sau khi trượt vào xong")]
    public Vector2 npcStopPosition = new Vector2(300f, 0f);

    [Tooltip("Khoảng cách đẩy NPC sang Phải ngoài màn hình để chuẩn bị trượt vào")]
    public float npcSlideOffsetDistance = 500f;

    [Tooltip("Thời gian hiệu ứng trượt hoàn thành (Giây) - Càng nhỏ trượt càng nhanh")]
    public float npcSlideDuration = 0.4f;

    [Header("--- CÁC NÚT BẤM (BUTTONS) ---")]
    public Button nutMoQuestMain;   // Nút bấm Quest chính ngoài màn hình để mở thoại
    public Button nutDongY;
    public Button nutTuChoi;
    public Button nutTraNhiemVu;
    public Button nutDongBang;

    [Header("--- CẤU HÌNH NHIỆM VỤ CHÍNH (MAIN QUEST DATA) ---")]
    public QuestData mainQuestData;  // Kéo Asset QuestData của Nhiệm vụ chính vào đây
    public NPC_QuestGiver mainNPC;   // Kéo NPC QuestGiver chính vào đây (hoặc để script tự gán)

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
        // Gán sự kiện OnClick cho Nút mở Quest chính nếu có kéo vào Inspector
        if (nutMoQuestMain != null)
        {
            nutMoQuestMain.onClick.AddListener(OnClickMoMainQuest);
        }
    }

    // 🎯 SỰ KIỆN GÁN VÀO BUTTON QUEST CHÍNH (MAIN QUEST BUTTON)
    public void OnClickMoMainQuest()
    {
        if (mainQuestData != null)
        {
            MoBangThoaiQuest(mainQuestData, mainNPC);
        }
        else
        {
            Debug.LogWarning("<color=red>[QuestUI]</color> Chưa kéo asset mainQuestData vào QuestUIManager!");
        }
    }

    // 🎯 HÀM MỞ BẢNG UI THOẠI DỰA THEO TRẠNG THÁI QUEST
    public void MoBangThoaiQuest(QuestData questData, NPC_QuestGiver npc)
    {
        questDangXem = questData;
        npcHienTai = npc;

        if (bangThoaiUI != null) bangThoaiUI.SetActive(true);

        // KÍCH HOẠT HIỆU ỨNG NPC TRƯỢT TỪ PHẢI SANG TRÁI
        TriggerNPCSlideIn();

        // Lấy tiến trình từ hệ thống Save TXT
        ProgressQuest progress = QuestSaveSystem.Instance.LayTiencTrinhQuest(questData.idQuest);

        if (textTenNhiemVu != null) textTenNhiemVu.text = questData.tenNhiemVu;

        // Ẩn tất cả nút trước khi kiểm tra trạng thái
        if (nutDongY != null) nutDongY.gameObject.SetActive(false);
        if (nutTuChoi != null) nutTuChoi.gameObject.SetActive(false);
        if (nutTraNhiemVu != null) nutTraNhiemVu.gameObject.SetActive(false);
        if (nutDongBang != null) nutDongBang.gameObject.SetActive(false);

        // Xử lý giao diện theo từng nấc trạng thái
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

    // Sự kiện gán vào Nút [Đồng Ý]
    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null) return;

        QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangXem.idQuest, TrangThaiQuest.DangLam);
        DongBangThoai();
        Debug.Log("<color=yellow>[QuestUI]</color> Đã nhận nhiệm vụ ID: " + questDangXem.idQuest);
    }

    // Sự kiện gán vào Nút [Từ Chối] / [Đóng]
    public void DongBangThoai()
    {
        if (npcSlideCoroutine != null) StopCoroutine(npcSlideCoroutine);
        if (bangThoaiUI != null) bangThoaiUI.SetActive(false);
    }

    // Sự kiện gán vào Nút [Trả Nhiệm Vụ]
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
    // HIỆU ỨNG TRƯỢT NPC TỪ PHẢI SANG TRÁI
    // ==========================================
    private void TriggerNPCSlideIn()
    {
        if (npcMainQuestImage == null) return;

        npcMainQuestImage.gameObject.SetActive(true);

        if (npcSlideCoroutine != null)
        {
            StopCoroutine(npcSlideCoroutine);
        }

        npcSlideCoroutine = StartCoroutine(Routine_SlideNPCFromRight());
    }

    private IEnumerator Routine_SlideNPCFromRight()
    {
        // 1. Vị trí bắt đầu: Đẩy lệch sang bên PHẢI một khoảng offset
        Vector2 startPos = npcStopPosition + new Vector2(npcSlideOffsetDistance, 0f);
        Vector2 targetPos = npcStopPosition;

        npcMainQuestImage.anchoredPosition = startPos;

        float elapsedTime = 0f;

        // 2. Di chuyển từ từ về vị trí dừng với hiệu ứng mượt mà (SmoothStep)
        while (elapsedTime < npcSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / npcSlideDuration;

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            npcMainQuestImage.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothProgress);
            yield return null;
        }

        npcMainQuestImage.anchoredPosition = targetPos;
    }
}