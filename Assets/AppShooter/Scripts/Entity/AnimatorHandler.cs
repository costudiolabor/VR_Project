using System;
using UnityEngine;

[Serializable]
public class AnimatorHandler : IAnimator {
    [SerializeField] private Animator animator;
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string run = "Run";
    [SerializeField] private string walk = "Walk";
    [SerializeField] private string crouching = "Crouching";
    
    private StateAnimation _lastState;
    
    public void Move(Vector2 direction, bool isWalk, bool isCrouching) {
        StateAnimation currentState = StateAnimation.Idle;
        if (isWalk) { currentState = StateAnimation.Walk; }
        else { currentState = StateAnimation.Run; }
        if (isCrouching) { currentState = StateAnimation.Crouching; }
        Direction(direction.x, direction.y);
        if (_lastState == currentState) return;
        _lastState = currentState;
        switch (currentState) {
            case StateAnimation.Idle: Idle(); break;
            case StateAnimation.Walk: 
                Walk(); 
                break;
            case StateAnimation.Run: Run(); break;
            case StateAnimation.Crouching: Crouching(); break;
        }
    }
    
    public void Jump() { animator.SetTrigger(jump); }
    public void Idle() { 
        animator.SetBool(run, false);
        animator.SetBool(walk, false);
        animator.SetBool(crouching, false);
    }
    public void Walk() {
        animator.SetBool(walk, true);
        animator.SetBool(run, false);
        animator.SetBool(crouching, false);
    }
    public void Run() {
        animator.SetBool(run, true);
        animator.SetBool(walk, false);
        animator.SetBool(crouching, false);
    }
    public void Crouching() {
        animator.SetBool(crouching, true);
        animator.SetBool(run, false);
        animator.SetBool(walk, false);
    }
    public void Direction(float horizontal, float vertical) {
        animator.SetFloat("Horiz", horizontal);
        animator.SetFloat("Vert", vertical);
    }
}