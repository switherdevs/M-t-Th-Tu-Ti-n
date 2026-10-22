using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Spawners
{
    public class EnemyRandomSpawner : MonoBehaviour
    {
        [Header("=== CẤU HÌNH DANH SÁCH MẢNG ===")]
        [Tooltip("Mảng chứa các Prefab Quái Vật (VD: Thỏ, Gấu, Toilet...)")]
        [SerializeField] private GameObject[] danhSachQuaiPrefab;

        [Tooltip("Mảng chứa các vị trí Transform Spawn quái trên bản đồ")]
        [SerializeField] private Transform[] danhSachDiemSpawn;

        [Header("=== CẤU HÌNH SỐ LƯỢNG SPAWN NGẪU NHIÊN ===")]
        [Tooltip("Số lượng quái tối thiểu có thể xuất hiện khi vào Scene")]
        [SerializeField] private int soLuongQuaiToiThieu = 1;

        [Tooltip("Số lượng quái tối đa có thể xuất hiện khi vào Scene")]
        [SerializeField] private int soLuongQuaiToiDa = 3;

        private void Start()
        {
            ThucHienSpawnQuaiNgauNhien();
        }

        /// <summary>
        /// Thuật toán chọn ngẫu nhiên vị trí và chủng quái để sinh ra khi Load Scene
        /// </summary>
        public void ThucHienSpawnQuaiNgauNhien()
        {
            // Kiểm tra an toàn dữ liệu đầu vào
            if (danhSachQuaiPrefab == null || danhSachQuaiPrefab.Length == 0)
            {
                Debug.LogWarning("[EnemyRandomSpawner] Chưa gán Prefab Quái vào danh sách!");
                return;
            }

            if (danhSachDiemSpawn == null || danhSachDiemSpawn.Length == 0)
            {
                Debug.LogWarning("[EnemyRandomSpawner] Chưa gán các điểm Transform Spawn vào danh sách!");
                return;
            }

            // 1. Xác định số lượng quái sẽ sinh ra ngẫu nhiên trong khoảng [ToiThieu, ToiDa]
            int soLuongMuonSpawn = Random.Range(soLuongQuaiToiThieu, soLuongQuaiToiDa + 1);

            // Giới hạn số lượng không vượt quá tổng số điểm Spawn có sẵn để tránh đè trùng vị trí
            soLuongMuonSpawn = Mathf.Min(soLuongMuonSpawn, danhSachDiemSpawn.Length);

            // 2. Tạo danh sách tạm thời chứa các chỉ số (index) điểm Spawn để xáo trộn không trùng vị trí
            List<int> danhSachIndexSpawnConLai = new List<int>();
            for (int i = 0; i < danhSachDiemSpawn.Length; i++)
            {
                if (danhSachDiemSpawn[i] != null)
                {
                    danhSachIndexSpawnConLai.Add(i);
                }
            }

            // 3. Tiến hành chọn ngẫu nhiên vị trí và loại quái để Instantiate
            for (int i = 0; i < soLuongMuonSpawn; i++)
            {
                if (danhSachIndexSpawnConLai.Count == 0) break;

                // Bốc ngẫu nhiên 1 điểm spawn từ danh sách còn lại
                int randSpawnPosIndex = Random.Range(0, danhSachIndexSpawnConLai.Count);
                int selectedPointIndex = danhSachIndexSpawnConLai[randSpawnPosIndex];

                Transform diemSpawnChon = danhSachDiemSpawn[selectedPointIndex];

                // Loại bỏ vị trí vừa chọn khỏi danh sách tạm để không spawn trùng 2 con chung 1 chỗ
                danhSachIndexSpawnConLai.RemoveAt(randSpawnPosIndex);

                // Bốc ngẫu nhiên 1 Prefab quái vật từ mảng Quái
                int randQuaiIndex = Random.Range(0, danhSachQuaiPrefab.Length);
                GameObject quaiChonPrefab = danhSachQuaiPrefab[randQuaiIndex];

                if (quaiChonPrefab != null && diemSpawnChon != null)
                {
                    // Sinh quái ra tại vị trí và góc xoay của điểm Spawn
                    Instantiate(quaiChonPrefab, diemSpawnChon.position, diemSpawnChon.rotation);
                }
            }

            Debug.Log($"<color=green>[EnemyRandomSpawner]</color> Đã ngẫu nhiên sinh {soLuongMuonSpawn} quái vật trên map!");
        }

        // Vẽ các điểm Spawn lên cửa sổ Scene để bạn dễ căn chỉnh độ cao / vị trí
        private void OnDrawGizmos()
        {
            if (danhSachDiemSpawn == null) return;

            Gizmos.color = Color.cyan;
            foreach (Transform t in danhSachDiemSpawn)
            {
                if (t != null)
                {
                    Gizmos.DrawWireSphere(t.position, 0.5f);
                    Gizmos.DrawLine(t.position, t.position + Vector3.up * 1f);
                }
            }
        }
    }
}