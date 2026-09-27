using UnityEngine;

public class RewardMultiplier : MonoBehaviour
{
    // Singleton để dễ dàng truy cập từ StageRewardManager
    public static RewardMultiplier Instance { get; private set; }

    [Header("--- CAU HINH HE SO NHAN PHAN THUONG ---")]
    [SerializeField, Tooltip("He so nhan phan thuong (Vi du: 2 = x2 phan thuong, 1.5 = x1.5 phan thuong)")]
    private float heSoNhan = 2f;

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
    /// Ham tinh so luong phan thuong sau khi nhan he so
    /// </summary>
    /// <param name="soLuongGoc">So luong item goc random ra</param>
    /// <returns>So luong item thuc te sau khi nhan (da lam tron)</returns>
    public int TinhSoLuongSauNhan(int soLuongGoc)
    {
        // Nhân hệ số và làm tròn về số nguyên gần nhất (tối thiểu là 1)
        int soLuongMoi = Mathf.Max(1, Mathf.RoundToInt(soLuongGoc * heSoNhan));
        return soLuongMoi;
    }

    /// <summary>
    /// Hàm thay đổi hệ số nhân linh hoạt từ script khác (Ví dụ: Buff x2 rơi đồ, VIP, v.v.)
    /// </summary>
    public void CapNhatHeSoNhan(float heSoMoi)
    {
        heSoNhan = Mathf.Max(1f, heSoMoi);
    }
}