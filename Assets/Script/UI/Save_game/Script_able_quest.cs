using UnityEngine;

[CreateAssetMenu(fileName = "QuestData_", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("--- THÔNG TIN CHUNG ---")]
    public int idQuest;                     // ID duy nhất của Quest (Dùng để lưu vào File TXT)
    public string tenNhiemVu;
    [TextArea(3, 5)]
    public string loiThoaiNhanQuest;        // Lời thoại NPC khi chào mời nhận quest
    [TextArea(3, 5)]
    public string loiThoaiDangLam;          // Lời thoại NPC khi đang làm quest
    [TextArea(3, 5)]
    public string loiThoaiHoanThanh;        // Lời thoại NPC khi trả quest nhận thưởng

    [Header("--- MỤC TIÊU NHIỆM VỤ ---")]
    public int soLuongBoXuongCanDiet = 10;   // Mục tiêu cần tiêu diệt

    [Header("--- PHẦN THƯỞNG ---")]
    public GameObject prefabItemPhanThuong; // Prefab Item truyền vào (Chỉ 1 loại)
    public int soLuongItemThuong = 1;        // Số lượng Item nhận được (Tùy chỉnh)
}