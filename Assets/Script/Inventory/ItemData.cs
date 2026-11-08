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
    [Header("--- THÔNG TIN CƠ BẢN (TIẾNG VIỆT - MẶC ĐỊNH) ---")]
    public string idItem;               // Mã định danh (Ví dụ: "HP_Potion_01")
    public string tenItem;              // Tên hiển thị (Ví dụ: "Hồi Sức Đan")
    public Sprite iconItem;             // Hình ảnh hiển thị trong UI
    
    [TextArea(3, 5)]
    public string moTaItem;             // Mô tả chi tiết món đồ

    [Header("--- THÔNG TIN TIẾNG ANH (ENGLISH) ---")]
    [Tooltip("Tên item bằng tiếng Anh")]
    public string tenItemEN;            
    
    [TextArea(3, 5)]
    [Tooltip("Mô tả item bằng tiếng Anh")]
    public string moTaItemEN;           
    
    [Tooltip("Tên cảnh giới bằng tiếng Anh (Ví dụ: Qi Condensation, Foundation Establishment...)")]
    public string canhGioiYeuCauEN;     

    [Header("--- THÔNG SỐ VẬT PHẨM ---")]
    public int soLuongDonToiDa = 64;   // Mặc định dồn tối đa 64 món/ô
    public CanhGioiYeuCau canhGioiYeuCau; // Cảnh giới tối thiểu để dùng item

    /// <summary>
    /// Hàm trả về tên cảnh giới tùy theo ngôn ngữ đang chọn
    /// </summary>
    public string GetCanhGioiText(bool isEnglish)
    {
        if (isEnglish && !string.IsNullOrEmpty(canhGioiYeuCauEN))
        {
            return canhGioiYeuCauEN;
        }
        
        // Mặc định trả về tiếng Việt theo Enum nếu chưa nhập tiếng Anh
        switch (canhGioiYeuCau)
        {
            case CanhGioiYeuCau.PhamNhan: return "Phàm Nhân";
            case CanhGioiYeuCau.LienKhi: return "Luyện Khí";
            case CanhGioiYeuCau.TrucCo: return "Trúc Cơ";
            case CanhGioiYeuCau.KimDan: return "Kim Đan";
            default: return canhGioiYeuCau.ToString();
        }
    }
}