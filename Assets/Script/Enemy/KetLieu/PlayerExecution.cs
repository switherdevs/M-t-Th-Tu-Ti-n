using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public struct CameraShakeTiming
{
    [Tooltip("Thời điểm bắt đầu rung tính từ lúc bắt đầu Execution (giây)")]
    public float delayTime;
    [Tooltip("Cường độ rung (Amplitude Gain)")]
    public float impulseForce;
    [Tooltip("Thời gian duy trì đợt rung này (giây)")]
    public float duration;
}

public class PlayerExecution : MonoBehaviour
{
    [Header("=== CẤU HÌNH KẾT LIỄU ===")]
    [Tooltip("Khoảng cách tối đa để bấm phím E kết liễu")]
    [SerializeField] private float executeRange = 2.5f;
    [Tooltip("Tốc độ Player lướt/tiến đến vị trí kết liễu")]
    [SerializeField] private float moveToTargetSpeed = 15f;

    [Header("=== ANIMATION PARAMETERS ===")]
    [Tooltip("Tên tham số Bool điều khiển Animation chạy/di chuyển")]
    [SerializeField] private string moveBoolName = "IsMoving";
    [Tooltip("Tên Trigger Animation kết liễu của Player")]
    [SerializeField] private string executionAnimName = "Execute";

    [Header("=== LAYER MỤC TIÊU ===")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("=== CẤU HÌNH CAMERA TARGET & ZOOM ===")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform defaultCamTarget;
    [SerializeField] private CinemachineMouseTarget mouseTargetScript;

    [Space(5)]
    [SerializeField] private float defaultLensSize = 5f;
    [SerializeField] private float executionLensSize = 3f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("=== CẤU HÌNH RUNG CAM (PERLIN NOISE) ===")]
    [SerializeField] private CameraShakeTiming[] shakeTimings;

    private Animator anim;
    private PlayerController playerMovementScript;
    private Luot playerDashScript;
    private TanCong playerAttackScript;
    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private bool isExecuting = false;
    private Coroutine zoomCoroutine;

    public bool IsExecuting => isExecuting;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovementScript = GetComponent<PlayerController>();
        playerDashScript = GetComponent<Luot>();
        playerAttackScript = GetComponent<TanCong>();

        if (virtualCamera != null)
        {
            perlinNoise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Update()
    {
        if (isExecuting) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryExecuteEnemy();
        }
    }

    private void TryExecuteEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, executeRange, enemyLayer);

        foreach (var hit in hits)
        {
            ExecutableEnemy executable = hit.GetComponentInParent<ExecutableEnemy>();
            if (executable != null && executable.IsCanBeExecuted)
            {
                StartCoroutine(ProcessExecutionSequence(executable, null));
                break;
            }

            BossExecution_TongQuan bossExecutable = hit.GetComponentInParent<BossExecution_TongQuan>();
            if (bossExecutable != null && bossExecutable.IsCanBeExecuted)
            {
                StartCoroutine(ProcessExecutionSequence(null, bossExecutable));
                break;
            }
        }
    }

    private IEnumerator ProcessExecutionSequence(ExecutableEnemy targetEnemy, BossExecution_TongQuan targetBoss)
    {
        isExecuting = true;

        // PHÁT TÍN HIỆU TOÀN MAP CHO QUÁI VÀO TRẠNG THÁI SỢ HÃI
        if (PlayerExecutionManager.Instance != null)
        {
            PlayerExecutionManager.Instance.BatDauExecution();
        }

        // Tắt điều khiển Player
        if (playerMovementScript != null)
        {
            playerMovementScript.StopMovementAndAnimation();
            playerMovementScript.enabled = false;
        }
        if (playerDashScript != null) playerDashScript.enabled = false;
        if (playerAttackScript != null) playerAttackScript.enabled = false;

        Transform executionPoint = targetEnemy != null ? targetEnemy.ExecutionPoint : targetBoss.ExecutionPoint;
        Transform targetTransform = targetEnemy != null ? targetEnemy.transform : targetBoss.transform;

        if (anim != null && !string.IsNullOrEmpty(moveBoolName))
        {
            anim.SetBool(moveBoolName, true);
        }

        Vector3 targetPos = executionPoint.position;
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            targetPos = executionPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveToTargetSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        if (anim != null && !string.IsNullOrEmpty(moveBoolName))
        {
            anim.SetBool(moveBoolName, false);
        }

        bool faceRight = targetTransform.position.x >= transform.position.x;
        transform.eulerAngles = faceRight ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 180f, 0f);

        SwitchCameraTarget(executionPoint);
        StartZoomCamera(executionLensSize);

        if (anim != null && !string.IsNullOrEmpty(executionAnimName))
        {
            anim.SetTrigger(executionAnimName);
        }

        StartCoroutine(ProcessCameraShakeSequence());

        if (targetEnemy != null)
        {
            targetEnemy.Execute(transform, OnExecutionFinished);
        }
        else if (targetBoss != null)
        {
            targetBoss.ExecuteBoss(transform, OnExecutionFinished);
        }
    }

    private IEnumerator ProcessCameraShakeSequence()
    {
        if (shakeTimings == null || shakeTimings.Length == 0 || perlinNoise == null) yield break;

        float elapsedTime = 0f;
        for (int i = 0; i < shakeTimings.Length; i++)
        {
            float waitTime = shakeTimings[i].delayTime - elapsedTime;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
                elapsedTime += waitTime;
            }

            perlinNoise.AmplitudeGain = shakeTimings[i].impulseForce;
            yield return new WaitForSeconds(shakeTimings[i].duration);
            elapsedTime += shakeTimings[i].duration;
            perlinNoise.AmplitudeGain = 0f;
        }
    }

    private void StartZoomCamera(float targetSize)
    {
        if (virtualCamera == null) return;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomCameraRoutine(targetSize));
    }

    private IEnumerator ZoomCameraRoutine(float targetSize)
    {
        while (Mathf.Abs(virtualCamera.Lens.OrthographicSize - targetSize) > 0.01f)
        {
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, targetSize, zoomSpeed * Time.deltaTime);
            yield return null;
        }
        virtualCamera.Lens.OrthographicSize = targetSize;
    }

    private void SwitchCameraTarget(Transform newTarget)
    {
        if (mouseTargetScript != null) mouseTargetScript.SetExecutingState(true);
        if (virtualCamera != null && newTarget != null) virtualCamera.Follow = newTarget;
    }

    private void OnExecutionFinished()
    {
        isExecuting = false;

        // PHÁT TÍN HIỆU KẾT THÚC EXECUTION DỰNG QUÁI DẬY KÈM KNOCKBACK
        if (PlayerExecutionManager.Instance != null)
        {
            PlayerExecutionManager.Instance.KetThucExecution();
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            playerMovementScript.ForceRefreshRotation();
        }

        if (playerDashScript != null) playerDashScript.enabled = true;
        if (playerAttackScript != null) playerAttackScript.enabled = true;
        if (mouseTargetScript != null) mouseTargetScript.SetExecutingState(false);

        if (virtualCamera != null && defaultCamTarget != null) virtualCamera.Follow = defaultCamTarget;

        StartZoomCamera(defaultLensSize);
        if (perlinNoise != null) perlinNoise.AmplitudeGain = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, executeRange);
    }
}