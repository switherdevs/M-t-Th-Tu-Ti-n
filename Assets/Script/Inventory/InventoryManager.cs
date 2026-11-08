using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("--- QUẢN LÝ BẬT / TẮT KHO ĐỒ ---")]
    [Tooltip("Kéo Panel UI chứa toàn bộ Kho đồ vào đây")]
    public GameObject panelKhoDo;
    public KeyCode phimBatTat = KeyCode.Tab;

    [Header("--- QUẢN LÝ BẢNG CỐ ĐỊNH ---")]
    [Tooltip("Panel khung viền của bảng thông tin (Nếu không dùng thì để trống)")]
    public GameObject panelBangCoDinh;
    public TextMeshProUGUI textTenItemCoDinh;
    public TextMeshProUGUI textCanhGioiCoDinh;
    public TextMeshProUGUI textMoTaCoDinh;

    [Header("--- DANH SÁCH 36 Ô UI ---")]
    public InventorySlotUI[] danhSach36SlotUI;

    [Header("--- CƠ SỞ DỮ LIỆU ITEM ---")]
    public List<ItemData> cacItemDataTrongGame;

    // Lưu lại item đang được chọn xem chi tiết để khi đổi ngôn ngữ tự động cập nhật lại ngay lập tức
    private ItemData itemDangXemChiTiet = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // Lắng nghe sự kiện đổi ngôn ngữ toàn cục để cập nhật giao diện kho đồ ngay lập tức
        GameCore.Settings.SettingsManager.OnLanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        GameCore.Settings.SettingsManager.OnLanguageChanged -= HandleLanguageChanged;
    }

    private void Start()
    {
        // Ban đầu xóa sạch chữ trên bảng cố định
        AnThongTinBangCoDinh();

        // Mặc định ẩn kho đồ khi vừa vào game
        if (panelKhoDo != null)
        {
            panelKhoDo.SetActive(false);
        }

        // Tải dữ liệu từ File Save ngay khi vào Game
        TaiDuLieuTuSaveGame();
    }

    private void Update()
    {
        // Bắt sự kiện nhấn phím TAB để Đóng/Mở Kho đồ
        if (Input.GetKeyDown(phimBatTat))
        {
            ToggleKhoDo();
        }
    }

    public void ToggleKhoDo()
    {
        if (panelKhoDo != null)
        {
            bool trangThaiHienTai = panelKhoDo.activeSelf;
            bool trangThaiMoi = !trangThaiHienTai;
            panelKhoDo.SetActive(trangThaiMoi);

            if (trangThaiMoi)
            {
                // Khi mở kho đồ lên thì tải/cập nhật lại dữ liệu mới nhất từ File Save
                TaiDuLieuTuSaveGame();
            }
            else
            {
                AnThongTinBangCoDinh();
            }
        }
    }

    /// <summary>
    /// Hàm đọc danh sách Item từ QuestSaveSystem và hiển thị lên các ô UI
    /// </summary>
    public void TaiDuLieuTuSaveGame()
    {
        // 1. Xóa sạch dữ liệu hiển thị cũ trên 36 ô UI
        if (danhSach36SlotUI != null)
        {
            for (int i = 0; i < danhSach36SlotUI.Length; i++)
            {
                if (danhSach36SlotUI[i] != null)
                {
                    danhSach36SlotUI[i].XoaRongO();
                }
            }
        }

        // 2. Kiểm tra Save System
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogWarning("[InventoryManager] Chưa tìm thấy dữ liệu QuestSaveSystem!");
            return;
        }

        List<SaveItemData> danhSachSave = QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachItemSave;
        if (danhSachSave == null || danhSachSave.Count == 0) return;

        // 3. Lặp qua danh sách item trong Save File và nạp vào từng ô UI
        int slotIndex = 0;
        for (int i = 0; i < danhSachSave.Count; i++)
        {
            if (slotIndex >= danhSach36SlotUI.Length) break;

            SaveItemData saveData = danhSachSave[i];

            ItemData dataMatch = TimItemDataTheoID(saveData.idItem);

            if (dataMatch != null && saveData.soLuong > 0)
            {
                if (danhSach36SlotUI[slotIndex] != null)
                {
                    danhSach36SlotUI[slotIndex].CapNhatOUI(dataMatch, saveData.soLuong);
                    slotIndex++;
                }
            }
        }
    }

    private ItemData TimItemDataTheoID(string id)
    {
        if (cacItemDataTrongGame == null) return null;

        for (int i = 0; i < cacItemDataTrongGame.Count; i++)
        {
            if (cacItemDataTrongGame[i] != null && cacItemDataTrongGame[i].idItem == id)
            {
                return cacItemDataTrongGame[i];
            }
        }
        return null;
    }

    // Xử lý khi người chơi bấm đổi ngôn ngữ
    private void HandleLanguageChanged(string langCode)
    {
        if (panelKhoDo != null && panelKhoDo.activeSelf)
        {
            TaiDuLieuTuSaveGame();
        }

        if (itemDangXemChiTiet != null)
        {
            HienThongTinBangCoDinh(itemDangXemChiTiet);
        }
    }

    // Kiểm tra ngôn ngữ từ hệ thống save / settings
    private bool KiemTraNgonNguTiengAnh()
    {
        if (QuestSaveSystem.Instance != null)
        {
            return QuestSaveSystem.Instance.IsEnglish();
        }
        return false;
    }

    public void HienThongTinBangCoDinh(ItemData data)
    {
        if (data == null) return;

        itemDangXemChiTiet = data;

        if (panelBangCoDinh != null) panelBangCoDinh.SetActive(true);

        // Kiểm tra xem người chơi chọn tiếng Anh hay tiếng Việt
        bool isEnglish = KiemTraNgonNguTiengAnh();

        // Lấy nội dung tương ứng dựa trên ngôn ngữ
        string tenHienThi = isEnglish && !string.IsNullOrEmpty(data.tenItemEN) ? data.tenItemEN : data.tenItem;
        string moTaHienThi = isEnglish && !string.IsNullOrEmpty(data.moTaItemEN) ? data.moTaItemEN : data.moTaItem;

        // Đã sửa: Gọi trực tiếp .ToString() vì CanhGioiYeuCau là kiểu giá trị không thể null
        string canhGioiHienThi = data.canhGioiYeuCau.ToString();

        // Dịch nhãn "Cảnh giới:" sang tiếng Anh: "Realm required:"
        string nhanCanhGioi = isEnglish ? "Realm required: " : "Cảnh giới: ";

        if (textTenItemCoDinh != null)
            textTenItemCoDinh.text = $"<b>{tenHienThi}</b>";

        if (textCanhGioiCoDinh != null)
            textCanhGioiCoDinh.text = $"{nhanCanhGioi}<color=yellow>{canhGioiHienThi}</color>";

        if (textMoTaCoDinh != null)
            textMoTaCoDinh.text = moTaHienThi;
    }

    public void AnThongTinBangCoDinh()
    {
        itemDangXemChiTiet = null;

        if (textTenItemCoDinh != null) textTenItemCoDinh.text = "";
        if (textCanhGioiCoDinh != null) textCanhGioiCoDinh.text = "";
        if (textMoTaCoDinh != null) textMoTaCoDinh.text = "";

        if (panelBangCoDinh != null && panelBangCoDinh != panelKhoDo)
        {
            panelBangCoDinh.SetActive(false);
        }
    }

    public void SuDungItem(InventorySlotUI slot)
    {
        ItemData item = slot.itemDataHienTai;
        if (item == null) return;

        Debug.Log($"[Kho Đồ] Đã click vào vật phẩm: {item.tenItem}");
    }
}