using System.Collections.Generic;
using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager Instance { get; private set; }

    [SerializeField] Camera _cam;
    [SerializeField] RectTransform _canvasRect; // Canvas gốc (Screen Space – Overlay hoặc Camera)

    struct IndicatorEntry
    {
        public GameObject Prefab;    // prefab gốc — dùng để Return đúng pool
        public GameObject Instance;  // UI object đang dùng
    }

    // key   = Transform của target
    // value = stack indicator (index cuối là cái đang hiển thị)
    readonly Dictionary<Transform, List<IndicatorEntry>> _tracked = new();

    // buffer tránh modify dict trong foreach
    readonly List<Transform> _toRemove = new();

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (_cam == null) _cam = Camera.main;
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    /// <summary>
    /// Thêm indicator cho target. Indicator mới nhất sẽ được hiển thị;
    /// indicator trước đó bị ẩn nhưng vẫn được giữ trong stack.
    /// </summary>
    public void AddTarget(GameObject indicatorPrefab, Transform target)
    {
        if (target == null || indicatorPrefab == null) return;

        GameObject instance = ObjectPool.Instance.Get(indicatorPrefab, Vector3.zero, Quaternion.identity);
        instance.transform.SetParent(_canvasRect, false);

        if (!_tracked.TryGetValue(target, out var list))
        {
            list = new List<IndicatorEntry>();
            _tracked[target] = list;
        }
        else if (list.Count > 0)
        {
            // ẩn top hiện tại (không trả pool — vẫn giữ trong stack)
            list[list.Count - 1].Instance.SetActive(false);
        }

        list.Add(new IndicatorEntry { Prefab = indicatorPrefab, Instance = instance });
        instance.SetActive(true);
    }

    /// <summary>
    /// Trả indicator trên cùng về pool, kích hoạt lại indicator bên dưới nó.
    /// </summary>
    public void RemoveTopMarker(Transform target)
    {
        if (target == null || !_tracked.TryGetValue(target, out var list) || list.Count == 0)
            return;

        var top = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        ObjectPool.Instance.Return(top.Instance); // Return tự gọi SetActive(false)

        if (list.Count > 0)
            list[list.Count - 1].Instance.SetActive(true);
        else
            _tracked.Remove(target);
    }

    /// <summary>Xoá toàn bộ indicator của target và trả hết về pool.</summary>
    public void RemoveTarget(Transform target)
    {
        if (!_tracked.TryGetValue(target, out var list)) return;

        foreach (var entry in list)
            ObjectPool.Instance.Return(entry.Instance);

        _tracked.Remove(target);
    }

    // ──────────────────────────────────────────────
    // Update — đồng bộ vị trí UI với vị trí world
    // ──────────────────────────────────────────────

    void Update()
    {
        _toRemove.Clear();

        foreach (var kvp in _tracked)
        {
            Transform target = kvp.Key;
            var list = kvp.Value;

            // target bị destroy
            if (target == null)
            {
                foreach (var e in list)
                    ObjectPool.Instance.Return(e.Instance);
                _toRemove.Add(target);
                continue;
            }

            if (list.Count == 0) { _toRemove.Add(target); continue; }

            var topInstance = list[list.Count - 1].Instance;

            Vector3 screenPos = _cam.WorldToScreenPoint(target.position);

            // chỉ hiện khi nằm trong frustum camera
            bool inView = screenPos.z > 0f
                       && screenPos.x >= 0f && screenPos.x <= Screen.width
                       && screenPos.y >= 0f && screenPos.y <= Screen.height;

            if (topInstance.activeSelf != inView)
                topInstance.SetActive(inView);

            if (inView)
            {
                // null = Screen Space Overlay; thay bằng canvas camera nếu dùng Screen Space – Camera
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect, screenPos, null, out Vector2 localPoint);

                topInstance.GetComponent<RectTransform>().anchoredPosition = localPoint;
            }
        }

        foreach (var t in _toRemove)
            _tracked.Remove(t);
    }
}
