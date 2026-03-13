using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Vector2 smoothInput;

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
        if(direction > 0)
        {
            animator.SetTrigger("RollRight");
        }
        else if(direction < 0)
        {
            animator.SetTrigger("RollLeft");
        }
    }
}
