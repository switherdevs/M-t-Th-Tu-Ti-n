using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script quản lý ẩn các GameObject UI khi bấm nút Resume / Đóng
/// </summary>
public class UIResumeController : MonoBehaviour
{
    [Header("--- DANH SÁCH OBJECT CẦN ẨN KHI RESUME ---")]
    [Tooltip("Kéo các GameObject UI (như Pause Panel, Quest Panel, Dialogue Panel,...) vào danh sách này")]
    [SerializeField] private List<GameObject> objectsToHide = new List<GameObject>();

    /// <summary>
    /// Hàm gán vào sự kiện OnClick() của Button Resume hoặc Button Close
    /// </summary>
    public void ResumeGameAndHideUI()
    {
        // Duyệt qua tất cả GameObject trong danh sách và tắt chúng (SetActive = false)
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Đảm bảo Time.timeScale trở lại 1 nếu trước đó game có Paused
        Time.timeScale = 1f;

        Debug.Log("<color=cyan>[UI Resume]</color> Đã ẩn các UI được gán và tiếp tục Game.");
    }

    /// <summary>
    /// Hàm bổ sung nếu muốn thêm 1 Object mới vào danh sách ẩn bằng Code
    /// </summary>
    public void AddObjectToHideList(GameObject targetObj)
    {
        if (targetObj != null && !objectsToHide.Contains(targetObj))
        {
            objectsToHide.Add(targetObj);
        }
    }
}