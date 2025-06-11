using UnityStandardAssets.Characters.FirstPerson;
using Object = UnityEngine.Object;
using UnityEngine;
using System;

[Serializable]
public class MoveHandler {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    public event Action JumpEvent;
    public event Action <Vector2, bool, bool>MoveEvent;
    public void Initialize() { }
    public void EnableComponents(bool state) {
        characterController.enabled = state;
        firstPersonController.enabled = state;
    }
    
    public void SetLocalPlayer() {
            firstPersonController.Initialize();
            firstPersonController.JumpEvent += Jump;
            firstPersonController.MoveEvent += CmdMove;
    }
    public void OnUpdate() { firstPersonController.OnUpdate(); }
    public void OnFixedUpdate() { firstPersonController.OnFixedUpdate(); }
    private void Jump() {JumpEvent?.Invoke(); }
    private void CmdMove(Vector2 direction, bool isWalk, bool isCrouching) {
        MoveEvent?.Invoke(direction, isWalk, isCrouching);
    }
    
    public void OnJump() { firstPersonController.OnJump(); }

    public void SetInputAxis(Vector2 inputAxis) { firstPersonController.SetInputAxis(inputAxis); } 

    public void PlayerDie() {
        if (characterController) characterController.enabled = false;
        if (firstPersonController) firstPersonController.enabled = false;
    }

    public void Destroy() {
        Object.Destroy(characterController);
        Object.Destroy(firstPersonController);
        characterController = null;
        firstPersonController = null;
    }
    
}
