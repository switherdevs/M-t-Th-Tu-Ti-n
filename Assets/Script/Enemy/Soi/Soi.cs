using UnityEngine;
using System.Collections;

public class DogEnemy : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Detect Player")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float loseRange = 10f;

    private Transform targetPoint;
    private Transform player;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool hasHowled = false;
    private bool isChasing = false;
    private bool isReturning = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        targetPoint = pointB;

        animator.SetBool("isPatrolling", true);
        animator.SetBool("isRunning", false);
    }

    private void Update()
    {
        DetectPlayer();

        if (isReturning)
        {
            ReturnToPatrol();
        }
        else if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    //==================== PATROL ====================

    private void Patrol()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            patrolSpeed * Time.deltaTime);

        Flip(targetPoint.position.x);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
        }
    }

    //==================== DETECT ====================

    private void DetectPlayer()
    {
        if (isChasing || isReturning)
            return;

        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            detectRange,
            playerLayer);

        if (hit != null)
        {
            player = hit.transform;

            if (!hasHowled)
            {
                StartCoroutine(HowlThenChase());
            }
        }
    }

    //==================== HOWL ====================

    private IEnumerator HowlThenChase()
    {
        hasHowled = true;

        animator.SetBool("isPatrolling", false);
        animator.SetBool("isRunning", false);

        animator.SetTrigger("Howl");

        yield return new WaitForSeconds(1f);

        isChasing = true;

        animator.SetBool("isRunning", true);
    }

    //==================== CHASE ====================

    private void ChasePlayer()
    {
        if (player == null)
        {
            StartReturn();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > loseRange)
        {
            StartReturn();
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime);

        Flip(player.position.x);
    }

    //==================== RETURN ====================

    private void StartReturn()
    {
        isChasing = false;
        isReturning = true;

        animator.SetBool("isRunning", false);
        animator.SetBool("isPatrolling", true);

        player = null;
        hasHowled = false;
    }

    private void ReturnToPatrol()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPoint.position,
            patrolSpeed * Time.deltaTime);

        Flip(targetPoint.position.x);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            isReturning = false;
        }
    }

    //==================== FLIP ====================

    private void Flip(float targetX)
    {
        spriteRenderer.flipX = targetX < transform.position.x;
    }

    //==================== ATTACK ====================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();

            if (p != null)
            {
                //p.TakeDamage();
            }
        }
    }

    //==================== GIZMOS ====================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.15f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}