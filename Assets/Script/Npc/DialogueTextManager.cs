using System.Collections.Generic;
using UnityEngine;
using TMPro; // Thư viện bắt buộc để làm việc với TextMeshPro

public class DialogueTextManager : MonoBehaviour
{
    // Cấu trúc một dòng thoại/chữ gồm Key (tên định danh) và nội dung tiếng Anh
    [System.Serializable]
    public struct DialogueEntry
    {
        [Tooltip("Tên này phải khớp với tên GameObject chứa TextMeshPro trên Scene")]
        public string textKey;      
        
        [TextArea(3, 5)] // Tạo ô nhập văn bản rộng nhiều dòng trong Inspector
        public string englishText;  
    }

    [Header("=== CẤU HÌNH NỘI DUNG TIẾNG ANH ===")]
    [SerializeField] private List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    // Dictionary giúp lưu trữ và tìm kiếm TextMeshPro cực kỳ nhanh chóng O(1)
    private Dictionary<string, TextMeshProUGUI> tmpCache = new Dictionary<string, TextMeshProUGUI>();

    private void Awake()
    {
        // Chạy tự động tìm kiếm và gán chữ ngay khi màn hình vừa khởi tạo
        FindAndCacheTextComponents();
        ApplyAllTexts();
    }

    // 🎯 THUẬT TOÁN 1: Tự động tìm tất cả TextMeshPro trong Scene và đưa vào bộ nhớ đệm (Cache)
    private void FindAndCacheTextComponents()
    {
        tmpCache.Clear();
        
        // Tìm toàn bộ component TextMeshProUGUI có trong Scene (kể cả các object đang bị ẩn/inactive)
        TextMeshProUGUI[] allTMPro = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var tmp in allTMPro)
        {
            // Lấy tên của GameObject làm khóa (Key) định danh
            string key = tmp.gameObject.name;
            
            if (!tmpCache.ContainsKey(key))
            {
                tmpCache.Add(key, tmp);
            }
        }
    }

    // 🎯 THUẬT TOÁN 2: So khớp Key trong danh sách với bộ nhớ đệm để thay thế văn bản
    public void ApplyAllTexts()
    {
        foreach (var entry in dialogueList)
        {
            // Kiểm tra xem Key (tên GameObject) có tồn tại trong Scene hay không
            if (tmpCache.TryGetValue(entry.textKey, out TextMeshProUGUI targetTMP))
            {
                if (targetTMP != null)
                {
                    // Thay thế văn bản cũ bằng văn bản tiếng Anh mới
                    targetTMP.text = entry.englishText;
                }
            }
            else
            {
                Debug.LogWarning($"[DialogueTextManager] Không tìm thấy TextMeshPro nào có tên GameObject khớp với Key: {entry.textKey}");
            }
        }
    }
}