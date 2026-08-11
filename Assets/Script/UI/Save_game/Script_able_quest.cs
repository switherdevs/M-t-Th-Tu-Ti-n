using UnityEngine;

[CreateAssetMenu(fileName = "QuestData_New", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("--- THÔNG TIN CHUNG ---")]

    [Tooltip("ID duy nhất của Quest. Không được trùng với Quest khác.")]
    public int idQuest;

    [Tooltip("Tên nhiệm vụ hiển thị trên UI.")]
    public string tenNhiemVu;

    [TextArea(3, 5)]
    [Tooltip("Nội dung/thoại hiển thị khi Quest chưa được nhận.")]
    public string loiThoaiNhanQuest;

    [TextArea(3, 5)]
    [Tooltip("Nội dung/thoại hiển thị khi Quest đang thực hiện.")]
    public string loiThoaiDangLam;

    [TextArea(3, 5)]
    [Tooltip("Nội dung hiển thị khi Quest đã hoàn thành.")]
    public string loiThoaiHoanThanh;


    [Header("--- MỤC TIÊU NHIỆM VỤ ---")]

    [Tooltip("ID của loại quái cần tiêu diệt.")]
    public int idQuaiCanDiet = 1;

    [Tooltip("Số lượng quái cần tiêu diệt.")]
    public int soLuongBoXuongCanDiet = 10;


    [Header("--- PHẦN THƯỞNG ---")]

    [Tooltip("Prefab vật phẩm nhận được sau khi trả Quest.")]
    public GameObject prefabItemPhanThuong;

    [Tooltip("Số lượng vật phẩm thưởng.")]
    public int soLuongItemThuong = 1;
}