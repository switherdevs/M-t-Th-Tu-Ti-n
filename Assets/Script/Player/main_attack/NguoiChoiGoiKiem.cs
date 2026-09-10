using UnityEngine;
using UnityEngine.InputSystem; // BẮT BUỘC: Sử dụng New Input System của Unity

public class NguoiChoiGoiKiem : MonoBehaviour
{
    [Header("=== CẤU HÌNH INPUT SYSTEM (TỰ ĐỘNG PHÍM Q) ===")]
    // Khởi tạo sẵn binding phím Q trực tiếp trong code. KHÔNG CẦN kéo thả trên Inspector.
    private InputAction hanhDongGoiKiem = new InputAction("GoiKiem", binding: "<Keyboard>/q");

    private void OnEnable()
    {
        // Bật kích hoạt Input Action khi Script được bật
        hanhDongGoiKiem.Enable();
    }

    private void OnDisable()
    {
        // Tắt Input Action khi Script bị ẩn/hủy để tránh rác bộ nhớ
        hanhDongGoiKiem.Disable();
    }

    private void Update()
    {
        // Kiểm tra tín hiệu bấm phím Q từ New Input System
        KiemTraInputGoiKiem();
    }

    /// <summary>
    /// GHI CHÚ QUAN TRỌNG: Hàm kiểm tra phím Q và tự tìm tất cả kiếm trên bản đồ để gọi về
    /// </summary>
    public void KiemTraInputGoiKiem()
    {
        // WasPressedThisFrame() trả về true đúng 1 lần tại frame người chơi nhấn phím Q
        if (hanhDongGoiKiem.WasPressedThisFrame())
        {
            // TỰ ĐỘNG QUÉT: Tìm tất cả các thanh kiếm PhiKiemGoiVe đang tồn tại trên Scene
            PhiKiemGoiVe[] danhSachKiemOnMap = FindObjectsByType<PhiKiemGoiVe>(FindObjectsSortMode.None);

            // Ra lệnh cho từng thanh kiếm bắt đầu bay về vị trí Player
            foreach (PhiKiemGoiVe thanhKiem in danhSachKiemOnMap)
            {
                if (thanhKiem != null && !thanhKiem.LaDangBayVe)
                {
                    // Truyền Transform của Player (transform) làm điểm đích bay về
                    thanhKiem.BatDauBayVe(transform);
                }
            }
        }
    }
}