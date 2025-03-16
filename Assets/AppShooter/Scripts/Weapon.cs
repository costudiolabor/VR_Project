using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour {
    [SerializeField] private string drawAnim = "draw";
    [SerializeField] private string fireLeftAnim = "fire";
    [SerializeField] private string reloadAnim = "reload";
    [SerializeField] private string fireKey = "Fire1";
    [SerializeField] private string reloadKey = "r";
    [SerializeField] private string changeKey = "1";
    [SerializeField] private Animation animationGO;
 
    private bool _drawWeapon = false;
    private bool _reloading = false;
    
    private void Start () { StartCoroutine(DrawWeapon()); }
 
    private void Update () {
        if (Input.GetButtonDown (fireKey) && _reloading == false && _drawWeapon == false){ Fire(); }
        if (Input.GetKeyDown (reloadKey) && _reloading == false && _drawWeapon == false){ StartCoroutine(Reloading()); }
        if (Input.GetKeyDown (changeKey) && _reloading == false){ StartCoroutine(DrawWeapon()); }      
    }
 
    private void Fire() { animationGO.CrossFadeQueued(fireLeftAnim, 0.08f, QueueMode.PlayNow); }
 
    private IEnumerator  DrawWeapon() {
        if(_drawWeapon) yield break;
        animationGO.Play(drawAnim);
        _drawWeapon = true;
        yield return new WaitForSeconds(0.6f);
        _drawWeapon = false;
       
    }
 
    private IEnumerator Reloading(){
        if(_reloading) yield break;
        animationGO.Play(reloadAnim);
        _reloading = true;
        yield return new WaitForSeconds(2.0f);
        _reloading = false;
    }
}
