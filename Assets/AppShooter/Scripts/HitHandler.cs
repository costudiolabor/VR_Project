using UnityEngine;

[System.Serializable]
public class HitHandler {
    [SerializeField] private Transform impactPrefab;
    [SerializeField] private float rayLength = 100.0f;
    private Camera _camera;
    
    private Transform[] _impacts;
    private int _currentImpact = 0;
    private const int MaxImpacts = 5;
    private Vector2 _center;
    
    public void Initialize(Camera camera) {
        _camera = camera;
        _center.x = Screen.width / 2.0f;
        _center.y = Screen.height / 2.0f;
        _impacts = new Transform[MaxImpacts];
        for (int i = 0; i < MaxImpacts; i++)
            _impacts[i] = Object.Instantiate(impactPrefab);
    }

    public void OnShoot() {
        var raycastHit = RayFromCamera(_center, out var isHitRayCast);
        if (isHitRayCast) {
            if (raycastHit.transform.CompareTag($"Enemy") || raycastHit.transform.CompareTag($"Player")) {
                _impacts[_currentImpact].transform.position = raycastHit.point;
                if (++_currentImpact >= MaxImpacts) _currentImpact = 0;
            }
        }
    }
    
    
    public RaycastHit RayFromCamera(Vector3 position, out bool isHitRayCast) {
        var ray = _camera.ScreenPointToRay(position);
        isHitRayCast = Physics.Raycast(ray, out var hit, rayLength);
        return hit;
    }
}
