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
    [Tooltip("Nếu để trống, script sẽ tự động dịch dự phòng dựa theo ID hoặc Tên Tiếng Việt")]
    public string tenItemEN;

    [TextArea(3, 5)]
    [Tooltip("Mô tả item bằng tiếng Anh")]
    public string moTaItemEN;

    [Tooltip("Tên cảnh giới bằng tiếng Anh")]
    public string canhGioiYeuCauEN;

    [Header("--- THÔNG SỐ VẬT PHẨM ---")]
    public int soLuongDonToiDa = 64;   // Mặc định dồn tối đa 64 món/ô
    public CanhGioiYeuCau canhGioiYeuCau; // Cảnh giới tối thiểu để dùng item

    /// <summary>
    /// Lấy tên vật phẩm tùy theo ngôn ngữ đang chọn
    /// </summary>
    public string GetItemName(bool isEnglish)
    {
        if (isEnglish)
        {
            if (!string.IsNullOrEmpty(tenItemEN)) return tenItemEN;
            return AutoTranslateName(idItem, tenItem);
        }
        return !string.IsNullOrEmpty(tenItem) ? tenItem : idItem;
    }

    /// <summary>
    /// Lấy mô tả vật phẩm tùy theo ngôn ngữ đang chọn
    /// </summary>
    public string GetItemDescription(bool isEnglish)
    {
        if (isEnglish)
        {
            if (!string.IsNullOrEmpty(moTaItemEN)) return moTaItemEN;
            return AutoTranslateDescription(idItem, moTaItem);
        }
        return moTaItem;
    }

    /// <summary>
    /// Hàm trả về tên cảnh giới tùy theo ngôn ngữ đang chọn
    /// </summary>
    public string GetCanhGioiText(bool isEnglish)
    {
        if (isEnglish)
        {
            if (!string.IsNullOrEmpty(canhGioiYeuCauEN)) return canhGioiYeuCauEN;

            switch (canhGioiYeuCau)
            {
                case CanhGioiYeuCau.PhamNhan: return "Mortal";
                case CanhGioiYeuCau.LienKhi: return "Qi Condensation";
                case CanhGioiYeuCau.TrucCo: return "Foundation Establishment";
                case CanhGioiYeuCau.KimDan: return "Golden Core";
                default: return canhGioiYeuCau.ToString();
            }
        }

        // Mặc định trả về tiếng Việt theo Enum
        switch (canhGioiYeuCau)
        {
            case CanhGioiYeuCau.PhamNhan: return "Phàm Nhân";
            case CanhGioiYeuCau.LienKhi: return "Luyện Khí";
            case CanhGioiYeuCau.TrucCo: return "Trúc Cơ";
            case CanhGioiYeuCau.KimDan: return "Kim Đan";
            default: return canhGioiYeuCau.ToString();
        }
    }

    // 🎯 TỰ ĐỘNG DỊCH THUẬT DỰ PHÒNG CHO CÁC VẬT PHẨM TU TIÊN THƯỜNG DÙNG
    private string AutoTranslateName(string id, string vnName)
    {
        string lowerId = id != null ? id.ToLower() : "";

        if (lowerId.Contains("hp") || lowerId.Contains("hoi_suc") || lowerId.Contains("mau"))
            return "Recovery Pill";
        if (lowerId.Contains("mp") || lowerId.Contains("nang_luong") || lowerId.Contains("khi"))
            return "Spirit Qi Pill";
        if (lowerId.Contains("bo_xuong") || lowerId.Contains("xuong"))
            return "Demon Bone";
        if (lowerId.Contains("dan") || lowerId.Contains("duoc"))
            return "Cultivation Pill";

        // Dịch dựa trên tên tiếng Việt quen thuộc nếu không khớp ID
        if (vnName == "Hồi Sức Đan") return "Recovery Pill";
        if (vnName == "Tụ Khí Đan") return "Qi Gathering Pill";
        if (vnName == "Trúc Cơ Đan") return "Foundation Establishment Pill";
        if (vnName == "Tẩy Tủy Đan") return "Marrow Cleansing Pill";

        return !string.IsNullOrEmpty(vnName) ? vnName : "Unknown Item";
    }

    private string AutoTranslateDescription(string id, string vnDesc)
    {
        string lowerId = id != null ? id.ToLower() : "";

        if (lowerId.Contains("hp") || lowerId.Contains("hoi_suc"))
            return "Consume to restore health points.";
        if (lowerId.Contains("mp"))
            return "Consume to restore energy / spirit Qi.";
        if (lowerId.Contains("bo_xuong"))
            return "Bones collected from defeated monsters in the overworld map.";

        return !string.IsNullOrEmpty(vnDesc) ? vnDesc : "No description available.";
    }
}