using UnityEngine;

public class LockCursor : MonoBehaviour
{
    [SerializeField] private KeyCode _escKey = KeyCode.Escape;

    private void OnApplicationFocus(bool hasFocus) {
        if (hasFocus) {
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("Application is focussed");
        }
        else {
            //Debug.Log("Application lost focus");
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update() {
        if (Input.GetKeyDown(_escKey)) {
            Debug.Log(_escKey);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
