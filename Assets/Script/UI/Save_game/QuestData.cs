using System;
using System.Collections.Generic;
using UnityEngine;

// Cấu trúc chứa 1 loại phần thưởng (Data + Số lượng)
[Serializable]
public class ItemRewardData
{
    [Tooltip("ScriptableObject / Able dữ liệu của Item phần thưởng")]
    public ScriptableObject itemData;

    [Tooltip("Sprite Icon hiển thị hình ảnh vật phẩm trên UI")]
    public Sprite iconItem;

    [Tooltip("Số lượng của loại vật phẩm này")]
    public int soLuong = 1;
}

[CreateAssetMenu(fileName = "QuestData_New", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("--- THÔNG TIN CHUNG ---")]
    [Tooltip("ID duy nhất của Quest (Dùng để lưu vào File TXT, không được trùng lặp)")]
    public int idQuest;

    [Tooltip("Tên nhiệm vụ hiển thị trên bảng UI")]
    public string tenNhiemVu;

    [TextArea(3, 5)]
    [Tooltip("Lời thoại NPC nói khi người chơi chưa nhận quest")]
    public string loiThoaiNhanQuest;

    [TextArea(3, 5)]
    [Tooltip("Lời thoại NPC nói khi người chơi đang làm quest")]
    public string loiThoaiDangLam;

    [TextArea(3, 5)]
    [Tooltip("Lời thoại NPC nói khi người chơi trả quest nhận thưởng")]
    public string loiThoaiHoanThanh;

    [Header("--- MỤC TIÊU NHIỆM VỤ ---")]
    [Tooltip("Mã ID của loại quái cần diệt (VD: 1 = Bộ Xương, 2 = Quái Cây)")]
    public int idQuaiCanDiet = 1;

    [Tooltip("Số lượng quái mục tiêu cần tiêu diệt để hoàn thành quest")]
    public int soLuongBoXuongCanDiet = 10;

    [Header("--- DANH SÁCH PHẦN THƯỞNG (NHIỀU ITEM) ---")]
    [Tooltip("Danh sách các Item phần thưởng nhận được khi hoàn thành Quest")]
    public List<ItemRewardData> danhSachPhanThuong = new List<ItemRewardData>();

    /// <summary>
    /// Hàm tự động duyệt qua toàn bộ mảng phần thưởng để lưu vào Save Game / Inventory
    /// </summary>
    public void LuuPhanThuongVaoSaveGame()
    {
        if (danhSachPhanThuong == null || danhSachPhanThuong.Count == 0)
        {
            Debug.LogWarning($"<color=yellow>[QUEST]</color> Quest {tenNhiemVu} (ID: {idQuest}) không có phần thưởng!");
            return;
        }

        foreach (var reward in danhSachPhanThuong)
        {
            if (reward != null && reward.itemData != null)
            {
                // Tự động đẩy từng Item trong mảng vào hệ thống lưu trữ/Túi đồ
                // Ví dụ: InventoryManager.Instance.AddItem(reward.itemData, reward.soLuong);
                Debug.Log($"<color=green>[QUEST REWARD]</color> Đã lưu {reward.soLuong}x {reward.itemData.name} vào Save Game!");
            }
        }
    }
}