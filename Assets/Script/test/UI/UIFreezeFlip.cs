using UnityEngine;

namespace StatsSystem.UI
{
    public class UIFreezeFlip : MonoBehaviour
    {
        private Vector3 initialScale;

        private void Awake()
        {
            // Lưu lại Scale ban đầu của Canvas/Slider UI
            initialScale = transform.localScale;
        }

        private void LateUpdate()
        {
            if (transform.parent == null) return;

            // Lấy Scale thực tế của Nhân vật cha (lossyScale)
            Vector3 parentScale = transform.parent.lossyScale;

            // Đảo ngược lại Scale X của UI nếu Cha bị lật âm (-1)
            float newScaleX = initialScale.x;
            if (parentScale.x < 0)
            {
                newScaleX = -initialScale.x;
            }

            // Giữ nguyên Scale Y và Z
            transform.localScale = new Vector3(newScaleX, initialScale.y, initialScale.z);
        }
    }
}