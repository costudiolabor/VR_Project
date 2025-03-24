using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TestRayCast : NetworkBehaviour {

    //[SerializeField] private LayerMask layerMask;
    void Start() {
        
    }

    void Update() {
        if(!isOwned) return;
        if (Input.GetKeyDown ("f")) {
            Debug.Log("TestRayCast");
            CmdTryShoot(transform.position, transform.forward);
        }
    }


    [Command]
    private void CmdTryShoot( Vector3 origin, Vector3 direction)
    {
        
        //int layerMask = 1 << 7;
        //layerMask = ~layerMask;
        
        direction *= 10;
        Ray ray = new Ray(origin, direction);
        Debug.DrawRay(origin, direction , Color.red, 0.5f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {

            Debug.Log("SERVER: Player shot: " + hit.collider.name);
            if (hit.collider.CompareTag("Player"))
            {
                //RpcPlayerFiredEntity(GetComponent<NetworkIdentity>().netId, hit.collider.GetComponent<NetworkIdentity>().netId, hit.point, hit.normal);
                //hit.collider.GetComponent<PlayerFPS>().Damage(weaponDamage, GetComponent<NetworkIdentity>().netId);
                hit.collider.GetComponent<TestRayCast>().Damage();
              
                Debug.Log(hit.collider.name);
            }
            else
            {
                //RpcPlayerFired(GetComponent<NetworkIdentity>().netId, hit.point, hit.normal);

            }

        }
    }
    
    private void Damage() {
        Debug.Log("Damage");
        CmdDamage();
    }
  
    [Server]
    private void CmdDamage() {
        Debug.Log("CmdDamage");
        RpcDamage();
    }

    [ClientRpc]
    private void RpcDamage() {
        Debug.Log("RpcDamage");
        gameObject.SetActive(false);
    }
    
    
    
}
