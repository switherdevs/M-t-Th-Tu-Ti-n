using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Bắt buộc phải có Collider2D để nhận click chuột
public class NPCWaypointController : MonoBehaviour
{
    // =========================================================
    // TRẠNG THÁI NPC
    // =========================================================

    [Header("===== CẤU HÌNH TRẠNG THÁI =====")]
    [Tooltip("Tích vào đây nếu muốn NPC chỉ đứng yên 1 chỗ (không bao giờ di chuyển)")]
    [SerializeField] private bool isIdle = false;

    [Tooltip("Tích vào đây nếu muốn NPC đi hết mảng điểm mốc thì dừng hẳn (không đi ngược lại)")]
    [SerializeField] private bool noLoop = false;

    [Tooltip("Tích vào đây nếu muốn quay mặt bằng Rotation Y (0/180 độ). Bỏ tích nếu muốn dùng Sprite FlipX.")]
    [SerializeField] private bool useRotationFlip = true;


    // =========================================================
    // MẢNG VỊ TRÍ TUẦN TRA (WAYPOINTS)
    // =========================================================

    [Header("===== MẢNG VỊ TRÍ TUẦN TRA =====")]
    [Tooltip("Danh sách các điểm mốc NPC sẽ đi qua theo thứ tự")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Tốc độ di chuyển của NPC")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Khoảng cách tối thiểu để tính là đã chạm mốc (Mét)")]
    [SerializeField] private float reachThreshold = 0.1f;


    // =========================================================
    // HỘP THOẠI & ANIMATION
    // =========================================================

    [Header("===== HỘP THOẠI & ANIMATION =====")]
    [Tooltip("Danh sách các Game Object UI/Hộp thoại sẽ hiển thị khi click vào NPC")]
    [SerializeField] private GameObject[] dialogueUIObjects;

    [Tooltip("Tên Parameter Bool trong Animator dùng cho Animation Move/Walk")]
    [SerializeField] private string moveAnimBool = "IsMoving";

    [SerializeField] private Animator animator;


    // =========================================================
    // BIẾN NỘI BỘ (PRIVATE)
    // =========================================================

    private int waypointIndex = 0;
    private bool movingForward = true; // true: đang đi tiến từ đầu -> cuối, false: đang đi lùi từ cuối -> đầu
    private bool isInteracting = false;
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }

    private void Start()
    {
        // Tắt toàn bộ mảng UI thoại khi mới bắt đầu
        SetDialogueUIActive(false);

        // Nếu không tick isIdle và có mảng điểm mốc thì bắt đầu di chuyển
        SetMoveAnimation(!isIdle && waypoints != null && waypoints.Length > 0);
    }

    private void Update()
    {
        // Nếu NPC được tích đứng yên (isIdle) Hoặc đang tương tác thoại
        if (isIdle || isInteracting)
        {
            SetMoveAnimation(false); // Đảm bảo luôn giữ trạng thái Idle

            if (isInteracting)
            {
                MonitorDialogueState();
            }
            return; // Dừng không chạy code di chuyển bên dưới
        }

        // Nếu không đứng yên và không tương tác thì di chuyển qua các điểm
        PatrolWaypoints();
    }


    // =========================================================
    // THUẬT TOÁN DI CHUYỂN THEO MẢNG WAYPOINTS
    // =========================================================

    private void PatrolWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetPoint = waypoints[waypointIndex];
        if (targetPoint == null) return;

        // Tính toán di chuyển từ vị trí hiện tại đến targetPoint
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // Quay mặt Sprite NPC theo hướng đi
        float directionX = targetPoint.position.x - transform.position.x;
        FlipSprite(directionX);

        SetMoveAnimation(true);

        // Kiểm tra xem NPC đã tới gần điểm mốc hiện tại chưa
        float distanceToTarget = Vector2.Distance(transform.position, targetPoint.position);
        if (distanceToTarget <= reachThreshold)
        {
            UpdateNextWaypoint();
        }
    }

    private void UpdateNextWaypoint()
    {
        // Kiểm tra nếu bật noLoop và đã đi tới điểm cuối mảng
        if (noLoop && waypointIndex >= waypoints.Length - 1)
        {
            isIdle = true; // Dừng di chuyển và chuyển hẳn về Idle
            SetMoveAnimation(false);
            return;
        }

        // Xử lý đảo chiều di chuyển khi đụng đầu hoặc cuối mảng (Ping-Pong)
        if (movingForward)
        {
            if (waypointIndex >= waypoints.Length - 1)
            {
                movingForward = false;
                waypointIndex--;
            }
            else
            {
                waypointIndex++;
            }
        }
        else
        {
            if (waypointIndex <= 0)
            {
                movingForward = true;
                waypointIndex++;
            }
            else
            {
                waypointIndex--;
            }
        }
    }


    // =========================================================
    // SỰ KIỆN CLICK CHUỘT VÀO NPC
    // =========================================================

    private void OnMouseDown()
    {
        // Khi người chơi click vào NPC, dừng lại và nói chuyện
        StartInteraction();
    }

    private void StartInteraction()
    {
        isInteracting = true;

        // Dừng Animation di chuyển, chuyển về Idle
        SetMoveAnimation(false);

        // Bật toàn bộ các Game Object UI có trong mảng
        SetDialogueUIActive(true);
    }

    private void MonitorDialogueState()
    {
        // Kiểm tra xem tất cả Game Object thoại trong mảng đã bị tắt hết chưa
        if (!IsAnyDialogueActive())
        {
            // Kết thúc nói chuyện
            isInteracting = false;

            // Nếu NPC không bị khóa đứng yên (isIdle = false) thì bật lại anim di chuyển
            if (!isIdle)
            {
                SetMoveAnimation(true);
            }
        }
    }


    // =========================================================
    // HELPER SUPPORT & XỬ LÝ MẢNG UI
    // =========================================================

    // Duyệt qua mảng và bật/tắt toàn bộ UI
    private void SetDialogueUIActive(bool active)
    {
        if (dialogueUIObjects == null || dialogueUIObjects.Length == 0) return;

        foreach (GameObject uiObject in dialogueUIObjects)
        {
            if (uiObject != null)
            {
                uiObject.SetActive(active);
            }
        }
    }

    // Kiểm tra xem có ít nhất 1 UI trong mảng còn đang bật hay không
    private bool IsAnyDialogueActive()
    {
        if (dialogueUIObjects == null || dialogueUIObjects.Length == 0) return false;

        foreach (GameObject uiObject in dialogueUIObjects)
        {
            if (uiObject != null && uiObject.activeSelf)
            {
                return true; // Vẫn còn ít nhất 1 UI đang mở
            }
        }

        return false; // Tất cả UI đều đã đóng
    }

    // Xoay hướng nhân vật bằng Rotation Y hoặc Sprite FlipX
    private void FlipSprite(float directionX)
    {
        if (Mathf.Abs(directionX) <= 0.01f) return;

        if (useRotationFlip)
        {
            // Xoay bằng góc Rotation Y (0 độ đi sang phải, 180 độ đi sang trái)
            if (directionX < 0f)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else if (spriteRenderer != null)
        {
            // Lật bằng flipX
            spriteRenderer.flipX = directionX < 0f;
        }
    }

    private void SetMoveAnimation(bool isMoving)
    {
        if (animator != null && !string.IsNullOrEmpty(moveAnimBool))
        {
            animator.SetBool(moveAnimBool, isMoving);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ đường nối giữa các waypoint trên Scene view để dễ hình dung
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, reachThreshold);
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
    }
}