using UnityEngine;
using Mirror;

public class LeftHandTagTracker : NetworkBehaviour
{
    [SerializeField]
    private GameObject leftTag;
    public string name = "oculus_controller_l_MeshX";

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // If this is not the local player, do nothing
        if (!isLocalPlayer)
        {
            return;
        }

        // If no Left is explicitly assigned, find the main Left in the scene
        if (leftTag == null)
        {
            leftTag = GameObject.Find(name);
        }

        if (leftTag != null)
        {
            Debug.Log("Local player: Scene Left found. Setting up tracking.");
            SetupLeftTracking();
        }
        else
        {
            Debug.LogWarning("Local player: No scene Left found in the scene.");
        }
    }

    private void SetupLeftTracking()
    {
        // Attach or configure the player to follow or track the scene Left
        Transform LeftTransform = leftTag.transform;

        //// Optional: Disable player's own Left if one exists
        //LeftHandTag playerLeft = GetComponentInChildren<LeftHandTag>();
        //if (playerLeft != null)
        //{
        //    playerLeft.enabled = false;
        //}

        // Continuously track the Left's position and rotation
        StartCoroutine(TrackLeftPosition(LeftTransform));
    }

    private System.Collections.IEnumerator TrackLeftPosition(Transform LeftTransform)
    {
        while (true)
        {
            yield return null; // Wait for the next frame

            if (LeftTransform != null)
            {
                // Synchronize the player's position and rotation with the scene Left
                transform.position = LeftTransform.position;
                transform.rotation = LeftTransform.rotation;
            }
        }
    }
}
