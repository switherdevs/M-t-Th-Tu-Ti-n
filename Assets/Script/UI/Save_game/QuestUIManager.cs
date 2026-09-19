using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCore.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

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

        private Coroutine warningFadeCoroutine;

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

            // Mặc định ẩn hoàn toàn Text cảnh báo khi vào Game bằng cách đặt Alpha = 0
            if (warningText != null)
            {
                SetTextAlpha(0f);
            }
        }

        // =========================================================
        // LOGIC NHẬN VÀ QUẢN LÝ NHIỆM VỤ
        // =========================================================

        /// <summary>
        /// Hàm gọi khi người chơi bấm nút Nhận Nhiệm Vụ
        /// </summary>
        /// <param name="newQuest">Thông tin Quest muốn nhận</param>
        /// <returns>Trả về true nếu nhận thành công, false nếu bị từ chối</returns>
        public bool AcceptQuest(QuestData newQuest)
        {
            if (newQuest == null) return false;

            // 1. Kiểm tra nếu Quest đã có trong danh sách đang làm
            if (activeQuests.Exists(q => q != null && q.idQuest == newQuest.idQuest))
            {
                TriggerWarning("Nhiệm vụ này đã được nhận từ trước!");
                return false;
            }

            // 2. Kiểm tra nếu đã đạt giới hạn tối đa (3 nhiệm vụ)
            if (activeQuests.Count >= maxActiveQuests)
            {
                // Phát hiệu ứng chữ cảnh báo Fade In/Out
                TriggerWarning(maxQuestWarningTextVI);
                return false; // Từ chối không cho nhận thêm
            }

            // 3. Đủ điều kiện -> Thêm nhiệm vụ vào danh sách
            activeQuests.Add(newQuest);
            Debug.Log($"[QuestManager] Đã nhận nhiệm vụ ID: {newQuest.idQuest} ({activeQuests.Count}/{maxActiveQuests})");
            return true;
        }

        /// <summary>
        /// Hàm gọi khi hoàn thành hoặc hủy bỏ nhiệm vụ để giải phóng ô chứa
        /// </summary>
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

            // Nếu đang có Coroutine Fade cũ đang chạy thì ngắt để chạy cái mới
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
            }

            warningFadeCoroutine = StartCoroutine(FadeSequence(message));
        }

        private IEnumerator FadeSequence(string message)
        {
            // Gán nội dung thông báo
            warningText.text = message;

            // 1. FADE IN (Hiện dần từ Alpha 0 -> 1)
            float speed = 1f / Mathf.Max(0.01f, fadeInDuration);
            float currentAlpha = warningText.color.a;

            while (currentAlpha < 1f)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, speed * Time.deltaTime);
                SetTextAlpha(currentAlpha);
                yield return null; // Chờ sang frame tiếp theo
            }
            SetTextAlpha(1f);

            // 2. GIỮ HIỂN THỊ (Display Duration)
            yield return new WaitForSeconds(displayDuration);

            // 3. FADE OUT (Mờ dần từ Alpha 1 -> 0)
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

        // Hàm hỗ trợ gán giá trị Alpha cho TextMeshProUGUI
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