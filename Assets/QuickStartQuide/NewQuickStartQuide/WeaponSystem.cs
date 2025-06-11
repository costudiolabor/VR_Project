using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class WeaponSystem {
    [SerializeField] private  Weapon[] weaponArray;
    
    //private SceneData _sceneData;
    private Weapon _activeWeapon;
    private int _selectedWeaponLocal = 1;
    private float _weaponCooldownTime; 

    public event Action ShootEvent;
    public event Action<int> ChangeWeaponEvent, ShowAmmoEvent;
    
    //public void Initialize(SceneData sceneData) {
    public void Initialize() {
        //_sceneData = sceneData;
        foreach (var item in weaponArray) if (item != null) item.Hide();
        if (_selectedWeaponLocal < weaponArray.Length && weaponArray[_selectedWeaponLocal] != null) {
            _activeWeapon = weaponArray[_selectedWeaponLocal];
            //_sceneData.UIAmmo(_activeWeapon.weaponAmmo);
            ShowAmmoEvent?.Invoke(_activeWeapon.weaponAmmo);
        }
    }
    
    public void OnWeaponChanged(int oldValue, int newValue) {
        // disable old weapon
        // in range and not null
        if (0 < oldValue && oldValue < weaponArray.Length && weaponArray[oldValue] != null) weaponArray[oldValue].Hide();
        // enable new weapon
        // in range and not null
        if (0 < newValue && newValue < weaponArray.Length && weaponArray[newValue] != null) {
            weaponArray[newValue].Show();
           _activeWeapon = weaponArray[newValue];
           //_activeWeapon = weaponArray[activeWeaponSynced];
          //  if (isLocalPlayer) _sceneData.UIAmmo(_activeWeapon.weaponAmmo);
          ShowAmmoEvent?.Invoke(_activeWeapon.weaponAmmo);
        }
    }
    
    public void ChangeWeapon() {
        _selectedWeaponLocal += 1;
        if (_selectedWeaponLocal > weaponArray.Length) _selectedWeaponLocal = 1;
        ChangeWeaponEvent?.Invoke(_selectedWeaponLocal);
        //CmdChangeActiveWeapon(selectedWeaponLocal);
    }

    
    public void FireWeapon() {
        //bulletAudio.Play(); muzzleflash  etc
        Bullet bullet = Object.Instantiate(_activeWeapon.weaponBullet, 
            _activeWeapon.weaponFirePosition.position, _activeWeapon.weaponFirePosition.rotation);
        bullet.rigidBody.velocity = bullet.transform.forward * _activeWeapon.weaponSpeed;
        Object.Destroy(bullet, _activeWeapon.weaponLife);
    }
    
    public void Fire() {
        if (_activeWeapon && Time.time > _weaponCooldownTime && _activeWeapon.weaponAmmo > 0) {
            _weaponCooldownTime = Time.time + _activeWeapon.weaponCooldown;
            _activeWeapon.weaponAmmo -= 1;
            //_sceneData.UIAmmo(_activeWeapon.weaponAmmo);
            ShowAmmoEvent?.Invoke(_activeWeapon.weaponAmmo);
            //CmdShootRay();
            ShootEvent?.Invoke();
        }
    }
}