using UnityEngine;
using System;
using TMPro;

[Serializable]
public class InfoHandler {
    [SerializeField] private TMP_Text textName;
    
    public void Initialize() { }
    
    public void ChangeName(string newName) {
        Debug.Log("RpcChangeName " + newName);
        textName.text = newName;
    }
    
}
