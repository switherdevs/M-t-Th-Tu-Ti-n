using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script quản lý ẩn các GameObject UI khi bấm nút Resume / Đóng (Hoạt động hoàn hảo ngay cả khi Time.timeScale = 0)
/// </summary>
public class UIResumeController : MonoBehaviour
{
    [Header("--- CẤU HÌNH TỰ ĐỘNG ẨN ---")]
    [SerializeField, Tooltip("Tích chọn nếu muốn tự động ẩn Panel/GameObject chứa script này khi bấm Resume")]
    private bool tuDongAnChinhNo = true;

    [SerializeField, Tooltip("Kéo Panel cha lớn vào đây (Nếu để trống, script sẽ tự động ẩn chính GameObject mang script này hoặc GameObject cha trực tiếp của nó)")]
    private GameObject customPanelChaToHide;

    [Header("--- DANH SÁCH OBJECT KHÁC CẦN ẨN ---")]
    [Tooltip("Kéo thêm các GameObject UI khác (như Quest Panel, Dialogue Panel,...) vào danh sách này nếu có")]
    [SerializeField] private List<GameObject> objectsToHide = new List<GameObject>();

    /// <summary>
    /// Hàm gán vào sự kiện OnClick() của Button Resume hoặc Button Close
    /// </summary>
    public void ResumeGameAndHideUI()
    {
        // 🎯 1. Rã đông thời gian ngay lập tức trước khi thực hiện bất kỳ thao tác ẩn UI nào
        Time.timeScale = 1f;

        // 2. Ẩn tất cả các object phụ trong danh sách thủ công
        if (objectsToHide != null)
        {
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // 3. Tự động ẩn Panel cha hoặc GameObject chứa script ngay lập tức
        if (tuDongAnChinhNo)
        {
            if (customPanelChaToHide != null)
            {
                customPanelChaToHide.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        Debug.Log("<color=cyan>[UI Resume]</color> Đã rã đông game và ẩn UI thành công!");
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