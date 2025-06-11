using System;
using UnityEngine;

public class Damagable : MonoBehaviour, IDamageable_Custom {
    [SerializeField] private int headDamage = 0;
    [SerializeField] private Rigidbody rigidBody;
    public event Action<int> DamageEvent;

    public void Damage(int damage) {
        damage += headDamage;
        DamageEvent?.Invoke(damage);
        //Debug.Log("Damagable: " + damage);
    }
    
    public Rigidbody GetRigidbody() => rigidBody;
}
