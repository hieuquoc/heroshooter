using System.Collections;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private const int MaxTargets = 3;

    [SerializeField] private bool isAimming;
    [SerializeField] private float lockDuration = 3f;
    [SerializeField] private float shootDelay   = 0.15f;
    [SerializeField] private WeaponData _launcherData;
    public LayerMask enemyLayerMask;

    public GameObject MarkerPrefab;   // prefab marker để hiển thị trên target đã lock

    readonly Transform[] _targets = new Transform[MaxTargets];
    readonly float[]     _timers  = new float[MaxTargets];
    int _lockedCount = 0;
    Transform _lastSeenTarget;


    void Update()
    {
        if (!isAimming) return;

        ScanForTargets();

        for (int i = 0; i < MaxTargets; i++)
        {
            if (_targets[i] == null) continue;

            _timers[i] -= Time.deltaTime;
            if (_timers[i] <= 0f)
            {
                _targets[i] = null;
                _lockedCount--;
                RocketAimUI.Instance.SetSlotFill(i, 0f);
            }
            else
            {
                RocketAimUI.Instance.SetSlotFill(i, _timers[i] / lockDuration);
            }
        }
    }

    /// <summary>Bắt đầu aim: hiện cả 3 slot ngay lập tức với fill = 0.</summary>
    public void StartAiming()
    {
        RocketAimUI.Instance.gameObject.SetActive(true);
        isAimming       = true;
        _lockedCount    = 0;
        _lastSeenTarget = null;

        for (int i = 0; i < MaxTargets; i++)
        {
            _targets[i] = null;
            _timers[i]  = 0f;
            RocketAimUI.Instance.ActivateSlot(i, 0f);
        }
    }

    /// <summary>Gọi mỗi frame khi player giữ nút aim.</summary>
    public void ScanForTargets()
    {
        if (!isAimming || _lockedCount >= MaxTargets) return;

        Vector3 origin    = GameManager.Camera.transform.position;
        Vector3 direction = GameManager.Camera.transform.forward;
        Debug.DrawRay(origin, direction * _launcherData.range, Color.red);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, _launcherData.range, enemyLayerMask)
            && hit.collider.CompareTag("enemy"))
        {
            Debug.Log($"Locked target: {hit.transform.name}");
            _lastSeenTarget = hit.transform;
            MarkerManager.Instance.AddTarget(MarkerPrefab, hit.transform);

            bool alreadyLocked = false;
            for (int i = 0; i < MaxTargets; i++)
                if (_targets[i] == hit.transform) { alreadyLocked = true; break; }

            if (!alreadyLocked)
            {
                for (int i = 0; i < MaxTargets; i++)
                {
                    if (_targets[i] != null) continue;
                    _targets[i] = hit.transform;
                    _timers[i]  = lockDuration;
                    _lockedCount++;
                    RocketAimUI.Instance.SetSlotFill(i, 1f);
                    break;
                }
            }
        }

        if (_lockedCount >= MaxTargets)
        {
            RocketAimUI.Instance.gameObject.SetActive(false);
            StartCoroutine(FireSequence());
            isAimming = false;
        }
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
            if (t != null) ShootAt(t);
            _targets[i] = null;
            yield return new WaitForSeconds(shootDelay);
        }

        for (int i = 0; i < MaxTargets; i++)
            RocketAimUI.Instance.DeactivateSlot(i);

        _lockedCount    = 0;
        _lastSeenTarget = null;
        GameManager.Player.ChangeState(PlayerState.Idle);
    }

    private void ShootAt(Transform target)
    {
        // TODO: pool/instantiate rocket toward target
    }

    public void SetUp(WeaponData data)
    {
        _launcherData = data;
    }
}
