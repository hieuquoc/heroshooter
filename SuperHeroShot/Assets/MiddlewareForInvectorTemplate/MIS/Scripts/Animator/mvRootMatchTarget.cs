using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvRootMatchTarget
    {
#if MIS
        public bool useMatchTarget = true;
        public string animationState;
        public Transform target;
        public mvMinMaxNormalizedTime matchTime = new(0f, 0.9f);


        // ----------------------------------------------------------------------------------------------------
        // 
        public virtual AvatarTarget avatarTarget => AvatarTarget.Root;


        // ----------------------------------------------------------------------------------------------------
        // 
        public virtual bool HasAnimation => !string.IsNullOrEmpty(animationState);
        public virtual bool HasTarget => target != null && (object)target != null;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvRootMatchTarget()
        {
            useMatchTarget = true;
            matchTime = new(0f, 0.9f);
        }
        public mvRootMatchTarget(mvMinMaxNormalizedTime matchTime) : this()
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
        public virtual void Clone(out mvRootMatchTarget matchTarget)
        {
            matchTarget = new()
            {
                animationState = this.animationState,
                target = this.target,
                
                matchTime = new mvMinMaxNormalizedTime(this.matchTime)
            };
        }
        public virtual mvRootMatchTarget Clone()
        {
            mvRootMatchTarget matchTarget = new()
            {
                animationState = this.animationState,
                target = this.target
            };

            return matchTarget;
        }
#endif
    }
}