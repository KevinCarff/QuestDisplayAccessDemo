using UnityEngine;
using Mirror;

public class TrackLocalPlayer : NetworkBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // If this is not the local player, do nothing
        if (!isLocalPlayer)
        {
            return;
        }

        // If no camera is explicitly assigned, find the main camera in the scene
        if (sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }

        if (sceneCamera != null)
        {
            Debug.Log("Local player: Scene camera found. Setting up tracking.");
            SetupCameraTracking();
        }
        else
        {
            Debug.LogWarning("Local player: No scene camera found in the scene.");
        }
    }

    private void SetupCameraTracking()
    {
        // Attach or configure the player to follow or track the scene camera
        Transform cameraTransform = sceneCamera.transform;

        // Optional: Disable player's own camera if one exists
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }

        // Continuously track the camera's position and rotation
        StartCoroutine(TrackCameraPosition(cameraTransform));
    }

    private System.Collections.IEnumerator TrackCameraPosition(Transform cameraTransform)
    {
        while (true)
        {
            yield return null; // Wait for the next frame

            if (cameraTransform != null)
            {
                // Synchronize the player's position and rotation with the scene camera
                transform.position = cameraTransform.position;
                transform.rotation = cameraTransform.rotation;
            }
        }
    }
}
