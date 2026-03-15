using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [System.Serializable]
    private class AimTarget
    {
        public Transform transform;
        public float aimTimer;
        public bool IsReady => aimTimer <= 0f;

        public AimTarget(Transform t, float duration)
        {
            transform = t;
            aimTimer = duration;
        }
    }

    private const int MaxTargets = 3;

    [SerializeField] private List<AimTarget> _aimTargets = new List<AimTarget>();
    [SerializeField] private bool isAimming;
    [SerializeField] private float aimDuration = 1f;
    [SerializeField] private float shootDelay = 0.15f;
    public WeaponData launcherData;
    public LayerMask enemyLayerMask;

    void Update()
    {
        if (!isAimming) return;

        // Remove targets whose GameObject has been destroyed
        _aimTargets.RemoveAll(t => t.transform == null);

        // No targets at all — don't shoot
        if (_aimTargets.Count == 0)
        {
            isAimming = false;
            return;
        }

        // Tick each target's individual aim timer
        foreach (var t in _aimTargets)
            t.aimTimer -= Time.deltaTime;

        // Only fire when ALL tracked targets are ready
        bool allReady = true;
        foreach (var t in _aimTargets)
        {
            if (!t.IsReady) { allReady = false; break; }
        }

        if (allReady)
        {
            StartCoroutine(FireSequence());
            isAimming = false;
        }
    }

    public void StartAiming()
    {
        isAimming = true;
    }

    // Call this each frame while player holds aim button.
    // Adds newly found targets up to MaxTargets.
    // Previously acquired targets are kept as fallback if fewer than MaxTargets found.
    public void ScanForTargets()
    {
        // Clean up dead references first
        _aimTargets.RemoveAll(t => t.transform == null);

        if (_aimTargets.Count >= MaxTargets) return;

        Vector3 scanOrigin = GameManager.Camera.transform.position;
        Vector3 scanDirection = GameManager.Camera.transform.forward;

        if (!Physics.Raycast(scanOrigin, scanDirection, out RaycastHit hit, launcherData.range, enemyLayerMask))
            return; // No new target — existing targets serve as fallback

        if (!hit.collider.CompareTag("enemy"))
            return;

        // Skip if already tracked
        foreach (var t in _aimTargets)
        {
            if (t.transform == hit.transform) return;
        }

        _aimTargets.Add(new AimTarget(hit.transform, aimDuration));
    }

    private IEnumerator FireSequence()
    {
        List<AimTarget> toFire = new List<AimTarget>(_aimTargets);
        _aimTargets.Clear();

        foreach (var t in toFire)
        {
            if (t.transform != null)
                ShootAt(t.transform);
            yield return new WaitForSeconds(shootDelay);
        }
    }

    private void ShootAt(Transform target)
    {
        // TODO: pool/instantiate rocket toward target
    }
}
