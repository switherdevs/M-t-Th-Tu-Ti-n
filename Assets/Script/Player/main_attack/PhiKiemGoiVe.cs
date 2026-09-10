using StatsSystem.Components;
using UnityEngine;

public class PhiKiemGoiVe : MonoBehaviour
{
    [Header("=== CẤU HÌNH THỜI GIAN & TỐC ĐỘ ===")]
    [Tooltip("Thời gian kiếm tự hủy nếu không được gọi về (giây)")]
    [SerializeField] private float thoiGianTonTai = 15f;
    [Tooltip("Tốc độ kiếm bay ngược trở về Player")]
    [SerializeField] private float tocDoBayVe = 40f;

    [Header("=== CẤU HÌNH CÂN BẰNG GÓC & ĐỘ ĐÂM SÂU ===")]
    [Tooltip("Nếu ảnh kiếm bị lệch hướng, điền góc bù vào đây (VD: 90, -90, 180)")]
    [SerializeField] private float gocBuSprite = 0f;
    [Tooltip("Khoảng cách kiếm cắm ngập sâu vào thân quái (Số càng lớn cắm càng sâu)")]
    [SerializeField] private float doDamSau = 0.5f;

    [Header("=== CẤU HÌNH SÁT THƯƠNG GỌI VỀ ===")]
    [Tooltip("Hệ số nhân sát thương khi gọi kiếm bay về đâm trúng quái/mục tiêu (VD: 2 = Sát thương X2)")]
    [SerializeField] private float heSoNhanSatThuong = 2f;

    [Header("=== CẤU HÌNH KIẾM CẮM GIẢ (VISUAL IMPALED SWORD) ===")]
    [Tooltip("Kéo Prefab Kiếm Giả vào đây (Chỉ có SpriteRenderer, KHÔNG CÓ Collider/Rigidbody)")]
    [SerializeField] private GameObject prefabKiemCamGia;

    [Header("=== ÂM THANH & HIỆU ỨNG ===")]
    [SerializeField] private AudioClip amThanhTrung;
    [SerializeField] private GameObject hieuUngTrungPrefab;
    [SerializeField][Range(0f, 1f)] private float amLuongAmThanh = 1f;

    // Biến lưu trữ nội bộ
    private CharacterStats chiSoNguoiBan;
    private DamageDealer gaySatThuongGoc;
    private Rigidbody2D rb2D;
    private Collider2D vaCham2D;

    private bool laDangBayVe = false;
    private bool daCamVaoTuong = false;
    private Transform viTriPlayer;

    public bool LaDangBayVe => laDangBayVe;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        vaCham2D = GetComponent<Collider2D>();
        gaySatThuongGoc = GetComponent<DamageDealer>();
    }

    private void Start()
    {
        Destroy(gameObject, thoiGianTonTai);
    }

    public void Setup(Vector2 huongBay, CharacterStats chiSoPlayer)
    {
        chiSoNguoiBan = chiSoPlayer;
        XuLyTuXoayTheoVanToc(huongBay);
    }

    private void Update()
    {
        if (laDangBayVe && viTriPlayer != null)
        {
            XuLyBayVe();
        }
        else if (!daCamVaoTuong && rb2D != null && rb2D.linearVelocity.sqrMagnitude > 0.1f)
        {
            XuLyTuXoayTheoVanToc(rb2D.linearVelocity);
        }
    }

    private void XuLyTuXoayTheoVanToc(Vector2 vanToc)
    {
        float gocXoay = (Mathf.Atan2(vanToc.y, vanToc.x) * Mathf.Rad2Deg) + gocBuSprite;
        transform.rotation = Quaternion.Euler(0, 0, gocXoay);
    }

    public void BatDauBayVe(Transform bienPlayer)
    {
        laDangBayVe = true;
        daCamVaoTuong = false;
        viTriPlayer = bienPlayer;

        CancelInvoke();

        if (rb2D != null)
        {
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.linearVelocity = Vector2.zero;
        }

        if (vaCham2D != null)
        {
            vaCham2D.isTrigger = true;
        }
    }

    private void XuLyBayVe()
    {
        transform.position = Vector2.MoveTowards(transform.position, viTriPlayer.position, tocDoBayVe * Time.deltaTime);

        Vector2 huongBayVe = ((Vector2)viTriPlayer.position - (Vector2)transform.position).normalized;
        if (huongBayVe != Vector2.zero)
        {
            XuLyTuXoayTheoVanToc(huongBayVe);
        }

        if (Vector2.Distance(transform.position, viTriPlayer.position) < 0.4f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D vaCham)
    {
        // BỎ QUA VA CHẠM VỚI PLAYER ĐỂ KHÔNG BỊ BIẾN MẤT KHI VỪA BẮN RA
        if (vaCham.CompareTag("Player")) return;

        // 1. KIỂM TRA MỤC TIÊU CÓ PHẢI CÁI LU (MapLootJar) HAY KHÔNG
        MapLootJar caiLu = vaCham.GetComponentInParent<MapLootJar>();
        if (caiLu != null)
        {
            float satThuongGoc = (gaySatThuongGoc != null) ? gaySatThuongGoc.BaseDamage : 15f;
            float satThuongThucTe = laDangBayVe ? (satThuongGoc * heSoNhanSatThuong) : satThuongGoc;

            // Gọi hàm đập vỡ cái lu (MapLootJar tự sinh VFX và tự Drop đồ)
            caiLu.TakeDamage(satThuongThucTe);
            PhatHieuUng();

            // QUAN TRỌNG: KHÔNG Destroy(gameObject) ĐỂ KIẾM BAY XUYÊN QUA CÁI LU
            return;
        }

        // 2. THƯỜNG HỢP KIẾM ĐANG BAY VỀ ĐÂM TRÚNG QUÁI
        if (laDangBayVe)
        {
            if (vaCham.CompareTag("Enemy"))
            {
                float satThuongGoc = (gaySatThuongGoc != null) ? gaySatThuongGoc.BaseDamage : 15f;
                float tongSatThuongGoiVe = satThuongGoc * heSoNhanSatThuong;

                var quaiStats = vaCham.GetComponentInParent<CharacterStats>();
                if (quaiStats != null)
                {
                    quaiStats.TakeDamage(tongSatThuongGoiVe);
                }

                Vector3 diemVaCham = vaCham.ClosestPoint(transform.position);
                if (gaySatThuongGoc != null)
                {
                    gaySatThuongGoc.HienThiPopupGoiVe(tongSatThuongGoiVe, diemVaCham);
                }

                PhatHieuUng();
                TaoKiemCamGiaTrenQuai(vaCham.transform);

                // Bay về đâm trúng quái thì hủy kiếm gốc và cắm kiếm giả
                Destroy(gameObject);
            }
            return;
        }

        // 3. THƯỜNG HỢP KIẾM BẮN RA CHẠM TƯỜNG (CẮM VÀO TƯỜNG)
        if (!daCamVaoTuong && vaCham.CompareTag("Wall"))
        {
            daCamVaoTuong = true;
            if (rb2D != null)
            {
                rb2D.linearVelocity = Vector2.zero;
                rb2D.bodyType = RigidbodyType2D.Static;
            }
            PhatHieuUng();
            return;
        }

        // 4. THƯỜNG HỢP KIẾM BẮN RA CHẠM QUÁI LẦN ĐẦU
        if (!daCamVaoTuong && vaCham.CompareTag("Enemy"))
        {
            PhatHieuUng();
            Destroy(gameObject); // Chạm quái thì tiêu biến
        }
    }

    private void TaoKiemCamGiaTrenQuai(Transform quaiTransform)
    {
        if (prefabKiemCamGia == null) return;

        Vector3 huongMuiKiem = transform.right;

        if (gocBuSprite != 0)
        {
            float gocRadian = (transform.eulerAngles.z - gocBuSprite) * Mathf.Deg2Rad;
            huongMuiKiem = new Vector3(Mathf.Cos(gocRadian), Mathf.Sin(gocRadian), 0);
        }

        Vector3 viTriCamSau = transform.position + (huongMuiKiem * doDamSau);

        GameObject kiemCamGia = Instantiate(prefabKiemCamGia, viTriCamSau, transform.rotation);
        kiemCamGia.transform.localScale = transform.localScale;
        kiemCamGia.transform.SetParent(quaiTransform);
    }

    private void PhatHieuUng()
    {
        if (hieuUngTrungPrefab != null)
        {
            Instantiate(hieuUngTrungPrefab, transform.position, transform.rotation);
        }

        if (amThanhTrung != null)
        {
            AudioSource.PlayClipAtPoint(amThanhTrung, transform.position, amLuongAmThanh);
        }
    }
}