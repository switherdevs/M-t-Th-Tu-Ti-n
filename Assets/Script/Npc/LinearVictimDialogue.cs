using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Quests;
using GameCore.Settings; // Bổ sung Namespace Settings để đọc ngôn ngữ

[Serializable]
public class CauThoaiTuyenTinhData
{
    [Tooltip("Tên NPC hiển thị")]
    public string tenNPC = "Nạn Nhân";

    [TextArea(3, 5)]
    [Tooltip("Nội dung lời thoại")]
    public string noiDungThoai = "Cứu tôi với!";

    [Tooltip("Text hiển thị trên nút duy nhất này")]
    public string textNut = "Tiếp tục";

    [Header("--- ĐÁNH DẤU KẾT THÚC & HOÀN THÀNH QUEST ---")]
    [Tooltip("TÍCH VÀO ĐÂY nếu đây là câu thoại cuối cùng! Bấm nút này sẽ cứu NPC và hoàn thành Quest.")]
    public bool isNutKetThucHoanThanhQuest = false;
}

// =========================================================
// STRUCT DỮ LIỆU TIẾNG ANH (CHỈ CHỨA VĂN BẢN ĐỂ GHI ĐÈ)
// =========================================================
[Serializable]
public class CauThoaiTuyenTinhTiengAnhData
{
    [Tooltip("Tên NPC Tiếng Anh (Để trống nếu giữ nguyên)")]
    public string tenNPC = "Victim";

    [TextArea(3, 5)]
    [Tooltip("Nội dung lời thoại Tiếng Anh")]
    public string noiDungThoai = "Save me!";

    [Tooltip("Text hiển thị trên nút duy nhất Tiếng Anh (Để trống nếu giữ nguyên)")]
    public string textNut = "Continue";
}

public class LinearVictimDialogue : MonoBehaviour
{
    [Header("--- CẤU HÌNH DATA QUEST ---")]
    [Tooltip("Kéo File QuestData (Nhiệm vụ giải cứu) vào đây")]
    [SerializeField] private QuestData questGiaiCuuData;

    [Header("--- THÀNH PHẦN UI ---")]
    [SerializeField] private GameObject uiThoaiRootObject;
    [SerializeField] private TextMeshProUGUI txtTenNPC;
    [SerializeField] private TextMeshProUGUI txtNoiDungThoai;

    [Header("--- NÚT BẤM DUY NHẤT ---")]
    [SerializeField] private Button btnLuaChon;
    [SerializeField] private TextMeshProUGUI txtNut;

    [Header("--- CẤU HÌNH GÕ CHỮ & ÂM THANH ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amThanhThoai;
    [SerializeField] private float tocDoGoChu = 0.03f;

    [Header("--- DANH SÁCH CÂU THOẠI TUYẾN TÍNH (TIẾNG VIỆT - MẶC ĐỊNH) ---")]
    [SerializeField] private List<CauThoaiTuyenTinhData> danhSachCauThoai = new List<CauThoaiTuyenTinhData>();

    [Header("--- DANH SÁCH CÂU THOẠI TUYẾN TÍNH (TIẾNG ANH - OVERRIDE) ---")]
    [Tooltip("Mảng Tiếng Anh tương ứng theo thứ tự Index. Nếu không điền phần tử tương ứng sẽ dùng mặc định Tiếng Việt.")]
    [SerializeField] private List<CauThoaiTuyenTinhTiengAnhData> danhSachCauThoaiTiengAnh = new List<CauThoaiTuyenTinhTiengAnhData>();

    private int indexThoaiHienTai = 0;
    private Coroutine coroutineGoChu;
    private NPCGiaiCuu npcGiaiCuuHienTai;

    public void MoHoiThoaiTuyenTinh(NPCGiaiCuu npcTarget = null)
    {
        npcGiaiCuuHienTai = npcTarget;
        indexThoaiHienTai = 0;

        if (uiThoaiRootObject != null)
        {
            uiThoaiRootObject.SetActive(true);
        }

        PhatAmThoaiOneShot();
        HienThiCauThoaiHienTai();
    }

    private void HienThiCauThoaiHienTai()
    {
        if (danhSachCauThoai == null || indexThoaiHienTai >= danhSachCauThoai.Count)
        {
            DongUiThoai();
            return;
        }

        // 1. Lấy dữ liệu Tiếng Việt mặc định
        CauThoaiTuyenTinhData dataGoc = danhSachCauThoai[indexThoaiHienTai];

        string tenHienThi = dataGoc.tenNPC;
        string noiDungHienThi = dataGoc.noiDungThoai;
        string textNutHienThi = dataGoc.textNut;

        // 2. Kiểm tra Cài đặt Ngôn ngữ & Ghi đè Tiếng Anh nếu có
        bool isEN = SettingsManager.Instance != null && SettingsManager.Instance.IsEnglish();
        if (isEN && danhSachCauThoaiTiengAnh != null && indexThoaiHienTai < danhSachCauThoaiTiengAnh.Count)
        {
            CauThoaiTuyenTinhTiengAnhData dataEN = danhSachCauThoaiTiengAnh[indexThoaiHienTai];
            if (dataEN != null)
            {
                if (!string.IsNullOrEmpty(dataEN.tenNPC)) tenHienThi = dataEN.tenNPC;
                if (!string.IsNullOrEmpty(dataEN.noiDungThoai)) noiDungHienThi = dataEN.noiDungThoai;
                if (!string.IsNullOrEmpty(dataEN.textNut)) textNutHienThi = dataEN.textNut;
            }
        }

        if (txtTenNPC != null) txtTenNPC.text = tenHienThi;

        StartGoChuRoutine(noiDungHienThi);

        if (btnLuaChon != null)
        {
            btnLuaChon.gameObject.SetActive(true);
            if (txtNut != null) txtNut.text = textNutHienThi;

            btnLuaChon.onClick.RemoveAllListeners();
            btnLuaChon.onClick.AddListener(ChuyenCauThoaiKeTiep);
        }
    }

    private void ChuyenCauThoaiKeTiep()
    {
        // Luôn sử dụng dữ liệu mảng gốc (Tiếng Việt) để kiểm tra logic Quest
        CauThoaiTuyenTinhData dataHienTai = danhSachCauThoai[indexThoaiHienTai];

        if (dataHienTai.isNutKetThucHoanThanhQuest)
        {
            XuLyHoanThanhQuestVaLuuGame();

            if (npcGiaiCuuHienTai != null)
            {
                npcGiaiCuuHienTai.ThucHienGiaiCuuHoanThanh();
            }

            DongUiThoai();
            return;
        }

        indexThoaiHienTai++;
        if (indexThoaiHienTai < danhSachCauThoai.Count)
        {
            HienThiCauThoaiHienTai();
        }
        else
        {
            DongUiThoai();
        }
    }

    private void XuLyHoanThanhQuestVaLuuGame()
    {
        if (questGiaiCuuData != null)
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteOrAbandonQuest(questGiaiCuuData.idQuest);
            }

            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questGiaiCuuData.idQuest, TrangThaiQuest.DaXongChuaTra);
                Debug.Log($"<color=green>[Nạn Nhân]</color> Thoại kết thúc! Quest ID {questGiaiCuuData.idQuest} đã chuyển sang DaXongChuaTra.");

                QuestHUDTracker.ThongBaoCapNhatHUD();
            }
        }
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

    public void DongUiThoai()
    {
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