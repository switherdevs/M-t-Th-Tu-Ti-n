using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerSlider : MonoBehaviour
{
    public static CameraControllerSlider Instance;

    [Header("--- DANH SÁCH VỊ TRÍ CAMERA (Empty GameObjects) ---")]
    [Tooltip("Kéo các Transform vị trí (Kinh Thành, Tu Luyện, ...) vào đây")]
    public List<Transform> danhSachViTri = new List<Transform>();

    [Header("--- CẤU HÌNH TRƯỢT ---")]
    [Tooltip("Tốc độ trượt camera")]
    public float tocDoTruot = 5f;

    private int indexViTriHienTai = 0;
    private Coroutine coroutineTruotCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Hàm di chuyển Camera tới vị trí theo Index trong danh sách
    /// </summary>
    /// <param name="index">Chỉ số vị trí (0, 1, 2,...)</param>
    public void ChuyenDenViTri(int index)
    {
        if (index < 0 || index >= danhSachViTri.Count || danhSachViTri[index] == null)
        {
            Debug.LogWarning("[CameraSlider] Index vị trí không hợp lệ!");
            return;
        }

        indexViTriHienTai = index;

        if (coroutineTruotCamera != null)
        {
            StopCoroutine(coroutineTruotCamera);
        }

        coroutineTruotCamera = StartCoroutine(CoTruotCamera(danhSachViTri[index].position));
    }

    /// <summary>
    /// Hàm trượt tới vị trí kế tiếp
    /// </summary>
    public void ViTriKeTiep()
    {
        if (indexViTriHienTai < danhSachViTri.Count - 1)
        {
            ChuyenDenViTri(indexViTriHienTai + 1);
        }
    }

    /// <summary>
    /// Hàm trượt về vị trí trước
    /// </summary>
    public void ViTriTruocDo()
    {
        if (indexViTriHienTai > 0)
        {
            ChuyenDenViTri(indexViTriHienTai - 1);
        }
    }

    private IEnumerator CoTruotCamera(Vector3 viTriDich)
    {
        // Giữ nguyên trục Z của Camera (thường là -10 trong 2D)
        viTriDich.z = transform.position.z;

        while (Vector3.Distance(transform.position, viTriDich) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, viTriDich, Time.deltaTime * tocDoTruot);
            yield return null;
        }

        transform.position = viTriDich;
    }
}