using System.Collections.Generic;
using UnityEngine;

public class MapZoneChecker : MonoBehaviour
{
    private List<GameObject> enemiesInThisZone = new List<GameObject>();

    // Khi quái bước vào vùng của map này
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!enemiesInThisZone.Contains(collision.gameObject))
            {
                enemiesInThisZone.Add(collision.gameObject);
            }
        }
    }

    // Khi quái rời khỏi vùng (hoặc chết đi bị disable)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (enemiesInThisZone.Contains(collision.gameObject))
            {
                enemiesInThisZone.Remove(collision.gameObject);
            }
        }
    }

    // Hàm trả về số lượng quái còn sống thực tế trong vùng
    public int GetRemainingEnemiesCount()
    {
        // Dọn sạch các đối tượng đã bị Destroy khỏi danh sách
        enemiesInThisZone.RemoveAll(enemy => enemy == null || !enemy.activeSelf);
        return enemiesInThisZone.Count;
    }
}