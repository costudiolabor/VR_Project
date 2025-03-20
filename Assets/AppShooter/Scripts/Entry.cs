using System.Collections;
using Mirror;
using UnityEngine;

public class Entry : MonoBehaviour, Initializable, ISubscriptionable {
    [SerializeField] private HealthArmorService healthArmorService;
    [SerializeField] private PlayerController _playerController;
    private void Start() { Initialize(); }

    public void Initialize() {
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI() {
        NetworkManager networkManager = NetworkManager.singleton;
        while (networkManager == null) {
            yield return null;
            networkManager = NetworkManager.singleton;
        }
        GameObject player = networkManager.GetPlayer();
        while (player == null) {
             yield return null;
             player = networkManager.GetPlayer();
        }
        _playerController = player.GetComponent<PlayerController>();
        
        healthArmorService.SetMaxHealth(_playerController.GetMaxHealth());
        healthArmorService.SetMaxArmor(_playerController.GetMaxArmor());
        healthArmorService.Initialize();
        Subscription();
        _playerController.HealthHandlerInitialize();
    }

    public void Subscription() {
       _playerController.HealthChangedEvent += healthArmorService.SetHealth;
       _playerController.ArmorChangedEvent += healthArmorService.SetArmor;
       
       _playerController.DeathEvent += healthArmorService.ShowDeath;
    }

    public void UnSubscription() {
        
    }
    
    private void OnDestroy() { UnSubscription(); }
    
}