using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Bắt buộc phải có Collider2D để nhận click chuột
public class NPCWaypointController : MonoBehaviour
{
    // =========================================================
    // VỊ TRÍ TUẦN TRA (A & B)
    // =========================================================

    [Header("===== VỊ TRÍ TUẦN TRA =====")]
    [Tooltip("Điểm mốc A (Kéo Transform A vào đây)")]
    [SerializeField] private Transform pointA;

    [Tooltip("Điểm mốc B (Kéo Transform B vào đây)")]
    [SerializeField] private Transform pointB;

    [Tooltip("Tốc độ di chuyển của NPC")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Khoảng cách tối thiểu để tính là đã chạm mốc (Mét)")]
    [SerializeField] private float reachThreshold = 0.1f;


    // =========================================================
    // HỘP THOẠI & ANIMATION
    // =========================================================

    [Header("===== HỘP THOẠI & ANIMATION =====")]
    [Tooltip("Game Object chứa Giao diện/Hộp thoại thoại (UI)")]
    [SerializeField] private GameObject dialogueUI;

    [Tooltip("Tên Parameter Bool trong Animator dùng cho Animation Move/Walk")]
    [SerializeField] private string moveAnimBool = "IsMoving";

    [SerializeField] private Animator animator;


    // =========================================================
    // BIẾN NỘI BỘ (PRIVATE)
    // =========================================================

    private Transform currentTarget;
    private bool isInteracting = false;


    private void Awake()
    {
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
        // Điểm đến ban đầu sẽ là B
        if (pointB != null)
        {
            currentTarget = pointB;
        }

        // Tắt thoại khi mới bắt đầu
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        SetMoveAnimation(true);
    }

    private void Update()
    {
        // Nếu đang tương tác với người chơi
        if (isInteracting)
        {
            MonitorDialogueState();
            return;
        }

        // Nếu chưa tương tác thì di chuyển qua lại
        PatrolBetweenPoints();
    }


    // =========================================================
    // THUẬT TOÁN DI CHUYỂN QUAN LẠI A - B
    // =========================================================

    private void PatrolBetweenPoints()
    {
        if (pointA == null || pointB == null || currentTarget == null) return;

        // Tính toán di chuyển từ vị trí hiện tại đến target
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        // Quay mặt Sprite NPC theo hướng đi
        float directionX = currentTarget.position.x - transform.position.x;
        FlipSprite(directionX);

        SetMoveAnimation(true);

        // Kiểm tra xem NPC đã tới gần điểm mốc chưa
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= reachThreshold)
        {
            // Đổi mục tiêu qua lại giữa A và B
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
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

        // Bật Game Object thoại
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }
    }

    private void MonitorDialogueState()
    {
        // Kiểm tra xem Game Object thoại đã bị tắt (đóng thoại) chưa
        if (dialogueUI != null && !dialogueUI.activeSelf)
        {
            // Kết thúc nói chuyện -> Tiếp tục đi bộ
            isInteracting = false;
            SetMoveAnimation(true);
        }
    }


    // =========================================================
    // HELPER SUPPORT
    // =========================================================

    private void FlipSprite(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);
            transform.localScale = scale;
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
        // Vẽ đường nối giữa A và B trên Scene view để trực quan
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, reachThreshold);
            Gizmos.DrawWireSphere(pointB.position, reachThreshold);
        }
    }
}