using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour
{
    // ĐÃ XÓA BỎ: public static SimpleObjectPool Instance { get; private set; } 
    // Lý do xóa: Tránh việc các kho đạn tự xóa lẫn nhau!

    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 20;

    private readonly Queue<GameObject> poolQueue = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        if (prefab == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        // Lặp để lọc các Object bị null trong Queue
        while (poolQueue.Count > 0)
        {
            obj = poolQueue.Dequeue();
            if (obj != null) break;
        }

        // Nếu Queue hết đạn hợp lệ, tạo mới 1 đạn Prefab
        if (obj == null)
        {
            obj = Instantiate(prefab, transform);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolQueue.Enqueue(obj);
    }
}