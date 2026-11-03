using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct YeuCauNguyenLieuPet
{
    public string idItem;
    public int soLuongCan;
}

public class PetController : MonoBehaviour
{
    [Header("--- THÔNG TIN LINH THỦ (PET STATS) ---")]
    [SerializeField] private string tenPet = "Hỏa Hồ Tinh";
    [SerializeField] private float sátThươngGốc = 30f;
    [SerializeField] private float sátThương = 30f;
    [Tooltip("Số lần đánh trong 1 giây")]
    [SerializeField] private float tốcĐộĐánh = 1.5f;
    [SerializeField] private int cấpĐộ = 1;
    [SerializeField] private int capDoToiDa = 10;
    [SerializeField] private string hệNguHanh = "Hỏa";

    [Header("--- TỌA ĐỘ & DI CHUYỂN ---")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float bánKínhQuétPlayer = 50f;
    [SerializeField] private float tốcĐộTheoChân = 4f;
    [SerializeField] private float khoảngCáchQuáXaPlayer = 8f;
    [SerializeField] private float tốcĐộTăngTốcRượtPlayer = 8f;
    [SerializeField] private float tốcĐộRượtQuái = 5f;
    [SerializeField] private float khoảngCáchDừngTheoPlayer = 1.5f;
    [SerializeField] private float khoảngCáchDừngĐánhQuái = 1.2f;

    [Header("--- TẤN CÔNG & QUAN SÁT ---")]
    [SerializeField] private float bánKínhPhátHiệnQuáiGốc = 6f;
    [SerializeField] private float bánKínhPhátHiệnQuái = 6f;
    [SerializeField] private Vector2 offsetVùngTấnCông = Vector2.zero;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private GameObject hitboxTấnCông;
    [SerializeField] private float thờiGianBậtHitbox = 0.2f;

    [Header("--- THỜI GIAN & HIỆU ỨNG NGỦ ---")]
    [SerializeField] private float thờiGianNgủ = 20f;
    [SerializeField] private GameObject visualBinhThuongObject;
    [SerializeField] private GameObject visualNguObject;
    [SerializeField] private GameObject matNguObject;
    [SerializeField] private GameObject hieuUngNguObject;

    [Header("--- ANIMATION PARAMETERS ---")]
    [SerializeField] private string animIsMoving = "isMoving";
    [SerializeField] private string animSleepTrigger = "SleepTrigger";
    [SerializeField] private string animIsSleepIdle = "isSleepIdle";
    [SerializeField] private string animAttackTrigger = "AttackTrigger";
    [SerializeField] private string animAttackSpeedParam = "AttackSpeed";

    private Transform playerTransform;
    private Animator anim;
    private Vector3 vịTríCũPlayer;
    private float đếmNgượcĐứngYên = 0f;
    private float đếmNgượcHồiChiêuĐánh = 0f;
    private Transform mụcTiêuQuái;
    private bool đangTấnCông = false;
    private bool đangTrạngTháiNgủ = false;

    public string TenPet => tenPet;
    public float SátThương => sátThương;
    public int CấpĐộ => cấpĐộ;
    public int CapDoToiDa => capDoToiDa;
    public string HệNguHanh => hệNguHanh;
    public float BánKínhTấnCông => bánKínhPhátHiệnQuái;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (hitboxTấnCông != null) hitboxTấnCông.SetActive(false);
        DatTrangThaiHieuUngNgu(false);
        TimPlayerTheoLayer();

        // 🎯 ĐỌC CẤP ĐỘ TỪ FILE SAVE KHI VÀO TRẬN
        LoadCapDoTuSaveSystem();
    }

    public void LoadCapDoTuSaveSystem()
    {
        if (QuestSaveSystem.Instance != null)
        {
            cấpĐộ = QuestSaveSystem.Instance.LayCapDoPet();
        }
        else
        {
            cấpĐộ = 1;
        }
        TinhToanChiSoTheoCapDo();
    }

    public void TinhToanChiSoTheoCapDo()
    {
        float heSo = Mathf.Pow(1.5f, cấpĐộ - 1);
        sátThương = Mathf.Round(sátThươngGốc * heSo);
        bánKínhPhátHiệnQuái = Mathf.Round(bánKínhPhátHiệnQuáiGốc * heSo * 10f) / 10f;
    }

    public List<YeuCauNguyenLieuPet> LayDanhSachNguyenLieuNangCap()
    {
        List<YeuCauNguyenLieuPet> danhSach = new List<YeuCauNguyenLieuPet>();

        if (cấpĐộ >= capDoToiDa) return danhSach;

        int soLuong1 = 10 + (cấpĐộ * 5);
        int soLuong2 = 5 + (cấpĐộ * 2);
        int soLuong3 = 1 + cấpĐộ;

        danhSach.Add(new YeuCauNguyenLieuPet { idItem = "1", soLuongCan = soLuong1 });
        danhSach.Add(new YeuCauNguyenLieuPet { idItem = "2", soLuongCan = soLuong2 });
        danhSach.Add(new YeuCauNguyenLieuPet { idItem = "3", soLuongCan = soLuong3 });

        return danhSach;
    }

    public bool KiemTraDuTatCaNguyenLieu()
    {
        if (QuestSaveSystem.Instance == null) return false;
        var danhSach = LayDanhSachNguyenLieuNangCap();

        foreach (var nl in danhSach)
        {
            int trongKho = QuestSaveSystem.Instance.LaySoLuongItemTrongKho(nl.idItem);
            if (trongKho < nl.soLuongCan) return false;
        }
        return true;
    }

    public bool ThucHienNangCapPet()
    {
        if (cấpĐộ >= capDoToiDa) return false;
        if (QuestSaveSystem.Instance == null) return false;

        if (!KiemTraDuTatCaNguyenLieu()) return false;

        var danhSach = LayDanhSachNguyenLieuNangCap();
        foreach (var nl in danhSach)
        {
            QuestSaveSystem.Instance.LuuItemVaoSaveGame(nl.idItem, -nl.soLuongCan);
        }

        cấpĐộ++;
        TinhToanChiSoTheoCapDo();

        // 🎯 LƯU NGAY CẤP ĐỘ MỚI VÀO FILE SAVE
        QuestSaveSystem.Instance.LuuCapDoPet(cấpĐộ);

        return true;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            TimPlayerTheoLayer();
            return;
        }

        XửLýThờiGianĐứngYênCủaPlayer();

        if (đếmNgượcHồiChiêuĐánh > 0) đếmNgượcHồiChiêuĐánh -= Time.deltaTime;
        if (đangTấnCông) return;

        if (mụcTiêuQuái == null || !mụcTiêuQuái.gameObject.activeInHierarchy)
        {
            QuétTìmQuáiGầnNhất();
        }

        if (mụcTiêuQuái != null && mụcTiêuQuái.gameObject.activeInHierarchy)
        {
            RượtĐuổiVàTấnCôngQuái();
            return;
        }

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
            if (đangTrạngTháiNgủ) DatTrangThaiHieuUngNgu(false);
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
        if (đangTrạngTháiNgủ) DatTrangThaiHieuUngNgu(false);

        float khoảngCáchĐếnQuái = Vector2.Distance(transform.position, mụcTiêuQuái.position);
        LậtMặtTheoHướng(mụcTiêuQuái.position.x);

        if (khoảngCáchĐếnQuái > khoảngCáchDừngĐánhQuái)
        {
            transform.position = Vector2.MoveTowards(transform.position, mụcTiêuQuái.position, tốcĐộRượtQuái * Time.deltaTime);
            SetAnimBool(animIsMoving, true);
            SetAnimBool(animIsSleepIdle, false);
        }
        else
        {
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
            float tốcĐộHiệnTại = (khoảngCáchĐếnPlayer >= khoảngCáchQuáXaPlayer) ? tốcĐộTăngTốcRượtPlayer : tốcĐộTheoChân;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, tốcĐộHiệnTại * Time.deltaTime);
            SetAnimBool(animIsMoving, true);
            SetAnimBool(animIsSleepIdle, false);
            if (đangTrạngTháiNgủ) DatTrangThaiHieuUngNgu(false);
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
                if (đangTrạngTháiNgủ) DatTrangThaiHieuUngNgu(false);
            }
        }
    }

    private IEnumerator Routine_ThựcThiTấnCông()
    {
        đangTấnCông = true;
        float thờiGianHồiChiêu = 1f / Mathf.Max(0.1f, tốcĐộĐánh);
        đếmNgượcHồiChiêuĐánh = thờiGianHồiChiêu;

        SetAnimBool(animIsMoving, false);
        SetAnimBool(animIsSleepIdle, false);

        if (mụcTiêuQuái != null) LậtMặtTheoHướng(mụcTiêuQuái.position.x);

        SetAnimFloat(animAttackSpeedParam, tốcĐộĐánh);
        SetAnimTrigger(animAttackTrigger);

        if (hitboxTấnCông != null) hitboxTấnCông.SetActive(true);

        float thờiGianBậtThựcTế = Mathf.Min(thờiGianBậtHitbox, thờiGianHồiChiêu * 0.5f);
        yield return new WaitForSeconds(thờiGianBậtThựcTế);

        if (hitboxTấnCông != null) hitboxTấnCông.SetActive(false);

        yield return new WaitForSeconds(thờiGianHồiChiêu - thờiGianBậtThựcTế);
        đangTấnCông = false;
    }

    private void DatTrangThaiHieuUngNgu(bool kichHoat)
    {
        đangTrạngTháiNgủ = kichHoat;
        if (!kichHoat) SetAnimBool(animIsSleepIdle, false);

        if (visualBinhThuongObject != null) visualBinhThuongObject.SetActive(!kichHoat);
        if (visualNguObject != null) visualNguObject.SetActive(kichHoat);
        if (matNguObject != null) matNguObject.SetActive(kichHoat);
        if (hieuUngNguObject != null) hieuUngNguObject.SetActive(kichHoat);
    }

    private void LậtMặtTheoHướng(float targetX)
    {
        Vector3 scale = transform.localScale;
        if (targetX > transform.position.x) scale.x = Mathf.Abs(scale.x);
        else if (targetX < transform.position.x) scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void SetAnimBool(string paramName, bool value) => anim?.SetBool(paramName, value);
    private void SetAnimFloat(string paramName, float value) => anim?.SetFloat(paramName, value);
    private void SetAnimTrigger(string paramName) => anim?.SetTrigger(paramName);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(LayTamVungTanCong(), bánKínhPhátHiệnQuái);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, bánKínhQuétPlayer);
    }
}