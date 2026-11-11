using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Player
{
    public class PlayerUrinationMechanism : MonoBehaviour
    {
        public static PlayerUrinationMechanism Instance { get; private set; }

        public event Action OnExecutionStart;
        public event Action OnExecutionEnd;

        [Header("=== CẤU HÌNH CƠ CHẾ MẮC TIỀU ===")]
        [Tooltip("Mức độ mắc tiểu hiện tại (Float)")]
        [SerializeField] private float mucDoMacTieuHienTai = 0f;

        [Tooltip("Mức độ mắc tiểu tối đa")]
        [SerializeField] private float mucDoMacTieuToiDa = 100f;

        [Tooltip("Tốc độ tăng mức mắc tiểu theo thời gian (Số điểm / Giây)")]
        [SerializeField] private float tocDoTangMacTieu = 5f;

        [Header("--- CẤU HÌNH HIỂN THỊ KHI ĐẦY ---")]
        [Tooltip("GameObject sẽ hiện lên khi thanh mắc tiểu đầy (>= 100%) và ẩn đi khi chưa đầy")]
        [SerializeField] private GameObject notifyGameObject;

        [Header("=== CẤU HÌNH TEST GAME ===")]
        [Tooltip("Tick chọn để ngăn không tự động tiểu khi thanh mắc tiểu đầy")]
        [SerializeField] private bool testGame = false;

        [Header("=== CẤU HÌNH TỐC ĐỘ XẢ TIỀU ===")]
        [Tooltip("Tốc độ tuột thanh mắc tiểu khi đang tiểu (Số điểm / Giây)")]
        [SerializeField] private float tocDoXamTieu = 25f;

        [Tooltip("Phím bấm để thực hiện hành động tiểu (Mặc định phím U)")]
        [SerializeField] private KeyCode phimTieu = KeyCode.U;

        [Header("=== CẤU HÌNH CHẾ ĐỘ ASIAN (MEME DEAD) ===")]
        [Tooltip("Bán kính vòng tròn kiểm tra va chạm xung quanh Player khi tiểu")]
        [SerializeField] private float checkRadius = 1.5f;

        [Tooltip("Tag của Pet")]
        [SerializeField] private string petTag = "pet";

        [Tooltip("Tag của Tế Đàn")]
        [SerializeField] private string teDanTag = "Tedan";

        [Tooltip("LayerMask dùng để lọc các vật thể kiểm tra (Mặc định kiểm tra tất cả)")]
        [SerializeField] private LayerMask checkLayerMask = ~0;

        [Header("=== CẤU HÌNH GAMEOBJECT NHÂN VẬT ===")]
        [Tooltip("Game Object Nhân vật gốc (Sẽ bị ẨN khi đang tiểu)")]
        [SerializeField] private GameObject gameObjectNhanVatGoc;

        [Tooltip("Game Object Nhân vật đang tiểu (Sẽ HIỂN THỊ khi đang tiểu)")]
        [SerializeField] private GameObject gameObjectNhanVatDangTieu;

        [Header("=== CẤU HÌNH UI ===")]
        [Tooltip("Slider hiển thị mức độ mắc tiểu")]
        [SerializeField] private Slider sliderMacTieu;

        private PlayerController playerController;
        private TanCong tanCongScript;
        private Luot luotScript;
        private CharacterStats characterStats;
        private Rigidbody2D rb;
        private Animator animGoc;

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
            tanCongScript = GetComponent<TanCong>();
            luotScript = GetComponent<Luot>();
            characterStats = GetComponent<CharacterStats>();
            rb = GetComponent<Rigidbody2D>();

            if (gameObjectNhanVatGoc != null)
            {
                animGoc = gameObjectNhanVatGoc.GetComponent<Animator>();
                if (animGoc == null)
                {
                    animGoc = gameObjectNhanVatGoc.GetComponentInChildren<Animator>();
                }
            }
            if (animGoc == null)
            {
                animGoc = GetComponentInChildren<Animator>();
            }
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

            CapNhatUI();
        }

        private void TangMucMacTieuTheoThoiGian()
        {
            if (mucDoMacTieuHienTai < mucDoMacTieuToiDa)
            {
                mucDoMacTieuHienTai += tocDoTangMacTieu * Time.deltaTime;
                mucDoMacTieuHienTai = Mathf.Min(mucDoMacTieuHienTai, mucDoMacTieuToiDa);
                CapNhatUI();
            }
        }

        private void CapNhatUI()
        {
            if (sliderMacTieu != null)
            {
                sliderMacTieu.value = mucDoMacTieuHienTai;
            }

            // 🎯 KIỂM TRA TRẠNG THÁI HIỂN THỊ GAMEOBJECT KHI ĐẦY/CHƯA ĐẦY THANH MẮC TIỂU
            if (notifyGameObject != null)
            {
                bool daDayThanh = (mucDoMacTieuHienTai >= mucDoMacTieuToiDa);
                notifyGameObject.SetActive(daDayThanh);
            }
        }

        private void KiemTraDieuKienTieu()
        {
            if (mucDoMacTieuHienTai >= mucDoMacTieuToiDa)
            {
                if (testGame)
                {
                    return;
                }

                int currentDifficulty = PlayerPrefs.GetInt("GameDifficulty", 1);
                if (currentDifficulty == 3)
                {
                    Debug.LogWarning("<color=red>[Asian Mode Meme]</color> Nhịn tiểu quá lâu nhịn không nổi vỡ bàng quang chết!");
                    KichHoatNhanVatChet();
                    return;
                }

                StartCoroutine(ThucHienHanhDongTieu());
                return;
            }

            if (Input.GetKeyDown(phimTieu) && mucDoMacTieuHienTai > 0f)
            {
                StartCoroutine(ThucHienHanhDongTieu());
            }
        }

        private IEnumerator ThucHienHanhDongTieu()
        {
            đangTieu = true;

            ThietLapTrangThaiKhoaHanhDong(true);
            OnExecutionStart?.Invoke();

            // 🔄 VÒNG LẶP XẢ TIỂU:
            // - Lặp liên tục cho đến khi giá trị mắc tiểu hiện tại giảm về 0.
            // - Trong mỗi khung hình, trừ dần theo tốc độ xả (tocDoXamTieu * Time.deltaTime).
            // - Cập nhật lại giao diện Slider và trạng thái bất tử cho nhân vật cho tới khi xả xong.
            while (mucDoMacTieuHienTai > 0f)
            {
                if (KiemTraXuatTieuAsianMode())
                {
                    yield break;
                }

                mucDoMacTieuHienTai -= tocDoXamTieu * Time.deltaTime;
                mucDoMacTieuHienTai = Mathf.Max(0f, mucDoMacTieuHienTai);

                CapNhatUI();

                if (characterStats != null)
                {
                    characterStats.SetInvincible(0.5f);
                }

                yield return null;
            }

            ThietLapTrangThaiKhoaHanhDong(false);
            OnExecutionEnd?.Invoke();

            đangTieu = false;
        }

        private bool KiemTraXuatTieuAsianMode()
        {
            int currentDifficulty = PlayerPrefs.GetInt("GameDifficulty", 1);
            if (currentDifficulty != 3) return false;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, checkLayerMask);

            // 🔄 VÒNG LẶP QUÉT VA CHẠM ASIAN MODE:
            // - Kiểm tra các đối tượng xung quanh trong phạm vi bán kính checkRadius.
            // - Nếu phát hiện chạm Pet hoặc Tế Đàn, kích hoạt xử lý chết ngay lập tức.
            foreach (Collider2D col in hitColliders)
            {
                if (col.CompareTag(petTag) || col.CompareTag(teDanTag))
                {
                    Debug.LogWarning($"<color=red>[Asian Mode Meme]</color> Người chơi tiểu lên {col.tag}! Bị phạt chết ngay!");
                    KichHoatNhanVatChet();
                    return true;
                }
            }

            return false;
        }

        public void KichHoatNhanVatChet()
        {
            ThietLapTrangThaiKhoaHanhDong(false);
            đangTieu = false;

            if (characterStats != null)
            {
                characterStats.StopAllCoroutines();
                characterStats.TakeDamage(9999999f);
            }
        }

        private void ThietLapTrangThaiKhoaHanhDong(bool isUrination)
        {
            if (gameObjectNhanVatGoc != null) gameObjectNhanVatGoc.SetActive(!isUrination);
            if (gameObjectNhanVatDangTieu != null) gameObjectNhanVatDangTieu.SetActive(isUrination);

            if (isUrination)
            {
                if (playerController != null) playerController.StopMovementAndAnimation();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
            else
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;

                if (animGoc != null)
                {
                    animGoc.SetFloat("speed", 0f);
                    animGoc.SetFloat("Speed", 0f);
                    animGoc.SetBool("isMoving", false);
                    animGoc.SetBool("IsMoving", false);
                    animGoc.SetBool("walk", false);
                }

                if (playerController != null)
                {
                    playerController.StopMovementAndAnimation();
                }
            }

            if (playerController != null) playerController.enabled = !isUrination;
            if (tanCongScript != null) tanCongScript.enabled = !isUrination;
            if (luotScript != null) luotScript.enabled = !isUrination;
        }

        public void CongMucMacTieu(float luongTang)
        {
            mucDoMacTieuHienTai = Mathf.Clamp(mucDoMacTieuHienTai + luongTang, 0f, mucDoMacTieuToiDa);
            CapNhatUI();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
    }
}