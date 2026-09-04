using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("--- DANH SÁCH CÂU THOẠI TUYẾN TÍNH ---")]
    [SerializeField] private List<CauThoaiTuyenTinhData> danhSachCauThoai = new List<CauThoaiTuyenTinhData>();

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

        CauThoaiTuyenTinhData data = danhSachCauThoai[indexThoaiHienTai];

        if (txtTenNPC != null) txtTenNPC.text = data.tenNPC;

        StartGoChuRoutine(data.noiDungThoai);

        if (btnLuaChon != null)
        {
            btnLuaChon.gameObject.SetActive(true);
            if (txtNut != null) txtNut.text = data.textNut;

            btnLuaChon.onClick.RemoveAllListeners();
            btnLuaChon.onClick.AddListener(ChuyenCauThoaiKeTiep);
        }
    }

    private void ChuyenCauThoaiKeTiep()
    {
        CauThoaiTuyenTinhData dataHienTai = danhSachCauThoai[indexThoaiHienTai];

        // Nếu là câu thoại cuối cùng đánh dấu hoàn thành Quest
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
        if (questGiaiCuuData != null && QuestSaveSystem.Instance != null)
        {
            QuestSaveSystem.Instance.CapNhatTrangThaiQuest(questGiaiCuuData.idQuest, TrangThaiQuest.DaXongChuaTra);
            Debug.Log($"<color=green>[Nạn Nhân]</color> Thoại kết thúc! Quest ID {questGiaiCuuData.idQuest} đã chuyển sang DaXongChuaTra.");

            if (QuestUIManager.Instance != null)
            {
                QuestUIManager.Instance.KhoiTaoDanhSachQuestUI();
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