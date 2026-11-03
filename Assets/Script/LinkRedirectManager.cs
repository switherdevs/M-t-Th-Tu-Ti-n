using UnityEngine;
using UnityEngine.UI;

public class LinkRedirectManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH ĐƯỜNG DẪN URL ---")]
    [Tooltip("Nhập đường dẫn trang Facebook của bạn vào đây")]
    [SerializeField] private string facebookUrl = "https://www.facebook.com";
    
    [Tooltip("Nhập đường dẫn Server Discord của bạn vào đây")]
    [SerializeField] private string discordUrl = "https://discord.com";

    [Header("--- UI BUTTONS ---")]
    [SerializeField] private Button facebookButton;
    [SerializeField] private Button discordButton;

    private void Start()
    {
        // Tự động gán sự kiện click cho nút Facebook
        if (facebookButton != null)
        {
            facebookButton.onClick.RemoveAllListeners();
            facebookButton.onClick.AddListener(OpenFacebookLink);
        }

        // Tự động gán sự kiện click cho nút Discord
        if (discordButton != null)
        {
            discordButton.onClick.RemoveAllListeners();
            discordButton.onClick.AddListener(OpenDiscordLink);
        }
    }

    /// <summary>
    /// Mở link Facebook ra trình duyệt bên ngoài
    /// </summary>
    public void OpenFacebookLink()
    {
        if (!string.IsNullOrEmpty(facebookUrl))
        {
            Application.OpenURL(facebookUrl);
        }
    }

    /// <summary>
    /// Mở link Discord ra trình duyệt bên ngoài
    /// </summary>
    public void OpenDiscordLink()
    {
        if (!string.IsNullOrEmpty(discordUrl))
        {
            Application.OpenURL(discordUrl);
        }
    }
}