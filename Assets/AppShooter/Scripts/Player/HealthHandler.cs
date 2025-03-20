using System;
using UnityEngine;

[Serializable]
public class HealthHandler: Initializable {
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxArmor;
    
    private int _currentHealth;
    private int _currentArmor;
    
    public event Action<int> HealthChangedEvent, ArmorChangedEvent;
    public event Action DeathEvent;

    public void Initialize() {
        _currentHealth = maxHealth;
        _currentArmor = maxArmor;
        HealthChangedEvent?.Invoke(_currentHealth);
        ArmorChangedEvent?.Invoke(_currentArmor);
    }
    
    public int GetMaxHealth() => maxHealth;
    public int GetMaxArmor() => maxArmor;

    public void TakeDamage(int damage) {
        _currentArmor -= damage;
        _currentHealth += _currentArmor;
        _currentArmor = Mathf.Clamp(_currentArmor, 0, maxArmor);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        HealthChangedEvent?.Invoke(_currentHealth);
        ArmorChangedEvent?.Invoke(_currentArmor);
        if (_currentHealth == 0) DeathEvent?.Invoke();
    }
    
}
