using Mirror;
using TMPro;
using UnityEngine;

public class EntryBoot : MonoBehaviour {
    [SerializeField] private NetworkManager networkManager; 
    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        Debug.Log("start entry boot");
        inputField.onEndEdit.AddListener(SetPlayer);
    }

    public void SetPlayer(string namePlayer) { networkManager.SetPlayer(namePlayer); }
}
