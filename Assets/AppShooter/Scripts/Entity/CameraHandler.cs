using UnityEngine;
using System;

[Serializable]
public class CameraHandler {
    [SerializeField] private Camera cameraPlayer;
    [SerializeField] private AudioListener audioListener;

    public void Initialize() { }

    public Camera GetCamera() => cameraPlayer;
    
    public void EnableComponents(bool state) {
        cameraPlayer.enabled = state;
        audioListener.enabled = state;
    }

}
