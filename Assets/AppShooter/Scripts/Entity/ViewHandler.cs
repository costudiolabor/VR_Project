using UnityEngine;

[System.Serializable]
public class ViewHandler {
    [SerializeField] private View visualLocal;
    [SerializeField] private View visualRemote;
    
    public void Initialize() { }

    public void SetLocalPlayer() {
        visualLocal.Show();
        visualRemote.Hide();
    }

    public void SetRemotePlayer() {
        visualLocal.Hide();
        visualRemote.Show();
    }
}
