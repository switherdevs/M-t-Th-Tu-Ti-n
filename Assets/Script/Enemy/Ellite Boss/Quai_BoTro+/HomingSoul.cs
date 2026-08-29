using UnityEngine;

public class HomingSoul : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float damage = 15f;
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null) return;
        
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var stats = collision.GetComponent<CharacterStats>();
            if (stats != null) stats.TakeDamage(damage);
            
            // Trả về Pool
            SimpleObjectPool.Instance.ReturnToPool(gameObject);
        }
    }
}