using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneData : NetworkBehaviour {
    public Text canvasStatusText;
    public Text canvasAmmoText;
    //public PlayerScript playerScript;
    [FormerlySerializedAs("netWorkEntity")] public NetWorkEntityTest netWorkEntityTest;
    public SceneReference sceneReference;

    [SyncVar(hook = nameof(OnStatusTextChanged))]
    public string statusText;
    
    public void UIAmmo(int _value) {
        canvasAmmoText.text = "Ammo: " + _value;
    }
    
    private void OnStatusTextChanged(string _Old, string _New) {
        //called from sync var hook, to update info on screen for all players
        canvasStatusText.text = statusText;
    }

    public void ButtonSendMessage() {
        if (netWorkEntityTest != null) netWorkEntityTest.CmdSendPlayerMessage();
    }
    
    public void ButtonChangeScene()
    {
        if (isServer)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MyScene")
                NetworkManager.singleton.ServerChangeScene("MyOtherScene");
            else
                NetworkManager.singleton.ServerChangeScene("MyScene");
        }
        else
            Debug.Log("You are not Host.");
    }
}
