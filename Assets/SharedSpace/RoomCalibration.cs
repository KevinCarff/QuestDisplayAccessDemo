using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCalibration : MonoBehaviour
{
    public GameObject markerPrefab; // Assign a marker prefab in the inspector
    private GameObject[] markers = new GameObject[4]; // Store marker GameObjects
    private Vector3[] cornerPoints = new Vector3[4]; // Store corner points
    private int clickCount = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button for clicks
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlaceMarker(hit.point);
            }
        }

        // Check for an erase button press (e.g., "E" key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            EraseLastMarker();
        }

        // Check for confirmation button press (e.g., "C" key)
        if (Input.GetKeyDown(KeyCode.C) && clickCount == 4)
        {
            ConfirmSelection();
        }
    }

    private void PlaceMarker(Vector3 position)
    {
        if (clickCount < 4)
        {
            // Place a marker at the clicked position
            GameObject marker = Instantiate(markerPrefab, position, Quaternion.identity);
            markers[clickCount] = marker; // Store the marker object
            cornerPoints[clickCount] = position; // Store the position
            clickCount++;

            Debug.Log($"Marker {clickCount} placed at: {position}");
        }
        else
        {
            Debug.Log("All 4 markers are already placed. Press 'E' to erase the last marker or 'C' to confirm.");
        }
    }

    private void EraseLastMarker()
    {
        if (clickCount > 0)
        {
            clickCount--;
            Destroy(markers[clickCount]); // Remove the last placed marker
            markers[clickCount] = null; // Clear the marker reference
            cornerPoints[clickCount] = Vector3.zero; // Clear the corner point
            Debug.Log($"Marker {clickCount + 1} erased.");
        }
        else
        {
            Debug.Log("No markers to erase.");
        }
    }

    private void ConfirmSelection()
    {
        Debug.Log("Selection confirmed. Processing room calibration...");

        // Define the forward wall (between the first two points)
        Vector3 forwardVector = (cornerPoints[1] - cornerPoints[0]).normalized;

        // Determine the depth direction using the third point
        Vector3 depthVector = Vector3.Cross(forwardVector, Vector3.up).normalized;

        // Compute the center of the room
        Vector3 roomCenter = (cornerPoints[0] + cornerPoints[1] + cornerPoints[2]) / 3;

        // Orient the VR player's view
        Transform vrPlayer = Camera.main.transform; // Use your VR camera's transform
        vrPlayer.position = roomCenter; // Set the position to the room center
        vrPlayer.rotation = Quaternion.LookRotation(forwardVector, Vector3.up); // Align forward direction

        Debug.Log("Room calibrated successfully!");
    }
}

