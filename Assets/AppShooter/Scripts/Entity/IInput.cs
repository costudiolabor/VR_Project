using System;
using UnityEngine;

public interface IInput {
    public event Action LeftButtonEvent, RightButtonEvent, JumpEvent, LeftShiftEvent, LeftControlEvent;
    public void Initialize();

    public void OnUpdate();

    public void UpdateInputAxis();

    public void UpdateKeyBoard();

    public void UpdateMouse();
    
    public Vector2 GetAxis();
    public bool GetStateWalk();
    public bool GetStateCrouch();
}
