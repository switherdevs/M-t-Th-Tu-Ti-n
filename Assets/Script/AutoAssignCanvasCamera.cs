using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AutoAssignCanvasCamera : MonoBehaviour
{
    [Header("--- KIỂM TRA TRẠNG THÁI CAMERA ---")]
    [SerializeField] private Camera assignedCamera;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        GanCameraTrucTiep();
    }

    private void OnEnable()
    {
        GanCameraTrucTiep();
    }

    private void LateUpdate()
    {
        // Liên tục kiểm tra đề phòng trường hợp camera load chậm hơn prefab lúc vừa vào scene
        if (canvas != null && canvas.worldCamera == null)
        {
            GanCameraTrucTiep();
        }
    }

    private void GanCameraTrucTiep()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        // 1. Tìm Camera chính của Scene
        if (Camera.main != null)
        {
            assignedCamera = Camera.main;
        }
        else
        {
            // Dự phòng nếu Scene không đặt Tag MainCamera
            Camera[] allCams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in allCams)
            {
                if (cam.gameObject.activeInHierarchy)
                {
                    assignedCamera = cam;
                    break;
                }
            }
        }

        // 2. Ép gán vào Canvas
        if (assignedCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = assignedCamera;
        }
    }
}