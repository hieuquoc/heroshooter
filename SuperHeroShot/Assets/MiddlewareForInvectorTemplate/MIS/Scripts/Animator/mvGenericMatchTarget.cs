using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvGenericMatchTarget
    {
#if MIS
        public bool useMatchTarget = true;
        public string animationState;
        public AvatarTarget avatarTarget;
        public Transform target;
        public mvMinMaxNormalizedTime matchTime = new(0f, 0.9f);


        // ----------------------------------------------------------------------------------------------------
        // 
        public virtual bool HasAnimation => !string.IsNullOrEmpty(animationState);
        public virtual bool HasTarget => target != null && (object)target != null;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvGenericMatchTarget()
        {
            useMatchTarget = true;
            matchTime = new(0f, 0.9f);
        }
        public mvGenericMatchTarget(mvMinMaxNormalizedTime matchTime) : this()
        {
            this.matchTime = new mvMinMaxNormalizedTime()
            {
                min = matchTime.min,
                max = matchTime.max
            };
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Clone(out mvGenericMatchTarget matchTarget)
        {
            matchTarget = new()
            {
                animationState = this.animationState,
                avatarTarget = this.avatarTarget,
                target = this.target,

                matchTime = new mvMinMaxNormalizedTime(this.matchTime)
            };
        }
        public virtual mvGenericMatchTarget Clone()
        {
            mvGenericMatchTarget matchTarget = new()
            {
                animationState = this.animationState,
                avatarTarget = this.avatarTarget,
                target = this.target
            };

            return matchTarget;
        }
#endif
    }
}