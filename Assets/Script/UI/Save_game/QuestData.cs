using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Định nghĩa các loại nhiệm vụ trong game Tu Tiên
public enum LoaiQuest
{
    DietQuai = 0,    // Đánh quái tích điểm
    GiaiCuu = 1     // Tương tác/Giải cứu NPC, con nhân nén
}

[Serializable]
public class ItemRewardData
{
    [Tooltip("Kéo ScriptableObject ItemData vào đây")]
    public ScriptableObject itemData;

    [Tooltip("Icon hiển thị trên UI")]
    public Sprite iconItem;

    [Tooltip("Số lượng vật phẩm nhận được")]
    public int soLuong = 1;
}

[CreateAssetMenu(fileName = "NewQuestData", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("--- THÔNG TIN CHUNG (TIẾNG VIỆT) ---")]
    public int idQuest;
    public string tenNhiemVu;

    [Header("--- THÔNG TIN CHUNG (TIẾNG ANH) ---")]
    [Tooltip("Tên nhiệm vụ bằng Tiếng Anh (Để trống sẽ tự dùng Tiếng Việt)")]
    public string tenNhiemVuEN;

    [Tooltip("Chọn loại nhiệm vụ muốn tạo")]
    public LoaiQuest loaiQuest = LoaiQuest.DietQuai;

    [Header("--- MỤC TIÊU DIỆT QUÁI (Dùng cho loại DietQuai) ---")]
    public int idQuaiCanDiet;
    public int soLuongBoXuongCanDiet;

    [Header("--- MỤC TIÊU GIẢI CỨU (Dùng cho loại GiaiCuu) ---")]
    [Tooltip("ID của NPC hoặc Dân Lành cần tương tác giải cứu")]
    public int idDoiTuongCanGiaiCuu;
    [Tooltip("Số lượng người/NPC cần giải cứu")]
    public int soLuongCanGiaiCuu = 1;

    [Header("--- LỜI THOẠI NPC (TIẾNG VIỆT - MẶC ĐỊNH) ---")]
    [TextArea(2, 5)]
    public string loiThoaiNhanQuest;
    [TextArea(2, 5)]
    public string loiThoaiDangLam;
    [TextArea(2, 5)]
    public string loiThoaiHoanThanh;

    [Header("--- LỜI THOẠI NPC (TIẾNG ANH - OVERRIDE) ---")]
    [TextArea(2, 5)]
    public string loiThoaiNhanQuestEN;
    [TextArea(2, 5)]
    public string loiThoaiDangLamEN;
    [TextArea(2, 5)]
    public string loiThoaiHoanThanhEN;

    [Header("--- PHẦN THƯỜNG ---")]
    public List<ItemRewardData> danhSachPhanThuong = new List<ItemRewardData>();

    // =========================================================
    // CÁC HÀM HỖ TRỢ LẤY THÔNG TIN THEO NGÔN NGỮ HỆ THỐNG
    // =========================================================

    public string GetTenNhiemVu(bool isEnglish)
    {
        if (isEnglish && !string.IsNullOrEmpty(tenNhiemVuEN))
        {
            return tenNhiemVuEN;
        }
        return tenNhiemVu;
    }

    public string GetLoiThoaiNhanQuest(bool isEnglish)
    {
        if (isEnglish && !string.IsNullOrEmpty(loiThoaiNhanQuestEN))
        {
            return loiThoaiNhanQuestEN;
        }
        return loiThoaiNhanQuest;
    }

    public string GetLoiThoaiDangLam(bool isEnglish)
    {
        if (isEnglish && !string.IsNullOrEmpty(loiThoaiDangLamEN))
        {
            return loiThoaiDangLamEN;
        }
        return loiThoaiDangLam;
    }

    public string GetLoiThoaiHoanThanh(bool isEnglish)
    {
        if (isEnglish && !string.IsNullOrEmpty(loiThoaiHoanThanhEN))
        {
            return loiThoaiHoanThanhEN;
        }
        return loiThoaiHoanThanh;
    }

    // =========================================================
    // LOGIC LƯU PHẦN THƯỜNG
    // =========================================================

    public void LuuPhanThuongVaoSaveGame()
    {
        if (danhSachPhanThuong == null || QuestSaveSystem.Instance == null) return;

        foreach (ItemRewardData reward in danhSachPhanThuong)
        {
            if (reward != null && reward.itemData != null)
            {
                ItemData actualItemData = reward.itemData as ItemData;
                if (actualItemData != null)
                {
                    QuestSaveSystem.Instance.LuuItemVaoSaveGame(actualItemData.idItem, reward.soLuong);
                }
            }
        }
    }
}