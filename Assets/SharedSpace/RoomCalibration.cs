using UnityEngine;
using UnityEngine.InputSystem;

public class RoomCalibration : MonoBehaviour
{
    public GameObject playerRig; // Assign a marker prefab in the inspector
    public GameObject playerCamera; // Assign a marker prefab in the inspector
    public GameObject markerPrefab; // Assign a marker prefab in the inspector
    public GameObject floor; // Assign a marker prefab in the inspector
    private GameObject[] markers = new GameObject[4]; // Store marker GameObjects
    private Vector3[] cornerPoints = new Vector3[4]; // Store corner points
    private int clickCount = 0;
    private float durration = 500f;

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
        // If no camera is explicitly assigned, find the main camera in the scene
        if (playerCamera == null)
        {
            playerCamera = Camera.main.gameObject;
        }
        // Enable Actions
        primaryButtonAction.Enable();
        secondaryButtonAction.Enable();

        Debug.Log("Input Actions created and enabled.");
    }

    void Update()
    {
        if (!floor.activeSelf && !isCalibrating)
            floor.SetActive(true);

        if (isSearching)
        {
            return;
        }

        if (!isCalibrating)
            return; // Only process input if calibration is active
        else if (floor.activeSelf)
            floor.SetActive(false);

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
        isSearching = false;

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

        playerRig.transform.position = Vector3.zero;
        playerRig.transform.eulerAngles = new Vector3(0,0,0);
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
        //Debug.DrawLine(cornerPoints[3], cornerPoints[0], Color.yellow, durration);

        // Define the forward wall (between the first two points)
        Vector3 pt1 = new Vector3(cornerPoints[0].x, cornerPoints[3].y, cornerPoints[0].z);
        Vector3 pt2 = new Vector3(cornerPoints[1].x, cornerPoints[3].y, cornerPoints[1].z);
        Vector3 pt3 = new Vector3(cornerPoints[2].x, cornerPoints[3].y, cornerPoints[2].z);

        Debug.Log($"Adjusted Points: Pt1: {pt1}, Pt2: {pt2}, Pt3: {pt3}");

        // Compute the center of the room
        Vector3 roomCenter = (pt1 + pt2) / 2;
        Debug.DrawLine(roomCenter, pt2, Color.green, durration); // Line showing center to Pt2
        Debug.Log($"Calculated room center: {roomCenter}");

        // Compute the vector from the room center to Pt2
        Vector3 centerToPoint = (roomCenter - pt2);
        Debug.DrawLine(roomCenter, roomCenter + centerToPoint, Color.magenta, durration); // Show the vector
        Debug.Log($"Vector from room center to Pt2: {centerToPoint}");

        // Recalculate Pt3 to fit specific geometry
        pt3 = CalculateThirdPoint(new Vector2(pt1.x, pt1.z), new Vector2(pt2.x, pt2.z), new Vector2(pt3.x, pt3.z));
        Debug.DrawLine(pt2, pt3, Color.blue, durration); // Line to new Pt3
        Debug.Log($"Recalculated Pt3: {pt3}");

        // Clear all existing markers
        while (clickCount > 0)
        {
            clickCount--;
            Destroy(markers[clickCount]);
            markers[clickCount] = null;
            cornerPoints[clickCount] = Vector3.zero;
            Debug.Log($"Cleared marker {clickCount + 1}");
        }

        // Step 1: Translate points so that Pt1 is at the origin
        Vector3 translationVector2 = pt2 - pt1;
        Vector3 translationVector3 = pt3 - pt2;

        Debug.DrawLine(pt1, pt1 + translationVector2, Color.red, durration); // Show translation to Pt2
        Debug.DrawLine(pt2, pt2 + translationVector3, Color.red, durration); // Show translation to Pt3
        Debug.Log($"Translation Vectors: To Pt2: {translationVector2}, To Pt3: {translationVector3}");

        Vector3 reorientedPoint2 = translationVector2.magnitude * Vector3.right;
        Vector3 reorientedPoint3 = reorientedPoint2 + (translationVector3.magnitude * (-Vector3.forward));

        Debug.DrawLine(Vector3.zero, reorientedPoint2, Color.white, durration); // Line to reoriented Pt2
        Debug.DrawLine(reorientedPoint2, reorientedPoint3, Color.white, durration); // Line to reoriented Pt3
        Debug.Log($"Reoriented Points: Pt2: {reorientedPoint2}, Pt3: {reorientedPoint3}");

        // Place reoriented markers
        PlaceMarker(Vector3.zero);
        PlaceMarker(reorientedPoint2);
        PlaceMarker(reorientedPoint3);

        //--------------------------------------------------------------------------------------------------

        Debug.DrawLine(pt1, playerCamera.transform.position, Color.yellow, durration); // Show player's calculated position
        Debug.DrawLine(pt2, playerCamera.transform.position, Color.yellow, durration); // Show player's calculated position
        Vector3 cameraForward = playerCamera.transform.position + playerCamera.transform.forward;
        Vector3 cameraUp = playerCamera.transform.position + playerCamera.transform.up;
        Debug.DrawLine(playerCamera.transform.position, cameraForward / 2, Color.red, durration); // Show player's calculated position
        Debug.DrawLine(playerCamera.transform.position, cameraUp / 2, Color.blue, durration); // Show player's calculated position

        // Calculate player position relative to the room
        Vector3 cameraPosition = new Vector3(playerCamera.transform.position.x, pt1.y, playerCamera.transform.position.z);

        Debug.DrawLine(pt1, cameraPosition, Color.white, durration); // Show player's calculated position
        Debug.DrawLine(pt2, cameraPosition, Color.white, durration); // Show player's calculated position

        float expectedHeight = playerRig.transform.position.y;
        float actualHeight = playerRig.transform.position.y - pt1.y; // (pt1.y > 0 && pt1.y < playerRig.transform.position.y ? pt1.y : 0);
        float yHeight = actualHeight - expectedHeight;

        Vector3 pt1Normal = new Vector3(pt2.x - pt1.x, 0, pt2.z - pt1.z).normalized;
        float rotateAngle = Vector3.Angle(pt1Normal, Vector3.right);
        if (pt1Normal.z < 0)
        {
            rotateAngle = -rotateAngle;
        }
        Debug.Log("Rotation angle: " + rotateAngle);
        // Create a rotation matrix around the Y-axis
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, rotateAngle, 0));
        Matrix4x4 rightRotMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, -rotateAngle, 0));

        //----------------------------------------------------------------------------------------------

        Vector3 newPlayerPosition = rotationMatrix.MultiplyPoint3x4((cameraPosition - pt1).normalized) * (cameraPosition - pt1).magnitude;
        Vector3 rightNewPlayerPosition = rightRotMatrix.MultiplyPoint3x4((cameraPosition - pt1).normalized) * (cameraPosition - pt1).magnitude;
        Debug.DrawLine(Vector3.zero, rightNewPlayerPosition, Color.green, durration); // Show player's calculated position

        //newPlayerPosition = rotationMatrix.MultiplyPoint3x4(newPlayerPosition);
        Debug.DrawLine(Vector3.zero, newPlayerPosition, Color.black, durration); // Show player's calculated position
        
        Vector3 vectorToOrigin = rotationMatrix.MultiplyPoint3x4((playerRig.transform.position - playerCamera.transform.position).normalized) * (playerRig.transform.position - playerCamera.transform.position).magnitude;
       // vectorToOrigin = rotationMatrix.MultiplyPoint3x4(vectorToOrigin);

        Vector3 newOriginForRig = newPlayerPosition + vectorToOrigin;
        Debug.DrawLine(newOriginForRig, newPlayerPosition, Color.black, durration); // Show player's calculated position
        newOriginForRig.y = yHeight;

        playerRig.transform.position = newOriginForRig;
        playerRig.transform.eulerAngles = new Vector3(0, rotateAngle, 0);

        isCalibrating = false;
        isSearching = true;
        buttonController.UpdateStatus("Please look for your aprilTag");
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
}
