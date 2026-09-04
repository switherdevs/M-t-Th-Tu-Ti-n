using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script quản lý ẩn các GameObject UI khi bấm nút Resume / Đóng (Đã tối ưu lỗi Click Button)
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
        // 1. Luôn trả Time.timeScale về 1 TRƯỚC TIÊN để khôi phục luồng game
        Time.timeScale = 1f;

        if (objectsToHide == null || objectsToHide.Count == 0)
        {
            Debug.LogWarning("<color=yellow>[UI Resume]</color> Danh sách UI ẩn bị rỗng!");
            return;
        }

        // 2. Tách UI chứa script này (Self/Parent Panel) ra để ẩn sau cùng, tránh gãy sự kiện Button mid-frame
        GameObject selfParent = this.gameObject;

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null && obj != selfParent)
            {
                obj.SetActive(false);
            }
        }

        // 3. Cuối cùng mới ẩn chính GameObject chứa Script/Panel chứa Button này
        if (objectsToHide.Contains(selfParent))
        {
            selfParent.SetActive(false);
        }

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