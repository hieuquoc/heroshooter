using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rescueforce
{
    public class LevelManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    [Tooltip("Prefab used for elite enemies. If null, enemyPrefab will be used.")]
    public GameObject eliteEnemyPrefab;
    [Tooltip("UI marker prefab used by MarkerManager to show spawned enemies on the HUD/canvas.")]
    public GameObject markerPrefab;
    [Tooltip("Positions computed by the grid bake. Use inspector Bake Grid to populate.")]
    public List<Vector3> spawnPositions = new List<Vector3>();

    [Header("Grid Bake Settings")]
    public Vector3 gridOrigin = Vector3.zero;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 2f;
    [Tooltip("Square check size (world units) inside a cell to validate corners / overlap checks.")]
    public float checkSize = 1.5f;
    [Tooltip("Height to cast down from when sampling ground.")]
    public float sampleHeight = 5f;
    [Tooltip("Maximum distance to look for ground when raycasting down.")]
    public float maxRayDistance = 10f;
    public LayerMask groundMask = ~0;
    public LayerMask obstacleMask = 0;
    public bool debugGizmos = true;
    [Tooltip("Toggle drawing of the corner downward raycast gizmos.")]
    public bool drawCornerRays = true;
    [Header("Level Spawn Rules")]
    [Tooltip("Base maximum enemies (at level 1..4, etc). Every 5 levels increases this max by +5.")]
    public int baseMaxEnemies = 20;
    [Tooltip("Minimum cell separation (in cells) between spawned enemies when selecting points")]
    public int minSeparationCells = 1;
    public static LevelManager Instance { get; private set; }

    // Track spawned enemies and the transform used for their marker (if any)
    class SpawnEntry { public GameObject Enemy; public Transform MarkerTarget; }
    List<SpawnEntry> _spawned = new List<SpawnEntry>();
    private int totalEnemies = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // ensure grid origin follows this object's transform
        gridOrigin = transform.position;
    }

    void OnValidate()
    {
        // keep grid origin synced to the transform in editor
        if (this != null)
            gridOrigin = transform.position;
    }

    public int GetCurrentEnemyCount()
    {
        return _spawned.Count;
    }

    /// <summary>
    /// Bake the grid and fill spawnPositions with valid positions.
    /// This can be invoked from the custom inspector button.
    /// </summary>
    public void BakeGrid()
    {
        spawnPositions.Clear();

        float halfCheck = checkSize * 0.5f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // cell center in XZ plane
                Vector3 center = gridOrigin + new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);

                // prepare corner positions relative to center
                Vector3[] corners = new Vector3[4]
                {
                    center + new Vector3(-halfCheck, sampleHeight, -halfCheck),
                    center + new Vector3(-halfCheck, sampleHeight,  halfCheck),
                    center + new Vector3( halfCheck, sampleHeight, -halfCheck),
                    center + new Vector3( halfCheck, sampleHeight,  halfCheck),
                };

                bool allCornersHit = true;
                Vector3 accumHit = Vector3.zero;
                int hitCount = 0;
                Vector3[] cornerHits = new Vector3[corners.Length];

                for (int i = 0; i < corners.Length; i++)
                {
                    if (Physics.Raycast(corners[i], Vector3.down, out RaycastHit hit, maxRayDistance, groundMask, QueryTriggerInteraction.Ignore))
                    {
                        accumHit += hit.point;
                        hitCount++;
                        cornerHits[i] = hit.point;
                    }
                    else
                    {
                        allCornersHit = false;
                        break;
                    }
                }

                if (!allCornersHit)
                {
                    // cell invalid
                    continue;
                }

                // ensure corner heights are similar (avoid steep slopes)
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                for (int i = 0; i < cornerHits.Length; i++)
                {
                    float y = cornerHits[i].y;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }

                // reject cell if vertical spread among corners is too large
                if (maxY - minY > 0.2f)
                {
                    // too sloped
                    continue;
                }

                // compute sample position (average corner ground height)
                Vector3 samplePos = (hitCount > 0) ? (accumHit / hitCount) : center;

                // Raycast down at the cell center and ensure its hit Y is consistent
                // with the four corner hits (i.e., all five points lie on same plane).
                Vector3 centerOrigin = center + Vector3.up * sampleHeight;
                if (!Physics.Raycast(centerOrigin, Vector3.down, out RaycastHit centerHit, maxRayDistance, groundMask, QueryTriggerInteraction.Ignore))
                {
                    // no ground at center
                    continue;
                }

                // additional vertical center box check to catch small columns near center
                float centerBoxSize = Mathf.Max(0.01f, checkSize * 0.2f);
                float centerBoxHalfHeight = Mathf.Max(0.1f, sampleHeight * 0.5f);
                Vector3 centerBoxHalfExtents = new Vector3(centerBoxSize * 0.5f, centerBoxHalfHeight, centerBoxSize * 0.5f);
                Vector3 centerBoxWorldPos = center + Vector3.up * centerBoxHalfHeight;
                Collider[] centerObstacles = Physics.OverlapBox(centerBoxWorldPos, centerBoxHalfExtents, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore);
                if (centerObstacles != null && centerObstacles.Length > 0)
                {
                    // small obstacle present near center (e.g., column) -> reject cell
                    continue;
                }

                // check vertical spread including the center hit. require very tight tolerance.
                float centerY = centerHit.point.y;
                float minY2 = centerY;
                float maxY2 = centerY;
                for (int i = 0; i < cornerHits.Length; i++)
                {
                    float y = cornerHits[i].y;
                    if (y < minY2) minY2 = y;
                    if (y > maxY2) maxY2 = y;
                }

                // require center + corners to be nearly coplanar (y difference <= 0.01)
                if (maxY2 - minY2 > 0.01f)
                {
                    // center differs from corners too much
                    continue;
                }

                // Accept position; use samplePos (average corner ground height)
                spawnPositions.Add(samplePos);
            }
        }

        Debug.Log($"BakeGrid finished: found {spawnPositions.Count} valid spawn positions.");
    }

    /// <summary>
    /// Spawn enemies at all baked positions using the project's ObjectPool.
    /// </summary>
    [ContextMenu("Spawn All Enemies")]
    public void SpawnAll()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("LevelManager: enemyPrefab is null");
            return;
        }

        foreach (var pos in spawnPositions)
        {
            var obj = ObjectPool.Instance.Get(enemyPrefab, pos, Quaternion.identity);
            Transform markerTarget = null;
            // add marker for this spawned enemy if MarkerManager + prefab are available
            if (MarkerManager.Instance != null && markerPrefab != null && obj != null)
            {
                Enemy comp = obj.GetComponent<Enemy>();
                Transform target = comp != null && comp.markPoint != null ? comp.markPoint : obj.transform;
                Vector3 worldPos = (target != null) ? target.position : pos;
                MarkerManager.Instance.AddTarget(markerPrefab, target, worldPos);
                markerTarget = target;
            }
            _spawned.Add(new SpawnEntry { Enemy = obj, MarkerTarget = markerTarget });
        }
    }

    /// <summary>
    /// Spawn enemies for a given level.
    /// Rules:
    /// - totalEnemies = min(10 + level, currentMax)
    /// - currentMax = baseMaxEnemies + ((level-1)/5)*5
    /// - eliteEnemies = min(level, totalEnemies)
    /// - select random spawn points from baked grid ensuring minSeparationCells between points
    /// - perform a downward raycast at chosen XZ to get the surface Y before spawning
    /// </summary>
    public void SpawnLevel(int level, int separationCells = -1)
    {
        if (separationCells <= 0) separationCells = minSeparationCells;

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("LevelManager: no spawn positions baked. Call BakeGrid first.");
            return;
        }

        int currentMax = baseMaxEnemies + ((Mathf.Max(1, level) - 1) / 5) * 5;
        totalEnemies = Mathf.Min(10 + level, currentMax);
        int eliteCount = Mathf.Min(level, totalEnemies);

        int normalCount = totalEnemies - eliteCount;

        // choose positions
        float minDist = separationCells * cellSize;
        float minDistSqr = minDist * minDist;

        var indices = new List<int>(spawnPositions.Count);
        for (int i = 0; i < spawnPositions.Count; i++) indices.Add(i);
        // shuffle
        for (int i = 0; i < indices.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, indices.Count);
            int tmp = indices[i]; indices[i] = indices[j]; indices[j] = tmp;
        }

        var chosen = new List<Vector3>();

        foreach (int idx in indices)
        {
            Vector3 candidate = spawnPositions[idx];

            bool ok = true;
            foreach (var c in chosen)
            {
                // compare XZ distance only
                float dx = c.x - candidate.x;
                float dz = c.z - candidate.z;
                if (dx * dx + dz * dz < minDistSqr)
                {
                    ok = false; break;
                }
            }

            if (!ok) continue;

            // confirm ground at this XZ by raycasting from above
            Vector3 origin = new Vector3(candidate.x, gridOrigin.y + sampleHeight, candidate.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxRayDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                chosen.Add(hit.point);
            }

            if (chosen.Count >= totalEnemies) break;
        }

        if (chosen.Count < totalEnemies)
        {
            Debug.LogWarning($"LevelManager.SpawnLevel: only found {chosen.Count} suitable points (requested {totalEnemies}).");
        }

        // spawn elites first
        for (int i = 0; i < eliteCount && i < chosen.Count; i++)
        {
            Vector3 pos = chosen[i];
            GameObject prefab = eliteEnemyPrefab != null ? eliteEnemyPrefab : enemyPrefab;
            var obj = ObjectPool.Instance.Get(prefab, pos, Quaternion.identity);
            Enemy comp = obj != null ? obj.GetComponent<Enemy>() : null;
            Transform markerTarget = null;
            if (MarkerManager.Instance != null && markerPrefab != null && obj != null)
            {
                Transform target = comp != null && comp.markPoint != null ? comp.markPoint : obj.transform;
                Vector3 worldPos = (target != null) ? target.position : pos;
                MarkerManager.Instance.AddTarget(markerPrefab, target, worldPos);
                markerTarget = target;
            }
            _spawned.Add(new SpawnEntry { Enemy = obj, MarkerTarget = markerTarget });
        }

        // spawn normal enemies for remaining chosen positions
        for (int i = eliteCount; i < chosen.Count; i++)
        {
            Vector3 pos = chosen[i];
            var obj = ObjectPool.Instance.Get(enemyPrefab, pos, Quaternion.identity);
            Transform markerTarget = null;
            if (MarkerManager.Instance != null && markerPrefab != null && obj != null)
            {
                Enemy comp = obj.GetComponent<Enemy>();
                Transform target = comp != null && comp.markPoint != null ? comp.markPoint : obj.transform;
                Vector3 worldPos = (target != null) ? target.position : pos;
                MarkerManager.Instance.AddTarget(markerPrefab, target, worldPos);
                markerTarget = target;
            }
            _spawned.Add(new SpawnEntry { Enemy = obj, MarkerTarget = markerTarget });
        }

        InGameHUD.Instance?.UpdateEnemyCount(0, totalEnemies);

        Debug.Log($"SpawnLevel {level}: spawned {Mathf.Min(chosen.Count, totalEnemies)} enemies ({eliteCount} elite)");
    }

    /// <summary>
    /// Return all active enemies (tagged "enemy") to the pool and remove their markers.
    /// </summary>
    public void ClearAllEnemies()
    {
        int returned = 0;
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            var entry = _spawned[i];
            if (entry == null || entry.Enemy == null) { _spawned.RemoveAt(i); continue; }

            if (MarkerManager.Instance != null && entry.MarkerTarget != null)
                MarkerManager.Instance.RemoveTarget(entry.MarkerTarget);

            ObjectPool.Instance.Return(entry.Enemy);
            returned++;
            _spawned.RemoveAt(i);
        }

        Debug.Log($"ClearAllEnemies: returned {returned} enemies to pool.");
    }

    /// <summary>
    /// Unregister a single spawned enemy (remove marker and forget it).
    /// Call this when an enemy dies so it is removed from the spawned list immediately.
    /// </summary>
    public void UnregisterSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            var entry = _spawned[i];
            if (entry != null && entry.Enemy == enemy)
            {
                if (MarkerManager.Instance != null && entry.MarkerTarget != null)
                    MarkerManager.Instance.RemoveTarget(entry.MarkerTarget);

                _spawned.RemoveAt(i);
                break;
            }
        }
        InGameHUD.Instance?.UpdateEnemyCount(totalEnemies - _spawned.Count, totalEnemies);
    }

    [ContextMenu("Clear Spawn Positions")]
    public void ClearSpawnPositions()
    {
        spawnPositions.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;

        if (spawnPositions == null || spawnPositions.Count == 0) return;

        // Draw only boxes at valid baked spawn positions
        Vector3 size = new Vector3(checkSize, 0.02f, checkSize);
        Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
        foreach (var p in spawnPositions)
        {
            Vector3 drawPos = p + Vector3.up * 0.01f;
            Gizmos.DrawCube(drawPos, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(drawPos, size);
            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
        }
    }

    public void StartLevel(int level)
    {
        StartCoroutine(StartLevelRoutine(level));
    }

    IEnumerator StartLevelRoutine(int level)
    {
        ClearAllEnemies();
        // optional: add some delay or transition effect here before spawning
        yield return new WaitForSeconds(0.1f);
        SpawnLevel(level, 2);
        InGameHUD.Instance?.UpdateEnemyCount(0, totalEnemies);
    }
}

}

