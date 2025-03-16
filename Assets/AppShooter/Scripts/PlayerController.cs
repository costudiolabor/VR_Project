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

    private void Initialize()
    {
        //bool isOwned = true;
        if (isOwned) {
            characterController.enabled = true;
            firstPersonController.enabled = true;
            cameraPlayer.enabled = true;
            weaponHandler.Initialize(this);
            hitHandler.Initialize(cameraPlayer);
            Subscription();
        }
        else {
            characterController.enabled = false;
            firstPersonController.enabled = false;
            cameraPlayer.enabled = false;
        }
    }

    void Update() { UpdateEvent?.Invoke(); }

    private void Subscription() {
        UpdateEvent += weaponHandler.OnUpdate;
        weaponHandler.ShootEvent += hitHandler.OnShoot;
    }
    
    private void UnSubscription() {
        UpdateEvent -= UpdateEvent;
        weaponHandler.ShootEvent -= hitHandler.OnShoot;
    }

    private void OnDestroy() { UnSubscription(); }
}
