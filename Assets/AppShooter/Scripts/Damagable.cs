using UnityEngine;

public class Damagable : MonoBehaviour, IDamageable_old {
    private uint _networkId = 1000;
    public uint GetIdentity() => _networkId;
}
