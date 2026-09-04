using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestHUDTracker : MonoBehaviour
{
    public static QuestHUDTracker Instance;

    [Header("--- THÔNG BÁO MẶC ĐỊNH ---")]
    public TextMeshProUGUI textThongBaoMacDinh;
    public string noiDungThongBaoMacDinh = "Hãy đến kinh thành nhận nhiệm vụ";
    public string noiDungHoanThanhTatCa = "Cảnh giới bạn đã cao, hãy đập đá";

    [Header("--- MẢNG TEXT HIỂN THỊ QUEST ĐỘNG ---")]
    public TextMeshProUGUI[] danhSachTextQuestUI;

    public static event Action OnQuestProgressChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        OnQuestProgressChanged += CapNhatGiaoDienHUD;
    }

    private void OnDisable()
    {
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
    }

    private IEnumerator Start()
    {
        yield return null;
        CapNhatGiaoDienHUD();
    }

    public static void ThongBaoCapNhatHUD()
    {
        OnQuestProgressChanged?.Invoke();
    }

    public void CapNhatGiaoDienHUD()
    {
        XoaRongToanBoText();

        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null) return;

        List<ProgressQuest> danhSachQuestDangActive = LayToanBoQuestDangKichHoat();

        if (danhSachQuestDangActive.Count == 0)
        {
            if (textThongBaoMacDinh != null)
            {
                textThongBaoMacDinh.gameObject.SetActive(true);
                textThongBaoMacDinh.text = KiemTraDaHoanThanhTatCaQuest() ? noiDungHoanThanhTatCa : noiDungThongBaoMacDinh;
            }
            return;
        }

        if (textThongBaoMacDinh != null) textThongBaoMacDinh.gameObject.SetActive(false);
        if (danhSachTextQuestUI == null || danhSachTextQuestUI.Length == 0) return;

        for (int i = 0; i < danhSachQuestDangActive.Count; i++)
        {
            if (i >= danhSachTextQuestUI.Length) break;
            if (danhSachTextQuestUI[i] == null) continue;

            ProgressQuest progress = danhSachQuestDangActive[i];
            QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(progress.idQuest);

            if (data != null)
            {
                danhSachTextQuestUI[i].gameObject.SetActive(true);

                int soYeuCau = data.loaiQuest == LoaiQuest.DietQuai ? data.soLuongBoXuongCanDiet : data.soLuongCanGiaiCuu;

                if (progress.trangThai == TrangThaiQuest.DangLam)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: {progress.soBoXuongDaDiet}/{soYeuCau}";
                }
                else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: <color=green>[Hoàn thành] Trả nhiệm vụ!</color>";
                }
            }
        }
    }

    private bool KiemTraDaHoanThanhTatCaQuest()
    {
        List<ProgressQuest> danhSachProgress = QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress;

        if (danhSachProgress == null || danhSachProgress.Count == 0) return false;

        int tongSoQuestTrongGame = QuestSaveSystem.Instance.danhSachQuestData.Count;
        if (danhSachProgress.Count < tongSoQuestTrongGame) return false;

        foreach (ProgressQuest progress in danhSachProgress)
        {
            if (progress.trangThai != TrangThaiQuest.HoanThanh) return false;
        }

        return true;
    }

    private List<ProgressQuest> LayToanBoQuestDangKichHoat()
    {
        List<ProgressQuest> ketQua = new List<ProgressQuest>();

        foreach (ProgressQuest progress in QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress)
        {
            if (progress.trangThai == TrangThaiQuest.DangLam || progress.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                ketQua.Add(progress);
            }
        }

        return ketQua;
    }

    private void XoaRongToanBoText()
    {
        for (int i = 0; i < danhSachTextQuestUI.Length; i++)
        {
            if (danhSachTextQuestUI[i] != null)
            {
                danhSachTextQuestUI[i].text = "";
                danhSachTextQuestUI[i].gameObject.SetActive(false);
            }
        }
    }
}