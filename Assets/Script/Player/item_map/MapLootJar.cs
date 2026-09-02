using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLootJar : MonoBehaviour, IDamageable
{
    [Header("--- THÔNG SỐ CÁI LU ---")]
    [Tooltip("Máu của cái lu")]
    [SerializeField] private float health = 10f;

    [Header("--- HIỆU ỨNG VỠ (MẢNG NGẪU NHIÊN) ---")]
    [Tooltip("Kéo thả các Prefab hiệu ứng vỡ vào đây, game sẽ chọn ngẫu nhiên 1 hiệu ứng khi bể")]
    [SerializeField] private List<GameObject> danhSachVFXVoLu = new List<GameObject>();

    [Tooltip("Thời gian tự hủy của hiệu ứng vỡ (giây)")]
    [SerializeField] private float vfxDestroyTime = 2f;

    [Header("--- DANH SÁCH ITEM RỚT NGẪU NHIÊN ---")]
    [Tooltip("Kéo thả các Prefab Item Map (Máu/Năng lượng) vào đây")]
    [SerializeField] private List<GameObject> danhSachItemRot = new List<GameObject>();

    [Tooltip("Tỷ lệ rớt Item (từ 0.0 đến 1.0, ví dụ 0.8 = 80%)")]
    [Range(0f, 1f)]
    [SerializeField] private float tyLeRotItem = 0.8f;

    private bool daVo = false;

    /// <summary>
    /// Nhận sát thương khi người chơi bắn trúng (Triển khai interface IDamageable)
    /// </summary>
    public void TakeDamage(float rawDamage)
    {
        if (daVo || rawDamage <= 0) return;

        health -= rawDamage;

        if (health <= 0)
        {
            VoLu();
        }
    }

    private void VoLu()
    {
        daVo = true;

        // 1. Hiện hiệu ứng vỡ ngẫu nhiên từ danh sách
        TaoHieuUngVoNgauNhien();

        // 2. Tính toán và rớt Item ngẫu nhiên
        RotItemNgauNhien();

        // 3. Phá hủy cái lu
        Destroy(gameObject);
    }

    /// <summary>
    /// Chọn ngẫu nhiên 1 hiệu ứng vỡ từ mảng danhSachVFXVoLu và tạo ra tại vị trí Cái Lu
    /// </summary>
    private void TaoHieuUngVoNgauNhien()
    {
        if (danhSachVFXVoLu == null || danhSachVFXVoLu.Count == 0) return;

        // Chọn index ngẫu nhiên trong mảng hiệu ứng
        int randomIndex = Random.Range(0, danhSachVFXVoLu.Count);
        GameObject vfxSelected = danhSachVFXVoLu[randomIndex];

        if (vfxSelected != null)
        {
            GameObject vfxInstance = Instantiate(vfxSelected, transform.position, Quaternion.identity);
            Destroy(vfxInstance, vfxDestroyTime);
        }
    }

    private void RotItemNgauNhien()
    {
        if (danhSachItemRot == null || danhSachItemRot.Count == 0) return;

        // Kiểm tra tỷ lệ rớt
        float randomRoll = Random.value;
        if (randomRoll <= tyLeRotItem)
        {
            // Chọn ngẫu nhiên 1 item trong mảng do bạn quy định
            int randomIndex = Random.Range(0, danhSachItemRot.Count);
            GameObject itemSelected = danhSachItemRot[randomIndex];

            if (itemSelected != null)
            {
                Instantiate(itemSelected, transform.position, Quaternion.identity);
            }
        }
    }
}