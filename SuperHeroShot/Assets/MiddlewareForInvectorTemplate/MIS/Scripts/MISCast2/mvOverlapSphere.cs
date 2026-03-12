using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvOverlapSphere : mvOverlapCast
    {
        [Space]
        [Min(0f)] public float radius;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvOverlapSphere() : base()
        {
            this.radius = 0.1f;
        }
        public mvOverlapSphere(float radius, int resultCount = DEFAULT_RESULT_COUNT) : base(resultCount)
        {
            this.radius = radius;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual Vector3 GetP1Position(Transform tr, Vector3 offset)
        {
            return tr.TransformPoint(offset);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public bool Overlap(
            Transform tr, Vector3 offset,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(offset);

            Collider[] overlaps = new Collider[resultCount];

            if (GetOverlaps(Physics.OverlapSphereNonAlloc(p1, radius, overlaps, layerMask, query), tagMask, filter, overlaps, out overlapList))
            {
                isDetected = true;
                distance = 0f;
            }
            else
            {
                isDetected = false;
                distance = 0f;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                MISDebugDrawExt.WireSphere(p1, radius, isDetected ? drawGizmo.color : Color.cyan);
#endif

            return isDetected;
        }
        public bool Overlap(
            Vector3 position,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position;

            Collider[] overlaps = new Collider[resultCount];

            if (GetOverlaps(Physics.OverlapSphereNonAlloc(p1, radius, overlaps, layerMask, query), tagMask, filter, overlaps, out overlapList))
            {
                isDetected = true;
                distance = 0f;
            }
            else
            {
                isDetected = false;
                distance = 0f;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                MISDebugDrawExt.WireSphere(p1, radius, isDetected ? drawGizmo.color : Color.cyan);
#endif

            return isDetected;
        }
    }
}
