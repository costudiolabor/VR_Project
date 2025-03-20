using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class HitHandler {
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private float rayLength = 100.0f;
    [SerializeField] private int damage = 10;
    
    private Camera _camera;
    private Bullet[] _bullets;
    private int _currentBullet = 0;
    private const int MaxBullets = 5;
    private Vector2 _center;
    private Transform _hitTransform;
    
    public event Action<Vector3> HitEvent;
    
    public void Initialize(Camera camera) {
        _camera = camera;
        _center.x = Screen.width / 2.0f;
        _center.y = Screen.height / 2.0f;
    }

    public void CreateImpacts() {
        _bullets = new Bullet[MaxBullets];
        for (int i = 0; i < MaxBullets; i++) {
            _bullets[i] = Object.Instantiate(bulletPrefab);
            _bullets[i].damage = damage;
            _bullets[i].Hide();
        }
        
    }

    public void OnShoot() {
       // Debug.Log("OnShoot");
        var raycastHit = RayFromCamera(_center, out var isHitRayCast);
        if (isHitRayCast) {
            _hitTransform = raycastHit.transform;
            if (_hitTransform.TryGetComponent(out IDamageable damageable)) {
                HitEvent?.Invoke(raycastHit.point);
            }
        }
    }
    
    public RaycastHit RayFromCamera(Vector3 position, out bool isHitRayCast) {
        var ray = _camera.ScreenPointToRay(position);
        isHitRayCast = Physics.Raycast(ray, out var hit, rayLength);
        return hit;
    }
    
    public void RpcSetPositionImpact(Vector3 position) {
        _bullets[_currentBullet].transform.position = position;
        _bullets[_currentBullet].Show();
        if (++_currentBullet >= MaxBullets) _currentBullet = 0;
    }
}
