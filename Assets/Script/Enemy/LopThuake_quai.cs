using UnityEngine;

// Lớp trừu tượng quản lý di chuyển và Animation cơ bản của mọi loại quái
public abstract class Enemy : MonoBehaviour
{
    [Header("Cấu Hình Di Chuyển")]
    [SerializeField] protected float enemyMoveSpeed = 1f;

    [Header("Cấu Hình Tên Animation (Có thể đổi tên trên Inspector)")]
    [SerializeField] protected string walkAnimName = "IsWalking";
    [SerializeField] protected string attackAnimName = "Attack";
    [SerializeField] protected string dieAnimName = "Die";

    protected Player player;
    protected SpriteRenderer spriteRenderer;
    protected Animator anim;

    // Các biến lưu mã Hash của Animation để tối ưu hiệu năng
    private int walkAnimHash;
    private int attackAnimHash;
    private int dieAnimHash;

    protected virtual void Start()
    {
        // Tự động lấy các Component
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Tự động tìm kiếm Player đang có mặt trên Map
        player = FindAnyObjectByType<Player>();

        // Khởi tạo Hash ID cho Animation
        InitAnimationHashes();
    }

    protected virtual void Update()
    {
        MoveToPlayer();
    }

    // Thuật toán khởi tạo mã Hash cho Animation
    private void InitAnimationHashes()
    {
        if (!string.IsNullOrEmpty(walkAnimName)) walkAnimHash = Animator.StringToHash(walkAnimName);
        if (!string.IsNullOrEmpty(attackAnimName)) attackAnimHash = Animator.StringToHash(attackAnimName);
        if (!string.IsNullOrEmpty(dieAnimName)) dieAnimHash = Animator.StringToHash(dieAnimName);
    }

    // Thuật toán kiểm tra an toàn xem Animator có chứa Tham Số (Parameter) đó không
    protected bool HasParameter(int paramHash)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.nameHash == paramHash) return true;
        }
        return false;
    }

    // Thuật toán di chuyển tịnh tiến thẳng về phía người chơi
    protected void MoveToPlayer()
    {
        if (player != null)
        {
            // Di chuyển vị trí của quái tiến lại gần vị trí của player
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                enemyMoveSpeed * Time.deltaTime
            );

            FlipEnemy();

            // Cập nhật Animation Đi bộ (Nếu có tham số trong Animator)
            if (HasParameter(walkAnimHash))
            {
                anim.SetBool(walkAnimHash, true);
            }
        }
        else
        {
            // Tắt Animation Đi bộ khi không tìm thấy Player
            if (HasParameter(walkAnimHash))
            {
                anim.SetBool(walkAnimHash, false);
            }
        }
    }

    // Thuật toán quay mặt quái theo hướng Player
    protected void FlipEnemy()
    {
        if (player != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = player.transform.position.x < transform.position.x;
        }
    }

    // --- CÁC HÀM GỌI ANIMATION DỰ PHÒNG (Sử dụng sau này khi làm Combat) ---

    // Gọi Animation Tấn Công
    public virtual void TriggerAttackAnimation()
    {
        if (HasParameter(attackAnimHash))
        {
            anim.SetTrigger(attackAnimHash);
        }
    }

    // Gọi Animation Chết
    public virtual void TriggerDieAnimation()
    {
        if (HasParameter(dieAnimHash))
        {
            anim.SetTrigger(dieAnimHash);
        }
    }
}