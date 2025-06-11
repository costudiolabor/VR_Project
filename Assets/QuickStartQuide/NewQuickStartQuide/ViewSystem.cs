using UnityEngine;

[System.Serializable]
public class ViewSystem {
    [SerializeField] private Renderer playerRenderer; 
    [SerializeField] private TextMesh playerNameText;
    [SerializeField] private Transform floatingInfo;
    
    private Material _playerMaterialClone;

    public void UpdateInfo() {
        floatingInfo.LookAt(Camera.main.transform);
    }

    public void NameChanged(string playerName) {
        playerNameText.text = playerName;
    }
    
    public void ColorChanged(Color _Old, Color _New) {
        playerNameText.color = _New;
        _playerMaterialClone = new Material(playerRenderer.material);
        _playerMaterialClone.color = _New;
        playerRenderer.material = _playerMaterialClone;
    }
    
}