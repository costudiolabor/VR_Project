using UnityEngine;
using System;

[Serializable]
public class DamageHandler {
    [SerializeField] private Damagable[] damages;
    
    public event Action<int> DamageEvent;
    private void OnDamage(int damage) { DamageEvent?.Invoke(damage); }
    
    public void Die() {
        Vector3 direction = new Vector3(0.1f, 0.1f, 0.1f);
        for (int i = 0; i < damages.Length; i++) {
            //Rigidbody rigidBody = damages[i].GetComponent<Rigidbody>();
            Rigidbody rigidBody = damages[i].GetRigidbody();
            rigidBody.isKinematic = false;
            rigidBody.AddForce(direction, ForceMode.Impulse);
        }
    }
    
    public void SubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent += OnDamage; }
    }
    public void UnSubscriptionDamages() {
        for (int i = 0; i  < damages.Length; i ++) { damages[i].DamageEvent -= OnDamage; }
    } 
}
