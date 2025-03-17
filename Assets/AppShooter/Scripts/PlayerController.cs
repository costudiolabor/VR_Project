using System;
using Mirror;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : NetworkBehaviour {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private HitHandler hitHandler;
    public event Action UpdateEvent;
    
    private void Start() { Initialize(); }

    private void Initialize() {
        if (isOwned) {
            hitHandler.Initialize(cameraPlayer);
            UpdateEvent += weaponHandler.OnUpdate;
        }
        
        hitHandler.CreateImpacts();
        weaponHandler.Initialize(this);
        SetComponents(isOwned);
        Subscription();
    }

    private void SetComponents(bool state) {
        characterController.enabled = state;
        firstPersonController.enabled = state;
        cameraPlayer.enabled = state;
    }

    private void Update() { UpdateEvent?.Invoke(); }

    [Command]
    private void CmdFire() {
        RpcOnFire();
    }

    [ClientRpc]
    private void RpcOnFire() { 
        weaponHandler.RpcOnFire();
    }
    
    private void Subscription() {
        weaponHandler.ShootEvent += CmdFire;
    }
    
    private void UnSubscription() {
        UpdateEvent = null;
        weaponHandler.ShootEvent -= CmdFire;
    }
    
    private void OnDestroy() { UnSubscription(); }
}
