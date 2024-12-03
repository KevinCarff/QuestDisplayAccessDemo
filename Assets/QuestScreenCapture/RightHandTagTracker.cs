using UnityEngine;
using Mirror;

public class RightHandTagTracker : NetworkBehaviour
{
    [SerializeField]
    private GameObject rightTag;
    public string name = "oculus_controller_r_MeshX";

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // If this is not the local player, do nothing
        if (!isLocalPlayer)
        {
            return;
        }

        // If no right is explicitly assigned, find the main right in the scene
        if (rightTag == null)
        {
            rightTag = GameObject.Find(name);
        }

        if (rightTag != null)
        {
            Debug.Log("Local player: Scene right found. Setting up tracking.");
            SetuprightTracking();
        }
        else
        {
            Debug.LogWarning("Local player: No scene right found in the scene.");
        }
    }

    private void SetuprightTracking()
    {
        // Attach or configure the player to follow or track the scene right
        Transform rightTransform = rightTag.transform;

        // Optional: Disable player's own right if one exists
        //RightHandTag playerRight = GetComponentInChildren<RightHandTag>();
        //if (playerRight != null)
        //{
        //    playerRight.enabled = false;
        //}

        // Continuously track the right's position and rotation
        StartCoroutine(TrackrightPosition(rightTransform));
    }

    private System.Collections.IEnumerator TrackrightPosition(Transform rightTransform)
    {
        while (true)
        {
            yield return null; // Wait for the next frame

            if (rightTransform != null)
            {
                // Synchronize the player's position and rotation with the scene right
                transform.position = rightTransform.position;
                transform.rotation = rightTransform.rotation;
            }
        }
    }
}
