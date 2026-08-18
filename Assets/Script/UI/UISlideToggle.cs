using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc để nhận diện Click trên UI/Collider

[RequireComponent(typeof(AudioSource))]
public class UISlideToggle : MonoBehaviour, IPointerClickHandler
{
    [Header("--- CẤU HÌNH ANIMATION ---")]
    [Tooltip("Kéo Animator của UI cần trượt vào đây (nếu để trống script sẽ tự lấy Animator trên chính GameObject này)")]
    public Animator animatorTarget;

    [Tooltip("Tên chính xác của Parameter kiểu Bool trong Animator")]
    public string tenBienBoolAnimation = "IsExpanded";

    [Header("--- CẤU HÌNH ÂM THANH ---")]
    [Tooltip("Kéo file âm thanh hiệu ứng (SFX) khi click vào đây")]
    public AudioClip amThanhClickUI;

    // Trạng thái hiện tại (true = Mở/Trượt lên, false = Đóng/Trượt xuống)
    private bool isExpanded = false;

    private AudioSource audioSource;

    private void Awake()
    {
        // Nếu chưa kéo Animator vào Inspector thì tự tìm trên GameObject này
        if (animatorTarget == null)
        {
            animatorTarget = GetComponent<Animator>();
        }

        // Khởi tạo AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Bắt sự kiện Click từ Unity EventSystem (Panel, Image, Button, Collider)
    /// </summary>
    /// <param name="eventData">Dữ liệu thao tác con trỏ chuột/chạm</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        ThucHienLoiTruotUI();
    }

    /// <summary>
    /// Logic chính: Đảo trạng thái Bool, chuyển Animation và phát âm thanh Click 1 lần
    /// </summary>
    public void ThucHienLoiTruotUI()
    {
        // 1. Đảo trạng thái bool
        isExpanded = !isExpanded;

        // 2. Cập nhật parameter bool cho Animator
        if (animatorTarget != null)
        {
            animatorTarget.SetBool(tenBienBoolAnimation, isExpanded);
        }
        else
        {
            Debug.LogWarning($"[UISlideToggle] Chưa gán Animator trên GameObject: {gameObject.name}");
        }

        // 3. Phát âm thanh SFX 1 lần
        if (audioSource != null && amThanhClickUI != null)
        {
            audioSource.PlayOneShot(amThanhClickUI);
        }
    }
}