using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomDialogueSystem : MonoBehaviour
{
    [Header("--- THÀNH PHẦN UI & ÂM THANH ---")]
    [SerializeField] private TextMeshProUGUI txtTenNPC;
    [SerializeField] private TextMeshProUGUI txtNoiDungThoai;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amThanhGoChu;

    [Header("--- CẤU HÌNH NPC & THỜI GIAN ---")]
    [SerializeField] private string tenNPC = "Cao Nhân Ẩn Danh";
    [SerializeField] private float tocDoGoChu = 0.04f;
    [SerializeField] private float thoiGianDoiThoai = 30f;

    [Header("--- DANH SÁCH THOẠI RANDOM ---")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> danhSachThoaiRandom = new List<string>();

    private Coroutine coroutineGoChu;
    private Coroutine coroutineDemThoiGian;
    private int indexThoaiVuaChay = -1;

    private void Start()
    {
        if (txtTenNPC != null) txtTenNPC.text = tenNPC;

        // Bắt đầu luồng chạy thoại tự động
        coroutineDemThoiGian = StartCoroutine(DemThoiGianDoiThoaiRoutine());
    }

    private IEnumerator DemThoiGianDoiThoaiRoutine()
    {
        while (true)
        {
            ChayThoaiRandomMoi();
            yield return new WaitForSeconds(thoiGianDoiThoai);
        }
    }

    public void ChayThoaiRandomMoi()
    {
        if (danhSachThoaiRandom == null || danhSachThoaiRandom.Count == 0) return;

        // Chọn index ngẫu nhiên (tránh lặp lại câu vừa nói nếu mảng có từ 2 câu trở lên)
        int indexMoi = Random.Range(0, danhSachThoaiRandom.Count);
        if (danhSachThoaiRandom.Count > 1)
        {
            while (indexMoi == indexThoaiVuaChay)
            {
                indexMoi = Random.Range(0, danhSachThoaiRandom.Count);
            }
        }
        indexThoaiVuaChay = indexMoi;

        string cauThoaiChon = danhSachThoaiRandom[indexMoi];

        // Dừng hiệu ứng gõ chữ cũ nếu đang chạy dở
        if (coroutineGoChu != null)
        {
            StopCoroutine(coroutineGoChu);
        }

        coroutineGoChu = StartCoroutine(GoChuKemAmThanhRoutine(cauThoaiChon));
    }

    private IEnumerator GoChuKemAmThanhRoutine(string chuoiVanBan)
    {
        if (txtNoiDungThoai == null) yield break;

        txtNoiDungThoai.text = "";

        foreach (char c in chuoiVanBan.ToCharArray())
        {
            txtNoiDungThoai.text += c;

            // Phát âm thanh gõ chữ theo từng ký tự (bỏ qua khoảng trắng)
            if (c != ' ' && audioSource != null && amThanhGoChu != null)
            {
                audioSource.PlayOneShot(amThanhGoChu);
            }

            yield return new WaitForSeconds(tocDoGoChu);
        }
    }

    private void OnDisable()
    {
        // Dọn dẹp Coroutine khi GameObject bị ẩn/tắt
        if (coroutineGoChu != null) StopCoroutine(coroutineGoChu);
        if (coroutineDemThoiGian != null) StopCoroutine(coroutineDemThoiGian);
    }
}