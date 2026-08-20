using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotPhaSliderManager : MonoBehaviour
{
    [Header("--- PANEL CHA (Chứa toàn bộ các Bảng UI con) ---")]
    [Tooltip("Kéo Panel cha chứa các bảng DotPha_0, DotPha_1... vào đây")]
    public RectTransform panelChaChuaCacBang;

    [Header("--- CẤU HÌNH TRƯỢT ---")]
    [Tooltip("Khoảng cách vị trí X giữa 2 Bảng UI (Ví dụ: 800 hoặc 1000)")]
    public float khoangCachMoiBangX = 800f;
    public float tocDoTruot = 10f;
    public int tongSoBangUI = 3;

    private int indexBangHienTai = 0;
    private Coroutine coroutineTruot;

    /// <summary>
    /// Gán vào OnClick() của Mũi Tên Phải (Trượt sang bảng kế tiếp)
    /// </summary>
    public void NutSangBangPhai()
    {
        if (indexBangHienTai < tongSoBangUI - 1)
        {
            indexBangHienTai++;
            CapNhatViTriTruot();
        }
    }

    /// <summary>
    /// Gán vào OnClick() của Mũi Tên Trái (Trượt về bảng trước)
    /// </summary>
    public void NutSangBangTrai()
    {
        if (indexBangHienTai > 0)
        {
            indexBangHienTai--;
            CapNhatViTriTruot();
        }
    }

    private void CapNhatViTriTruot()
    {
        if (panelChaChuaCacBang == null) return;

        // Tọa độ X mục tiêu = âm (index * khoảng cách)
        Vector2 viTriDich = new Vector2(-indexBangHienTai * khoangCachMoiBangX, panelChaChuaCacBang.anchoredPosition.y);

        if (coroutineTruot != null) StopCoroutine(coroutineTruot);
        coroutineTruot = StartCoroutine(CoTruotPanel(viTriDich));
    }

    private IEnumerator CoTruotPanel(Vector2 viTriDich)
    {
        while (Vector2.Distance(panelChaChuaCacBang.anchoredPosition, viTriDich) > 0.1f)
        {
            panelChaChuaCacBang.anchoredPosition = Vector2.Lerp(
                panelChaChuaCacBang.anchoredPosition,
                viTriDich,
                Time.deltaTime * tocDoTruot
            );
            yield return null;
        }

        panelChaChuaCacBang.anchoredPosition = viTriDich;
    }
}