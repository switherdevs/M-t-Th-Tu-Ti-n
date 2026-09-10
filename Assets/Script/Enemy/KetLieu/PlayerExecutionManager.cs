using System;
using UnityEngine;

public class PlayerExecutionManager : MonoBehaviour
{
    // Singleton giúp quái dễ dàng đăng ký nhận sự kiện từ bất kỳ đâu
    public static PlayerExecutionManager Instance { get; private set; }

    // Cặp Event phát tín hiệu toàn cục khi Bắt đầu / Kết thúc Execution
    public event Action OnExecutionStart;
    public event Action OnExecutionEnd;

    [Header("=== TRẠNG THÁI ===")]
    [SerializeField] private bool đangExecution = false;
    public bool DangExecution => đangExecution;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi hàm này trong Animation Event hoặc Script kết liễu khi Player BẮT ĐẦU Execution
    /// </summary>
    public void BatDauExecution()
    {
        đangExecution = true;
        OnExecutionStart?.Invoke(); // Phát thông báo cho tất cả quái xung quanh
    }

    /// <summary>
    /// Gọi hàm này khi Player KẾT THÚC Execution
    /// </summary>
    public void KetThucExecution()
    {
        đangExecution = false;
        OnExecutionEnd?.Invoke(); // Phát thông báo kết thúc
    }
}