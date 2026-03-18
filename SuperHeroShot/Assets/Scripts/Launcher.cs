using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private const int MaxTargets = 3;

    [SerializeField] private bool isAimming;
    [SerializeField] private float lockDuration = 3f;    [SerializeField] private WeaponData _launcherData;
    public LayerMask enemyLayerMask;

    public GameObject MarkerPrefab;   // prefab marker để hiển thị trên target đã lock

    [SerializeField] private Transform[] _targets = new Transform[MaxTargets];
    [SerializeField] private float[] _timers  = new float[MaxTargets];
    private List<GameObject> markers = new List<GameObject>();
    [SerializeField] private List<Vector3> hitPoints = new List<Vector3>();
    int _lockedCount = 0;
    Transform _lastSeenTarget;


    void Update()
    {
        if (!isAimming) return;
        ScanForTargets();
    }

    /// <summary>Bắt đầu aim: hiện cả 3 slot ngay lập tức với fill = 0.</summary>
    public void StartAiming()
    {
        RocketAimUI.Instance.SetFillBarGroupActive(true);
        isAimming       = true;
        _lockedCount    = 0;
        _lastSeenTarget = null;

        for (int i = 0; i < MaxTargets; i++)
        {
            _targets[i] = null;
            _timers[i]  = lockDuration;
            RocketAimUI.Instance.ActivateSlot(i, 1f);
        }
        markers = new List<GameObject>();
        hitPoints = new List<Vector3>();
    }

    /// <summary>Gọi mỗi frame khi player giữ nút aim.</summary>
    public void ScanForTargets()
    {
        if(_lockedCount >= MaxTargets) return;
        bool addTarget = false;
        _timers[_lockedCount] -= Time.deltaTime;
        RocketAimUI.Instance.SetSlotFill(_lockedCount, _timers[_lockedCount] / lockDuration);

        Vector3 origin    = GameManager.Camera.transform.position;
        Vector3 direction = GameManager.Camera.transform.forward;

        if (Physics.BoxCast(origin, Vector3.one * 0.5f, direction, out RaycastHit hit, Quaternion.identity, _launcherData.range, enemyLayerMask))
        {
            
            if(_timers[_lockedCount] > 0f && hit.collider.CompareTag("enemy"))
            {
                bool alreadyLocked = false;
                 for (int i = 0; i < MaxTargets; i++)
                    if (_targets[i] == hit.transform) { alreadyLocked = true; break; }
                    if (!alreadyLocked)
                        addTarget = true;
            }
            else if(_timers[_lockedCount] <= 0f )
                addTarget = true;
        }

        if (addTarget)
        {
            Debug.Log($"Locked target: {hit.transform.name}");
            _lastSeenTarget = hit.transform;
            AddTarget(hit.transform, hit.point);
            _lockedCount++;
            RocketAimUI.Instance.SetSlotFill(_lockedCount - 1, 0);
        }

        if (_lockedCount >= MaxTargets)
        {
            RocketAimUI.Instance.SetFillBarGroupActive(false);
            StartCoroutine(FireSequence());
        }
    }

    private void AddTarget(Transform target, Vector3 hitPoint)
    {
        markers.Add(MarkerManager.Instance.AddTarget(MarkerPrefab, target, hitPoint));
        _targets[_lockedCount] = target;
        hitPoints.Add(hitPoint);
    }

    /// <summary>Player thả nút aim sớm → bắn những gì đã lock được.</summary>
    public void StopAiming()
    {
        if (!isAimming) return;
        isAimming = false;
        StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        for (int i = 0; i < MaxTargets; i++)
        {
            Transform t = _targets[i] != null ? _targets[i] : _lastSeenTarget;
            if (t != null) ShootAt(hitPoints[i], i);
            _targets[i] = null;
            yield return new WaitForSeconds(_launcherData.fireInterval);
        }

        for (int i = 0; i < MaxTargets; i++)
            RocketAimUI.Instance.DeactivateSlot(i);
        _lastSeenTarget = null;        
        yield return new WaitForSeconds(2f);
        GameManager.Player.ChangeState(PlayerState.Idle);
        markers.ForEach(m => MarkerManager.Instance.RemoveMarker(m));
        RocketAimUI.Instance.SetFillBarGroupActive(false);
    }

    private void ShootAt(Vector3 target, int index)
    {
        Bullet bullet = ObjectPool.Instance.Get(_launcherData.bulletPrefab, _launcherData.shootingPoints[index].position, _launcherData.shootingPoints[index].rotation).GetComponent<Bullet>();
        bullet.Shoot(target, _launcherData.damage);
    }

    public void SetUp(WeaponData data)
    {
        _launcherData = data;
    }
}
