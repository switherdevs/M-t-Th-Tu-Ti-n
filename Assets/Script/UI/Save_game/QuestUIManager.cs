using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


[Serializable]
public class QuestUISlot
{
    [Tooltip("ScriptableObject chứa dữ liệu Quest.")]
    public QuestData questData;

    [Tooltip("Button dùng để mở Quest này.")]
    public Button nutMoQuest;
}


public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;


    // =========================================================
    // DANH SÁCH QUEST
    // =========================================================

    [Header("--- DANH SÁCH QUEST ---")]

    [Tooltip(
        "Mỗi Element gồm 1 QuestData và 1 Button Mở Quest."
    )]
    public QuestUISlot[] danhSachQuest;


    // =========================================================
    // PANEL CHI TIẾT QUEST
    // =========================================================

    [Header("--- PANEL CHI TIẾT QUEST ---")]

    [Tooltip("Panel chứa toàn bộ thông tin Quest.")]
    public GameObject bangThoaiUI;

    [Tooltip("Tên Quest.")]
    public TextMeshProUGUI textTenNhiemVu;

    [Tooltip("Nội dung thoại / mô tả Quest.")]
    public TextMeshProUGUI textLoiThoaiNPC;

    [Tooltip("Tiến trình Quest.")]
    public TextMeshProUGUI textTienTrinh;


    // =========================================================
    // BUTTON TRONG PANEL
    // =========================================================

    [Header("--- BUTTON QUEST ---")]

    [Tooltip("Nút nhận Quest.")]
    public Button nutDongY;

    [Tooltip("Nút từ chối Quest. Có thể bỏ trống.")]
    public Button nutTuChoi;

    [Tooltip("Nút trả Quest.")]
    public Button nutTraNhiemVu;

    [Tooltip("Nút đóng bảng Quest.")]
    public Button nutDongBang;


    // =========================================================
    // THÀNH PHẦN THƯỞNG
    // =========================================================

    [Header("--- PHẦN THƯỞNG ---")]

    [Tooltip(
        "Vị trí World mà prefab phần thưởng sẽ được tạo ra. " +
        "Có thể bỏ trống nếu hệ thống Item của bạn xử lý phần thưởng riêng."
    )]
    public Transform viTriTraThuong;


    // =========================================================
    // QUEST ĐANG XEM
    // =========================================================

    private QuestData questDangXem;


    // =========================================================
    // AWAKE
    // =========================================================

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


        // Khi bắt đầu:
        // Panel chi tiết phải ẩn.
        if (bangThoaiUI != null)
        {
            bangThoaiUI.SetActive(false);
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        KhoiTaoDanhSachQuest();

        if (nutDongY != null)
        {
            nutDongY.onClick.AddListener(
                OnClickDongYNhanQuest
            );
        }

        if (nutTraNhiemVu != null)
        {
            nutTraNhiemVu.onClick.AddListener(
                OnClickTraNhiemVu
            );
        }

        if (nutDongBang != null)
        {
            nutDongBang.onClick.AddListener(
                DongBangQuest
            );
        }

        if (nutTuChoi != null)
        {
            nutTuChoi.onClick.AddListener(
                DongBangQuest
            );
        }

        AnNoiDungQuest();
    }


    // =========================================================
    // KHỞI TẠO CÁC SLOT QUEST
    // =========================================================

    private void KhoiTaoDanhSachQuest()
    {
        if (danhSachQuest == null)
        {
            Debug.LogWarning(
                "[QuestUI] Chưa có danh sách Quest."
            );

            return;
        }


        for (
            int i = 0;
            i < danhSachQuest.Length;
            i++
        )
        {
            QuestUISlot slot =
                danhSachQuest[i];

            if (slot == null)
            {
                continue;
            }


            if (slot.questData == null)
            {
                Debug.LogWarning(
                    "[QuestUI] Quest Slot "
                    + i
                    + " chưa có QuestData."
                );

                continue;
            }


            if (slot.nutMoQuest == null)
            {
                Debug.LogWarning(
                    "[QuestUI] Quest Slot "
                    + i
                    + " chưa có Button Mở Quest."
                );

                continue;
            }


            // Xóa listener cũ để tránh đăng ký nhiều lần
            slot.nutMoQuest.onClick.RemoveAllListeners();


            int index = i;

            slot.nutMoQuest.onClick.AddListener(
                () =>
                {
                    OnClickMoQuest(index);
                }
            );


            CapNhatButtonMoQuest(i);
        }
    }


    // =========================================================
    // CLICK MỞ QUEST
    // =========================================================

    public void OnClickMoQuest(int index)
    {
        if (
            index < 0
            || index >= danhSachQuest.Length
        )
        {
            return;
        }


        QuestUISlot slot =
            danhSachQuest[index];


        if (slot == null || slot.questData == null)
        {
            return;
        }


        QuestData questData =
            slot.questData;


        // Không cho mở Quest đã hoàn thành
        ProgressQuest progress =
            QuestSaveSystem.Instance
            .LayTienTrinhQuest(
                questData.idQuest
            );


        if (
            progress.trangThai
            == TrangThaiQuest.HoanThanh
        )
        {
            return;
        }


        questDangXem =
            questData;


        if (bangThoaiUI != null)
        {
            bangThoaiUI.SetActive(true);
        }


        HienThiQuest(
            questData,
            progress
        );
    }


    // =========================================================
    // HIỂN THỊ QUEST
    // =========================================================

    private void HienThiQuest(
        QuestData questData,
        ProgressQuest progress
    )
    {
        if (questData == null)
        {
            return;
        }


        // Tên Quest
        if (textTenNhiemVu != null)
        {
            textTenNhiemVu.text =
                questData.tenNhiemVu;
        }


        // Mặc định ẩn Button
        AnTatCaButtonChiTiet();


        switch (progress.trangThai)
        {
            // =================================================
            // CHƯA NHẬN
            // =================================================

            case TrangThaiQuest.ChuaNhan:

                if (textLoiThoaiNPC != null)
                {
                    textLoiThoaiNPC.text =
                        questData.loiThoaiNhanQuest;
                }


                if (textTienTrinh != null)
                {
                    textTienTrinh.text =
                        "Mục tiêu: Tiêu diệt "
                        + questData.soLuongBoXuongCanDiet
                        + " mục tiêu.";
                }


                if (nutDongY != null)
                {
                    nutDongY.gameObject.SetActive(true);
                    nutDongY.interactable = true;
                }


                break;


            // =================================================
            // ĐANG LÀM
            // =================================================

            case TrangThaiQuest.DangLam:

                if (textLoiThoaiNPC != null)
                {
                    textLoiThoaiNPC.text =
                        questData.loiThoaiDangLam;
                }


                if (textTienTrinh != null)
                {
                    textTienTrinh.text =
                        "Tiến trình: "
                        + progress.soBoXuongDaDiet
                        + "/"
                        + questData.soLuongBoXuongCanDiet;
                }


                HienThiNutTraNhiemVu(false);

                break;


            // =================================================
            // ĐỦ ĐIỀU KIỆN TRẢ
            // =================================================

            case TrangThaiQuest.DaXongChuaTra:

                if (textLoiThoaiNPC != null)
                {
                    textLoiThoaiNPC.text =
                        questData.loiThoaiHoanThanh;
                }


                if (textTienTrinh != null)
                {
                    textTienTrinh.text =
                        "Tiến trình: Hoàn thành ("
                        + questData.soLuongBoXuongCanDiet
                        + "/"
                        + questData.soLuongBoXuongCanDiet
                        + ")";
                }


                HienThiNutTraNhiemVu(true);

                break;


            // =================================================
            // ĐÃ HOÀN THÀNH
            // =================================================

            case TrangThaiQuest.HoanThanh:

                if (textLoiThoaiNPC != null)
                {
                    textLoiThoaiNPC.text =
                        questData.loiThoaiHoanThanh;
                }


                if (textTienTrinh != null)
                {
                    textTienTrinh.text =
                        "Nhiệm vụ đã hoàn thành.";
                }


                break;
        }
    }


    // =========================================================
    // NHẬN QUEST
    // =========================================================

    public void OnClickDongYNhanQuest()
    {
        if (questDangXem == null)
        {
            return;
        }


        QuestSaveSystem.Instance
            .CapNhatTrangThaiQuest(
                questDangXem.idQuest,
                TrangThaiQuest.DangLam
            );


        ProgressQuest progress =
            QuestSaveSystem.Instance
            .LayTienTrinhQuest(
                questDangXem.idQuest
            );


        HienThiQuest(
            questDangXem,
            progress
        );


        Debug.Log(
            "<color=yellow>[Quest UI]</color> "
            + "Đã nhận Quest: "
            + questDangXem.idQuest
        );
    }


    // =========================================================
    // TRẢ QUEST
    // =========================================================

    public void OnClickTraNhiemVu()
    {
        if (questDangXem == null)
        {
            return;
        }


        ProgressQuest progress =
            QuestSaveSystem.Instance
            .LayTienTrinhQuest(
                questDangXem.idQuest
            );


        // Không đủ điều kiện → không cho trả
        if (
            progress.trangThai
            != TrangThaiQuest.DaXongChuaTra
        )
        {
            Debug.LogWarning(
                "[Quest UI] Quest chưa đủ điều kiện trả."
            );

            return;
        }


        // =====================================================
        // TRAO PHẦN THƯỞNG
        // =====================================================

        TraoPhanThuong();


        // =====================================================
        // ĐÁNH DẤU QUEST HOÀN THÀNH
        // =====================================================

        QuestSaveSystem.Instance
            .CapNhatTrangThaiQuest(
                questDangXem.idQuest,
                TrangThaiQuest.HoanThanh
            );


        // =====================================================
        // KHÓA BUTTON MỞ QUEST
        // =====================================================

        KhoaButtonMoQuestTheoID(
            questDangXem.idQuest
        );


        // Cập nhật UI
        ProgressQuest progressMoi =
            QuestSaveSystem.Instance
            .LayTienTrinhQuest(
                questDangXem.idQuest
            );


        HienThiQuest(
            questDangXem,
            progressMoi
        );


        Debug.Log(
            "<color=green>[Quest UI]</color> "
            + "Đã trả Quest: "
            + questDangXem.idQuest
        );
    }


    // =========================================================
    // TRAO PHẦN THƯỞNG
    // =========================================================

    private void TraoPhanThuong()
    {
        if (
            questDangXem == null
            || questDangXem.prefabItemPhanThuong == null
        )
        {
            return;
        }


        if (viTriTraThuong == null)
        {
            Debug.LogWarning(
                "[Quest Reward] Chưa gán ViTriTraThuong."
                + " Quest vẫn được hoàn thành nhưng"
                + " chưa tạo Prefab phần thưởng."
            );

            return;
        }


        for (
            int i = 0;
            i < questDangXem.soLuongItemThuong;
            i++
        )
        {
            Instantiate(
                questDangXem.prefabItemPhanThuong,
                viTriTraThuong.position,
                Quaternion.identity
            );
        }


        Debug.Log(
            "<color=green>[Quest Reward]</color> "
            + "Nhận "
            + questDangXem.soLuongItemThuong
            + "x "
            + questDangXem.prefabItemPhanThuong.name
        );
    }


    // =========================================================
    // HIỂN THỊ / KHÓA BUTTON TRẢ QUEST
    // =========================================================

    private void HienThiNutTraNhiemVu(
        bool coTheTra
    )
    {
        if (nutTraNhiemVu == null)
        {
            return;
        }


        nutTraNhiemVu.gameObject.SetActive(true);

        // Đây là phần quan trọng:
        // Chưa hoàn thành → mờ + không click
        // Hoàn thành → sáng + click được
        nutTraNhiemVu.interactable =
            coTheTra;
    }


    // =========================================================
    // KHÓA BUTTON MỞ QUEST
    // =========================================================

    private void KhoaButtonMoQuestTheoID(
        int idQuest
    )
    {
        if (danhSachQuest == null)
        {
            return;
        }


        foreach (
            QuestUISlot slot
            in danhSachQuest
        )
        {
            if (
                slot == null
                || slot.questData == null
                || slot.nutMoQuest == null
            )
            {
                continue;
            }


            if (
                slot.questData.idQuest
                == idQuest
            )
            {
                slot.nutMoQuest.interactable =
                    false;

                break;
            }
        }
    }


    // =========================================================
    // CẬP NHẬT BUTTON MỞ QUEST
    // =========================================================

    private void CapNhatButtonMoQuest(
        int index
    )
    {
        if (
            index < 0
            || index >= danhSachQuest.Length
        )
        {
            return;
        }


        QuestUISlot slot =
            danhSachQuest[index];


        if (
            slot == null
            || slot.questData == null
            || slot.nutMoQuest == null
        )
        {
            return;
        }


        ProgressQuest progress =
            QuestSaveSystem.Instance
            .LayTienTrinhQuest(
                slot.questData.idQuest
            );


        if (
            progress.trangThai
            == TrangThaiQuest.HoanThanh
        )
        {
            slot.nutMoQuest.interactable =
                false;
        }
        else
        {
            slot.nutMoQuest.interactable =
                true;
        }
    }


    // =========================================================
    // ẨN NỘI DUNG QUEST BAN ĐẦU
    // =========================================================

    private void AnNoiDungQuest()
    {
        if (textTenNhiemVu != null)
        {
            textTenNhiemVu.text = "";
        }

        if (textLoiThoaiNPC != null)
        {
            textLoiThoaiNPC.text = "";
        }

        if (textTienTrinh != null)
        {
            textTienTrinh.text = "";
        }


        AnTatCaButtonChiTiet();
    }


    // =========================================================
    // ẨN BUTTON CHI TIẾT
    // =========================================================

    private void AnTatCaButtonChiTiet()
    {
        if (nutDongY != null)
        {
            nutDongY.gameObject.SetActive(false);
        }

        if (nutTuChoi != null)
        {
            nutTuChoi.gameObject.SetActive(false);
        }

        if (nutTraNhiemVu != null)
        {
            nutTraNhiemVu.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ĐÓNG QUEST
    // =========================================================

    public void DongBangQuest()
    {
        if (bangThoaiUI != null)
        {
            bangThoaiUI.SetActive(false);
        }

        questDangXem = null;

        AnNoiDungQuest();
    }


    // =========================================================
    // TÌM QUEST DATA THEO ID
    // ĐƯỢC QUEST SAVE SYSTEM SỬ DỤNG
    // =========================================================

    public QuestData LayQuestDataTheoID(
        int idQuest
    )
    {
        if (danhSachQuest == null)
        {
            return null;
        }


        foreach (
            QuestUISlot slot
            in danhSachQuest
        )
        {
            if (
                slot == null
                || slot.questData == null
            )
            {
                continue;
            }


            if (
                slot.questData.idQuest
                == idQuest
            )
            {
                return slot.questData;
            }
        }


        return null;
    }
}