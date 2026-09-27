using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Player
{
    public class PlayerUrinationMechanism : MonoBehaviour
    {
        // =========================================================
        // SINGLETON & SỰ KIỆN KẾT NỐI VỚI SCRIPT QUÁI (EnemyFearState)
        // =========================================================
        public static PlayerUrinationMechanism Instance { get; private set; }

        public event Action OnExecutionStart;
        public event Action OnExecutionEnd;

        // =========================================================
        // CẤU HÌNH THÔNG SỐ MẮC TIỂU
        // =========================================================
        [Header("=== CẤU HÌNH CƠ CHẾ MẮC TIỂU ===")]
        [Tooltip("Mức độ mắc tiểu hiện tại (Float)")]
        [SerializeField] private float mucDoMacTieuHienTai = 0f;

        [Tooltip("Mức độ mắc tiểu tối đa")]
        [SerializeField] private float mucDoMacTieuToiDa = 100f;

        [Tooltip("Tốc độ tăng mức mắc tiểu theo thời gian (Số điểm / Giây)")]
        [SerializeField] private float tocDoTangMacTieu = 5f;

        [Header("=== CẤU HÌNH THỜI GIAN TIỂU THEO TỶ LỆ ===")]
        [Tooltip("Thời gian tiểu ngắn nhất (khi mức tiểu > 0 nhưng rất ít) - tính bằng Giây")]
        [SerializeField] private float thoiGianTieuToiThieu = 1f;

        [Tooltip("Thời gian tiểu lâu nhất (khi mức tiểu đầy 100%) - tính bằng Giây")]
        [SerializeField] private float thoiGianTieuToiDa = 5f;

        [Tooltip("Phím bấm để thực hiện hành động tiểu (Mặc định phím U)")]
        [SerializeField] private KeyCode phimTieu = KeyCode.U;

        // =========================================================
        // CẤU HÌNH GAMEOBJECT NHÂN VẬT & THÀNH PHẦN KHÁC
        // =========================================================
        [Header("=== CẤU HÌNH GAMEOBJECT NHÂN VẬT ===")]
        [Tooltip("Game Object Nhân vật gốc (Sẽ bị ẨN khi đang tiểu)")]
        [SerializeField] private GameObject gameObjectNhanVatGoc;

        [Tooltip("Game Object Nhân vật đang tiểu (Sẽ HIỂN THỊ khi đang tiểu)")]
        [SerializeField] private GameObject gameObjectNhanVatDangTieu;

        [Header("=== CẤU HÌNH UI ===")]
        [Tooltip("Slider hiển thị mức độ mắc tiểu")]
        [SerializeField] private Slider sliderMacTieu;

        // Bổ sung tham chiếu nội bộ
        private PlayerController playerController;
        private CharacterStats characterStats;
        private bool đangTieu = false;

        public bool DangTieu => đangTieu;

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

            playerController = GetComponent<PlayerController>();
            characterStats = GetComponent<CharacterStats>();
        }

        private void Start()
        {
            KhoiTaoGiaoDien();
        }

        private void Update()
        {
            if (đangTieu) return;

            TangMucMacTieuTheoThoiGian();
            KiemTraDieuKienTieu();
        }

        /// <summary>
        /// Khởi tạo trạng thái ban đầu của Game Object và Slider UI
        /// </summary>
        private void KhoiTaoGiaoDien()
        {
            if (gameObjectNhanVatGoc != null) gameObjectNhanVatGoc.SetActive(true);
            if (gameObjectNhanVatDangTieu != null) gameObjectNhanVatDangTieu.SetActive(false);

            if (sliderMacTieu != null)
            {
                sliderMacTieu.minValue = 0f;
                sliderMacTieu.maxValue = mucDoMacTieuToiDa;
                sliderMacTieu.value = mucDoMacTieuHienTai;
            }
        }

        /// <summary>
        /// Thuật toán tăng dần thông số mắc tiểu theo thời gian thực
        /// </summary>
        private void TangMucMacTieuTheoThoiGian()
        {
            if (mucDoMacTieuHienTai < mucDoMacTieuToiDa)
            {
                mucDoMacTieuHienTai += tocDoTangMacTieu * Time.deltaTime;
                mucDoMacTieuHienTai = Mathf.Min(mucDoMacTieuHienTai, mucDoMacTieuToiDa);
                CapNhatUI();
            }
        }

        /// <summary>
        /// Cập nhật giá trị float lên Slider UI
        /// </summary>
        private void CapNhatUI()
        {
            if (sliderMacTieu != null)
            {
                sliderMacTieu.value = mucDoMacTieuHienTai;
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện tự động tiểu hoặc kích hoạt chủ động từ phím bấm
        /// </summary>
        private void KiemTraDieuKienTieu()
        {
            // 1. Tự động tiểu ngay lập tức bất chấp hành động nếu thanh slider đầy (>= 100%)
            if (mucDoMacTieuHienTai >= mucDoMacTieuToiDa)
            {
                StartCoroutine(ThucHienHanhDongTieu());
                return;
            }

            // 2. Người chơi tự bấm phím tiểu bất kể khi nào (chỉ cần mức mắc tiểu > 0)
            if (Input.GetKeyDown(phimTieu) && mucDoMacTieuHienTai > 0f)
            {
                StartCoroutine(ThucHienHanhDongTieu());
            }
        }

        /// <summary>
        /// Coroutine xử lý tiến trình tiểu, tính toán thời gian theo tỷ lệ mắc tiểu,
        /// bật bất tử và khóa di chuyển
        /// </summary>
        private IEnumerator ThucHienHanhDongTieu()
        {
            đangTieu = true;

            // Tính toán thời gian tiểu tuyến tính (Mắc tiểu càng ít tiểu càng nhanh, mắc tiểu đầy tiểu lâu nhất)
            float tyLeMacTieu = Mathf.Clamp01(mucDoMacTieuHienTai / mucDoMacTieuToiDa);
            float thoiGianTieuThucTe = Mathf.Lerp(thoiGianTieuToiThieu, thoiGianTieuToiDa, tyLeMacTieu);

            // Bật trạng thái nhân vật đang tiểu (Khóa di chuyển + Bật Bất tử + Đổi GameObject)
            ThietLapTrangThaiDangTieu(true, thoiGianTieuThucTe);

            // Phát sự kiện thông báo cho Quái hoảng sợ bỏ chạy
            OnExecutionStart?.Invoke();

            // Chờ hết thời gian tiểu thực tế
            yield return new WaitForSeconds(thoiGianTieuThucTe);

            // Reset mức mắc tiểu về 0 và cập nhật Slider UI
            mucDoMacTieuHienTai = 0f;
            CapNhatUI();

            // Trả lại trạng thái nhân vật bình thường
            ThietLapTrangThaiDangTieu(false, 0f);

            // Phát sự kiện kết thúc (Quái hết sợ, dính Knockback và đánh tiếp)
            OnExecutionEnd?.Invoke();

            đangTieu = false;
        }

        /// <summary>
        /// Thiết lập ẩn hiện GameObject, khóa di chuyển và bật/tắt bất tử
        /// </summary>
        private void ThietLapTrangThaiDangTieu(bool isUrination, float duration)
        {
            // 1. Hoán đổi GameObject hiển thị
            if (gameObjectNhanVatGoc != null) gameObjectNhanVatGoc.SetActive(!isUrination);
            if (gameObjectNhanVatDangTieu != null) gameObjectNhanVatDangTieu.SetActive(isUrination);

            // 2. Khóa / Mở lại di chuyển người chơi
            if (playerController != null)
            {
                if (isUrination)
                {
                    playerController.StopMovementAndAnimation();
                    playerController.enabled = false;
                }
                else
                {
                    playerController.enabled = true;
                }
            }

            // 3. Bật / Tắt trạng thái Bất tử không bị mất máu
            if (characterStats != null)
            {
                if (isUrination)
                {
                    characterStats.SetInvincible(duration + 0.1f);
                }
            }
        }

        /// <summary>
        /// Hàm hỗ trợ cộng trực tiếp mức mắc tiểu (Ví dụ: khi uống nước/ăn đồ ăn)
        /// </summary>
        public void CongMucMacTieu(float luongTang)
        {
            mucDoMacTieuHienTai = Mathf.Clamp(mucDoMacTieuHienTai + luongTang, 0f, mucDoMacTieuToiDa);
            CapNhatUI();
        }
    }
}