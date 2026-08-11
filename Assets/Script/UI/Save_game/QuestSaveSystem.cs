using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public enum TrangThaiQuest
{
    ChuaNhan = 0,
    DangLam = 1,
    DaXongChuaTra = 2,
    HoanThanh = 3
}

[Serializable]
public class ProgressQuest
{
    public int idQuest;
    public TrangThaiQuest trangThai;
    public int soBoXuongDaDiet;
}

[Serializable]
public class DanhSachSaveQuest
{
    public List<ProgressQuest> danhSachProgress = new List<ProgressQuest>();
}

public class QuestSaveSystem : MonoBehaviour
{
    public static QuestSaveSystem Instance;

    [Header("--- CẤU HÌNH SAVE ---")]

    [Tooltip("Tên file lưu tiến trình Quest.")]
    public string tenFileSave = "QuestProgressData.txt";

    private string duongDanTuyetDoi;

    public DanhSachSaveQuest duLieuSaveHienTai = new DanhSachSaveQuest();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        duongDanTuyetDoi = Path.Combine(
            Application.persistentDataPath,
            tenFileSave
        );

        LoadDuLieuQuestFromTxt();
    }


    // =========================================================
    // SAVE
    // =========================================================

    public void SaveDuLieuQuestToTxt()
    {
        try
        {
            string chuoiJson = JsonUtility.ToJson(
                duLieuSaveHienTai,
                true
            );

            File.WriteAllText(
                duongDanTuyetDoi,
                chuoiJson
            );

            Debug.Log(
                "<color=green>[Quest Save]</color> Đã lưu Quest: "
                + duongDanTuyetDoi
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[Quest Save] Lỗi ghi file: "
                + e.Message
            );
        }
    }


    // =========================================================
    // LOAD
    // =========================================================

    public void LoadDuLieuQuestFromTxt()
    {
        if (File.Exists(duongDanTuyetDoi))
        {
            try
            {
                string chuoiJson = File.ReadAllText(
                    duongDanTuyetDoi
                );

                duLieuSaveHienTai =
                    JsonUtility.FromJson<DanhSachSaveQuest>(
                        chuoiJson
                    );

                if (duLieuSaveHienTai == null)
                {
                    duLieuSaveHienTai =
                        new DanhSachSaveQuest();
                }

                if (duLieuSaveHienTai.danhSachProgress == null)
                {
                    duLieuSaveHienTai.danhSachProgress =
                        new List<ProgressQuest>();
                }

                Debug.Log(
                    "<color=cyan>[Quest Load]</color> Đã load dữ liệu Quest."
                );
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "[Quest Load] Lỗi đọc file: "
                    + e.Message
                );

                TaoFileSaveMoi();
            }
        }
        else
        {
            TaoFileSaveMoi();
        }
    }


    private void TaoFileSaveMoi()
    {
        duLieuSaveHienTai =
            new DanhSachSaveQuest();

        SaveDuLieuQuestToTxt();
    }


    // =========================================================
    // LẤY TIẾN TRÌNH QUEST
    // =========================================================

    public ProgressQuest LayTienTrinhQuest(int idQuest)
    {
        if (duLieuSaveHienTai == null)
        {
            duLieuSaveHienTai =
                new DanhSachSaveQuest();
        }

        if (duLieuSaveHienTai.danhSachProgress == null)
        {
            duLieuSaveHienTai.danhSachProgress =
                new List<ProgressQuest>();
        }

        foreach (
            ProgressQuest quest
            in duLieuSaveHienTai.danhSachProgress
        )
        {
            if (quest.idQuest == idQuest)
            {
                return quest;
            }
        }


        // Nếu Quest chưa từng xuất hiện trong Save
        ProgressQuest questMoi =
            new ProgressQuest
            {
                idQuest = idQuest,
                trangThai = TrangThaiQuest.ChuaNhan,
                soBoXuongDaDiet = 0
            };

        duLieuSaveHienTai.danhSachProgress.Add(
            questMoi
        );

        SaveDuLieuQuestToTxt();

        return questMoi;
    }


    // =========================================================
    // CẬP NHẬT TRẠNG THÁI QUEST
    // =========================================================

    public void CapNhatTrangThaiQuest(
        int idQuest,
        TrangThaiQuest trangThaiMoi
    )
    {
        ProgressQuest quest =
            LayTienTrinhQuest(idQuest);

        quest.trangThai =
            trangThaiMoi;

        SaveDuLieuQuestToTxt();

        Debug.Log(
            "<color=yellow>[Quest]</color> Quest ID "
            + idQuest
            + " → "
            + trangThaiMoi
        );
    }


    // =========================================================
    // GHI NHẬN QUÁI BỊ TIÊU DIỆT
    // =========================================================

    public void GhiNhanDietQuai(
        int idQuai,
        int soLuong = 1
    )
    {
        if (soLuong <= 0)
        {
            return;
        }

        bool coThayDoi = false;


        foreach (
            ProgressQuest questProgress
            in duLieuSaveHienTai.danhSachProgress
        )
        {
            // Chỉ xử lý Quest đang làm
            if (
                questProgress.trangThai
                != TrangThaiQuest.DangLam
            )
            {
                continue;
            }


            // =================================================
            // QUAN TRỌNG:
            // Phần này cần QuestData để biết idQuaiCanDiet.
            // Vì Save chỉ lưu ID Quest nên lấy QuestData
            // thông qua QuestUIManager.
            // =================================================

            QuestData questData =
                QuestUIManager.Instance
                .LayQuestDataTheoID(
                    questProgress.idQuest
                );

            if (questData == null)
            {
                continue;
            }


            // Không đúng loại quái → không cộng
            if (
                questData.idQuaiCanDiet
                != idQuai
            )
            {
                continue;
            }


            questProgress.soBoXuongDaDiet += soLuong;


            // Không cho vượt quá mục tiêu
            if (
                questProgress.soBoXuongDaDiet
                >= questData.soLuongBoXuongCanDiet
            )
            {
                questProgress.soBoXuongDaDiet =
                    questData.soLuongBoXuongCanDiet;

                questProgress.trangThai =
                    TrangThaiQuest.DaXongChuaTra;

                Debug.Log(
                    "<color=green>[Quest]</color> "
                    + questData.tenNhiemVu
                    + " đã đạt đủ điều kiện trả nhiệm vụ."
                );
            }

            coThayDoi = true;
        }


        if (coThayDoi)
        {
            SaveDuLieuQuestToTxt();
        }
    }
}