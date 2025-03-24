using System;
using UnityEngine;

[Serializable]
public class HitHandler {
    [SerializeField] private float rayLength = 100.0f;
    
    private Camera _camera;
    private Bullet[] _bullets;
    private int _currentBullet = 0;
    private const int MaxBullets = 5;
    private Vector2 _center;
    private Transform _hitTransform;
   
    
    public void Initialize(Camera camera) { _camera = camera; }

    // public void CreateImpacts() {
    //     _bullets = new Bullet[MaxBullets];
    //     for (int i = 0; i < MaxBullets; i++) {
    //         _bullets[i] = Object.Instantiate(bulletPrefab);
    //         _bullets[i].damage = damage;
    //         _bullets[i].Hide();
    //     }
    //     
    // }
    
    public void TryShoot(byte damage) {
        
        
        
        Debug.Log("OnShoot");
        _center.x = _camera.pixelWidth / 2.0f;
        _center.y = _camera.pixelHeight / 2.0f;
        
        var raycastHit = RayFromCamera(_center, out var isHitRayCast);
        // if (isHitRayCast) {
        //     _hitTransform = raycastHit.transform;
        //     if (_hitTransform.TryGetComponent(out PlayerController playerController)) {
        //         //playerController.TakeDamage(damage);
        //         Debug.Log("PlayerController ");
        //     }
        //     
        //     if (_hitTransform.CompareTag("Player")) {
        //         Debug.Log("CompareTag(Player) ");
        //     }
        //     
        //     if (_hitTransform.CompareTag("Head")) {
        //         Debug.Log("CompareTag(Head) ");
        //     }
        // }
    }

    private RaycastHit RayFromCamera(Vector3 position, out bool isHitRayCast) {
        
        isHitRayCast = false;
        // получаем маску, которая затрагивает только слой Player
        int layerMaskOnlyPlayer = 1 << 6;
        // получаем маску, которая затрагивает все слои, кроме слоя Player
        //int layerMaskWithoutPlayer = ~layerMaskOnlyPlayer;
        int layerMaskWithoutPlayer = layerMaskOnlyPlayer;
        
        var ray = _camera.ScreenPointToRay(position);
        //isHitRayCast = Physics.Raycast(ray, out var hit, rayLength, layerMaskWithoutPlayer);
        var results = Physics.RaycastAll(ray, rayLength);
        
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red, 0.5f);
        int length = results.Length;
        for (int i = 0; i < length; i++) {
            if (results[i].transform.CompareTag("Head")) {
                Debug.Log("CompareTag(Head) ");
            }
            
            if (results[i].transform.CompareTag("Body")) {
                Debug.Log("CompareTag(Body) ");
            }
            
        }
        
        RaycastHit raycastHit = new RaycastHit();
        return raycastHit;
    }
    
}
