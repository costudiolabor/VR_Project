using UnityEngine;
using System;

[Serializable]
public class InputHandler : IInput{
    private Vector2 _direction;
    private bool _isWalk;
    private bool _isCrouch;
    
    public event Action LeftButtonEvent, RightButtonEvent, JumpEvent, LeftShiftEvent, LeftControlEvent; 
    public void Initialize() { }
    
    public void OnUpdate() {
        UpdateInputAxis();
        UpdateKeyBoard();
        UpdateMouse();
    }

    public void UpdateInputAxis() {
        _direction.x = Input.GetAxis("Horizontal");
        _direction.y = Input.GetAxis("Vertical");
    }

    public void UpdateKeyBoard() {
        if (Input.GetButtonDown("Jump")) { JumpEvent?.Invoke(); }
        _isWalk = !Input.GetKey(KeyCode.LeftShift);
        _isCrouch = Input.GetKey(KeyCode.LeftControl);
    }

    public void UpdateMouse() {
        if (Input.GetButtonDown("Fire1")) { LeftButtonEvent?.Invoke(); }
    }
    
    public Vector2  GetAxis() => _direction;
    public bool GetStateWalk() => _isWalk;
    public bool GetStateCrouch() => _isCrouch;
    
}