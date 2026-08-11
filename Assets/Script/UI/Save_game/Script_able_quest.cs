using UnityEngine;

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

    [Header("--- PHẦN THƯỞNG ---")]
    [Tooltip("Prefab của vật phẩm thưởng rơi ra khi trả quest")]
    public GameObject prefabItemPhanThuong;

    [Tooltip("Số lượng vật phẩm thưởng")]
    public int soLuongItemThuong = 1;
}