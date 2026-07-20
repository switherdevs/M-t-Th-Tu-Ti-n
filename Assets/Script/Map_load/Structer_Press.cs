using UnityEngine;

public class InteractableStructure : MonoBehaviour
{
    // Nơi lưu trữ GameObject Canvas UI (Kéo thả từ Hierarchy vào)
    [SerializeField] private GameObject campaignCanvas;

    // Biến nội bộ để kiểm tra xem người chơi có đang đứng trong vùng chạm hay không
    private bool isPlayerInside = false;

    private void Start()
    {
        // Đảm bảo Canvas luôn ẩn khi game vừa bắt đầu
        if (campaignCanvas != null)
        {
            campaignCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        // Nếu người chơi đang đứng trong vùng va chạm
        if (isPlayerInside)
        {
            // Kiểm tra nếu người chơi nhấn nút E (hoặc nút bạn muốn)
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Bật Canvas UI lên
                OpenUI();
            }
        }
    }

    // Hàm gọi khi có một Collider khác đi vào vùng Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng chạm vào có phải là Người chơi (Player) không
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Mẹo: Bạn có thể hiện một dòng chữ gợi ý "[E] Tương tác" ở đây
            Debug.Log("Nguoi choi da den gan cong trinh. Nhan [E] de mo.");
        }
    }

    // Hàm gọi khi Collider đó đi ra khỏi vùng Trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // Nếu người chơi đi ra xa khỏi công trình
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // Tự động đóng Canvas UI lại nếu người chơi bỏ đi không muốn vào nữa
            CloseUI();
            Debug.Log("Nguoi choi da di xa, tu dong dong UI.");
        }
    }

    // Hàm bổ trợ: Bật UI
    private void OpenUI()
    {
        if (campaignCanvas != null)
        {
            campaignCanvas.SetActive(true);
        }
    }

    // Hàm bổ trợ: Ẩn UI (Có thể gắn vào nút "Hủy/Thoát" trên giao diện UI)
    public void CloseUI()
    {
        if (campaignCanvas != null)
        {
            campaignCanvas.SetActive(false);
        }
    }
}