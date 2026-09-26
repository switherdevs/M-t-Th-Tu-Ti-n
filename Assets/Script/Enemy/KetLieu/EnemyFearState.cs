using System.Collections;
using UnityEngine;
using GameCore.Player; // Kết nối với Namespace chứa PlayerUrinationMechanism

public class EnemyFearState : MonoBehaviour
{
    [Header("=== CẤU HÌNH SCRIPT AI GỐC ===")]
    [Tooltip("Kéo ĐÚNG 1 script AI duy nhất của quái vào đây")]
    [SerializeField] private MonoBehaviour scriptAIGoc;

    [Header("=== CẤU HÌNH ANIMATION ===")]
    [SerializeField] private Animator animator;
    [Tooltip("Tên tham số Bool điều khiển Animation di chuyển (VD: isWalking, isMoving, Walk)")]
    [SerializeField] private string tenParamAnimation = "isWalking";

    [Header("=== CẤU HÌNH SỢ HÃI & DI CHUYỂN ===")]
    [Tooltip("Khoảng cách giữ an toàn so với Player")]
    [SerializeField] private float khoangCachAnToan = 8f;
    [Tooltip("Tốc độ di chuyển lùi/né chậm rãi")]
    [SerializeField] private float tocDoLui = 4f;

    [Header("=== CẤU HÌNH KNOCKBACK & SLOW ===")]
    [Tooltip("Lực hất văng quái ra xa khi hết kết liễu")]
    [SerializeField] private float lucBatRa = 15f;
    [Tooltip("Thời gian bị choáng sau khi bị hất văng (giây)")]
    [SerializeField] private float thoiGianChoang = 0.3f;

    // Biến lưu nội bộ
    private Rigidbody2D rb2D;
    private Transform playerTransform;
    private bool đangSoHai = false;
    private bool đangBiKnockback = false;
    private int huongNeTranh = 1;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(KetNoiPlayer());
    }

    private void OnDisable()
    {
        HuyDangKySuKien();
    }

    /// <summary>
    /// COROUTINE KẾT NỐI TỰ ĐỘNG VỚI SCRIPT MẮC TIỂU CỦA PLAYER
    /// </summary>
    private IEnumerator KetNoiPlayer()
    {
        while (PlayerUrinationMechanism.Instance == null)
        {
            yield return null;
        }

        HuyDangKySuKien();

        PlayerUrinationMechanism.Instance.OnExecutionStart += BatDauSoHai;
        PlayerUrinationMechanism.Instance.OnExecutionEnd += KetThucSoHai;
        playerTransform = PlayerUrinationMechanism.Instance.transform;
    }

    /// <summary>
    /// HỦY ĐĂNG KÝ SỰ KIỆN TRÁNH LỖI BỘ NHỚ
    /// </summary>
    private void HuyDangKySuKien()
    {
        if (PlayerUrinationMechanism.Instance != null)
        {
            PlayerUrinationMechanism.Instance.OnExecutionStart -= BatDauSoHai;
            PlayerUrinationMechanism.Instance.OnExecutionEnd -= KetThucSoHai;
        }
    }

    private void FixedUpdate()
    {
        if (!đangSoHai || đangBiKnockback || playerTransform == null) return;

        XuLyLuiVaDiChuyen();
    }

    /// <summary>
    /// THUẬT TOÁN DI CHUYỂN LÙI (GIỮ NGUYÊN HƯỚNG QUAY MẶT CỦA QUÁI)
    /// </summary>
    private void XuLyLuiVaDiChuyen()
    {
        Vector2 viTriQuai = transform.position;
        Vector2 viTriPlayer = playerTransform.position;

        Vector2 huongDayRa = (viTriQuai - viTriPlayer).normalized;
        float khoangCach = Vector2.Distance(viTriQuai, viTriPlayer);

        Vector2 vanTocTarget = Vector2.zero;

        // 1. Nếu Player ở quá gần -> Lùi thẳng ra xa Player
        if (khoangCach < khoangCachAnToan)
        {
            vanTocTarget = huongDayRa * tocDoLui;
        }
        // 2. Nếu đã đủ khoảng cách -> Đi lùi/né ngang chậm rãi
        else
        {
            Vector2 huongNeNgang = new Vector2(-huongDayRa.y, huongDayRa.x) * huongNeTranh;
            vanTocTarget = huongNeNgang * (tocDoLui * 0.6f);
        }

        // Áp dụng lực di chuyển vào Rigidbody2D
        rb2D.linearVelocity = vanTocTarget;

        // Bật Animation di chuyển/chạy lùi
        BatAnimationMove(true);
    }

    /// <summary>
    /// BẮT ĐẦU TRẠNG THÁI SỢ HÃI (KHI PLAYER BẮT ĐẦU TIỂU)
    /// </summary>
    private void BatDauSoHai()
    {
        đangSoHai = true;

        if (scriptAIGoc != null)
        {
            // Ép dừng toàn bộ Coroutine đang xả đạn/tấn công của AI gốc
            scriptAIGoc.StopAllCoroutines();
            scriptAIGoc.enabled = false;
        }

        // Ép hướng mặt quái quay về phía Player tại thời điểm bắt đầu
        if (playerTransform != null)
        {
            float xDiff = playerTransform.position.x - transform.position.x;

            Vector3 rot = transform.eulerAngles;
            rot.y = xDiff > 0 ? 0f : 180f;
            transform.eulerAngles = rot;
        }

        huongNeTranh = (Random.value > 0.5f) ? 1 : -1;
    }

    /// <summary>
    /// KẾT THÚC TRẠNG THÁI SỢ HÃI (KHI PLAYER TIỂU XONG)
    /// </summary>
    private void KetThucSoHai()
    {
        if (!đangSoHai) return;

        đangSoHai = false;
        BatAnimationMove(false);

        StartCoroutine(XuLyKnockbackVaBatLaiAI());
    }

    private IEnumerator XuLyKnockbackVaBatLaiAI()
    {
        đangBiKnockback = true;

        // Hất văng quái ra xa Player
        if (playerTransform != null)
        {
            Vector2 huongBatRa = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(huongBatRa * lucBatRa, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(thoiGianChoang);

        rb2D.linearVelocity = Vector2.zero;
        đangBiKnockback = false;

        // Bật lại AI gốc để quái tiếp tục chiến đấu
        if (scriptAIGoc != null)
        {
            scriptAIGoc.enabled = true;
        }
    }

    private void BatAnimationMove(bool dangChay)
    {
        if (animator != null && !string.IsNullOrEmpty(tenParamAnimation))
        {
            animator.SetBool(tenParamAnimation, dangChay);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (đangSoHai && collision.gameObject.CompareTag("Wall"))
        {
            huongNeTranh *= -1;
        }
    }
}