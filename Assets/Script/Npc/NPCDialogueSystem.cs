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
    DongThoai,
    ChuyenMap
}

[Serializable]
public class LuaChonUiData
{
    public string textNut = "Tiếp tục";
    public HanhDongLuaChon hanhDong = HanhDongLuaChon.ChuyenThoaiKeTiep;
    public int idThoaiTiepTheo = 0;
    public QuestData questDataToAccept;

    [Header("--- Cấu hình chuyển map (Dành cho hành động Chuyển Map) ---")]
    [Tooltip("Tên Scene cần chuyển đến khi bấm lựa chọn này")]
    public string tenSceneChuyenDen = "";
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
    [Header("--- CẤU HÌNH MAP OPEN / DỪNG THỜI GIAN ---")]
    [SerializeField, Tooltip("Tích chọn nếu muốn khi bật thoại này, thời gian trong game dừng lại (Time.timeScale = 0) nhưng vẫn tương tác UI và nghe âm thanh bình thường")]
    private bool mapOpen = false;

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

    [Header("--- THOẠI TRẠNG THÁI QUEST (TIẾNG VIỆT) ---")]
    [SerializeField] private QuestData questKiemTra;

    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiDaNhanQuest = "Đại hiệp hãy giúp tôi hoàn thành nhiệm vụ nhanh nhé!";

    [TextArea(2, 4)]
    [Tooltip("NPC hỏi tình hình khi người chơi đã làm xong việc nhưng chưa xác nhận trả quest")]
    [SerializeField] private string loiThoaiHoiTraQuest = "Đại hiệp đã cứu được cháu tôi/hoàn thành việc tôi nhờ chưa?";

    [TextArea(2, 4)]
    [Tooltip("Thoại cảm ơn vĩnh viễn khi đã xác nhận hoàn thành quest")]
    [SerializeField] private string loiThoaiCamOnHoanThanh = "Cảm ơn ơn trên! Đại hiệp đã cứu nguy cho thôn làng chúng tôi!";

    [Header("--- THOẠI TẠM BIỆT (TIẾNG VIỆT) ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiTamBiet = "Hẹn gặp lại đại hiệp sau!";

    [Header("--- BỔ SUNG: THOẠI TRẠNG THÁI QUEST (TIẾNG ANH) ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiDaNhanQuestEN = "Please help me complete the quest quickly!";

    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiHoiTraQuestEN = "Have you completed the task I asked of you, hero?";

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

    private int idThoaiHienTaiDangXem = 0;
    private bool dangMoThoai = false; // 🎯 Cờ kiểm soát trạng thái hội thoại

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
        SettingsManager.OnLanguageChanged += HandleLanguageChanged;

        if (danhSachCauThoai != null && danhSachCauThoai.Count > 0)
        {
            MoHoiThoai(danhSachCauThoai[0].idThoai);
        }
    }

    private void OnDisable()
    {
        SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
        DungToanBoGoChuVaAmThanh();

        // Đảm bảo trả lại timeScale khi tắt script
        if (mapOpen)
        {
            Time.timeScale = 1f;
        }
        dangMoThoai = false;
    }

    // 🎯 Ép cứng Time.timeScale = 0 liên tục qua mỗi frame khi UI thoại đang bật
    private void Update()
    {
        if (mapOpen && dangMoThoai)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }
    }

    private void HandleLanguageChanged(string newLang)
    {
        if (uiThoaiRootObject != null && uiThoaiRootObject.activeSelf)
        {
            if (KiemTraQuestDaHoanThanhHoanToan())
            {
                HienThiThoaiCamOnHoanThanh();
            }
            else if (KiemTraQuestDaXongChuaTra())
            {
                HienThiThoaiHoiTraQuest();
            }
            else if (KiemTraDaNhanQuestChua())
            {
                HienThiThoaiDaNhanQuest();
            }
            else if (dangTrongTrangThaiTamBiet)
            {
                HienThiCauThoaiTamBiet(loiThoaiTamBiet);
            }
            else
            {
                HienThiCauThoaiTheoID(idThoaiHienTaiDangXem);
            }
        }
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
        dangMoThoai = true; // 🎯 Bật cờ đánh dấu đang mở thoại

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(true);
        }

        if (KiemTraQuestDaHoanThanhHoanToan())
        {
            HienThiThoaiCamOnHoanThanh();
            return;
        }

        if (KiemTraQuestDaXongChuaTra())
        {
            HienThiThoaiHoiTraQuest();
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

    private bool KiemTraQuestDaHoanThanhHoanToan()
    {
        if (questKiemTra == null) return false;
        TrangThaiQuest trangThai = LayTrangThaiQuestSave(questKiemTra.idQuest);
        return trangThai == TrangThaiQuest.HoanThanh;
    }

    private bool KiemTraQuestDaXongChuaTra()
    {
        if (questKiemTra == null) return false;
        TrangThaiQuest trangThai = LayTrangThaiQuestSave(questKiemTra.idQuest);
        return trangThai == TrangThaiQuest.DaXongChuaTra;
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
            if (txtNut1 != null) txtNut1.text = isEN ? "Goodbye" : "Tạm biệt";

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
            if (txtNut1 != null) txtNut1.text = isEN ? "I'll keep working on it" : "Tôi sẽ đi làm ngay";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    private void HienThiThoaiHoiTraQuest()
    {
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();

        if (txtTenNPC != null) txtTenNPC.text = LayTenNPCMacDinh();

        string noiDungHoi = isEN ? loiThoaiHoiTraQuestEN : loiThoaiHoiTraQuest;
        StartGoChuRoutine(noiDungHoi);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = isEN ? "I have completed the task!" : "Ta đã hoàn thành nhiệm vụ!";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(() =>
            {
                XuLyXacNhanTraQuest(questKiemTra);
                HienThiThoaiCamOnHoanThanh();
            });
        }

        if (btnLuaChon2 != null)
        {
            btnLuaChon2.gameObject.SetActive(true);
            if (txtNut2 != null) txtNut2.text = isEN ? "Not yet" : "Để sau nhé";

            btnLuaChon2.onClick.RemoveAllListeners();
            btnLuaChon2.onClick.AddListener(ThucHienDongUiThoai);
        }
    }

    public void HienThiCauThoaiTheoID(int id)
    {
        if (dictionaryCauThoai == null || !dictionaryCauThoai.ContainsKey(id))
        {
            ThucHienDongUiThoai();
            return;
        }

        idThoaiHienTaiDangXem = id;
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
                    questDataToAccept = dataGoc.luaChon1.questDataToAccept,
                    tenSceneChuyenDen = dataGoc.luaChon1.tenSceneChuyenDen
                };
            }

            if (!string.IsNullOrEmpty(dataEN.textNut2))
            {
                luaChon2HienThi = new LuaChonUiData
                {
                    textNut = dataEN.textNut2,
                    hanhDong = dataGoc.luaChon2.hanhDong,
                    idThoaiTiepTheo = dataGoc.luaChon2.idThoaiTiepTheo,
                    questDataToAccept = dataGoc.luaChon2.questDataToAccept,
                    tenSceneChuyenDen = dataGoc.luaChon2.tenSceneChuyenDen
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

            if (c != ' ' && Time.realtimeSinceStartup - thoiGianPhatAmCuoi >= tanSuatPhatAm)
            {
                PhatAmThoaiCoKiemTra();
                thoiGianPhatAmCuoi = Time.realtimeSinceStartup;
            }

            yield return new WaitForSecondsRealtime(tocDoGoChu);
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
                XuLyNhanQuestVaSave(luaChon.questDataToAccept);
                HienThiThoaiDaNhanQuest();
                break;

            case HanhDongLuaChon.DongThoai:
                HienThiCauThoaiTamBiet(loiThoaiTamBiet);
                break;

            case HanhDongLuaChon.ChuyenMap:
                XuLyChuyenMapTrucTiep(luaChon.tenSceneChuyenDen);
                break;
        }
    }

    private void XuLyChuyenMapTrucTiep(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[NPC Dialogue] Hành động 'Chuyển Map' được chọn nhưng chưa điền tên Scene chuyển đến!");
            ThucHienDongUiThoai();
            return;
        }

        DungToanBoGoChuVaAmThanh();

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(false);
        }

        // 🎯 Tắt cờ và trả timeScale về 1 trước khi chuyển map
        dangMoThoai = false;
        if (mapOpen)
        {
            Time.timeScale = 1f;
        }

        if (imgFadeScreen != null)
        {
            StartCoroutine(FadeOutAndChangeScene(targetScene));
        }
        else
        {
            Debug.Log("<color=green>[NPC Dialogue]</color> Chuyển thẳng sang Scene: " + targetScene);
            SceneManager.LoadScene(targetScene);
        }
    }

    private bool XuLyNhanQuestVaSave(QuestData quest)
    {
        if (quest == null) return false;

        if (questKiemTra == null)
        {
            questKiemTra = quest;
        }

        if (QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.CapNhatTrangThaiQuest(quest.idQuest, TrangThaiQuest.DangLam);
            QuestHUDTracker.ThongBaoCapNhatHUD();
        }

        if (QuestManager.Instance != null)
        {
            return QuestManager.Instance.AcceptQuest(quest);
        }

        return true;
    }

    private void XuLyXacNhanTraQuest(QuestData quest)
    {
        QuestData targetQuest = quest != null ? quest : questKiemTra;
        if (targetQuest == null) return;

        targetQuest.LuuPhanThuongVaoSaveGame();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteOrAbandonQuest(targetQuest.idQuest);
        }

        if (QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.CapNhatTrangThaiQuest(targetQuest.idQuest, TrangThaiQuest.HoanThanh);
            Debug.Log($"<color=green>[NPC Dialogue]</color> Đã xác nhận trả nhiệm vụ ID {targetQuest.idQuest} thành công!");
            QuestHUDTracker.ThongBaoCapNhatHUD();
        }
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

    private bool KiemTraDaHoanThanhHetQuestPhu()
    {
        if (danhSachQuestPhuYeuCau == null || danhSachQuestPhuYeuCau.Count == 0) return true;

        foreach (QuestData quest in danhSachQuestPhuYeuCau)
        {
            if (quest == null) continue;

            TrangThaiQuest trangThai = LayTrangThaiQuestSave(quest.idQuest);
            if (trangThai != TrangThaiQuest.HoanThanh)
            {
                return false;
            }
        }

        return true;
    }

    public void ThucHienDongUiThoai()
    {
        dangTrongTrangThaiTamBiet = false;
        DungToanBoGoChuVaAmThanh();

        // 🎯 Tắt cờ và trả timeScale về 1 ngay lập tức khi đóng thoại
        dangMoThoai = false;
        if (mapOpen)
        {
            Time.timeScale = 1f;
        }

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
            mauNen.a += tocDoFadeOut * Time.unscaledDeltaTime;
            mauNen.a = Mathf.Clamp01(mauNen.a);
            imgFadeScreen.color = mauNen;

            yield return null;
        }

        if (thoiGianDelayChuyenScene > 0f)
        {
            yield return new WaitForSecondsRealtime(thoiGianDelayChuyenScene);
        }

        Debug.Log("<color=cyan>[NPC Dialogue]</color> Fade Out hoàn tất! Đang chuyển sang Scene: " + targetScene);
        SceneManager.LoadScene(targetScene);
    }
}