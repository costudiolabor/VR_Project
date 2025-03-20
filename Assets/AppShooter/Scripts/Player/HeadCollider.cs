using System;
using UnityEngine;

public class HeadCollider : MonoBehaviour {
    
    public event Action  HeadShootEvent;
    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Bullet bullet)) {
            HeadShootEvent?.Invoke();
            bullet.Hide();
        }
    }
}
