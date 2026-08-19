using UnityEngine;

// Định nghĩa các Cảnh Giới trong game
public enum CanhGioiYeuCau
{
    PhamNhan = 0,      // Khởi đầu
    LienKhi = 1,       // Đột phá 1
    TrucCo = 2,        // Đột phá 2
    KimDan = 3         // Đột phá 3
}

[CreateAssetMenu(fileName = "ItemData_New", menuName = "Tu Tien/Create Item Data")]
public class ItemData : ScriptableObject
{
    [Header("--- THÔNG TIN CƠ BẢN ---")]
    public string idItem;               // Mã định danh (Ví dụ: "HP_Potion_01")
    public string tenItem;              // Tên hiển thị (Ví dụ: "Hồi Sức Đan")
    public Sprite iconItem;             // Hình ảnh hiển thị trong UI
    
    [TextArea(3, 5)]
    public string moTaItem;             // Mô tả chi tiết món đồ

    [Header("--- THÔNG SỐ VẬT PHẨM ---")]
    public int soLuongDonToiDa = 64;   // Mặc định dồn tối đa 64 món/ô
    public CanhGioiYeuCau canhGioiYeuCau; // Cảnh giới tối thiểu để dùng item
}