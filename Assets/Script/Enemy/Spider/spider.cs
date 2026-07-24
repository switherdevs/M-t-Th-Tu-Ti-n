using UnityEngine;

public class SpiderEnemy : MonoBehaviour
{
    [Header("Đạn độc")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootCooldown = 2f;


    [Header("Phát hiện Player")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private LayerMask playerLayer;


    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 2f;


    [Header("Tấn công")]
    [SerializeField] private float attackRange = 2f;


    private Transform player;
    private Animator animator;

    private float shootTimer;


    private void Start()
    { 
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        // Vòng 1: Phát hiện Player
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer
        );


        if (playerCollider != null)
        {
            // Lưu Player
            player = playerCollider.transform;


            // Vòng 2: Kiểm tra tầm tấn công
            Collider2D attackCollider = Physics2D.OverlapCircle(
                transform.position,
                attackRange,
                playerLayer
            );


            if (attackCollider != null)
            {
                // Player trong vùng bắn
                animator.SetBool("isWalking", false);


                shootTimer -= Time.deltaTime;


                if (shootTimer <= 0)
                {
                    Shoot();
                    shootTimer = shootCooldown;
                }
            }
            else
            {
                // Player chưa tới vùng bắn thì chạy theo
                animator.SetBool("isWalking", true);


                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime
                );


                Debug.Log("Đang tiến tới Player");
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }



    private void Shoot()
    {
        Debug.Log("Spider bắn độc");


        GameObject bullet = Instantiate(
            enemyBullet,
            firePoint.position,
            Quaternion.identity
        );


        // Nếu đạn có script điều hướng
        EnemyBullet bulletScript =
            bullet.GetComponent<EnemyBullet>();


        if (bulletScript != null)
        {
            Vector2 direction =
                (player.position - firePoint.position).normalized;


            bulletScript.SetDirection(direction);
        }
    }



    private void OnDrawGizmosSelected()
    {
        // Vòng phát hiện Player
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );
        // Vòng tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}