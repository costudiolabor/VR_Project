using System;
using UnityEngine;

public class HeadCollider : MonoBehaviour {
    //[SerializeField] private NetworkIdentity networkIdentity;
    //private uint _netId;
    //public uint GetIdentity() => networkIdentity.netId;
    //public void SetIdentity(uint netId) => _netId = netId;
    public event Action HeadShootEvent;
    public void TakeDamage(byte damage) { HeadShootEvent?.Invoke(); }
}
