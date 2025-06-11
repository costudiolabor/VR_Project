using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    [SerializeField] private string sceneToLoad;
    public void LoadScene() { 
        SceneManager.LoadScene(sceneToLoad);
    }
}