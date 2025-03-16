using UnityEngine;

public class LockCursor : MonoBehaviour {
    void OnApplicationFocus(bool hasFocus) {
        if (hasFocus) {
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("Application is focussed");
        }
        else {
            //Debug.Log("Application lost focus");
        }
    }
}
