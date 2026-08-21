using System.Collections.Generic;
using UnityEngine;

public class DotPhaSimpleManager : MonoBehaviour
{
    [Header("--- DANH SÁCH CÁC BẢNG UI ĐỘT PHÁ ---")]
    [Tooltip("Kéo tất cả các Bảng UI (DotPha_0, DotPha_UI_1,...) vào đây theo thứ tự")]
    public List<GameObject> danhSachBangUI = new List<GameObject>();

    private int indexHienTai = 0;

    private void OnEnable()
    {
        // Khi mở bảng lên, luôn hiển thị bảng đầu tiên (Index 0)
        indexHienTai = 0;
        CapNhatHienThiBang();
    }

    /// <summary>
    /// Gán vào OnClick() của Mũi Tên Phải
    /// </summary>
    public void NutSangBangPhai()
    {
        if (indexHienTai < danhSachBangUI.Count - 1)
        {
            indexHienTai++;
            CapNhatHienThiBang();
        }
    }

    /// <summary>
    /// Gán vào OnClick() của Mũi Tên Trái
    /// </summary>
    public void NutSangBangTrai()
    {
        if (indexHienTai > 0)
        {
            indexHienTai--;
            CapNhatHienThiBang();
        }
    }

    private void CapNhatHienThiBang()
    {
        // Vòng lặp kiểm tra từng Bảng trong danh sách
        for (int i = 0; i < danhSachBangUI.Count; i++)
        {
            if (danhSachBangUI[i] != null)
            {
                // Nếu đúng chỉ số Index hiện tại thì BẬT (true), còn lại TẮT (false)
                bool laBangChon = (i == indexHienTai);
                danhSachBangUI[i].SetActive(laBangChon);
            }
        }
    }
}