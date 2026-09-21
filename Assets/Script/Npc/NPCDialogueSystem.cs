using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using GameCore.Quests;
using GameCore.Settings;

public enum HanhDongLuaChon
{
    ChuyenThoaiKeTiep,
    MoNhiemVu,
    NhanQuest,
    DongThoai
}

[Serializable]
public class LuaChonUiData
{
    public string textNut = "Tiếp tục";
    public HanhDongLuaChon hanhDong = HanhDongLuaChon.ChuyenThoaiKeTiep;
    public int idThoaiTiepTheo = 0;
    public QuestData questDataToAccept;
}

[Serializable]
public class CauThoaiData
{
    public int idThoai = 0;
    public string tenNPC = "Ông lão tìm cháu";

    [TextArea(3, 5)]
    public string noiDungThoai = "Chào đạo hữu!";

    public bool suDungNut1 = true;
    public LuaChonUiData luaChon1 = new LuaChonUiData();

    public bool suDungNut2 = false;
    public LuaChonUiData luaChon2 = new LuaChonUiData();
}

[Serializable]
public class CauThoaiTiengAnhData
{
    [Tooltip("ID thoại phải trùng khớp với idThoai bên mảng Tiếng Việt")]
    public int idThoai = 0;
    public string tenNPC = "Old Man";

    [TextArea(3, 5)]
    public string noiDungThoai = "Greetings, cultivator!";

    [Tooltip("Ghi đè text nút 1 (Để trống nếu giữ nguyên)")]
    public string textNut1 = "";

    [Tooltip("Ghi đè text nút 2 (Để trống nếu giữ nguyên)")]
    public string textNut2 = "";
}

public class NPCDialogueSystem : MonoBehaviour
{
    [Header("--- THÀNH PHẦN UI CỐ ĐỊNH ---")]
    [SerializeField] private GameObject uiThoaiRootObject;
    [SerializeField] private TextMeshProUGUI txtTenNPC;
    [SerializeField] private TextMeshProUGUI txtNoiDungThoai;

    [Header("--- 2 BUTTON LỰA CHỌN CỐ ĐỊNH ---")]
    [SerializeField] private Button btnLuaChon1;
    [SerializeField] private TextMeshProUGUI txtNut1;
    [SerializeField] private Button btnLuaChon2;
    [SerializeField] private TextMeshProUGUI txtNut2;

    [Header("--- CẤU HÌNH ÂM THANH & TỐC ĐỘ GÕ CHỮ ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amThanhThoai;
    [SerializeField] private float tocDoGoChu = 0.03f;

    [Tooltip("Khoảng cách tối thiểu giữa 2 lần phát âm thanh (giây) để tránh bị ồn/dồn âm")]
    [SerializeField] private float tanSuatPhatAm = 0.08f;

    [Header("--- THOẠI KHI ĐÃ NHẬN QUEST / HOÀN THÀNH (TIẾNG VIỆT) ---")]
    [SerializeField] private QuestData questKiemTra;
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiDaNhanQuest = "Đại hiệp hãy giúp tôi hoàn thành nhiệm vụ nhanh nhé!";
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiCamOnHoanThanh = "Cảm ơn ơn trên! Đại hiệp đã cứu nguy cho thôn làng chúng tôi!";

    [Header("--- THOẠI TẠM BIỆT (TIẾNG VIỆT) ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiTamBiet = "Hẹn gặp lại đại hiệp sau!";

    [Header("--- BỔ SUNG: THOẠI ĐẶC BIỆT (TIẾNG ANH) ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiDaNhanQuestEN = "Please help me complete the quest quickly!";
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiCamOnHoanThanhEN = "Thank you! You saved our village!";
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiTamBietEN = "See you again later!";

    [Header("--- CẤU HÌNH CHUYỂN MAP & FADE OUT ---")]
    [SerializeField] private bool chuyenMapKhiTamBiet = false;
    [SerializeField] private string tenSceneChuyenDen = "KinhThanh";
    [SerializeField] private Image imgFadeScreen;
    [SerializeField] private float tocDoFadeOut = 1.0f;
    [SerializeField] private float thoiGianDelayChuyenScene = 0.5f;

    [Header("--- CẤU HÌNH THOẠI CUỐI (FINAL DIALOGUE & ENDING) ---")]
    [Tooltip("Đánh dấu đây là cuộc hội thoại cuối game để rẽ nhánh Good / Bad Ending")]
    [SerializeField] private bool isFinalDialogue = false;
    [Tooltip("Danh sách các Quest phụ bắt buộc phải hoàn thành để đạt Good Ending")]
    [SerializeField] private List<QuestData> danhSachQuestPhuYeuCau = new List<QuestData>();
    [SerializeField] private string tenSceneGoodEnding = "GoodEndingScene";
    [SerializeField] private string tenSceneBadEnding = "BadEndingScene";

    [Header("--- DANH SÁCH CÂU THOẠI NPC (TIẾNG VIỆT - MẶC ĐỊNH) ---")]
    [SerializeField] private List<CauThoaiData> danhSachCauThoai = new List<CauThoaiData>();

    [Header("--- DANH SÁCH CÂU THOẠI NPC (TIẾNG ANH - OVERRIDE) ---")]
    [SerializeField] private List<CauThoaiTiengAnhData> danhSachCauThoaiTiengAnh = new List<CauThoaiTiengAnhData>();

    private Dictionary<int, CauThoaiData> dictionaryCauThoai;
    private Dictionary<int, CauThoaiTiengAnhData> dictionaryCauThoaiTiengAnh;
    private bool dangTrongTrangThaiTamBiet = false;
    private Coroutine coroutineGoChu;
    private float thoiGianPhatAmCuoi = 0f;

    private void Awake()
    {
        KhoiTaoDictionaryThoai();

        if (imgFadeScreen != null)
        {
            Color initColor = imgFadeScreen.color;
            initColor.a = 0f;
            imgFadeScreen.color = initColor;
            imgFadeScreen.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (danhSachCauThoai != null && danhSachCauThoai.Count > 0)
        {
            MoHoiThoai(danhSachCauThoai[0].idThoai);
        }
    }

    private void OnDisable()
    {
        DungToanBoGoChuVaAmThanh();
    }

    public void KhoiTaoDictionaryThoai()
    {
        dictionaryCauThoai = new Dictionary<int, CauThoaiData>();
        if (danhSachCauThoai != null)
        {
            foreach (var cauThoai in danhSachCauThoai)
            {
                if (cauThoai != null && !dictionaryCauThoai.ContainsKey(cauThoai.idThoai))
                {
                    dictionaryCauThoai.Add(cauThoai.idThoai, cauThoai);
                }
            }
        }

        dictionaryCauThoaiTiengAnh = new Dictionary<int, CauThoaiTiengAnhData>();
        if (danhSachCauThoaiTiengAnh != null)
        {
            foreach (var cauThoaiEN in danhSachCauThoaiTiengAnh)
            {
                if (cauThoaiEN != null && !dictionaryCauThoaiTiengAnh.ContainsKey(cauThoaiEN.idThoai))
                {
                    dictionaryCauThoaiTiengAnh.Add(cauThoaiEN.idThoai, cauThoaiEN);
                }
            }
        }
    }

    public void MoHoiThoai(int idThoaiBatDau)
    {
        if (dictionaryCauThoai == null || dictionaryCauThoai.Count == 0)
        {
            KhoiTaoDictionaryThoai();
        }

        dangTrongTrangThaiTamBiet = false;

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(true);
        }

        if (KiemTraQuestDaHoanThanh())
        {
            HienThiThoaiCamOnHoanThanh();
            return;
        }

        if (KiemTraDaNhanQuestChua())
        {
            HienThiThoaiDaNhanQuest();
            return;
        }

        HienThiCauThoaiTheoID(idThoaiBatDau);
    }

    private TrangThaiQuest LayTrangThaiQuestSave(int idQuest)
    {
        if (QuestSaveSystem.Instance == null) return TrangThaiQuest.ChuaNhan;

        ProgressQuest progress = QuestSaveSystem.Instance.LayTienTrinhQuest(idQuest);
        return progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan;
    }

    private bool KiemTraQuestDaHoanThanh()
    {
        if (questKiemTra == null) return false;
        TrangThaiQuest trangThai = LayTrangThaiQuestSave(questKiemTra.idQuest);
        return trangThai == TrangThaiQuest.DaXongChuaTra || trangThai == TrangThaiQuest.HoanThanh;
    }

    private bool KiemTraDaNhanQuestChua()
    {
        if (questKiemTra == null) return false;
        TrangThaiQuest trangThai = LayTrangThaiQuestSave(questKiemTra.idQuest);
        return trangThai == TrangThaiQuest.DangLam;
    }

    private string LayTenNPCMacDinh()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        if (isEN && danhSachCauThoaiTiengAnh != null && danhSachCauThoaiTiengAnh.Count > 0 && danhSachCauThoaiTiengAnh[0] != null)
        {
            return danhSachCauThoaiTiengAnh[0].tenNPC;
        }

        if (danhSachCauThoai != null && danhSachCauThoai.Count > 0 && danhSachCauThoai[0] != null)
        {
            return danhSachCauThoai[0].tenNPC;
        }

        return "NPC";
    }

    private void HienThiThoaiCamOnHoanThanh()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        if (txtTenNPC != null) txtTenNPC.text = LayTenNPCMacDinh();

        StartGoChuRoutine(isEN ? loiThoaiCamOnHoanThanhEN : loiThoaiCamOnHoanThanh);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = isEN ? "You're welcome" : "Không có gì";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    private void HienThiThoaiDaNhanQuest()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        if (txtTenNPC != null) txtTenNPC.text = LayTenNPCMacDinh();

        StartGoChuRoutine(isEN ? loiThoaiDaNhanQuestEN : loiThoaiDaNhanQuest);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = isEN ? "I'll do it right away" : "Tôi sẽ đi làm ngay";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    public void HienThiCauThoaiTheoID(int id)
    {
        if (dictionaryCauThoai == null || !dictionaryCauThoai.ContainsKey(id))
        {
            ThucHienDongUiThoai();
            return;
        }

        CauThoaiData dataGoc = dictionaryCauThoai[id];

        string tenHienThi = dataGoc.tenNPC;
        string noiDungHienThi = dataGoc.noiDungThoai;
        LuaChonUiData luaChon1HienThi = dataGoc.luaChon1;
        LuaChonUiData luaChon2HienThi = dataGoc.luaChon2;

        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
        if (isEN && dictionaryCauThoaiTiengAnh != null && dictionaryCauThoaiTiengAnh.TryGetValue(id, out CauThoaiTiengAnhData dataEN))
        {
            if (!string.IsNullOrEmpty(dataEN.tenNPC)) tenHienThi = dataEN.tenNPC;
            if (!string.IsNullOrEmpty(dataEN.noiDungThoai)) noiDungHienThi = dataEN.noiDungThoai;

            if (!string.IsNullOrEmpty(dataEN.textNut1))
            {
                luaChon1HienThi = new LuaChonUiData
                {
                    textNut = dataEN.textNut1,
                    hanhDong = dataGoc.luaChon1.hanhDong,
                    idThoaiTiepTheo = dataGoc.luaChon1.idThoaiTiepTheo,
                    questDataToAccept = dataGoc.luaChon1.questDataToAccept
                };
            }

            if (!string.IsNullOrEmpty(dataEN.textNut2))
            {
                luaChon2HienThi = new LuaChonUiData
                {
                    textNut = dataEN.textNut2,
                    hanhDong = dataGoc.luaChon2.hanhDong,
                    idThoaiTiepTheo = dataGoc.luaChon2.idThoaiTiepTheo,
                    questDataToAccept = dataGoc.luaChon2.questDataToAccept
                };
            }
        }

        if (txtTenNPC != null) txtTenNPC.text = tenHienThi;

        StartGoChuRoutine(noiDungHienThi);

        SetupButtonLuaChon(btnLuaChon1, txtNut1, dataGoc.suDungNut1, luaChon1HienThi);
        SetupButtonLuaChon(btnLuaChon2, txtNut2, dataGoc.suDungNut2, luaChon2HienThi);
    }

    private void StartGoChuRoutine(string chuoiVanBan)
    {
        DungToanBoGoChuVaAmThanh();
        coroutineGoChu = StartCoroutine(GoChuCoRoutine(chuoiVanBan));
    }

    private IEnumerator GoChuCoRoutine(string chuoiVanBan)
    {
        if (txtNoiDungThoai == null) yield break;

        txtNoiDungThoai.text = "";

        foreach (char c in chuoiVanBan.ToCharArray())
        {
            txtNoiDungThoai.text += c;

            if (c != ' ' && Time.time - thoiGianPhatAmCuoi >= tanSuatPhatAm)
            {
                PhatAmThoaiCoKiemTra();
                thoiGianPhatAmCuoi = Time.time;
            }

            yield return new WaitForSeconds(tocDoGoChu);
        }

        TieuDungAmThoai();
    }

    private void PhatAmThoaiCoKiemTra()
    {
        if (audioSource != null && amThanhThoai != null)
        {
            audioSource.PlayOneShot(amThanhThoai);
        }
    }

    private void DungToanBoGoChuVaAmThanh()
    {
        if (coroutineGoChu != null)
        {
            StopCoroutine(coroutineGoChu);
            coroutineGoChu = null;
        }
        TieuDungAmThoai();
    }

    private void TieuDungAmThoai()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void SetupButtonLuaChon(Button btn, TextMeshProUGUI txt, bool suDung, LuaChonUiData luaChonData)
    {
        if (btn == null) return;

        if (!suDung || luaChonData == null || string.IsNullOrEmpty(luaChonData.textNut))
        {
            btn.gameObject.SetActive(false);
            return;
        }

        btn.gameObject.SetActive(true);
        if (txt != null) txt.text = luaChonData.textNut;

        btn.onClick.RemoveAllListeners();
        LuaChonUiData tempLuaChon = luaChonData;
        btn.onClick.AddListener(() => XuLyKhiBamNut(tempLuaChon));
    }

    private void XuLyKhiBamNut(LuaChonUiData luaChon)
    {
        if (dangTrongTrangThaiTamBiet)
        {
            ThucHienDongUiThoai();
            return;
        }

        switch (luaChon.hanhDong)
        {
            case HanhDongLuaChon.ChuyenThoaiKeTiep:
            case HanhDongLuaChon.MoNhiemVu:
                HienThiCauThoaiTheoID(luaChon.idThoaiTiepTheo);
                break;

            case HanhDongLuaChon.NhanQuest:
                bool daNhanThanhCong = XuLyNhanQuestVaSave(luaChon.questDataToAccept);
                if (daNhanThanhCong)
                {
                    bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
                    HienThiCauThoaiTamBiet(isEN ? "Thank you! Take care." : "Cảm ơn ngươi! Hãy bảo trọng.");
                }
                break;

            case HanhDongLuaChon.DongThoai:
                HienThiCauThoaiTamBiet(loiThoaiTamBiet);
                break;
        }
    }

    private bool XuLyNhanQuestVaSave(QuestData quest)
    {
        if (quest == null) return false;

        if (QuestManager.Instance != null)
        {
            bool ketQuaNhan = QuestManager.Instance.AcceptQuest(quest);

            if (ketQuaNhan && QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.CapNhatTrangThaiQuest(quest.idQuest, TrangThaiQuest.DangLam);
                QuestHUDTracker.ThongBaoCapNhatHUD();
            }

            return ketQuaNhan;
        }

        return false;
    }

    private void HienThiCauThoaiTamBiet(string loiTamBiet)
    {
        dangTrongTrangThaiTamBiet = true;

        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
        string noiDungTamBiet = isEN ? loiThoaiTamBietEN : loiTamBiet;

        StartGoChuRoutine(noiDungTamBiet);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = isEN ? "Goodbye" : "Tạm biệt";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    /// <summary>
    /// Kiểm tra xem người chơi đã hoàn tất toàn bộ danh sách Quest phụ yêu cầu hay chưa
    /// </summary>
    private bool KiemTraDaHoanThanhHetQuestPhu()
    {
        if (danhSachQuestPhuYeuCau == null || danhSachQuestPhuYeuCau.Count == 0) return true;

        foreach (QuestData quest in danhSachQuestPhuYeuCau)
        {
            if (quest == null) continue;

            TrangThaiQuest trangThai = LayTrangThaiQuestSave(quest.idQuest);
            if (trangThai != TrangThaiQuest.HoanThanh)
            {
                return false; // Chỉ cần 1 quest chưa hoàn thành -> Không đủ điều kiện Good Ending
            }
        }

        return true;
    }

    public void ThucHienDongUiThoai()
    {
        dangTrongTrangThaiTamBiet = false;

        DungToanBoGoChuVaAmThanh();

        // 🎯 KIỂM TRA NẾU LÀ THOẠI CUỐI GAME (FINAL DIALOGUE)
        if (isFinalDialogue)
        {
            bool daXongHetQuestPhu = KiemTraDaHoanThanhHetQuestPhu();
            string sceneTarget = daXongHetQuestPhu ? tenSceneGoodEnding : tenSceneBadEnding;

            Debug.Log($"<color=yellow>[Ending Check]</color> Đã hoàn thành hết quest phụ: {daXongHetQuestPhu}. Chuyển sang Scene: {sceneTarget}");

            if (imgFadeScreen != null)
            {
                StartCoroutine(FadeOutAndChangeScene(sceneTarget));
            }
            else
            {
                if (uiThoaiRootObject != null) uiThoaiRootObject.SetActive(false);
                SceneManager.LoadScene(sceneTarget);
            }
            return;
        }

        // 🎯 NẾU KHÔNG PHẢI FINAL THOẠI -> CHẠY LUỒNG THƯỜNG
        if (chuyenMapKhiTamBiet)
        {
            if (!string.IsNullOrEmpty(tenSceneChuyenDen))
            {
                if (imgFadeScreen != null)
                {
                    StartCoroutine(FadeOutAndChangeScene(tenSceneChuyenDen));
                }
                else
                {
                    if (uiThoaiRootObject != null) uiThoaiRootObject.SetActive(false);
                    Debug.Log("<color=green>[NPC Dialogue]</color> Chuyển thẳng sang Scene: " + tenSceneChuyenDen);
                    SceneManager.LoadScene(tenSceneChuyenDen);
                }
            }
            else
            {
                Debug.LogError("[NPC Dialogue] Đã tích 'Chuyen Map Khi Tam Biet' nhưng chưa điền 'Ten Scene Chuyen Den'!");
            }
        }
        else
        {
            if (uiThoaiRootObject != null)
            {
                uiThoaiRootObject.SetActive(false);
            }
        }
    }

    private IEnumerator FadeOutAndChangeScene(string targetScene)
    {
        imgFadeScreen.gameObject.SetActive(true);

        if (imgFadeScreen.sprite == null)
        {
            Texture2D whiteTexture = Texture2D.whiteTexture;
            imgFadeScreen.sprite = Sprite.Create(whiteTexture, new Rect(0, 0, whiteTexture.width, whiteTexture.height), new Vector2(0.5f, 0.5f));
        }

        Color mauNen = Color.black;
        mauNen.a = 0f;
        imgFadeScreen.color = mauNen;

        while (mauNen.a < 1.0f)
        {
            mauNen.a += tocDoFadeOut * Time.deltaTime;
            mauNen.a = Mathf.Clamp01(mauNen.a);
            imgFadeScreen.color = mauNen;

            yield return null;
        }

        if (thoiGianDelayChuyenScene > 0f)
        {
            yield return new WaitForSeconds(thoiGianDelayChuyenScene);
        }

        Debug.Log("<color=cyan>[NPC Dialogue]</color> Fade Out hoàn tất! Đang chuyển sang Scene: " + targetScene);
        SceneManager.LoadScene(targetScene);
    }
}