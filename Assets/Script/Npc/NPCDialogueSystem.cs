using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("--- THOẠI KHI ĐÃ NHẬN QUEST / HOÀN THÀNH ---")]
    [Tooltip("Nhiệm vụ cần kiểm tra xem người chơi đã nhận hoặc xong chưa")]
    [SerializeField] private QuestData questKiemTra;
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiDaNhanQuest = "Đại hiệp hãy giúp tôi hoàn thành nhiệm vụ nhanh nhé!";
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiCamOnHoanThanh = "Cảm ơn ơn trên! Đại hiệp đã cứu nguy cho thôn làng chúng tôi!";

    [Header("--- THOẠI TẠM BIỆT ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiTamBiet = "Hẹn gặp lại đại hiệp sau!";

    [Header("--- DANH SÁCH CÂU THOẠI NPC ---")]
    [SerializeField] private List<CauThoaiData> danhSachCauThoai = new List<CauThoaiData>();

    private Dictionary<int, CauThoaiData> dictionaryCauThoai;
    private bool dangTrongTrangThaiTamBiet = false;
    private Coroutine coroutineGoChu;

    private void Awake()
    {
        KhoiTaoDictionaryThoai();
    }

    private void OnEnable()
    {
        // Khi GameObject được Bật/Mở lên, tự động chạy hội thoại ban đầu
        if (danhSachCauThoai != null && danhSachCauThoai.Count > 0)
        {
            MoHoiThoai(danhSachCauThoai[0].idThoai);
        }
    }

    public void KhoiTaoDictionaryThoai()
    {
        dictionaryCauThoai = new Dictionary<int, CauThoaiData>();
        if (danhSachCauThoai == null) return;

        foreach (var cauThoai in danhSachCauThoai)
        {
            if (cauThoai != null && !dictionaryCauThoai.ContainsKey(cauThoai.idThoai))
            {
                dictionaryCauThoai.Add(cauThoai.idThoai, cauThoai);
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

        PhatAmThoaiOneShot();

        // 1. Kiểm tra nếu Quest đã Hoàn Thành / Đã cứu -> Hiện thoại cảm ơn
        if (KiemTraQuestDaHoanThanh())
        {
            HienThiThoaiCamOnHoanThanh();
            return;
        }

        // 2. Kiểm tra nếu Quest Đang Làm -> Hiện thoại giục làm Quest
        if (KiemTraDaNhanQuestChua())
        {
            HienThiThoaiDaNhanQuest();
            return;
        }

        // 3. Hiển thị câu thoại chuẩn theo ID
        HienThiCauThoaiTheoID(idThoaiBatDau);
    }

    private TrangThaiQuest LayTrangThaiQuestSave()
    {
        if (questKiemTra == null || QuestSaveSystem.Instance == null) return TrangThaiQuest.ChuaNhan;

        // Chuẩn hóa: Đọc chuẩn từ QuestSaveSystem file .txt
        ProgressQuest progress = QuestSaveSystem.Instance.LayTienTrinhQuest(questKiemTra.idQuest);
        return progress != null ? progress.trangThai : TrangThaiQuest.ChuaNhan;
    }

    private bool KiemTraQuestDaHoanThanh()
    {
        TrangThaiQuest trangThai = LayTrangThaiQuestSave();
        return trangThai == TrangThaiQuest.DaXongChuaTra || trangThai == TrangThaiQuest.HoanThanh;
    }

    private bool KiemTraDaNhanQuestChua()
    {
        TrangThaiQuest trangThai = LayTrangThaiQuestSave();
        return trangThai == TrangThaiQuest.DangLam;
    }

    private string LayTenNPCMacDinh()
    {
        if (danhSachCauThoai != null && danhSachCauThoai.Count > 0 && danhSachCauThoai[0] != null)
        {
            return danhSachCauThoai[0].tenNPC;
        }
        return "NPC";
    }

    private void HienThiThoaiCamOnHoanThanh()
    {
        if (txtTenNPC != null) txtTenNPC.text = LayTenNPCMacDinh();

        StartGoChuRoutine(loiThoaiCamOnHoanThanh);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = "Không có gì";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    private void HienThiThoaiDaNhanQuest()
    {
        if (txtTenNPC != null) txtTenNPC.text = LayTenNPCMacDinh();

        StartGoChuRoutine(loiThoaiDaNhanQuest);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = "Tôi sẽ đi làm ngay";

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

        CauThoaiData data = dictionaryCauThoai[id];

        if (txtTenNPC != null) txtTenNPC.text = data.tenNPC;

        StartGoChuRoutine(data.noiDungThoai);

        SetupButtonLuaChon(btnLuaChon1, txtNut1, data.suDungNut1, data.luaChon1);
        SetupButtonLuaChon(btnLuaChon2, txtNut2, data.suDungNut2, data.luaChon2);
    }

    private void StartGoChuRoutine(string chuoiVanBan)
    {
        if (coroutineGoChu != null)
        {
            StopCoroutine(coroutineGoChu);
        }
        coroutineGoChu = StartCoroutine(GoChuCoRoutine(chuoiVanBan));
    }

    private IEnumerator GoChuCoRoutine(string chuoiVanBan)
    {
        if (txtNoiDungThoai == null) yield break;

        txtNoiDungThoai.text = "";
        foreach (char c in chuoiVanBan.ToCharArray())
        {
            txtNoiDungThoai.text += c;
            yield return new WaitForSeconds(tocDoGoChu);
        }
    }

    private void PhatAmThoaiOneShot()
    {
        if (audioSource != null && amThanhThoai != null)
        {
            audioSource.PlayOneShot(amThanhThoai);
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
                HienThiCauThoaiTamBiet("Cảm ơn ngươi! Hãy bảo trọng.");
                break;

            case HanhDongLuaChon.DongThoai:
                HienThiCauThoaiTamBiet(loiThoaiTamBiet);
                break;
        }
    }

    private void XuLyNhanQuestVaSave(QuestData quest)
    {
        if (quest != null)
        {
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.CapNhatTrangThaiQuest(quest.idQuest, TrangThaiQuest.DangLam);
            }

            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
            }
        }
    }

    private void HienThiCauThoaiTamBiet(string loiTamBiet)
    {
        dangTrongTrangThaiTamBiet = true;

        StartGoChuRoutine(loiTamBiet);

        if (btnLuaChon1 != null)
        {
            btnLuaChon1.gameObject.SetActive(true);
            if (txtNut1 != null) txtNut1.text = "Tạm biệt";

            btnLuaChon1.onClick.RemoveAllListeners();
            btnLuaChon1.onClick.AddListener(ThucHienDongUiThoai);
        }

        if (btnLuaChon2 != null) btnLuaChon2.gameObject.SetActive(false);
    }

    public void ThucHienDongUiThoai()
    {
        dangTrongTrangThaiTamBiet = false;

        if (coroutineGoChu != null)
        {
            StopCoroutine(coroutineGoChu);
        }

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(false);
        }
    }
}