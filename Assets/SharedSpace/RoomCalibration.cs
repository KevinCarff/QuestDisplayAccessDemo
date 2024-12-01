using UnityEngine;
using UnityEngine.InputSystem;

public class RoomCalibration : MonoBehaviour
{
    public GameObject cube; // Assign a marker prefab in the inspector
    public GameObject markerPrefab; // Assign a marker prefab in the inspector
    private GameObject[] markers = new GameObject[4]; // Store marker GameObjects
    private Vector3[] cornerPoints = new Vector3[4]; // Store corner points
    private int clickCount = 0;

    public CalibrationButtonController buttonController; // Reference to button script

    // Input Actions
    private InputAction primaryButtonAction;
    private InputAction secondaryButtonAction;

    public bool isSearching = false;

    public bool isCalibrating = false;

    void Awake()
    {
        Debug.Log("Initializing Room Calibration...");

        // Create Input Actions
        primaryButtonAction = new InputAction("PrimaryButton", InputActionType.Button, "<XRController>{RightHand}/primaryButton");
        secondaryButtonAction = new InputAction("SecondaryButton", InputActionType.Button, "<XRController>{RightHand}/secondaryButton");

        // Enable Actions
        primaryButtonAction.Enable();
        secondaryButtonAction.Enable();

        Debug.Log("Input Actions created and enabled.");
    }

    void Update()
    {
        if (isSearching)
        {
            return;
        }

        if (!isCalibrating) return; // Only process input if calibration is active
                                    // Handle confirmation when all markers are placed

        if (primaryButtonAction.WasPressedThisFrame() && clickCount == 4)
        {
            Debug.Log("Primary button pressed and all markers are placed. Confirming selection...");
            ConfirmSelection();
            return;
        }// Handle primary button press to place markers
        else if (primaryButtonAction.WasPressedThisFrame() && clickCount <= 4)
        {
            Debug.Log("Primary button pressed. Attempting to place a marker...");
            Debug.Log($"Raycast hit at: {this.transform.position}");
            PlaceMarker(this.transform.position);
        }// Handle secondary button press to erase last marker
        else if (secondaryButtonAction.WasPressedThisFrame())
        {
            Debug.Log("Secondary button pressed. Attempting to erase the last marker...");
            EraseLastMarker();
        }
    }

    public void StartCalibration()
    {
        isCalibrating = true;

        while (clickCount > 0)
        {
            clickCount--;
            Destroy(markers[clickCount]); // Remove the last placed marker
            markers[clickCount] = null; // Clear the marker reference
            cornerPoints[clickCount] = Vector3.zero; // Clear the corner point
        }
        //clickCount = 0; // Reset the marker count
        Debug.Log("Calibration started.");
        buttonController.UpdateStatus("Place the first marker.");
    }

    private void PlaceMarker(Vector3 position)
    {
        if (clickCount < 4)
        {
            Debug.Log($"Placing marker {clickCount + 1} at position: {position}");

            // Place a marker at the clicked position
            GameObject marker = Instantiate(markerPrefab, position, Quaternion.identity);
            markers[clickCount] = marker; // Store the marker object
            cornerPoints[clickCount] = position; // Store the position
            clickCount++;

            if (clickCount == 3)
            {
                Debug.Log($"Marker {clickCount} successfully placed.");
                buttonController.UpdateStatus($"Marker {clickCount} placed. Place the next marker at your desired floor height");
            }
            else
            {
                Debug.Log($"Marker {clickCount} successfully placed.");
                buttonController.UpdateStatus($"Marker {clickCount} placed. Place the next marker.");
            }
        }
        else
        {
            Debug.LogWarning("All 4 markers are already placed. Use the primary button to erase the last marker or confirm.");
            buttonController.UpdateStatus("All 4 markers placed. Confirm or erase the last marker.");
        }
    }

    private void EraseLastMarker()
    {
        if (clickCount > 0)
        {
            Debug.Log($"Erasing marker {clickCount}...");

            clickCount--;
            Destroy(markers[clickCount]); // Remove the last placed marker
            markers[clickCount] = null; // Clear the marker reference
            cornerPoints[clickCount] = Vector3.zero; // Clear the corner point
        }
    }

    private void ConfirmSelection()
    {
        Debug.Log("Selection confirmed. Processing room calibration...");

        if (clickCount < 4)
        {
            Debug.LogError("Cannot confirm selection: Less than 4 markers placed.");
            buttonController.UpdateStatus("Cannot confirm: Place all 4 markers first.");
            return;
        }

        // Define the forward wall (between the first two points)
        Vector3 pt1 = new Vector3(cornerPoints[0].x, cornerPoints[3].y, cornerPoints[0].z);
        Vector3 pt2 = new Vector3(cornerPoints[1].x, cornerPoints[3].y, cornerPoints[1].z);
        Vector3 pt3 = new Vector3(cornerPoints[2].x, cornerPoints[3].y, cornerPoints[2].z);

        // Compute the center of the room
        Vector3 roomCenter = (pt1 + pt2) / 2;
        Debug.Log($"Calculated room center: {roomCenter}");

        // Compute the center of the room
        Vector3 centerToPoint = (roomCenter - pt2);
        Debug.Log($"Calculated room center: {centerToPoint}");

        pt3 = CalculateThirdPoint(new Vector2(pt1.x, pt1.z), new Vector2(pt2.x, pt2.z), new Vector2(pt3.x, pt3.z));



        while (clickCount > 0)
        {
            clickCount--;
            Destroy(markers[clickCount]); // Remove the last placed marker
            markers[clickCount] = null; // Clear the marker reference
            cornerPoints[clickCount] = Vector3.zero; // Clear the corner point
        }

        PlaceMarker(pt1);
        PlaceMarker(pt2);
        PlaceMarker(pt3);

        cube.SetActive(true);
        FitCube(pt1, pt3, pt2);

        //buttonController.UpdateStatus("Look For the AprilTag");
        //isSearching = true;

        //// Orient the VR player's view
        //Transform vrPlayer = Camera.main.transform; // Use your VR camera's transform
        //vrPlayer.position = roomCenter; // Set the position to the room center
        //vrPlayer.rotation = Quaternion.LookRotation(forwardVector, Vector3.up); // Align forward direction

        //Debug.Log("Room calibrated successfully!");
        //buttonController.UpdateStatus("Room calibrated successfully!");
        //isCalibrating = false; // End calibration
    }

    public Vector3 CalculateThirdPoint(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        float distance = DistancePointToLine(pointC, pointA, pointB);

        // Get the vector from A to B
        Vector2 vectorAB = pointB - pointA;

        // Calculate the third point by adding the perpendicular vector to A
        Vector3 newPointC = new Vector3(pointB.x, 0, pointB.y) - (Vector3.Cross(new Vector3(vectorAB.x, 0, vectorAB.y).normalized, Vector3.up).normalized * distance);
        newPointC.y = cornerPoints[3].y;
        return newPointC;
    }

    public float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // Vector from lineStart to lineEnd
        Vector2 line = lineEnd - lineStart;
        // Vector from lineStart to the point
        Vector2 pointToStart = point - lineStart;

        // Project pointToStart onto the line
        float t = Vector2.Dot(pointToStart, line) / line.sqrMagnitude;

        // Calculate the nearest point on the infinite line
        Vector2 projection = lineStart + t * line;

        // Return the distance between the point and the projected point
        return Vector2.Distance(point, projection);
    }

    public void FitCube(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        // Calculate vectors representing the two arms of the right angle
        Vector3 forward = (pointC - pointB).normalized;
        Vector3 upwards = Vector3.Cross((pointA - pointC), (pointB - pointC)).normalized;
        float sizeAC = (pointC - pointA).magnitude;
        float sizeBC = (pointC - pointB).magnitude;

        // Find the midpoint of the hypotenuse of the right angle
        Vector3 hypotenuseMidpoint = (pointA + pointB) / 2;

        // Position the cube at the midpoint of the hypotenuse
        cube.transform.position = hypotenuseMidpoint;

        // Rotate the cube to align with the plane of the right angle
        Quaternion rotation = Quaternion.LookRotation(forward.normalized, upwards.normalized);
        cube.transform.rotation = rotation;

        // Scale the cube to fit inside the right angle
        cube.transform.localScale = new Vector3(sizeAC, 0.01f, sizeBC);
    }

    // Place Room Corner:
    // Look for vector from pt1 to pt2
    // Find midpoint of the 
}
