using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum HanhDongLuaChon
{
    ChuyenThoaiKeTiep, // Chuyển đến câu thoại có idThoaiTiepTheo
    MoNhiemVu,         // Chuyển tới thoại mô tả Quest
    NhanQuest,         // Đồng ý nhận Quest và Save game
    DongThoai          // Nói câu tạm biệt rồi tắt UI gốc
}

[Serializable]
public class LuaChonUiData
{
    [Tooltip("Text hiển thị riêng cho nút bấm này")]
    public string textNut = "Tiếp tục";

    [Tooltip("Hành động khi bấm nút")]
    public HanhDongLuaChon hanhDong = HanhDongLuaChon.ChuyenThoaiKeTiep;

    [Tooltip("ID của câu thoại tiếp theo (Nếu chọn ChuyenThoaiKeTiep hoặc MoNhiemVu)")]
    public int idThoaiTiepTheo = 0;

    [Tooltip("Gán QuestData trực tiếp vào đây nếu hành động là NhanQuest")]
    public QuestData questDataToAccept;
}

[Serializable]
public class CauThoaiData
{
    [Tooltip("ID duy nhất của câu thoại này trong mảng")]
    public int idThoai = 0;

    [Tooltip("Tên hiển thị của NPC")]
    public string tenNPC = "Lão Trưởng Làng";

    [TextArea(3, 5)]
    [Tooltip("Nội dung câu thoại")]
    public string noiDungThoai = "Chào đạo hữu!";

    [Header("--- CẤU HÌNH 2 NÚT LỰA CHỌN ---")]
    public bool suDungNut1 = true;
    public LuaChonUiData luaChon1 = new LuaChonUiData();

    public bool suDungNut2 = false;
    public LuaChonUiData luaChon2 = new LuaChonUiData();
}

public class NPCDialogueSystem : MonoBehaviour
{
    [Header("--- THÀNH PHẦN UI CỐ ĐỊNH (KÉO VÀO INSPECTOR) ---")]
    [Tooltip("GameObject con chứa khung thoại (KHÔNG kéo chính Canvas chứa script vào đây)")]
    [SerializeField] private GameObject uiThoaiRootObject;

    [SerializeField] private TextMeshProUGUI txtTenNPC;
    [SerializeField] private TextMeshProUGUI txtNoiDungThoai;

    [Header("--- 2 BUTTON LỰA CHỌN CỐ ĐỊNH ---")]
    [SerializeField] private Button btnLuaChon1;
    [SerializeField] private TextMeshProUGUI txtNut1;
    [SerializeField] private Button btnLuaChon2;
    [SerializeField] private TextMeshProUGUI txtNut2;

    [Header("--- THOẠI TẠM BIỆT (KHI ĐÓNG THOẠI) ---")]
    [TextArea(2, 4)]
    [SerializeField] private string loiThoaiTamBiet = "Hẹn gặp lại đại hiệp sau!";

    [Header("--- DANH SÁCH CÂU THOẠI NPC (CẤU HÌNH IN INSPECTOR) ---")]
    [SerializeField] private List<CauThoaiData> danhSachCauThoai = new List<CauThoaiData>();

    private Dictionary<int, CauThoaiData> dictionaryCauThoai;
    private bool dangTrongTrangThaiTamBiet = false;

    private void Awake()
    {
        KhoiTaoDictionaryThoai();
    }

    private void Start()
    {
        // Tự động mở câu thoại đầu tiên để test khi bấm Play Game
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
            if (cauThoai != null)
            {
                if (!dictionaryCauThoai.ContainsKey(cauThoai.idThoai))
                {
                    dictionaryCauThoai.Add(cauThoai.idThoai, cauThoai);
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

        HienThiCauThoaiTheoID(idThoaiBatDau);
    }

    public void HienThiCauThoaiTheoID(int id)
    {
        if (dictionaryCauThoai == null || !dictionaryCauThoai.ContainsKey(id))
        {
            Debug.LogError($"[NPC Dialogue] Không tìm thấy câu thoại ID: {id}");
            ThucHienDongUiThoai();
            return;
        }

        CauThoaiData data = dictionaryCauThoai[id];

        // 1. Cập nhật Text Tên và Lời thoại
        if (txtTenNPC != null) txtTenNPC.text = data.tenNPC;
        if (txtNoiDungThoai != null) txtNoiDungThoai.text = data.noiDungThoai;

        // 2. Cập nhật Nút 1
        SetupButtonLuaChon(btnLuaChon1, txtNut1, data.suDungNut1, data.luaChon1);

        // 3. Cập nhật Nút 2
        SetupButtonLuaChon(btnLuaChon2, txtNut2, data.suDungNut2, data.luaChon2);
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

        if (txtNoiDungThoai != null) txtNoiDungThoai.text = loiTamBiet;

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

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(false);
        }
    }
}