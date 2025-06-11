using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetWorkEntityTest : NetworkBehaviour {
    //[SerializeField] private InputSystem inputSystem;
    [SerializeField] private ViewSystem viewSystem;
    [SerializeField] private MoveHandlerTest moveHandlerTest;
    [SerializeField] private WeaponSystem weaponSystem;
    
    private SceneData _sceneData;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;

    [SyncVar(hook = nameof(OnWeaponChanged))]
    public int activeWeaponSynced = 1;
    
    void Awake() { Initialize(); }

    private void Initialize() {
        _sceneData = GameObject.Find("SceneReference").GetComponent<SceneReference>().sceneData;
        //inputSystem.Initialize();
        moveHandlerTest.Initialize(transform);
        weaponSystem.Initialize();
        Subscription();
    }
    private void OnWeaponChanged(int oldValue, int newValue) { weaponSystem.OnWeaponChanged(oldValue, newValue); }
    private void ShowWeaponAmmo(int weaponAmmo) { if (isLocalPlayer) _sceneData.UIAmmo(weaponAmmo); }
    [Command] private void CmdChangeActiveWeapon(int newIndex) { activeWeaponSynced = newIndex; }
    [Command] public void CmdSendPlayerMessage() { if (_sceneData) 
        _sceneData.statusText = $"{playerName} says hello {Random.Range(10, 99)}"; }
    private void OnNameChanged(string _Old, string _New) {
        //playerNameText.text = playerName;
        viewSystem.NameChanged(playerName);
    }
    private void OnColorChanged(Color oldValue, Color newValue) {
        viewSystem.ColorChanged(oldValue, newValue);
    }

    public override void OnStartLocalPlayer() {
        _sceneData.netWorkEntityTest = this;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        string name = "Player" + Random.Range(100, 999);
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        CmdSetupPlayer(name, color);
    }

    [Command] private void CmdSetupPlayer(string _name, Color _col) {
        playerName = _name;
        playerColor = _col;
        _sceneData.statusText = $"{playerName} joined.";
    }

    void Update() {
        if (!isLocalPlayer) {
            viewSystem.UpdateInfo();
            return;
        }
        //inputSystem.OnUpdate();
        UpdateMove();
    }

    private void UpdateMove() {
        //Vector2 axis = inputSystem.GetAxis();
        //moveSystem.Move(axis);
    }
    
    private void OnFire() { weaponSystem.Fire(); }
    private void OnChangeWeapon() { weaponSystem.ChangeWeapon(); }
    [Command] void CmdShootRay() { RpcFireWeapon(); }
    [ClientRpc] void RpcFireWeapon() { weaponSystem.FireWeapon(); }
    private void Subscription() {
        //inputSystem.LeftButtonEvent += OnFire;
       // inputSystem.RightButtonEvent += OnChangeWeapon;
        weaponSystem.ChangeWeaponEvent += CmdChangeActiveWeapon;
        weaponSystem.ShootEvent += CmdShootRay;
        weaponSystem.ShowAmmoEvent += ShowWeaponAmmo;
    }
    private void UnSubscription() {
      //  inputSystem.LeftButtonEvent -= OnFire;
        //inputSystem.RightButtonEvent -= OnChangeWeapon;
        weaponSystem.ChangeWeaponEvent -= CmdChangeActiveWeapon;
        weaponSystem.ShootEvent -= CmdShootRay;
        weaponSystem.ShowAmmoEvent -= ShowWeaponAmmo;
    }
    private void OnDestroy() { UnSubscription(); }
}