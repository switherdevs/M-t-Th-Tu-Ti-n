using UnityEngine;

public class FixHealthBarRotation : MonoBehaviour
{
    private Vector3 initialScale;

    private void Start()
    {
        // Lưu lại Scale ban đầu của Canvas
        initialScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (transform.parent == null) return;

        // Ép Canvas giữ nguyên góc xoay chuẩn không bị quay Y = 180
        transform.rotation = Quaternion.identity;

        // Triệt tiêu lỗi bị Scale âm (Lật ngược) từ quái vật cha
        Vector3 parentScale = transform.parent.localScale;

        transform.localScale = new Vector3(
            Mathf.Abs(initialScale.x) * (parentScale.x < 0 ? -1 : 1),
            initialScale.y,
            initialScale.z
        );

        // Đảm bảo Pos Z luôn luôn nổi lên trước Sprite quái
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(currentPos.x, currentPos.y, -0.1f);
    }
}