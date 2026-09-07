using System.Collections;
using UnityEngine;

public class PlayerExecution : MonoBehaviour
{
    [Header("=== CẤU HÌNH KẾT LIỄU ===")]
    [Tooltip("Khoảng cách tối đa để bấm phím E kết liễu")]
    [SerializeField] private float executeRange = 2.5f;
    [Tooltip("Tên Trigger Animation kết liễu của Player")]
    [SerializeField] private string executionAnimName = "Execute";

    [Header("=== LAYER MỤC TIÊU ===")]
    [SerializeField] private LayerMask enemyLayer;

    private Animator anim;
    private MonoBehaviour playerMovementScript; // Script di chuyển của Player (ví dụ: TanCong/PlayerController)
    private bool isExecuting = false;

    public bool IsExecuting => isExecuting;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        // Tự lấy script di chuyển chính trên Player để bật/tắt khi kết liễu
        playerMovementScript = GetComponent<MonoBehaviour>(); 
    }

    private void Update()
    {
        if (isExecuting) return;

        // BẤM PHÍM 'E' ĐỂ KẾT LIỄU
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryExecuteEnemy();
        }
    }

    private void TryExecuteEnemy()
    {
        // Quét tìm Boss/Quái xung quanh trong phạm vi executeRange
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, executeRange, enemyLayer);

        foreach (var hit in hits)
        {
            ExecutableEnemy executable = hit.GetComponentInParent<ExecutableEnemy>();
            if (executable != null && executable.IsCanBeExecuted)
            {
                StartExecutionProcess(executable);
                break;
            }
        }
    }

    private void StartExecutionProcess(ExecutableEnemy targetEnemy)
    {
        isExecuting = true;

        // 1. Khóa di chuyển của Player
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // 2. Hút nhẹ Player về điểm đứng chuẩn trước mặt Boss (Snap Position)
        transform.position = targetEnemy.ExecutionPoint.position;

        // Xoay mặt Player về phía Boss
        if (targetEnemy.transform.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // 3. Phát Animation kết liễu của Player
        if (anim != null)
        {
            anim.SetTrigger(executionAnimName);
        }

        // 4. Kích hoạt chuỗi kết liễu trên Boss
        targetEnemy.Execute(transform, OnExecutionFinished);
    }

    // Mở khóa Player khi hoàn thành xong kết liễu
    private void OnExecutionFinished()
    {
        isExecuting = false;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm bấm E kết liễu
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, executeRange);
    }
}