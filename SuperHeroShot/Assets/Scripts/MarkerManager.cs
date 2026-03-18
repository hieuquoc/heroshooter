using System.Collections.Generic;
using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager Instance { get; private set; }

    [SerializeField] Camera _cam;
    [SerializeField] RectTransform _canvasRect;
    [SerializeField] bool showOffscreen = true;
    [SerializeField] float screenBorder = 40f; // pixels margin from screen edge when clamping

    private struct IndicatorEntry
    {
        public Transform Target;
        public GameObject Prefab;
        public GameObject Instance;
        public Vector3 Position;  // vị trí world dùng để đặt marker
        public bool IsRemoving;
    }

    // flat list; 1 target có thể xuất hiện nhiều lần
    // index cuối cùng của mỗi target là indicator đang hiển thị
    private List<IndicatorEntry> _tracked = new();

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
    /// Thêm indicator cho target. Indicator mới trở thành top;
    /// indicator trước đó của cùng target bị ẩn nhưng giữ nguyên trong list.
    /// </summary>
    public GameObject AddTarget(GameObject indicatorPrefab, Transform target, Vector3 hitPoint)
    {
        if (target == null || indicatorPrefab == null) return null;

        GameObject instance = ObjectPool.Instance.Get(indicatorPrefab, Vector3.zero, Quaternion.identity);
        instance.transform.SetParent(_canvasRect, false);
        instance.SetActive(true);

        _tracked.Add(new IndicatorEntry
        {
            Target   = target,
            Prefab   = indicatorPrefab,
            Instance = instance,
            Position = hitPoint
        });
        return instance;

    }

    public Vector3 GetHitPoint(GameObject marker)
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            if (_tracked[i].Instance == marker)
                return _tracked[i].Position;
        }
        return Vector3.zero;
    }

    /// <summary>Cập nhật vị trí world cho tất cả indicator của target.</summary>
    public void SetPosition(Transform target, Vector3 worldPosition)
    {
        for (int i = 0; i < _tracked.Count; i++)
        {
            if (_tracked[i].Target != target) continue;
            var e = _tracked[i];
            e.Position = worldPosition;
            _tracked[i] = e;
        }
    }

    /// <summary>Xoá toàn bộ indicator của target và trả hết về pool.</summary>
    public void RemoveTarget(Transform target)
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            if (_tracked[i].Target == target)
            {
                ObjectPool.Instance.Return(_tracked[i].Instance);
                _tracked.RemoveAt(i);
            }
        }
    }

    public void RemoveMarker(GameObject marker)
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            if (_tracked[i].Instance == marker)
            {
                var e = _tracked[i];
                e.IsRemoving = true;
                _tracked[i] = e;
                break;
            }
        }
    }
    

    // ──────────────────────────────────────────────
    // Update — đồng bộ vị trí UI với vị trí world
    // chỉ xử lý entry là top của từng target
    // ──────────────────────────────────────────────

    void Update()
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
        {
            var entry = _tracked[i];

            // target bị destroy
            if (entry.Target == null || entry.IsRemoving)
            {
                ObjectPool.Instance.Return(entry.Instance);
                _tracked.RemoveAt(i);
                continue;
            }

            Vector3 screenPos = _cam.WorldToScreenPoint(entry.Position);

            bool inView = screenPos.z > 0f
                       && screenPos.x >= 0f && screenPos.x <= Screen.width
                       && screenPos.y >= 0f && screenPos.y <= Screen.height;

            if (inView)
            {
                if (!entry.Instance.activeSelf) entry.Instance.SetActive(true);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect, screenPos, null, out Vector2 localPoint);
                entry.Instance.GetComponent<RectTransform>().anchoredPosition = localPoint;
            }
            else if (showOffscreen)
            {
                // show marker clamped to screen edge toward the target
                if (!entry.Instance.activeSelf) entry.Instance.SetActive(true);

                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Vector2 pos2D = new Vector2(screenPos.x, screenPos.y);

                // If behind camera, flip direction
                Vector2 dir = (pos2D - screenCenter);
                if (screenPos.z < 0f)
                    dir = -dir;

                if (dir.sqrMagnitude < 0.0001f)
                    dir = Vector2.up * 0.001f;

                Vector2 dirN = dir.normalized;

                float maxX = screenCenter.x - screenBorder;
                float maxY = screenCenter.y - screenBorder;

                // compute scale so point lies within the rectangle defined by maxX/maxY
                float scaleX = Mathf.Abs(dirN.x) > 0.0001f ? maxX / Mathf.Abs(dirN.x) : float.MaxValue;
                float scaleY = Mathf.Abs(dirN.y) > 0.0001f ? maxY / Mathf.Abs(dirN.y) : float.MaxValue;
                float scale = Mathf.Min(scaleX, scaleY);

                Vector2 edgeScreenPos = screenCenter + dirN * scale;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect, edgeScreenPos, null, out Vector2 localEdge);

                entry.Instance.GetComponent<RectTransform>().anchoredPosition = localEdge;
            }
            else
            {
                if (entry.Instance.activeSelf) entry.Instance.SetActive(false);
            }
        }
    }
}
