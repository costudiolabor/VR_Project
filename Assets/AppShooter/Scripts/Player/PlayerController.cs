using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : NetworkBehaviour, ISubscriptionable {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private MeshRenderer[] visuals;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private WeaponHandler weaponHandler;
    //[SerializeField] private HitHandler hitHandler;
    [SerializeField] private HealthHandler healthHandler;
    [SerializeField] private HeadCollider headCollider;
    [SerializeField] private TMP_Text textName;

    //private uint _netId;
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action UpdateEvent, DeathEvent;

    private void Start() { Initialize(); }

    private void Initialize() {
        if (isOwned) { UpdateEvent += weaponHandler.OnUpdate; }
        
        //hitHandler.Initialize(cameraPlayer);
        // hitHandler.CreateImpacts();
        weaponHandler.Initialize(this);
      
        EnableComponents(isOwned);
        Subscription();
        SetLocalPlayer();
        HealthHandlerInitialize();
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
    public uint GetIdentity() => netId;
    
    private void Fire() {
        //hitHandler.OnShoot();
        OnShoot();
        CmdFire();
    }
    public void HealthHandlerInitialize() { healthHandler.Initialize(); }
    public int GetMaxHealth() => healthHandler.GetMaxHealth();
    public int GetMaxArmor() => healthHandler.GetMaxArmor();
    private void OnHealthChanged(int value) { HealthChangedEvent?.Invoke(value); }
    private void OnArmorChanged(int value) { ArmorChangedEvent?.Invoke(value); }
   
    private void OnHeadShoot() {
        byte headShoot = 255;
        //CmdTakeDamage(headShoot);
    }

    public void TakeDamage(byte damage) {
        if (!isLocalPlayer) return;
        Debug.Log("TakeDamage " + damage);
        CmdTakeDamage(damage);
    }

    [Command]
    private void CmdTakeDamage(byte damage) {
        Debug.Log("CmdTakeDamage " + damage);
        RpcTakeDamage(damage);
    }

    [ClientRpc]
    private void RpcTakeDamage(byte damage) {
        Debug.Log("RpcTakeDamage " + damage);
        healthHandler.TakeDamage(damage);
    }
    
    [Command]
    private void CmdFire() { RpcOnFire(); }

    [ClientRpc]
    private void RpcOnFire() { weaponHandler.RpcOnFire(); }
    
    //[Command]
    //private void CmdSetPosImpact(Vector3 position) { RpcSetPosImpact(position); }
   // private void CmdHitPlayer(uint netInId, byte damage) { RpcHitPlayer(netInId, damage); }
    
    //[ClientRpc]
    //private void RpcSetPosImpact(Vector3 position) { hitHandler.RpcSetPositionImpact(position); }

    [ClientRpc]
    private void RpcHitPlayer(uint netInId, byte damage) {
        Debug.Log("HitPlayer " + netInId);
        Debug.Log("LocalPlayer " + netId);
        if (netId == netInId) healthHandler.TakeDamage(damage);
    }

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

    private void LateUpdate() {
        textName.text = netId.ToString();
    }

    public void Subscription() {
        weaponHandler.ShootEvent += Fire;
        //hitHandler.HitEvent += CmdHitPlayer;
        
        healthHandler.HealthChangedEvent += OnHealthChanged;
        healthHandler.ArmorChangedEvent += OnArmorChanged;
        healthHandler.DeathEvent += OnDeath;
        headCollider.HeadShootEvent += OnHeadShoot;
    }
    
    public void UnSubscription() {
        UpdateEvent = null;
        weaponHandler.ShootEvent -= Fire;
        //hitHandler.HitEvent -= CmdHitPlayer;
        
        healthHandler.HealthChangedEvent -= OnHealthChanged;
        healthHandler.ArmorChangedEvent -= OnArmorChanged;
        healthHandler.DeathEvent -= OnDeath;
        headCollider.HeadShootEvent -= OnHeadShoot;
        
        HealthChangedEvent = null;
        ArmorChangedEvent = null;
        DeathEvent = null;
    }
    
    private void OnDestroy() { UnSubscription(); }

    // private void OnTriggerEnter(Collider other) {
    //     if (other.TryGetComponent(out Bullet bullet)) {
    //         healthHandler.TakeDamage(bullet.damage);
    //         bullet.Hide();
    //     }
    // }
    
    private Camera _camera; 
    private Vector2 _center;
    private Transform _hitTransform;
    [SerializeField] private byte damage = 10;
    [SerializeField] private byte damageHead = 10;
    [SerializeField] private float rayLength = 100.0f;
    
    public void OnShoot() {
        Debug.Log("OnShoot");
        _camera = cameraPlayer;
        _center.x = _camera.pixelWidth / 2.0f;
        _center.y = _camera.pixelHeight / 2.0f;
        
        var raycastHit = RayFromCamera(_center, out var isHitRayCast);
        if (isHitRayCast) {
            _hitTransform = raycastHit.transform;
            
            if (_hitTransform.TryGetComponent(out PlayerController playerController) ) {
                playerController.TakeDamage(damage);
            }
        }
    }
    
    public RaycastHit RayFromCamera(Vector3 position, out bool isHitRayCast) {
        var ray = _camera.ScreenPointToRay(position);
        isHitRayCast = Physics.Raycast(ray, out var hit, rayLength);
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, 0.5f);
        return hit;
    }
}
