using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine;
using System;
using TMPro;


public class PlayerLocal : MonoBehaviour, ISubscriptionable {
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private AnimatorHandler animatorHandler;
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private GameObject visualLocal;
    [SerializeField] private GameObject visual;
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
    private StateAnimation _lastState;
    
    //[SyncVar] 
    public int Health = 100;
    //[SyncVar] 
    public int HealthMax = 100;
    //[SyncVar] 
    public int Kills;
    //[SyncVar] 
    public int Deaths;
    //[SyncVar] 
    public bool isDead;
    
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action UpdateEvent, FixedUpdateEvent, DeathEvent;
    private void Start() { Initialize(); }

    private void Initialize() {
        
        var isOwned = true;
        if (isOwned)
        {
            UpdateEvent += weaponHandler.OnUpdate;
        }
        
        inputHandler.Initialize();
        hitHandler.Initialize(cameraPlayer);
        // hitHandler.CreateImpacts();
        weaponHandler.Initialize(this);
        EnableComponents(isOwned);
        Subscription();
        SetLocalPlayer();
        HealthHandlerInitialize();
        
    }

    private void SetLocalPlayer() {
        //if (isLocalPlayer) {
           // NetworkManager networkManager = NetworkManager.singleton;
           // networkManager.SetPlayer(gameObject);
            // visualLocal.SetActive(true);
            // visual.SetActive(false);
            // firstPersonController.Initialize();
            // UpdateEvent += firstPersonController.OnUpdate;
            // FixedUpdateEvent += firstPersonController.OnFixedUpdate;
            // firstPersonController.JumpEvent += CmdJump;
            // firstPersonController.MoveEvent += CmdMove;
            // SetName();
        //}
        //else {
            // visualLocal.SetActive(false);
            // visual.SetActive(true);
            //
            // Destroy(characterController);
            // Destroy(firstPersonController);
            // characterController = null;
            // firstPersonController = null;
       // }
       
        visualLocal.SetActive(false);
        visual.SetActive(true);
        
        firstPersonController.Initialize();
        UpdateEvent += firstPersonController.OnUpdate;
        FixedUpdateEvent += firstPersonController.OnFixedUpdate;
        firstPersonController.JumpEvent += CmdJump;
        firstPersonController.MoveEvent += CmdMove;
        SetName();
        
    }
    
    public void SetName() {
        //NetworkManager networkManager = NetworkManager.singleton;
       // string namePlayer = networkManager.GetNamePlayer();
       string namePlayer = "1235";
        Debug.Log("SetName " + namePlayer);
        CmdChangeName(namePlayer);
    }

    private void EnableComponents(bool state) {
        characterController.enabled = state;
        firstPersonController.enabled = state;
        cameraPlayer.enabled = state;
        audioListener.enabled = state;
    }
 
    //[Command]
    private void CmdChangeName(string newName) {
        Debug.Log("CmdChangeName " + newName);
        RpcChangeName(newName);
    }
    
    //[ClientRpc]
    private void RpcChangeName(string newName) {
        Debug.Log("RpcChangeName " + newName);
        textName.text = newName;
    }

    private void Update()
    {
        UpdateEvent?.Invoke();
        inputHandler.OnUpdate();
        Vector2 axis = inputHandler.GetAxis();
        
    }

    //[Command]
    private void CmdJump() { RpcJump(); } 
    
    //[ClientRpc]
    private void RpcJump() { animatorHandler.Jump(); }
    
   // [Command]
   private void CmdMove(Vector2 direction, bool isWalk, bool isCrouching) {
       RpcMove(direction, isWalk, isCrouching);
   }

    //[ClientRpc]
    private void RpcMove(Vector2 direction, bool isWalk, bool isCrouching) {
        
        StateAnimation currentState = StateAnimation.Idle;
        
        if (isWalk) { currentState = StateAnimation.Walk; }
        else { currentState = StateAnimation.Run; }
        
        if (isCrouching) { currentState = StateAnimation.Crouching; }
        animatorHandler.Direction(direction.x, direction.y);
        
        if (_lastState == currentState) return;
        _lastState = currentState;
        
        switch (currentState) {
            case StateAnimation.Idle: animatorHandler.Idle(); break;
            case StateAnimation.Walk: 
                animatorHandler.Walk(); 
                break;
            case StateAnimation.Run: animatorHandler.Run(); break;
            case StateAnimation.Crouching: animatorHandler.Crouching(); break;
        }
        
        // StateAnimation currentState = StateAnimation.Idle;
        //
        // if (isWalk) {
        //     currentState = StateAnimation.Walk;
        // }
        // else {
        //     currentState = StateAnimation.Run;
        // }
        //
        // if (isCrouching) {
        //     currentState = StateAnimation.Crouching;
        // }
        //
        // //Debug.Log(direction);
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
        //     //case StateAnimation.WalkBack: animatorHandler.WalkBack(); break;
        //     //case StateAnimation.RunBack: animatorHandler.RunBack(); break;
        // }
    }
    // private void RpcMove(Vector2 direction, bool isWalk) {
    //     StateAnimation currentState = StateAnimation.Idle;
    //     if (isWalk) {
    //         if (direction.y > 0) { currentState = StateAnimation.Walk; }
    //         if (direction.y < 0) { currentState = StateAnimation.WalkBack; }
    //     }
    //     else {
    //         if (direction.y > 0) { currentState = StateAnimation.Run; }
    //         if (direction.y < 0) { currentState = StateAnimation.RunBack; }
    //     }
    //
    //     if (currentState == StateAnimation.Idle) {
    //         if (direction.x != 0) { currentState = StateAnimation.Walk; }
    //     }
    //     
    //     if (_lastState == currentState) return;
    //     _lastState = currentState;
    //     
    //     switch (currentState) {
    //         case StateAnimation.Idle: animatorHandler.Idle(); break;
    //         case StateAnimation.Walk: animatorHandler.Walk(); break;
    //         case StateAnimation.Run: animatorHandler.Run(); break;
    //         case StateAnimation.WalkBack: animatorHandler.WalkBack(); break;
    //         case StateAnimation.RunBack: animatorHandler.RunBack(); break;
    //     }
    // }
    
    private void Fire() {
        TryShoot();
        CmdFire();
    }
    
    //[Command]
    private void TryShoot() { hitHandler.TryShoot(); }
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
    
    //[TargetRpc]
    public void TargetGotDamage(int damage){
        healthHandler.TakeDamage(damage);
        Debug.Log("We got hit!");
    }
    
    //[Server]
    public void Die() {
        Deaths++;
        isDead = true;
        Debug.Log("SERVER: Player died.");
        TargetDie();
        RpcPlayerDie();
    }
    
   // [TargetRpc]
    void TargetDie() {
        PlayerDie();
        Debug.Log("You died.");
    }
    
  //  [ClientRpc]
    void RpcPlayerDie() { PlayerDie(); }

    private void PlayerDie() {
        if (characterController) characterController.enabled = false;
        if (firstPersonController) firstPersonController.enabled = false;
        Vector3 direction = new Vector3(0.1f, 0.1f, 0.1f);
        for (int i = 0; i < damages.Length; i++) {
            Rigidbody rigidBody = damages[i].GetComponent<Rigidbody>();
            rigidBody.isKinematic = false;
            rigidBody.AddForce(direction, ForceMode.Impulse);
        }
        DeathEvent?.Invoke();
        UpdateEvent = null;
        FixedUpdateEvent = null;
    }
    
    //[TargetRpc]
    public void TargetGotKill() { Debug.Log("You got kill."); }
  
   // [Command]
    private void CmdFire() { RpcOnFire(); }

    //[ClientRpc]
    private void RpcOnFire() { weaponHandler.RpcOnFire(); }

    //private void LateUpdate() { ChangeName(namePlayer); }
    private void FixedUpdate() { FixedUpdateEvent?.Invoke();}

    public void Subscription() {
        weaponHandler.ShootEvent += Fire;
        healthHandler.HealthChangedEvent += OnHealthChanged;
       // healthHandler.ArmorChangedEvent += OnArmorChanged;
        SubscriptionDamages();
    }

    public void SubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent += OnDamage; }
    } 
    
    public void UnSubscription() {
        weaponHandler.ShootEvent -= Fire;
        healthHandler.HealthChangedEvent -= OnHealthChanged;
        //healthHandler.ArmorChangedEvent -= OnArmorChanged;
        UnSubscriptionDamages();
        UpdateEvent = null;
        FixedUpdateEvent = null;
        HealthChangedEvent = null;
        ArmorChangedEvent = null;
        DeathEvent = null;
    }
    
    public void UnSubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent -= OnDamage; }
    } 

    private void OnDestroy() { UnSubscription(); }

}