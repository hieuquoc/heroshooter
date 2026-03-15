using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Mouse Input")]
    public float mouseSensitivity = 3f;
    public bool enableYaw = true;
    public bool enablePitch = true;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    [Header("Normal Offset")]
    public Vector3 offset = new Vector3(0f, 1.8f, -4f);

    [Header("Shoulder Offset (aim)")]
    public Vector3 shoulderOffset = new Vector3(0.6f, 1.6f, -2f);
    public bool useShoulder = false;
    public bool shoulderRight = true;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float offsetLerpSpeed = 10f;

    [Header("Collision")]
    public float collisionRadius = 0.2f;
    public LayerMask collisionMask = ~0;
    public Vector3 sprintShoulderOffset = new Vector3(0.8f, 1.6f, -2.5f);

    float yaw;
    float pitch;
    Vector3 currentOffset;

    public float Yaw => yaw;
    public float Pitch => pitch;
    Vector3 velocity = Vector3.zero;
    private bool _cameraRecovering = false;
    private Vector3 _originalShoulderOffset;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        currentOffset = offset;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _originalShoulderOffset = shoulderOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- Mouse input ---
        if (enableYaw)   yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
        if (enablePitch) pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // --- Shoulder offset blend ---
        Vector3 desired = useShoulder ? shoulderOffset : offset;
        if (useShoulder && !shoulderRight) desired.x = -desired.x;
        currentOffset = Vector3.Lerp(currentOffset, desired, Time.deltaTime * offsetLerpSpeed);

        // Pivot: target position + local-space horizontal/vertical offset, rotated by camera yaw/pitch
        Vector3 pivot = target.position + rotation * new Vector3(currentOffset.x, currentOffset.y, 0f);

        // Desired camera position behind pivot
        Vector3 desiredPos = pivot + rotation * new Vector3(0f, 0f, currentOffset.z);

        // --- Collision: SphereCast from pivot toward camera ---
        Vector3 dir = desiredPos - pivot;
        float desiredDist = dir.magnitude;
        float actualDist = desiredDist;

        if (Physics.SphereCast(pivot, collisionRadius, dir.normalized, out RaycastHit hit, desiredDist, collisionMask))
            actualDist = Mathf.Max(hit.distance - collisionRadius, 0f);

        Vector3 targetPos = pivot + dir.normalized * actualDist;

        // --- Smooth position ---
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, positionSmoothTime);
        transform.rotation = rotation;
        if(GameManager.Player.CurrentState == PlayerState.Sprint)
        {
            shoulderOffset = Vector3.Lerp(shoulderOffset, sprintShoulderOffset, Time.deltaTime * offsetLerpSpeed);
            _cameraRecovering = false;
        }else if(useShoulder && !_cameraRecovering)
        {
            shoulderOffset = Vector3.Lerp(shoulderOffset, _originalShoulderOffset, Time.deltaTime * offsetLerpSpeed);
            if(Vector3.Distance(shoulderOffset, _originalShoulderOffset) < 0.1f)
            {
                _cameraRecovering = true;
            }
        }
    }

    public void ToggleShoulder()
    {
        useShoulder = !useShoulder;
    }

    public void SetShoulderSide(bool right)
    {
        shoulderRight = right;
    }
}
