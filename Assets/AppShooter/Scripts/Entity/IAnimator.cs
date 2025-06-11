using UnityEngine;
public interface IAnimator {
    public void Move(Vector2 direction, bool isWalk, bool isCrouching) { }
    public void Jump() { }
    public void Idle() { }
    public void Walk() { }
    public void Run() { }
    public void Crouching() { }
    public void Direction(float horizontal, float vertical) { }
}