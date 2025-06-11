using System.Collections;
using Mirror;
using UnityEngine;

public class Entry : MonoBehaviour, Initializable, ISubscriptionable {
    [SerializeField] private HealthArmorService healthArmorService;
    private NetWorkEntity _netWorkEntity;
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
        _netWorkEntity = player.GetComponent<NetWorkEntity>();
      
        healthArmorService.SetMaxHealth(_netWorkEntity.GetMaxHealth());
        healthArmorService.SetMaxArmor(_netWorkEntity.GetMaxArmor());
        healthArmorService.Initialize();
        Subscription();
        _netWorkEntity.HealthHandlerInitialize();
    }

    
    
    
    
    public void Subscription() {
       _netWorkEntity.HealthChangedEvent += healthArmorService.SetHealth;
       _netWorkEntity.ArmorChangedEvent += healthArmorService.SetArmor;
       _netWorkEntity.DeathEvent += healthArmorService.ShowDeath;
    }
    
    public void UnSubscription() {  }
    private void OnDestroy() { UnSubscription(); }
    
}