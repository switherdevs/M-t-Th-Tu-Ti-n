using UnityEngine;

// Lớp trừu tượng quản lý di chuyển cơ bản của mọi loại quái
public abstract class Enemy : MonoBehaviour
{
    [Header("Cấu Hình Di Chuyển")]
    [SerializeField] protected float enemyMoveSpeed = 1f;

    protected Player player;

    protected virtual void Start()
    {
        // Tự động tìm kiếm Player đang có mặt trên Map lúc bắt đầu game
        player = FindAnyObjectByType<Player>();
    }

    protected virtual void Update()
    {
        MoveToPlayer();
    }

    // Thuật toán di chuyển tịnh tiến thẳng về phía người chơi
    protected void MoveToPlayer()
    {
        if (player != null)
        {
            // Di chuyển vị trí của quái dần dần tiến lại gần vị trí của player
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                enemyMoveSpeed * Time.deltaTime
            );

            FlipEnemy();
        }
    }

    // Thuật toán quay mặt quái theo hướng Player
    protected void FlipEnemy()
    {
        if (player != null)
        {
            // Nếu Player nằm bên trái quái (x_player < x_quai) -> scale X = -1 (quay trái)
            // Nếu Player nằm bên phải quái (x_player >= x_quai) -> scale X = 1 (quay phải)
            float scaleX = player.transform.position.x < transform.position.x ? -1f : 1f;
            transform.localScale = new Vector3(scaleX, 1f, 1f);
        }
    }
}