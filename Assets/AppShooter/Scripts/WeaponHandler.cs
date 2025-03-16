using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class WeaponHandler {
    [SerializeField] private string drawAnim = "draw";
    [SerializeField] private string fireLeftAnim = "fire";
    [SerializeField] private string reloadAnim = "reload";
    [SerializeField] private string fireKey = "Fire1";
    [SerializeField] private string reloadKey = "r";
    [SerializeField] private string changeKey = "1";
    [SerializeField] private float rateFire = 0.5f;
    [SerializeField] private Animation anim;
    [SerializeField] private ParticleSystem muzzleFlash;
 
    private bool _drawWeapon = false;
    private bool _reloading = false;
    private bool _shooting = false;
    private MonoBehaviour _mono;

    public event Action ShootEvent;

    public void Initialize(MonoBehaviour mono) {
        _mono = mono;
        _mono.StartCoroutine(DrawWeapon());
    }
 
    public void OnUpdate () {
        if (Input.GetButton (fireKey) && _reloading == false && _drawWeapon == false){  Fire(); }
        if (Input.GetKeyDown (reloadKey) && _reloading == false && _drawWeapon == false){ _mono.StartCoroutine(Reloading()); }
        if (Input.GetKeyDown (changeKey) && _reloading == false){ _mono.StartCoroutine(DrawWeapon()); }      
    }

    private void Fire() {
        if (_shooting) return;
        anim.CrossFadeQueued(fireLeftAnim, 0.08f, QueueMode.PlayNow);
        muzzleFlash.Play();
        _mono.StartCoroutine(TimerShoot());
        ShootEvent?.Invoke();
    }

    private IEnumerator TimerShoot() {
        _shooting = true;
        yield return new WaitForSeconds(rateFire);
        _shooting = false;
    }
 
    private IEnumerator  DrawWeapon() {
        if(_drawWeapon) yield break;
        anim.Play(drawAnim);
        _drawWeapon = true;
        yield return new WaitForSeconds(0.6f);
        _drawWeapon = false;
    }
 
    private IEnumerator Reloading(){
        if(_reloading) yield break;
        anim.Play(reloadAnim);
        _reloading = true;
        yield return new WaitForSeconds(2.0f);
        _reloading = false;
    }
}
