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

        [Header("=== CẤU HÌNH TỐC ĐỘ XẢ TIỂU ===")]
        [Tooltip("Tốc độ tuột thanh mắc tiểu khi đang tiểu (Số điểm / Giây)")]
        [SerializeField] private float tocDoXamTieu = 25f;

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

        // Tham chiếu các script điều khiển
        private PlayerController playerController;
        private TanCong tanCongScript;
        private Luot luotScript;
        private CharacterStats characterStats;
        private Rigidbody2D rb;

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

            // Tự động tìm tất cả các script chức năng trên Player để vô hiệu hóa khi tiểu
            playerController = GetComponent<PlayerController>();
            tanCongScript = GetComponent<TanCong>();
            luotScript = GetComponent<Luot>();
            characterStats = GetComponent<CharacterStats>();
            rb = GetComponent<Rigidbody2D>();
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
            // 1. Tự động tiểu ngay lập tức khi thanh slider đầy (>= 100%)
            if (mucDoMacTieuHienTai >= mucDoMacTieuToiDa)
            {
                StartCoroutine(ThucHienHanhDongTieu());
                return;
            }

            // 2. Người chơi tự bấm phím tiểu (chỉ cần mức mắc tiểu > 0)
            if (Input.GetKeyDown(phimTieu) && mucDoMacTieuHienTai > 0f)
            {
                StartCoroutine(ThucHienHanhDongTieu());
            }
        }

        /// <summary>
        /// Coroutine xử lý tiến trình xả tiểu: thanh slider tuột dần về 0 mới kết thúc
        /// </summary>
        private IEnumerator ThucHienHanhDongTieu()
        {
            đangTieu = true;

            // Khóa di chuyển, tấn công, lướt và kích hoạt nhân vật tiểu
            ThietLapTrangThaiKhoaHanhDong(true);

            // Phát sự kiện thông báo cho Quái hoảng sợ bỏ chạy
            OnExecutionStart?.Invoke();

            // VÒNG LẶP TUỘT SLIDER: Xả dần năng lượng cho đến khi hết hẳn
            while (mucDoMacTieuHienTai > 0f)
            {
                mucDoMacTieuHienTai -= tocDoXamTieu * Time.deltaTime;
                mucDoMacTieuHienTai = Mathf.Max(0f, mucDoMacTieuHienTai);

                CapNhatUI();

                // Duy trì bất tử liên tục trong suốt thời gian xả tiểu
                if (characterStats != null)
                {
                    characterStats.SetInvincible(0.5f);
                }

                yield return null; // Chờ sang khung hình tiếp theo
            }

            // Mở lại hoạt động nhân vật bình thường
            ThietLapTrangThaiKhoaHanhDong(false);

            // Phát sự kiện kết thúc
            OnExecutionEnd?.Invoke();

            đangTieu = false;
        }

        /// <summary>
        /// Khóa/Mở lại các Script điều khiển (Di chuyển, Bắn, Lướt) và đổi GameObject
        /// </summary>
        private void ThietLapTrangThaiKhoaHanhDong(bool isUrination)
        {
            // 1. Hoán đổi GameObject hiển thị
            if (gameObjectNhanVatGoc != null) gameObjectNhanVatGoc.SetActive(!isUrination);
            if (gameObjectNhanVatDangTieu != null) gameObjectNhanVatDangTieu.SetActive(isUrination);

            // 2. Vô hiệu hóa/Kích hoạt lại các script hành động của Player
            if (playerController != null)
            {
                if (isUrination) playerController.StopMovementAndAnimation();
                playerController.enabled = !isUrination;
            }

            if (tanCongScript != null)
            {
                tanCongScript.enabled = !isUrination;
            }

            if (luotScript != null)
            {
                luotScript.enabled = !isUrination;
            }

            // 3. Triệt tiêu ngay vận tốc hiện tại nếu đang di chuyển dở
            if (isUrination && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
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