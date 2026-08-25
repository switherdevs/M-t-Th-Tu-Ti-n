using UnityEngine;

/// <summary>
/// Khóa góc quay World Space Canvas và tự động sửa lỗi bị che/mất UI trên Game View.
/// </summary>
public class KeepCanvasFacingCamera : MonoBehaviour
{
    private Transform targetParent;
    private Quaternion fixedRotation;
    private Vector3 offset;

    private void Start()
    {
        targetParent = transform.parent;
        fixedRotation = transform.rotation;

        if (targetParent != null)
        {
            // Tính khoảng cách lệch offset chính xác trước khi tách
            offset = transform.position - targetParent.position;

            // SỬA LỖI: Thêm đối số 'true' (worldPositionStays) để Canvas KHÔNG BỊ VĂNG VỊ TRÍ khi tách khỏi Quái
            transform.SetParent(null, true);
        }

        // Tự động điều chỉnh Canvas hiển thị đè lên trên cùng của Game View
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999; // Đưa lên mức cao nhất để không bị Tilemap/Background che
        }
    }

    private void LateUpdate()
    {
        if (targetParent != null)
        {
            // Cập nhật vị trí đi theo Quái
            transform.position = targetParent.position + offset;

            // Khóa cứng góc quay
            transform.rotation = fixedRotation;
        }
        else
        {
            // Tự hủy khi Quái chết
            Destroy(gameObject);
        }
    }
}