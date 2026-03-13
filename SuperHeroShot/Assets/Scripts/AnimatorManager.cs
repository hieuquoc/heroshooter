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
    }
}
