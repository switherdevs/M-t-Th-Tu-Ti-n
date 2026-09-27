using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Quests
{
    // =========================================================
    // KHAI BÁO CLASS HỖ TRỢ HIỂN THỊ ITEM TRONG DANH SÁCH QUEST
    // =========================================================
    [System.Serializable]
    public class QuestListItemUI
    {
        [Tooltip("Kéo Nút bấm đại diện cho Quest này vào đây")]
        public Button btnQuestItem;

        [Tooltip("Kéo TextMeshProUGUI hiển thị tên Quest trên Nút vào đây")]
        public TextMeshProUGUI txtTenQuestItem;

        [Tooltip("Dữ liệu ScriptableObject QuestData tương ứng")]
        public QuestData questData;

        /// <summary>
        /// Cập nhật tên hiển thị lên Nút Bấm
        /// </summary>
        public void CapNhatTenNut()
        {
            if (txtTenQuestItem != null && questData != null)
            {
                txtTenQuestItem.text = questData.tenNhiemVu;
            }
        }
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        // =========================================================
        // CẤU HÌNH ÂM THANH (AUDIO)
        // =========================================================
        [Header("=== CẤU HÌNH ÂM THANH BUTTON ===")]
        [Tooltip("AudioSource dùng để phát hiệu ứng âm thanh (Nếu để trống script sẽ tự lấy trên GameObject này)")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("File âm thanh tiếng Click Button (mp3, wav, ogg)")]
        [SerializeField] private AudioClip soundClick;

        // =========================================================
        // CẤU HÌNH GIỚI HẠN NHIỆM VỤ
        // =========================================================
        [Header("=== CẤU HÌNH GIỚI HẠN ===")]
        [Tooltip("Số lượng nhiệm vụ tối đa có thể nhận cùng lúc")]
        [SerializeField] private int maxActiveQuests = 3;

        // =========================================================
        // CẤU HÌNH UI CẢNH BÁO & FADE IN / FADE OUT
        // =========================================================
        [Header("=== UI TEXT CẢNH BÁO & HIỆU ỨNG ===")]
        [Tooltip("Kéo trực tiếp TextMeshProUGUI hiển thị cảnh báo vào đây")]
        [SerializeField] private TextMeshProUGUI warningText;

        [Tooltip("Thời gian hiện rõ hoàn toàn (Giây)")]
        [SerializeField] private float fadeInDuration = 0.3f;

        [Tooltip("Thời gian giữ chữ trên màn hình trước khi ẩn (Giây)")]
        [SerializeField] private float displayDuration = 1.5f;

        [Tooltip("Thời gian mờ dần rồi ẩn hoàn toàn (Giây)")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("=== NỘI DUNG CẢNH BÁO ===")]
        [SerializeField] private string maxQuestWarningTextVI = "Bạn chỉ có thể nhận tối đa 3 nhiệm vụ cùng lúc!";

        // =========================================================
        // DANH SÁCH NHIỆM VỤ ĐANG LÀM
        // =========================================================
        [Header("=== DANH SÁCH NHIỆM VỤ ĐANG LÀM ===")]
        [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();

        // =========================================================
        // BỔ SUNG: BẢNG CHI TIẾT QUEST UI & NÚT BẤM
        // =========================================================
        [Header("=== BỔ SUNG: MẢNG DANH SÁCH NHIỆM VỤ ===")]
        [Tooltip("Danh sách các Nút Bấm Nhiệm Vụ trong UI")]
        [SerializeField] private List<QuestListItemUI> danhSachNhiemVu = new List<QuestListItemUI>();

        [Header("=== BỔ SUNG: NÚT CHỨC NĂNG ===")]
        [SerializeField] private Button btnNhanQuest;
        [SerializeField] private Button btnHuyQuest;
        [SerializeField] private Button btnHoanThanhQuest;

        [Header("=== BỔ SUNG: TEXT CHÍNH ===")]
        [SerializeField] private TextMeshProUGUI txtTenQuest;
        [SerializeField] private TextMeshProUGUI txtThoaiQuest;

        [Header("=== BỔ SUNG: PHẦN THƯỜNG (3 IMAGE & 3 TEXT SỐ LƯỢNG) ===")]
        [Tooltip("Kéo đúng 3 Image hiển thị Icon phần thưởng vào đây")]
        [SerializeField] private Image[] imgItems = new Image[3];

        [Tooltip("Kéo đúng 3 TextMeshProUGUI hiển thị số lượng phần thưởng vào đây")]
        [SerializeField] private TextMeshProUGUI[] txtSoLuongItems = new TextMeshProUGUI[3];

        private Coroutine warningFadeCoroutine;
        private QuestData questDangChon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Tự động tìm AudioSource nếu người chơi chưa gán vào Inspector
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (warningText != null)
            {
                SetTextAlpha(0f);
            }
        }

        private void Start()
        {
            DongBoActiveQuestsTuSaveSystem();
            KhoiTaoGiaoDienDanhSachQuest();
            AnToanBoThongTinChiTiet();
            DangKySuKienNutChucNang();
        }

        /// <summary>
        /// 🎯 HÀM PHÁT ÂM THANH CLICK NÚT (PHÁT 1 LẦN)
        /// </summary>
        public void PhatAmThanhClick()
        {
            if (audioSource != null && soundClick != null)
            {
                audioSource.PlayOneShot(soundClick);
            }
        }

        /// <summary>
        /// 🎯 HÀM ĐỒNG BỘ: Làm sạch và đồng bộ lại danh sách activeQuests từ File Save
        /// </summary>
        public void DongBoActiveQuestsTuSaveSystem()
        {
            activeQuests.Clear();

            if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null) return;

            foreach (ProgressQuest progress in QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress)
            {
                if (progress.trangThai == TrangThaiQuest.DangLam || progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(progress.idQuest);
                    if (data != null && !activeQuests.Contains(data))
                    {
                        activeQuests.Add(data);
                    }
                }
            }
        }

        private void AnToanBoThongTinChiTiet()
        {
            questDangChon = null;

            if (btnNhanQuest != null) btnNhanQuest.gameObject.SetActive(false);
            if (btnHuyQuest != null) btnHuyQuest.gameObject.SetActive(false);
            if (btnHoanThanhQuest != null) btnHoanThanhQuest.gameObject.SetActive(false);

            if (txtTenQuest != null)
            {
                txtTenQuest.text = "";
                txtTenQuest.gameObject.SetActive(false);
            }
            if (txtThoaiQuest != null)
            {
                txtThoaiQuest.text = "";
                txtThoaiQuest.gameObject.SetActive(false);
            }

            for (int i = 0; i < 3; i++)
            {
                if (i < imgItems.Length && imgItems[i] != null)
                    imgItems[i].gameObject.SetActive(false);

                if (i < txtSoLuongItems.Length && txtSoLuongItems[i] != null)
                {
                    txtSoLuongItems[i].text = "";
                    txtSoLuongItems[i].gameObject.SetActive(false);
                }
            }
        }

        private void KhoiTaoGiaoDienDanhSachQuest()
        {
            if (danhSachNhiemVu == null) return;

            for (int i = 0; i < danhSachNhiemVu.Count; i++)
            {
                int index = i;
                QuestListItemUI itemUI = danhSachNhiemVu[i];

                if (itemUI != null)
                {
                    itemUI.CapNhatTenNut();

                    if (itemUI.btnQuestItem != null)
                    {
                        itemUI.btnQuestItem.onClick.RemoveAllListeners();
                        itemUI.btnQuestItem.onClick.AddListener(() => ChonQuestFromList(index));
                    }
                }
            }
        }

        public void ChonQuestFromList(int index)
        {
            PhatAmThanhClick(); // Phát âm thanh khi click nút chọn Quest

            if (index < 0 || index >= danhSachNhiemVu.Count) return;

            QuestListItemUI itemUI = danhSachNhiemVu[index];
            if (itemUI == null || itemUI.questData == null) return;

            questDangChon = itemUI.questData;
            HienThiChiTietQuest(questDangChon);
        }

        private void HienThiChiTietQuest(QuestData questData)
        {
            if (questData == null) return;

            if (txtTenQuest != null)
            {
                txtTenQuest.gameObject.SetActive(true);
                txtTenQuest.text = questData.tenNhiemVu;
            }

            ProgressQuest progress = null;
            if (QuestSaveSystem.Instance != null)
            {
                progress = QuestSaveSystem.Instance.LayTienTrinhQuest(questData.idQuest);
            }

            TrangThaiQuest trangThai = progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan;

            CapNhatNutVaThoaiTheoTrangThai(questData, trangThai);
            HienThiPhanThuong(questData);
        }

        private void CapNhatNutVaThoaiTheoTrangThai(QuestData questData, TrangThaiQuest trangThai)
        {
            if (btnNhanQuest != null) btnNhanQuest.gameObject.SetActive(false);
            if (btnHuyQuest != null) btnHuyQuest.gameObject.SetActive(false);
            if (btnHoanThanhQuest != null) btnHoanThanhQuest.gameObject.SetActive(false);

            if (txtThoaiQuest != null) txtThoaiQuest.gameObject.SetActive(true);

            switch (trangThai)
            {
                case TrangThaiQuest.ChuaNhan:
                    if (txtThoaiQuest != null) txtThoaiQuest.text = questData.loiThoaiNhanQuest;
                    if (btnNhanQuest != null) btnNhanQuest.gameObject.SetActive(true);
                    break;

                case TrangThaiQuest.DangLam:
                    if (txtThoaiQuest != null) txtThoaiQuest.text = questData.loiThoaiDangLam;
                    if (btnHuyQuest != null) btnHuyQuest.gameObject.SetActive(true);
                    break;

                case TrangThaiQuest.DaXongChuaTra:
                    if (txtThoaiQuest != null) txtThoaiQuest.text = questData.loiThoaiHoanThanh;
                    if (btnHoanThanhQuest != null) btnHoanThanhQuest.gameObject.SetActive(true);
                    break;

                case TrangThaiQuest.HoanThanh:
                    if (txtThoaiQuest != null) txtThoaiQuest.text = "Bạn đã hoàn thành nhiệm vụ này rồi!";
                    break;
            }
        }

        private void HienThiPhanThuong(QuestData questData)
        {
            for (int i = 0; i < 3; i++)
            {
                if (questData.danhSachPhanThuong != null && i < questData.danhSachPhanThuong.Count)
                {
                    ItemRewardData reward = questData.danhSachPhanThuong[i];

                    if (reward != null && (reward.iconItem != null || reward.itemData != null))
                    {
                        if (i < imgItems.Length && imgItems[i] != null)
                        {
                            imgItems[i].gameObject.SetActive(true);
                            imgItems[i].sprite = reward.iconItem;
                        }

                        if (i < txtSoLuongItems.Length && txtSoLuongItems[i] != null)
                        {
                            if (reward.soLuong > 0)
                            {
                                txtSoLuongItems[i].gameObject.SetActive(true);
                                txtSoLuongItems[i].text = "x" + reward.soLuong;
                            }
                            else
                            {
                                txtSoLuongItems[i].text = "";
                                txtSoLuongItems[i].gameObject.SetActive(false);
                            }
                        }
                        continue;
                    }
                }

                if (i < imgItems.Length && imgItems[i] != null)
                    imgItems[i].gameObject.SetActive(false);

                if (i < txtSoLuongItems.Length && txtSoLuongItems[i] != null)
                {
                    txtSoLuongItems[i].text = "";
                    txtSoLuongItems[i].gameObject.SetActive(false);
                }
            }
        }

        private void DangKySuKienNutChucNang()
        {
            if (btnNhanQuest != null)
            {
                btnNhanQuest.onClick.RemoveAllListeners();
                btnNhanQuest.onClick.AddListener(OnNutNhanQuestClick);
            }

            if (btnHuyQuest != null)
            {
                btnHuyQuest.onClick.RemoveAllListeners();
                btnHuyQuest.onClick.AddListener(OnNutHuyQuestClick);
            }

            if (btnHoanThanhQuest != null)
            {
                btnHoanThanhQuest.onClick.RemoveAllListeners();
                btnHoanThanhQuest.onClick.AddListener(OnNutHoanThanhQuestClick);
            }
        }

        private void OnNutNhanQuestClick()
        {
            PhatAmThanhClick(); // Phát âm thanh khi bấm Nhận

            if (questDangChon == null) return;

            if (AcceptQuest(questDangChon))
            {
                if (QuestSaveSystem.Instance != null)
                {
                    QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangChon.idQuest, TrangThaiQuest.DangLam);
                }
                QuestHUDTracker.ThongBaoCapNhatHUD();
                HienThiChiTietQuest(questDangChon);
            }
        }

        private void OnNutHuyQuestClick()
        {
            PhatAmThanhClick(); // Phát âm thanh khi bấm Hủy

            if (questDangChon == null) return;

            CompleteOrAbandonQuest(questDangChon.idQuest);

            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangChon.idQuest, TrangThaiQuest.ChuaNhan);
            }
            QuestHUDTracker.ThongBaoCapNhatHUD();
            HienThiChiTietQuest(questDangChon);
        }

        private void OnNutHoanThanhQuestClick()
        {
            PhatAmThanhClick(); // Phát âm thanh khi bấm Hoàn thành

            if (questDangChon == null) return;

            questDangChon.LuuPhanThuongVaoSaveGame();
            CompleteOrAbandonQuest(questDangChon.idQuest);

            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questDangChon.idQuest, TrangThaiQuest.HoanThanh);
            }

            QuestHUDTracker.ThongBaoCapNhatHUD();
            HienThiChiTietQuest(questDangChon);
        }

        // =========================================================
        // LOGIC NHẬN VÀ QUẢN LÝ NHIỆM VỤ
        // =========================================================

        public bool AcceptQuest(QuestData newQuest)
        {
            if (newQuest == null) return false;

            // Đảm bảo đồng bộ danh sách trước khi kiểm tra
            DongBoActiveQuestsTuSaveSystem();

            // 1. Kiểm tra nếu Quest đã có trong danh sách đang làm
            if (activeQuests.Exists(q => q != null && q.idQuest == newQuest.idQuest))
            {
                TriggerWarning("Nhiệm vụ này đã được nhận từ trước!");
                return false;
            }

            // 2. ĐỒNG BỘ: Kiểm tra trực tiếp trên danh sách activeQuests thực tế của Manager
            if (activeQuests.Count >= maxActiveQuests)
            {
                TriggerWarning(maxQuestWarningTextVI);
                return false;
            }

            // 3. Đủ điều kiện -> Thêm nhiệm vụ vào danh sách
            activeQuests.Add(newQuest);
            Debug.Log($"[QuestManager] Đã nhận nhiệm vụ ID: {newQuest.idQuest} ({activeQuests.Count}/{maxActiveQuests})");
            return true;
        }

        public void CompleteOrAbandonQuest(int idQuest)
        {
            QuestData quest = activeQuests.Find(q => q != null && q.idQuest == idQuest);
            if (quest != null)
            {
                activeQuests.Remove(quest);
                Debug.Log($"[QuestManager] Đã xóa nhiệm vụ ID: {quest.idQuest}. Số lượng còn lại: {activeQuests.Count}/{maxActiveQuests}");
            }
        }

        // =========================================================
        // THUẬT TOÁN XỬ LÝ FADE IN / FADE OUT TRÊN TEXT
        // =========================================================

        private void TriggerWarning(string message)
        {
            if (warningText == null)
            {
                Debug.LogWarning("[QuestManager] Chưa gán TextMeshProUGUI warningText vào Inspector!");
                return;
            }

            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
            }

            warningFadeCoroutine = StartCoroutine(FadeSequence(message));
        }

        private IEnumerator FadeSequence(string message)
        {
            warningText.text = message;

            // 1. FADE IN
            float speed = 1f / Mathf.Max(0.01f, fadeInDuration);
            float currentAlpha = warningText.color.a;

            while (currentAlpha < 1f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, speed * Time.deltaTime);
                SetTextAlpha(currentAlpha);
                yield return null;
            }
            SetTextAlpha(1f);

            // 2. GIỮ HIỂN THỊ
            yield return new WaitForSeconds(displayDuration);

            // 3. FADE OUT
            speed = 1f / Mathf.Max(0.01f, fadeOutDuration);
            while (currentAlpha > 0f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, speed * Time.deltaTime);
                SetTextAlpha(currentAlpha);
                yield return null;
            }
            SetTextAlpha(0f);

            warningFadeCoroutine = null;
        }

        private void SetTextAlpha(float alpha)
        {
            if (warningText != null)
            {
                Color color = warningText.color;
                color.a = alpha;
                warningText.color = color;
            }
        }
    }
}