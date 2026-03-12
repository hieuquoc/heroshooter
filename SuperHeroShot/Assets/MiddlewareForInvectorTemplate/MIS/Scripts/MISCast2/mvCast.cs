using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvCast
    {
#if UNITY_EDITOR
        [Header("Debug")]
        public mvDrawGizmos drawGizmo = new(false, Color.red, "");
#endif
        [SerializeField] protected GameObject hitObject;

        [Header("Result")]
        [mvReadOnly] public bool isDetected = false;
        [mvReadOnly] public float distance = 0f;
        [mvReadOnly] public RaycastHit hit = default;

        [Header("Caster")]
        public Vector3 castOffset;
        [Min(0f)] public float backOffset = 0f;
        [Min(-1f)] public float maxDistance = 1f;

        [Space]
        public bool useLocalDirection;
        [vHideInInspector("useLocalDirection", false)] public Vector3 localDirection;

        protected const int DEFAULT_RESULT_COUNT = 8;
        protected int resultCount;
        protected RaycastHit[] hits;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvCast()
        {
            this.resultCount = DEFAULT_RESULT_COUNT;
            hits = new RaycastHit[DEFAULT_RESULT_COUNT];
        }
        public mvCast(int resultCount = DEFAULT_RESULT_COUNT)
        {
            this.resultCount = resultCount;
            hits = new RaycastHit[resultCount];
        }
        public mvCast(Vector3 castOffset, float backOffset, float maxDistance, int resultCount = DEFAULT_RESULT_COUNT) : this(resultCount)
        {
            this.castOffset = castOffset;
            this.backOffset = backOffset;
            this.maxDistance = maxDistance;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ClearResult()
        {
            isDetected = false;
            hit = default;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool GetClosestHit(int hitCount, vTagMask tagMask, IMISColliderFilter filter, RaycastHit[] hits, out RaycastHit hit)
        {
            hit = new();

            if (hitCount == 0)
                return false;

            int filteredCount = hitCount;
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].distance < 0f ||
                    (tagMask != null && tagMask.Count > 0 && !tagMask.Contains(hits[i].collider.tag)) ||
                    (filter != null && filter.FilterCollider(hits[i].collider)))
                {
                    filteredCount--;
                    continue;
                }

                if (hits[i].distance < closestDistance)
                {
                    hit = hits[i];
                    closestDistance = hit.distance;
                }
            }

            return filteredCount > 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool GetBlockedClosestHit(int hitCount, vTagMask tagMask, LayerMask layerMask, IMISColliderFilter filter, RaycastHit[] hits, out RaycastHit hit)
        {
            hit = new();

            if (hitCount == 0)
                return false;

            int filteredCount = hitCount;
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                if ((tagMask != null && tagMask.Count > 0 && !tagMask.Contains(hits[i].collider.tag))
                    || hits[i].distance < 0f
                    || (filter != null && filter.FilterCollider(hits[i].collider)))
                {
                    filteredCount--;
                    continue;
                }

                if (hits[i].distance < closestDistance)
                {
                    hit = hits[i];
                    closestDistance = hit.distance;
                }
            }

            return filteredCount > 0 && layerMask.LayerMaskContains(hit.collider.gameObject.layer);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void SetCapsuleDimension(Vector3 position, float radius, float height, out Vector3 topCenter, out Vector3 bottomCenter)
        {
            radius = Mathf.Max(radius, 0f);
            height = Mathf.Max(height, radius * 2f);

            Vector3 center = position + (height * 0.5f * Vector3.up);
            float sideHeight = height - radius * 2f;

            bottomCenter = center - sideHeight * 0.5f * Vector3.up;
            topCenter = center + sideHeight * 0.5f * Vector3.up;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public static class CastExtension
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static void DrawRayCast(this mvRayCast caster, Vector3 p1, Vector3 direction)
        {
#if UNITY_EDITOR
            if (caster.isDetected)
                Debug.DrawRay(p1, caster.hit.distance * direction, caster.drawGizmo.color);
            else
                Debug.DrawRay(p1, caster.maxDistance * direction, Color.white);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static void DrawBoxCast(this mvBoxCast caster, Vector3 p1, Vector3 halfExtents, Quaternion orientation, Vector3 lossyScale, Vector3 direction)
        {
#if UNITY_EDITOR
            MISDebugDrawExt.WireCube(p1, halfExtents, orientation, Color.cyan);

            if (caster.isDetected)
                MISDebugDrawExt.WireCube(p1 + (caster.hit.distance * direction), halfExtents, orientation, caster.drawGizmo.color);
            else
                MISDebugDrawExt.WireCube(p1 + (caster.maxDistance * direction), halfExtents, orientation, Color.white);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static void DrawSphereCast(this mvSphereCast caster, Vector3 p1, Vector3 direction)
        {
#if UNITY_EDITOR
            MISDebugDrawExt.WireSphere(p1, caster.radius, Color.cyan);

            if (caster.isDetected)
                MISDebugDrawExt.WireSphere(p1 + (caster.hit.distance * direction), caster.radius, caster.drawGizmo.color);
            else
                MISDebugDrawExt.WireSphere(p1 + (caster.hit.distance * direction), caster.radius, Color.white);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static void DrawCapsuleCast(this mvCapsuleCast caster, Vector3 p1, Vector3 p2, Vector3 direction)
        {
#if UNITY_EDITOR
            MISDebugDrawExt.WireCapsule(p1, p2, caster.radius, Color.cyan);

            if (caster.isDetected)
                MISDebugDrawExt.WireCapsule(p1 + (caster.hit.distance * direction), p2 + (caster.hit.distance * direction), caster.radius, caster.drawGizmo.color);
            else
                MISDebugDrawExt.WireCapsule(p1 + (caster.maxDistance * direction), p2 + (caster.maxDistance * direction), caster.radius, Color.white);
#endif
        }
    }
}