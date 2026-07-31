using UnityEngine;

namespace StatsSystem.UI
{
    public class UIFreezeFlip : MonoBehaviour
    {
        private Vector3 initialScale;

        private void Awake()
        {
            // Lưu lại Scale ban đầu của Canvas/Slider UI (VD: 0.5, 0.5, 0.5)
            initialScale = transform.localScale;
        }

        private void LateUpdate()
        {
            if (transform.parent == null) return;

            // Lấy Scale hiện tại của nhân vật cha
            Vector3 parentScale = transform.parent.lossyScale;

            // Thuật toán triệt tiêu Scale của cha: 
            // Scale mong muốn = Scale ban đầu / Scale của Cha
            // Giúp Canvas LUÔN LUÔN giữ nguyên Scale dương tuyệt đối trong Thế Giới (World Space)
            float newScaleX = Mathf.Abs(initialScale.x) / (parentScale.x < 0 ? -1f : 1f);
            float newScaleY = Mathf.Abs(initialScale.y) / (parentScale.y < 0 ? -1f : 1f);

            transform.localScale = new Vector3(newScaleX, newScaleY, initialScale.z);
        }
    }
}