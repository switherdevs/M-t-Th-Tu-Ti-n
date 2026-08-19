using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; // Thư viện cốt lõi để chạy thuật toán quét biến tự động
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestHUDTracker : MonoBehaviour
{
    public static QuestHUDTracker Instance;

    [Header("--- THÔNG BÁO MẶC ĐỊNH (KHI CHƯA CÓ QUEST HOẶC SAI MAP) ---")]
    [Tooltip("Kéo Text TMP dùng để hiển thị thông báo hướng dẫn (Nằm NGOÀI mảng danh sách quest)")]
    public TextMeshProUGUI textThongBaoMacDinh;

    [Tooltip("Nội dung hiển thị khi không có nhiệm vụ thuộc Map này")]
    public string noiDungThongBaoMacDinh = "Hãy đến kinh thành nhận nhiệm vụ";

    [Header("--- MẢNG TEXT HIỂN THỊ QUEST ĐỘNG ---")]
    [Tooltip("Kéo các Text TMP dùng để hiển thị danh sách nhiệm vụ trên HUD vào đây.")]
    public TextMeshProUGUI[] danhSachTextQuestUI;

    // Sự kiện Cập nhật HUD real-time
    public static event Action OnQuestProgressChanged;

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

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện khi tiến trình Quest thay đổi
        OnQuestProgressChanged += CapNhatGiaoDienHUD;
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện tránh rò rỉ bộ nhớ
        OnQuestProgressChanged -= CapNhatGiaoDienHUD;
    }

    private IEnumerator Start()
    {
        // Trì hoãn 1 khung hình để đảm bảo QuestSaveSystem đã LoadSaveFile() hoàn tất
        yield return null;

        // Khởi tạo và hiển thị UI ngay lập tức khi vào Game
        CapNhatGiaoDienHUD();
    }

    /// <summary>
    /// Hàm gọi sự kiện cập nhật UI thời gian thực từ bên ngoài
    /// </summary>
    public static void ThongBaoCapNhatHUD()
    {
        OnQuestProgressChanged?.Invoke();
    }

    /// <summary>
    /// Hàm kiểm tra và hiển thị danh sách Quest phù hợp với Scene hiện tại
    /// </summary>
    public void CapNhatGiaoDienHUD()
    {
        // 1. Mặc định ẩn toàn bộ Text trong mảng trước khi quét dữ liệu
        XoaRongToanBoText();

        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
        {
            Debug.LogError("[QuestHUDTracker] Chưa đọc được dữ liệu QuestSaveSystem!");
            return;
        }

        // 2. Lấy danh sách các Quest đang kích hoạt THUỘC SCENE HIỆN TẠI
        List<ProgressQuest> danhSachTheoScene = LayDanhSachQuestThuocSceneHienTai();

        // 3. XỬ LÝ TRƯỜNG HỢP: Không có nhiệm vụ nào thuộc Scene này
        if (danhSachTheoScene.Count == 0)
        {
            if (textThongBaoMacDinh != null)
            {
                textThongBaoMacDinh.gameObject.SetActive(true);
                textThongBaoMacDinh.text = noiDungThongBaoMacDinh;
            }
            return;
        }

        // 4. XỬ LÝ TRƯỜNG HỢP: Có nhiệm vụ thuộc Scene này
        if (textThongBaoMacDinh != null)
        {
            textThongBaoMacDinh.gameObject.SetActive(false);
        }

        if (danhSachTextQuestUI == null || danhSachTextQuestUI.Length == 0)
        {
            Debug.LogWarning("[QuestHUDTracker] Mảng danhSachTextQuestUI đang trống! Hãy kéo Text TMP vào Inspector.");
            return;
        }

        // 5. Duyệt danh sách Quest đã lọc và đổ dữ liệu vào mảng Text UI
        for (int i = 0; i < danhSachTheoScene.Count; i++)
        {
            if (i >= danhSachTextQuestUI.Length)
            {
                Debug.LogWarning("[QuestHUDTracker] Số lượng Quest thuộc Scene này vượt quá số lượng Text UI trên HUD!");
                break;
            }

            if (danhSachTextQuestUI[i] == null) continue;

            ProgressQuest progress = danhSachTheoScene[i];
            QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(progress.idQuest);

            if (data != null)
            {
                danhSachTextQuestUI[i].gameObject.SetActive(true);

                if (progress.trangThai == TrangThaiQuest.DangLam)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: {progress.soBoXuongDaDiet}/{data.soLuongBoXuongCanDiet}";
                }
                else if (progress.trangThai == TrangThaiQuest.DaXongChuaTra)
                {
                    danhSachTextQuestUI[i].text = $"• <b>{data.tenNhiemVu}</b>: <color=green>[Hoàn thành] Trả nhiệm vụ!</color>";
                }
            }
        }
    }

    /// <summary>
    /// Hàm quét dữ liệu Save và tự động nhận diện biến Map nhờ Reflection
    /// </summary>
    private List<ProgressQuest> LayDanhSachQuestThuocSceneHienTai()
    {
        List<ProgressQuest> ketQua = new List<ProgressQuest>();
        string tenSceneHienTai = SceneManager.GetActiveScene().name;

        foreach (ProgressQuest progress in QuestSaveSystem.Instance.duLieuSaveHienTai.danhSachProgress)
        {
            if (progress.trangThai == TrangThaiQuest.DangLam || progress.trangThai == TrangThaiQuest.DaXongChuaTra)
            {
                QuestData data = QuestSaveSystem.Instance.LayQuestDataTheoID(progress.idQuest);

                if (data != null)
                {
                    bool daTimThayMap = false;

                    // Lấy ra danh sách toàn bộ các biến (Field) đang có trong class QuestData của bạn
                    FieldInfo[] cacBienTrongQuestData = data.GetType().GetFields();

                    // Quét từng biến một, không cần biết bạn đặt tên biến đó là gì
                    foreach (FieldInfo bien in cacBienTrongQuestData)
                    {
                        // Lấy giá trị của biến đó ra (đối số truyền vào là file data hiện tại)
                        object giaTriCuaBien = bien.GetValue(data);

                        // Nếu biến có chứa dữ liệu và khi chuyển thành chữ, nó khớp 100% với tên Scene
                        if (giaTriCuaBien != null && giaTriCuaBien.ToString() == tenSceneHienTai)
                        {
                            daTimThayMap = true;
                            break; // Dừng quét biến để tối ưu hiệu suất vì đã tìm thấy kết quả
                        }
                    }

                    // Nếu quét xong xác nhận đúng là Quest của map này thì nạp vào List trả về
                    if (daTimThayMap)
                    {
                        ketQua.Add(progress);
                    }
                }
            }
        }

        return ketQua;
    }

    /// <summary>
    /// Hàm ẩn tất cả các GameObject Text có trong mảng
    /// </summary>
    private void XoaRongToanBoText()
    {
        for (int i = 0; i < danhSachTextQuestUI.Length; i++)
        {
            if (danhSachTextQuestUI[i] != null)
            {
                danhSachTextQuestUI[i].text = "";
                danhSachTextQuestUI[i].gameObject.SetActive(false);
            }
        }
    }
}