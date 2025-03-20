using UnityEngine;

[CreateAssetMenu(fileName = "DataScene", menuName = "Scriptable Objects/Data Scene")]
public class DataScene : ScriptableObject {
    [SerializeField] private GameObject player;
    public int abc;

        //public PlayerController GetPlayer() => player;
    public void SetPlayer(GameObject player) => this.player = player;  
}
