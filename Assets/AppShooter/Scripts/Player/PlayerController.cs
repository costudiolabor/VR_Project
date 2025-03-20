using System;
using Mirror;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : NetworkBehaviour, ISubscriptionable, IDamageable {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private MeshRenderer[] visuals;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private HitHandler hitHandler;
    [SerializeField] private HealthHandler healthHandler;
    [SerializeField] private HeadCollider headCollider;
    
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action UpdateEvent, DeathEvent;

    private void Start() { Initialize(); }

    private void Initialize() {
        if (isOwned) { UpdateEvent += weaponHandler.OnUpdate; }
        
        hitHandler.Initialize(cameraPlayer);
        hitHandler.CreateImpacts();
        weaponHandler.Initialize(this);
      
        EnableComponents(isOwned);
        Subscription();
        SetLocalPlayer();
    }

    private void SetLocalPlayer() {
        if (!isLocalPlayer) return; 
        NetworkManager networkManager = NetworkManager.singleton;
        networkManager.SetPlayer(gameObject);
    }

    private void EnableComponents(bool state) {
        characterController.enabled = state;
        firstPersonController.enabled = state;
        cameraPlayer.enabled = state;
        audioListener.enabled = state;
        EnableVisual(!state);
    }
 
    private void EnableVisual(bool state) {
        foreach (var visual in visuals) visual.enabled = state;
    }
    
    private void Update() { UpdateEvent?.Invoke(); }
    
    private void Fire() {
        hitHandler.OnShoot();
        CmdFire();
    }

    public void HealthHandlerInitialize() {
        healthHandler.Initialize();
    }
    public int GetMaxHealth() => healthHandler.GetMaxHealth();
    public int GetMaxArmor() => healthHandler.GetMaxArmor();
    
    private void OnHealthChanged(int value) { HealthChangedEvent?.Invoke(value); }
    private void OnArmorChanged(int value) { ArmorChangedEvent?.Invoke(value); }

    private void OnHeadShoot() {
        int headShoot = 1000;
        healthHandler.TakeDamage(headShoot);
    }
    
    [Command]
    private void CmdFire() { RpcOnFire(); }

    [ClientRpc]
    private void RpcOnFire() { weaponHandler.RpcOnFire(); }
    
    [Command]
    private void CmdSetPosImpact(Vector3 position) { RpcSetPosImpact(position); }
    
    [ClientRpc]
    private void RpcSetPosImpact(Vector3 position) { hitHandler.RpcSetPositionImpact(position); }


    [Command]
    private void CmdDeath() { RpcDeath(); }
    
    [ClientRpc]
    private void RpcDeath() {
        rigidBody.isKinematic = false;
        Vector3 direction = new Vector3(10, 10, 10);
        rigidBody.AddForce(direction, ForceMode.Impulse);
    }
    
    private void OnDeath() {
        characterController.enabled = false;
        firstPersonController.enabled = false;
        UpdateEvent = null;
        DeathEvent?.Invoke();
        CmdDeath();
    }

    
    public void Subscription() {
        weaponHandler.ShootEvent += Fire;
        hitHandler.HitEvent += CmdSetPosImpact;
        
        healthHandler.HealthChangedEvent += OnHealthChanged;
        healthHandler.ArmorChangedEvent += OnArmorChanged;
        healthHandler.DeathEvent += OnDeath;
        headCollider.HeadShootEvent += OnHeadShoot;
    }
    
    public void UnSubscription() {
        UpdateEvent = null;
        weaponHandler.ShootEvent -= Fire;
        hitHandler.HitEvent -= CmdSetPosImpact;
        
        healthHandler.HealthChangedEvent -= OnHealthChanged;
        healthHandler.ArmorChangedEvent -= OnArmorChanged;
        healthHandler.DeathEvent -= OnDeath;
        headCollider.HeadShootEvent -= OnHeadShoot;
        
        HealthChangedEvent = null;
        ArmorChangedEvent = null;
        DeathEvent = null;
    }
    
    private void OnDestroy() { UnSubscription(); }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Bullet bullet)) {
            healthHandler.TakeDamage(bullet.damage);
            bullet.Hide();
        }
    }
}
