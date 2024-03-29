using UnityEngine;

public class EnableDepthTextureMode : MonoBehaviour {
    private void OnDrawGizmos() {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        enabled = false;
    }
}
