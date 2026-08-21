using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine; // Nếu Unity bản cũ hiển thị lỗi đỏ ở đây, bạn đổi thành: using Cinemachine;

public class CameraControllerSlider : MonoBehaviour
{
    public static CameraControllerSlider Instance;

    [Header("--- CINEMACHINE CAMERA ---")]
    [Tooltip("Kéo CinemachineCamera / VirtualCamera vào đây")]
    public CinemachineCamera cinemachineCamera;
    // Mẹo: Nếu Unity báo lỗi kiểu dữ liệu ở trên, đổi chữ "CinemachineCamera" thành "CinemachineVirtualCamera"

    [Header("--- DANH SÁCH TARGET CAMERA (Empty GameObjects) ---")]
    [Tooltip("Kéo các điểm Target (KinhThanh_cam, DotPha_cam,...) theo đúng thứ tự từ trái sang phải vào đây")]
    public List<Transform> danhSachTarget = new List<Transform>();

    private int indexHienTai = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Tự động gán vị trí đầu tiên (Index 0) khi bắt đầu Game
        CapNhatTargetCamera();
    }

    /// <summary>
    /// Gán vào OnClick() của Nút Mũi Tên Phải (Lướt sang vị trí kế tiếp)
    /// </summary>
    public void ViTriKeTiep()
    {
        if (indexHienTai < danhSachTarget.Count - 1)
        {
            indexHienTai++;
            CapNhatTargetCamera();
        }
    }

    /// <summary>
    /// Gán vào OnClick() của Nút Mũi Tên Trái (Lướt về vị trí trước đó)
    /// </summary>
    public void ViTriTruocDo()
    {
        if (indexHienTai > 0)
        {
            indexHienTai--;
            CapNhatTargetCamera();
        }
    }

    private void CapNhatTargetCamera()
    {
        if (cinemachineCamera == null || danhSachTarget.Count == 0) return;

        Transform targetMoi = danhSachTarget[indexHienTai];

        if (targetMoi != null)
        {
            // Đổi điểm Follow của Cinemachine sang Target mới
            cinemachineCamera.Follow = targetMoi;

            // Nếu Cinemachine của bạn có xài thuộc tính LookAt thì bật thêm dòng dưới:
            // cinemachineCamera.LookAt = targetMoi;
        }
    }
} 