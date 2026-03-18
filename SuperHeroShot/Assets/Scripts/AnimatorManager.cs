using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace rescueforce
{
    public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Vector2 smoothInput;
    [SerializeField] private LookAtConstraint[] lookAtConstraints;

    void Update()
    {
        smoothInput.x = Mathf.Lerp(smoothInput.x, InputManager.Instance.MoveInput.x, 0.1f);
        smoothInput.y = Mathf.Lerp(smoothInput.y, InputManager.Instance.MoveInput.z, 0.1f);
        animator.SetFloat("InputX", smoothInput.x);
        animator.SetFloat("InputZ", smoothInput.y);
        animator.SetInteger("CurrentState", (int)GameManager.Player.CurrentState);
        animator.SetBool("IsSprint", GameManager.Player.CurrentState == PlayerState.Sprint);
    }

    public void SetTriggerSprint()
    {
        animator.SetTrigger("Sprint");        
        SetWeightLayerShoot(0f, true);
    }

    public void SetTriggerIdle()
    {
        animator.SetTrigger("Idle");
    }

    public void SetTriggerMove()
    {
        animator.SetTrigger("Move");
    }

    public void SetTriggerDash(int direction)
    {
        Debug.Log("Dash direction: " + direction);
        if(direction > 0)
        {
            animator.SetTrigger("RollRight");
        }
        else if(direction < 0)
        {
            animator.SetTrigger("RollLeft");
        }
        SetWeightLayerShoot(0f, true);
    }

    public void ShootAnimation()
    {
        SetWeightLayerShoot(1f, false);
        animator.SetTrigger("PistolShoot");
    }

    public void SetWeightLayerShoot(float weight, bool stopAim =  false)
    {
        animator.SetLayerWeight(1, weight);
        if (stopAim)
        {
            animator.SetTrigger("StopAim");
        }
    }

    public void ShootRayAnimation()
    {
        SetWeightLayerShoot(0f, false);
        animator.SetTrigger("LaserShoot");
    }
}

}

