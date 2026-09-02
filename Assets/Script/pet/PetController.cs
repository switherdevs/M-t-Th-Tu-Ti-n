using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetController : MonoBehaviour
{
    [Header("--- THÔNG TIN LINH THỦ (PET STATS) ---")]
    [SerializeField] private string tenPet = "Hỏa Hồ Tinh";
    [SerializeField] private float sátThương = 30f;
    [Tooltip("Số lần đánh trong 1 giây (Ví dụ: 2 = đánh 2 lần/giây, 0.5 = 2 giây đánh 1 lần)")]
    [SerializeField] private float tốcĐộĐánh = 1.5f;
    [SerializeField] private float cấpĐộ = 1;
    [SerializeField] private string hệNguHanh = "Hỏa";

    [Header("--- TỌA ĐỘ & DI CHUYỂN ---")]
    [Tooltip("Chọn Layer đại diện cho Player (VD: Player)")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Bán kính tối đa để Pet quét tìm Player trong Scene")]
    [SerializeField] private float bánKínhQuétPlayer = 50f;
    [SerializeField] private float tốcĐộTheoChân = 4f;
    [SerializeField] private float tốcĐộRượtQuái = 5f;
    [SerializeField] private float khoảngCáchDừngTheoPlayer = 1.5f;
    [SerializeField] private float khoảngCáchDừngĐánhQuái = 1.2f;

    [Header("--- TẤN CÔNG & QUAN SÁT ---")]
    [Tooltip("Bán kính phát hiện quái để bắt đầu rượt đuổi")]
    [SerializeField] private float bánKínhPhátHiệnQuái = 6f;
    [Tooltip("Tùy chỉnh vị trí tâm của vùng quét tấn công theo trục X và Y")]
    [SerializeField] private Vector2 offsetVùngTấnCông = Vector2.zero;
    [SerializeField] private string enemyTag = "Enemy";
    [Tooltip("GameObject Hitbox tấn công (có chứa Collider2D Trigger)")]
    [SerializeField] private GameObject hitboxTấnCông;
    [SerializeField] private float thờiGianBậtHitbox = 0.2f;

    [Header("--- THỜI GIAN & HIỆU ỨNG NGỦ ---")]
    [Tooltip("Thời gian Player đứng yên để Pet đi ngủ (giây)")]
    [SerializeField] private float thờiGianNgủ = 20f;

    [Tooltip("GameObject hiển thị mặt ngủ (Mắt nhắm / Mặt ngủ)")]
    [SerializeField] private GameObject matNguObject;

    [Tooltip("GameObject hiệu ứng ngủ (Particle Zzz / Effect ngủ)")]
    [SerializeField] private GameObject hieuUngNguObject;

    [Header("--- ANIMATION PARAMETERS ---")]
    [SerializeField] private string animIsMoving = "isMoving";
    [SerializeField] private string animSleepTrigger = "SleepTrigger";
    [SerializeField] private string animIsSleepIdle = "isSleepIdle";
    [SerializeField] private string animAttackTrigger = "AttackTrigger";
    [Tooltip("Tên tham số Float trong Animator dùng để chỉnh tốc độ Animation Đánh")]
    [SerializeField] private string animAttackSpeedParam = "AttackSpeed";

    private Transform playerTransform;
    private Animator anim;
    private Vector3 vịTríCũPlayer;
    private float đếmNgượcĐứngYên = 0f;
    private float đếmNgượcHồiChiêuĐánh = 0f;
    private Transform mụcTiêuQuái;
    private bool đangTấnCông = false;
    private bool đangTrạngTháiNgủ = false;

    // Public properties để script UI truy cập dữ liệu
    public string TenPet => tenPet;
    public float SátThương => sátThương;
    public float CấpĐộ => cấpĐộ;
    public string HệNguHanh => hệNguHanh;
    public float BánKínhTấnCông => bánKínhPhátHiệnQuái;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (hitboxTấnCông != null)
        {
            hitboxTấnCông.SetActive(false);
        }

        DatTrangThaiHieuUngNgu(false);
        TimPlayerTheoLayer();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            TimPlayerTheoLayer();
            return;
        }

        XửLýThờiGianĐứngYênCủaPlayer();

        // Cập nhật đếm ngược hồi chiêu
        if (đếmNgượcHồiChiêuĐánh > 0) đếmNgượcHồiChiêuĐánh -= Time.deltaTime;

        // Nếu đang trong đòn đánh thì không can thiệp di chuyển
        if (đangTấnCông) return;

        // Quét tìm quái mới nếu chưa có hoặc quái cũ đã bị tiêu diệt (null/inactive)
        if (mụcTiêuQuái == null || !mụcTiêuQuái.gameObject.activeInHierarchy)
        {
            QuétTìmQuáiGầnNhất();
        }

        // ƯU TIÊN 1: Nếu có quái -> Rượt đuổi và đánh cho đến khi quái chết
        if (mụcTiêuQuái != null && mụcTiêuQuái.gameObject.activeInHierarchy)
        {
            RượtĐuổiVàTấnCôngQuái();
            return;
        }

        // ƯU TIÊN 2: Khi không còn quái -> Quay về theo chân Player
        TheoChânPlayer();
    }

    private void TimPlayerTheoLayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, bánKínhQuétPlayer, playerLayer);
        if (hit != null)
        {
            playerTransform = hit.transform;
            vịTríCũPlayer = playerTransform.position;
        }
    }

    private void XửLýThờiGianĐứngYênCủaPlayer()
    {
        float khoảngCáchDiChuyển = Vector3.Distance(playerTransform.position, vịTríCũPlayer);

        if (khoảngCáchDiChuyển > 0.01f)
        {
            đếmNgượcĐứngYên = 0f;
            vịTríCũPlayer = playerTransform.position;

            if (đangTrạngTháiNgủ)
            {
                DatTrangThaiHieuUngNgu(false);
            }
        }
        else
        {
            đếmNgượcĐứngYên += Time.deltaTime;
        }
    }

    public Vector3 LayTamVungTanCong()
    {
        float huongX = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(offsetVùngTấnCông.x * huongX, offsetVùngTấnCông.y, 0f);
    }

    private void QuétTìmQuáiGầnNhất()
    {
        GameObject[] danhSáchQuái = GameObject.FindGameObjectsWithTag(enemyTag);
        float khoảngCáchGầnNhất = bánKínhPhátHiệnQuái;
        mụcTiêuQuái = null;

        Vector3 tâmQuét = LayTamVungTanCong();

        foreach (GameObject quái in danhSáchQuái)
        {
            float d = Vector2.Distance(tâmQuét, quái.transform.position);
            if (d <= khoảngCáchGầnNhất)
            {
                khoảngCáchGầnNhất = d;
                mụcTiêuQuái = quái.transform;
            }
        }
    }

    private void RượtĐuổiVàTấnCôngQuái()
    {
        if (đangTrạngTháiNgủ)
        {
            DatTrangThaiHieuUngNgu(false);
        }

        float khoảngCáchĐếnQuái = Vector2.Distance(transform.position, mụcTiêuQuái.position);

        // Lật mặt hướng về phía quái
        LậtMặtTheoHướng(mụcTiêuQuái.position.x);

        if (khoảngCáchĐếnQuái > khoảngCáchDừngĐánhQuái)
        {
            // Chưa đủ tầm -> Tiếp tục di chuyển rượt theo quái
            transform.position = Vector2.MoveTowards(transform.position, mụcTiêuQuái.position, tốcĐộRượtQuái * Time.deltaTime);
            SetAnimBool(animIsMoving, true);
            SetAnimBool(animIsSleepIdle, false);
        }
        else
        {
            // Đã chạm tầm đánh -> Dừng di chuyển và thực hiện đòn đánh liên tục khi hết hồi chiêu
            SetAnimBool(animIsMoving, false);

            if (đếmNgượcHồiChiêuĐánh <= 0f && !đangTấnCông)
            {
                StartCoroutine(Routine_ThựcThiTấnCông());
            }
        }
    }

    private void TheoChânPlayer()
    {
        float khoảngCáchĐếnPlayer = Vector2.Distance(transform.position, playerTransform.position);

        LậtMặtTheoHướng(playerTransform.position.x);

        if (khoảngCáchĐếnPlayer > khoảngCáchDừngTheoPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, tốcĐộTheoChân * Time.deltaTime);

            SetAnimBool(animIsMoving, true);
            SetAnimBool(animIsSleepIdle, false);

            if (đangTrạngTháiNgủ)
            {
                DatTrangThaiHieuUngNgu(false);
            }
        }
        else
        {
            SetAnimBool(animIsMoving, false);

            if (đếmNgượcĐứngYên >= thờiGianNgủ)
            {
                if (!đangTrạngTháiNgủ)
                {
                    SetAnimTrigger(animSleepTrigger);
                    DatTrangThaiHieuUngNgu(true);
                }

                SetAnimBool(animIsSleepIdle, true);
            }
            else
            {
                SetAnimBool(animIsSleepIdle, false);

                if (đangTrạngTháiNgủ)
                {
                    DatTrangThaiHieuUngNgu(false);
                }
            }
        }
    }

    private IEnumerator Routine_ThựcThiTấnCông()
    {
        đangTấnCông = true;

        // Thời gian chờ giữa 2 lần đánh tính theo giây = 1 / tốcĐộĐánh
        float thờiGianHồiChiêu = 1f / Mathf.Max(0.1f, tốcĐộĐánh);
        đếmNgượcHồiChiêuĐánh = thờiGianHồiChiêu;

        SetAnimBool(animIsMoving, false);
        SetAnimBool(animIsSleepIdle, false);

        if (mụcTiêuQuái != null)
        {
            LậtMặtTheoHướng(mụcTiêuQuái.position.x);
        }

        // Ép tốc độ Animator khớp hoàn toàn với tốcĐộĐánh
        SetAnimFloat(animAttackSpeedParam, tốcĐộĐánh);
        SetAnimTrigger(animAttackTrigger);

        // Bật Hitbox gây sát thương
        if (hitboxTấnCông != null)
        {
            hitboxTấnCông.SetActive(true);
        }

        // Giữ Hitbox chủ động khớp theo tỉ lệ tốc độ đánh
        float thờiGianBậtThựcTế = Mathf.Min(thờiGianBậtHitbox, thờiGianHồiChiêu * 0.5f);
        yield return new WaitForSeconds(thờiGianBậtThựcTế);

        if (hitboxTấnCông != null)
        {
            hitboxTấnCông.SetActive(false);
        }

        // Chờ hết thời gian của đòn đánh hiện tại rồi mới giải phóng cờ đangTấnCông
        yield return new WaitForSeconds(thờiGianHồiChiêu - thờiGianBậtThựcTế);

        đangTấnCông = false;
    }

    private void DatTrangThaiHieuUngNgu(bool kichHoat)
    {
        đangTrạngTháiNgủ = kichHoat;

        if (!kichHoat)
        {
            SetAnimBool(animIsSleepIdle, false);
        }

        if (matNguObject != null)
        {
            matNguObject.SetActive(kichHoat);
        }

        if (hieuUngNguObject != null)
        {
            hieuUngNguObject.SetActive(kichHoat);
        }
    }

    private void LậtMặtTheoHướng(float targetX)
    {
        Vector3 scale = transform.localScale;

        if (targetX > transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (targetX < transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    private void SetAnimBool(string paramName, bool value)
    {
        if (anim != null && !string.IsNullOrEmpty(paramName))
        {
            anim.SetBool(paramName, value);
        }
    }

    private void SetAnimFloat(string paramName, float value)
    {
        if (anim != null && !string.IsNullOrEmpty(paramName))
        {
            anim.SetFloat(paramName, value);
        }
    }

    private void SetAnimTrigger(string paramName)
    {
        if (anim != null && !string.IsNullOrEmpty(paramName))
        {
            anim.SetTrigger(paramName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(LayTamVungTanCong(), bánKínhPhátHiệnQuái);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, bánKínhQuétPlayer);
    }
}