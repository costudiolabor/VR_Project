using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;


public class PlayerController : NetworkBehaviour, ISubscriptionable {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    //[SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private MeshRenderer[] visuals;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private HitHandler hitHandler;
    [SerializeField] private HealthHandler healthHandler;
    [SerializeField] private Damagable[] damages;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private byte damage = 10;
    
    private Camera _camera;
    private Vector2 _center;
    private Transform _hitTransform;
    
    [SyncVar] public int Health = 100;
    [SyncVar] public int HealthMax = 100;
    [SyncVar] public int Kills;
    [SyncVar] public int Deaths;
    [SyncVar] public bool isDead;
    
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action UpdateEvent, DeathEvent;
    private void Start() { Initialize(); }

    private void Initialize() {
        if (isOwned) { UpdateEvent += weaponHandler.OnUpdate; }
        hitHandler.Initialize(cameraPlayer);
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
 
    private void EnableVisual(bool state) { foreach (var visual in visuals) visual.enabled = state; }
    private void Update() { UpdateEvent?.Invoke(); }
    private void Fire() {
        TryShoot();
        CmdFire();
    }
    [Command]
    private void TryShoot() { hitHandler.TryShoot(damage); }
    public void HealthHandlerInitialize() { healthHandler.Initialize(); }
    public int GetMaxHealth() => healthHandler.GetMaxHealth();
    public int GetMaxArmor() => healthHandler.GetMaxArmor();
    private void OnHealthChanged(int value) { HealthChangedEvent?.Invoke(value); }
    private void OnArmorChanged(int value) { ArmorChangedEvent?.Invoke(value); }
    
    public void OnDamage(int damage) {
        TargetGotDamage(damage); 
        Health -= damage;
        if (Health < 1) {
            Die();
            Kills++;
            TargetGotKill();
        }
    }
    
    [TargetRpc]
    public void TargetGotDamage(int damage){
        //CanvasManager.instance.UpdateHP(Health, HealthMax);
        healthHandler.TakeDamage(damage);
        Debug.Log("We got hit!");
    }
    
    [Server]
    public void Die() {
        Deaths++;
        isDead = true;
        Debug.Log("SERVER: Player died.");
        TargetDie();
        RpcPlayerDie();
    }
    
    [TargetRpc]
    void TargetDie() {
        //Called on the died player.
        //CanvasManager.instance.ChangePlayerState(!isDead);
        PlayerDie();
        Debug.Log("You died.");
    }
    
    [ClientRpc]
    void RpcPlayerDie() { PlayerDie(); }

    private void PlayerDie() {
        characterController.enabled = false;
        firstPersonController.enabled = false;
        Vector3 direction = new Vector3(0.1f, 0.1f, 0.1f);
        for (int i = 0; i < damages.Length; i++) {
            Rigidbody rigidBody = damages[i].GetComponent<Rigidbody>();
            rigidBody.isKinematic = false;
            rigidBody.AddForce(direction, ForceMode.Impulse);
        }
        DeathEvent?.Invoke();
        UpdateEvent = null;
    }
    
    
    
    [TargetRpc]
    public void TargetGotKill() { Debug.Log("You got kill."); }
  
    [Command]
    private void CmdFire() { RpcOnFire(); }

    [ClientRpc]
    private void RpcOnFire() { weaponHandler.RpcOnFire(); }

    private void LateUpdate() { textName.text = netId.ToString(); }

    public void Subscription() {
        weaponHandler.ShootEvent += Fire;
        healthHandler.HealthChangedEvent += OnHealthChanged;
       // healthHandler.ArmorChangedEvent += OnArmorChanged;
        //healthHandler.DeathEvent += Die;
        SubscriptionDamages();
    }

    public void SubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent += OnDamage; }
    } 
    
    public void UnSubscription() {
        weaponHandler.ShootEvent -= Fire;
        healthHandler.HealthChangedEvent -= OnHealthChanged;
        //healthHandler.ArmorChangedEvent -= OnArmorChanged;
       // healthHandler.DeathEvent -= Die;
        UnSubscriptionDamages();
        UpdateEvent = null;
        HealthChangedEvent = null;
        ArmorChangedEvent = null;
        DeathEvent = null;
    }
    
    public void UnSubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent -= OnDamage; }
    } 

    private void OnDestroy() { UnSubscription(); }

  

}
