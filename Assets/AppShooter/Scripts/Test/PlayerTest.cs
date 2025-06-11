using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerTest : MonoBehaviour {
    [SerializeField] private IInput inputHandler;
    [SerializeField] private IAnimator animatorHandler;
    
    private RepeatTimer _repeatTimer;
    
    private void Start() { Initialize(); }

    private void Initialize() {
        
        _repeatTimer = new RepeatTimer();
        _repeatTimer.Start(1.0f);

        inputHandler = new InputHandler();
        inputHandler.Initialize();

        Subscription();
    }

    private void Update() {
        _repeatTimer.Tick();
        inputHandler.OnUpdate();
        Vector2 direction = inputHandler.GetAxis();
        bool isWalk = inputHandler.GetStateWalk();
        bool isCrouch = inputHandler.GetStateCrouch();
        animatorHandler.Move(direction, isWalk, isCrouch);
    }

    private void ChangeState() {
        Debug.Log("change state");
    }

    public void Subscription()
    {
        _repeatTimer.AlarmEvent += ChangeState;
        inputHandler.JumpEvent += animatorHandler.Jump;
    }

    public void UnSubscription() {
        _repeatTimer.AlarmEvent -= ChangeState;
        inputHandler.JumpEvent -= animatorHandler.Jump;
    }
    
    private void OnDestroy() { UnSubscription(); }

}
