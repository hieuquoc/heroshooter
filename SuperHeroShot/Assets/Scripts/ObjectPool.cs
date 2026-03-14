using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    // Pool riêng cho từng prefab, key = prefab instanceID
    readonly Dictionary<int, Queue<GameObject>> _pools = new();
    readonly Dictionary<int, GameObject>        _prefabMap = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Lấy object từ pool (hoặc tạo mới nếu hết).</summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int id = prefab.GetInstanceID();

        if (!_pools.ContainsKey(id))
        {
            _pools[id]    = new Queue<GameObject>();
            _prefabMap[id] = prefab;
        }

        GameObject obj;
        if (_pools[id].Count > 0)
        {
            obj = _pools[id].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation, transform);
            // gắn tag để Return() biết thuộc pool nào
            obj.AddComponent<PooledObject>().PrefabID = id;
        }

        return obj;
    }

    /// <summary>Trả object về pool. Gọi thay vì Destroy().</summary>
    public void Return(GameObject obj)
    {
        var tag = obj.GetComponent<PooledObject>();
        if (tag == null) { Destroy(obj); return; }

        obj.SetActive(false);

        if (!_pools.ContainsKey(tag.PrefabID))
            _pools[tag.PrefabID] = new Queue<GameObject>();

        _pools[tag.PrefabID].Enqueue(obj);
    }

    /// <summary>Trả về pool sau delay (tiện cho đạn, hiệu ứng).</summary>
    public void ReturnDelayed(GameObject obj, float delay)
        => StartCoroutine(ReturnRoutine(obj, delay));

    System.Collections.IEnumerator ReturnRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) Return(obj);
    }

    /// <summary>Khởi tạo sẵn pool để tránh lag khi spawn lần đầu.</summary>
    public void Prewarm(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Get(prefab, Vector3.zero, Quaternion.identity);
            Return(obj);
        }
    }
}

// Component nội bộ — đánh dấu object thuộc pool nào
public class PooledObject : MonoBehaviour
{
    public int PrefabID;
}
