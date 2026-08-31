using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StatsSystem.Components;

[Serializable]
public class DoKhoData
{
    [Tooltip("Tên độ khó (VD: Dễ, Thường, Khó, Ác Mộng)")]
    public string tenDoKho = "Thường";

    [Tooltip("Hệ số nhân EXP (VD: 1 = 100%, 1.5 = 150%, 2 = 200%)")]
    public float heSoNhanExp = 1.0f;
}

public class DifficultyManager : MonoBehaviour
{
    [Header("--- DANH SÁCH MẢNG ĐỘ KHÓ ---")]
    [Tooltip("Mảng thiết lập các mức độ khó trong game")]
    public DoKhoData[] mangDoKho;

    [Header("--- TÙY CHỌN ĐỘ KHÓ HỆ THỐNG ---")]
    [Tooltip("Vị trí độ khó được chọn trong Mảng (Index bắt đầu từ 0)")]
    public int indexDoKhoChon = 0;

    private void Start()
    {
        ApDungExpDoKhoChoToanBoQuai();
    }

    /// <summary>
    /// Tìm tất cả quái có script EnemyController và cộng dồn EXP theo độ khó được chọn
    /// </summary>
    public void ApDungExpDoKhoChoToanBoQuai()
    {
        // 1. Kiểm tra mảng độ khó hợp lệ
        if (mangDoKho == null || mangDoKho.Length == 0)
        {
            Debug.LogWarning("[DifficultyManager] Mảng độ khó đang bị trống!");
            return;
        }

        // 2. Ép vị trí chọn không bị vượt quá độ dài mảng
        indexDoKhoChon = Mathf.Clamp(indexDoKhoChon, 0, mangDoKho.Length - 1);
        DoKhoData doKhoHienTai = mangDoKho[indexDoKhoChon];

        // 3. Tìm tất cả GameObject có chứa script EnemyController trong Scene
        EnemyController[] danhSachQuai = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        // 4. Vòng lặp duyệt qua từng quái để dồn EXP
        foreach (EnemyController quai in danhSachQuai)
        {
            if (quai != null)
            {
                quai.NhanExpTheoDoKho(doKhoHienTai.heSoNhanExp);
            }
        }

        Debug.Log($"<color=green>[DifficultyManager]</color> Đã dồn EXP cho {danhSachQuai.Length} quái theo độ khó: <color=yellow>{doKhoHienTai.tenDoKho}</color> (Hệ số x{doKhoHienTai.heSoNhanExp})");
    }
}