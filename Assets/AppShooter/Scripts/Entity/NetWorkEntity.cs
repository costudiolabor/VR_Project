using UnityEngine;
using System;
using Mirror;
using TMPro;

public class NetWorkEntity : NetworkBehaviour {
    
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private MoveHandler moveHandler;
    [SerializeField] private ViewHandler viewHandler;
    [SerializeField] private InfoHandler infoHandler;
    [SerializeField] private AnimatorHandler animatorHandler;
    [SerializeField] private CameraHandler cameraHandler;
    [SerializeField] private WeaponHandler weaponHandler;
    [SerializeField] private HitHandler hitHandler;
    [SerializeField] private DamageHandler damageHandler;
    [SerializeField] private HealthHandler healthHandler;
    
    //[SerializeField] private TMP_Text textName;
    
    private Camera _camera;
    private Vector2 _center;
    private Transform _hitTransform;
    //private StateAnimation _lastState;
    private Camera cameraPlayer;
    
    [SyncVar] public int Health = 100;
    [SyncVar] public int HealthMax = 100;
    [SyncVar] public int Kills;
    [SyncVar] public int Deaths;
    [SyncVar] public bool isDead;
    
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action UpdateEvent, FixedUpdateEvent, DeathEvent;
    private void Start() { Initialize(); }

    private void Initialize() {
        inputHandler.Initialize();
        if (isOwned) { UpdateEvent += weaponHandler.OnUpdate; }

        cameraPlayer = cameraHandler.GetCamera();
        hitHandler.Initialize(cameraPlayer);
        // hitHandler.CreateImpacts();
        weaponHandler.Initialize(this);
        EnableComponents(isOwned);
        Subscription();
        SetLocalPlayer();
        HealthHandlerInitialize();
    }

    private void SetLocalPlayer() {
        if (isLocalPlayer) {
            NetworkManager networkManager = NetworkManager.singleton;
            networkManager.SetPlayer(gameObject);
            viewHandler.SetLocalPlayer();
            moveHandler.SetLocalPlayer();
            UpdateEvent += moveHandler.OnUpdate;
            FixedUpdateEvent += moveHandler.OnFixedUpdate;
            moveHandler.JumpEvent += CmdJump;
            moveHandler.MoveEvent += CmdMove;
            SetName();
        }
        else {
            viewHandler.SetRemotePlayer();
            moveHandler.Destroy();
        }
    }
    
    public void SetName() {
        NetworkManager networkManager = NetworkManager.singleton;
        string namePlayer = networkManager.GetNamePlayer();
        //Debug.Log("SetName " + namePlayer);
        CmdChangeName(namePlayer);
    }

    private void EnableComponents(bool state) {
        moveHandler.EnableComponents(state);
        cameraHandler.EnableComponents(state);
    }
 
    [Command] private void CmdChangeName(string newName) {
        //Debug.Log("CmdChangeName " + newName);
        RpcChangeName(newName);
    }
    
    [ClientRpc] private void RpcChangeName(string newName) {
        //Debug.Log("RpcChangeName " + newName);
        //textName.text = newName;
        infoHandler.ChangeName(newName);
    }

    private void Update() {
        if (isLocalPlayer) {
            inputHandler.OnUpdate();
            Vector2 axis = inputHandler.GetAxis();
            moveHandler.SetInputAxis(axis);
        }

        UpdateEvent?.Invoke();
    }

    [Command] private void CmdJump() { RpcJump(); } 
    
    [ClientRpc] private void RpcJump() { animatorHandler.Jump(); }

    [Command] private void CmdMove(Vector2 direction, bool isWalk, bool isCrouching) {
        RpcMove(direction, isWalk, isCrouching);
    }
    
    [ClientRpc] private void RpcMove(Vector2 direction, bool isWalk, bool isCrouching) {
        animatorHandler.Move(direction, isWalk, isCrouching);
        // StateAnimation currentState = StateAnimation.Idle;
        //
        // if (isWalk) { currentState = StateAnimation.Walk; }
        // else { currentState = StateAnimation.Run; }
        //
        // if (isCrouching) { currentState = StateAnimation.Crouching; }
        // animatorHandler.Direction(direction.x, direction.y);
        //
        // if (_lastState == currentState) return;
        // _lastState = currentState;
        //
        // switch (currentState) {
        //     case StateAnimation.Idle: animatorHandler.Idle(); break;
        //     case StateAnimation.Walk: 
        //         animatorHandler.Walk(); 
        //         break;
        //     case StateAnimation.Run: animatorHandler.Run(); break;
        //     case StateAnimation.Crouching: animatorHandler.Crouching(); break;
        // }
    }
    
    private void Fire() {
        TryShoot();
        CmdFire();
    }
    
    //[Command] private void TryShoot() { hitHandler.TryShoot(damage); }
    [Command] private void TryShoot() { hitHandler.TryShoot(); }
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
     
    [TargetRpc] public void TargetGotDamage(int damage){
        healthHandler.TakeDamage(damage);
        Debug.Log("We got hit!");
    }
    
    [Server] public void Die() {
        Deaths++;
        isDead = true;
        Debug.Log("SERVER: Player died.");
        TargetDie();
        RpcPlayerDie();
    }
    
    [TargetRpc] private void TargetDie() {
        PlayerDie();
        Debug.Log("You died.");
    }
    
    [ClientRpc] void RpcPlayerDie() { PlayerDie(); }

    private void PlayerDie() {
        moveHandler.PlayerDie();
        // Vector3 direction = new Vector3(0.1f, 0.1f, 0.1f);
        // for (int i = 0; i < damages.Length; i++) {
        //     Rigidbody rigidBody = damages[i].GetComponent<Rigidbody>();
        //     rigidBody.isKinematic = false;
        //     rigidBody.AddForce(direction, ForceMode.Impulse);
        // }
        damageHandler.Die();
        DeathEvent?.Invoke();
        UpdateEvent = null;
        FixedUpdateEvent = null;
    }
    
    [TargetRpc] private void TargetGotKill() { Debug.Log("You got kill."); }
  
    [Command] private void CmdFire() { RpcOnFire(); }

    [ClientRpc] private void RpcOnFire() { weaponHandler.RpcOnFire(); }
    private void FixedUpdate() { FixedUpdateEvent?.Invoke();}
    public void Subscription() {
        inputHandler.JumpEvent += moveHandler.OnJump;
        weaponHandler.ShootEvent += Fire;
        healthHandler.HealthChangedEvent += OnHealthChanged;
        SubscriptionDamages();
    }

    private void SubscriptionDamages() {
        //for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent += OnDamage; }
        damageHandler.SubscriptionDamages();
        damageHandler.DamageEvent += OnDamage;
    } 
    
    public void UnSubscription() {
        inputHandler.JumpEvent -= moveHandler.OnJump;
        weaponHandler.ShootEvent -= Fire;
        healthHandler.HealthChangedEvent -= OnHealthChanged;
        UnSubscriptionDamages();
        UpdateEvent = null;
        FixedUpdateEvent = null;
        HealthChangedEvent = null;
        ArmorChangedEvent = null;
        DeathEvent = null;
    }
    
    public void UnSubscriptionDamages() {
        //for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent -= OnDamage; }
        damageHandler.UnSubscriptionDamages();
        damageHandler.DamageEvent -= OnDamage;
    } 

    private void OnDestroy() { UnSubscription(); }

}