using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomCalibration : NetworkBehaviour
{
    public bool PlayerHasCalibrated = false;

    public GameObject playerRig; // Assign a marker prefab in the inspector
    public GameObject playerCamera; // Assign a marker prefab in the inspector
    public GameObject markerPrefab; // Assign a marker prefab in the inspector
    public GameObject floor; // Assign a marker prefab in the inspector
    private GameObject[] markers = new GameObject[4]; // Store marker GameObjects
    private Vector3[] cornerPoints = new Vector3[4]; // Store corner points
    private int clickCount = 0;
    private float durration = 500f;

    public GameObject myTagTransform;

    public GameObject averageMyTagTransform;

    public GameObject netTagTransform;

    public GameObject three;
    public GameObject two;
    public GameObject one;
    public GameObject exclamationPoint;

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

        if (isSearching) //This will run when you finally search for the AprilTag
        {
            LookForAndSetTagLocation();
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

    public void LookForAndSetTagLocation()
    {
        if (PlayerHasCalibrated)
        {
            if (primaryButtonAction.WasPressedThisFrame())
            {
                StartCoroutine(TakeAverageForCalibration());
                isSearching = false;
                buttonController.UpdateStatus("You have successfully calibrated using another player's localization. Wait for others now.");
                return;
            }
            if (playerRig.transform.position.x != 0 || playerRig.transform.position.y != 0 || playerRig.transform.position.z != 0)
            {
                playerRig.transform.position = Vector3.zero;
            }
            if (playerRig.transform.eulerAngles.x != 0 || playerRig.transform.eulerAngles.y != 0 || playerRig.transform.eulerAngles.z != 0)
            {
                playerRig.transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        else
        {
            if (primaryButtonAction.WasPressedThisFrame()) // set position of tag on button press
            {
                StartCoroutine(TakeAverageForSettingNetPoint());
                isSearching = false;
                return;
            }
        }
    }

    public IEnumerator TakeAverageForCalibration()
    {
        float numberOfImages = 3f * 30f; //30 snapshots
        ArrayList myVectorPositions = new ArrayList();
        ArrayList myVectorRotations = new ArrayList();

        for (int i = 0; i < numberOfImages; i++)
        {
            if (i/numberOfImages < 0.33f) //3
            {
                exclamationPoint.SetActive(false);
                one.SetActive(false);
                two.SetActive(false);
                three.SetActive(true);
            }
            else if (i/numberOfImages < 0.66f) //2
            {
                exclamationPoint.SetActive(false);
                one.SetActive(false);
                two.SetActive(true);
                three.SetActive(false);
            }
            else //1
            {
                exclamationPoint.SetActive(false);
                one.SetActive(true);
                two.SetActive(false);
                three.SetActive(false);
            }

            //Gather data
            myVectorPositions.Add(new Vector3(myTagTransform.transform.position.x, myTagTransform.transform.position.y, myTagTransform.transform.position.z));
            myVectorRotations.Add(new Vector3(myTagTransform.transform.eulerAngles.x, myTagTransform.transform.eulerAngles.y, myTagTransform.transform.eulerAngles.z));


            yield return new WaitForSeconds(1f / numberOfImages);
        }

        int count = 0;
        Vector3 total = Vector3.zero;
        foreach (Vector3 vector in myVectorPositions)
        {
            total += vector;
            count++;
        }
        averageMyTagTransform.transform.position = total / count;

        count = 0;
        total = Vector3.zero;
        foreach (Vector3 vector in myVectorRotations)
        {
            total += vector;
            count++;
        }
        averageMyTagTransform.transform.eulerAngles = total / count;


        exclamationPoint.SetActive(true);
        one.SetActive(false);
        two.SetActive(false);
        three.SetActive(false);

        // Rotate myTag to the netTag's orientation and use that rotation to orient the playerRig

        // This method could introduce error (This works! don't change)
        playerRig.transform.eulerAngles = new Vector3(0, playerRig.transform.rotation.eulerAngles.y + (netTagTransform.transform.eulerAngles.y - averageMyTagTransform.transform.eulerAngles.y), 0);

        // Set the position of myTag to the netTag's position

        Vector3 tagToPlr = playerCamera.transform.position - averageMyTagTransform.transform.position; // Vector of the seen tag to the player head
                                                                                                //SpawnVectorCube(myTagTransform.transform.position, tagToPlr, Color.red);

        Vector3 rotatedTagToPlr = Quaternion.AngleAxis(netTagTransform.transform.eulerAngles.y - averageMyTagTransform.transform.eulerAngles.y, Vector3.up) * tagToPlr;
        //SpawnVectorCube(myTagTransform.transform.position, rotatedTagToPlr, Color.blue);

        //SpawnVectorCube(playerRig.transform.position, (netTagTransform.transform.position - myTagTransform.transform.position), Color.green);
        //SpawnVectorCube(netTagTransform.transform.position, -((myTagTransform.transform.position + tagToPlr) - netTagTransform.transform.position), Color.yellow);
        playerRig.transform.position = rotatedTagToPlr - ((averageMyTagTransform.transform.position + tagToPlr) - netTagTransform.transform.position); //-(netTagTransform.transform.position - myTagTransform.transform.position) + 

    }

    public IEnumerator TakeAverageForSettingNetPoint()
    {
        float numberOfImages = 3f * 30f; //30 snapshots
        ArrayList myVectorPositions = new ArrayList();
        ArrayList myVectorRotations = new ArrayList();

        for (int i = 0; i < numberOfImages; i++)
        {
            if (i / numberOfImages < 0.33f) //3
            {
                exclamationPoint.SetActive(false);
                one.SetActive(false);
                two.SetActive(false);
                three.SetActive(true);
            }
            else if (i / numberOfImages < 0.66f) //2
            {
                exclamationPoint.SetActive(false);
                one.SetActive(false);
                two.SetActive(true);
                three.SetActive(false);
            }
            else //1
            {
                exclamationPoint.SetActive(false);
                one.SetActive(true);
                two.SetActive(false);
                three.SetActive(false);
            }

            //Gather data
            myVectorPositions.Add(new Vector3(myTagTransform.transform.position.x, myTagTransform.transform.position.y, myTagTransform.transform.position.z));
            myVectorRotations.Add(new Vector3(myTagTransform.transform.eulerAngles.x, myTagTransform.transform.eulerAngles.y, myTagTransform.transform.eulerAngles.z));

            yield return new WaitForSeconds(1f/numberOfImages);
        }

        int count = 0;
        Vector3 total = Vector3.zero;
        foreach (Vector3 vector in myVectorPositions)
        {
            total += vector;
            count++;
        }
        averageMyTagTransform.transform.position = total / count;

        count = 0;
        total = Vector3.zero;
        foreach (Vector3 vector in myVectorRotations)
        {
            total += vector;
            count++;
        }
        averageMyTagTransform.transform.eulerAngles = total / count;

        exclamationPoint.SetActive(true);
        one.SetActive(false);
        two.SetActive(false);
        three.SetActive(false);

        //Set netWorked Tag
        SetTransform(averageMyTagTransform.transform.position, averageMyTagTransform.transform.rotation);

        SetPlayerCalibrated(true);

        buttonController.UpdateStatus("You have successfully calibrated. Wait for others now.");
    }

    public void TryToGetCloser()
    {                
        playerRig.transform.position += (netTagTransform.transform.position - myTagTransform.transform.position);

        isSearching = false;
        buttonController.UpdateStatus("You have successfully calibrated using another player's localization. Wait for others now.");
        return;
    }

    public void SpawnVectorCube(Vector3 position, Vector3 vector, Color color)
    {
        // Create a cube primitive.
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Determine the magnitude (length) of the vector.
        float vectorLength = vector.magnitude;

        // Define a thickness for the cube (the width and height).
        float thickness = 0.1f; // Adjust this value as needed

        // Scale the cube so that its z-axis length represents the vector's magnitude.
        cube.transform.localScale = new Vector3(thickness, thickness, vectorLength);

        // Only set the rotation if the vector is not zero.
        if (vector != Vector3.zero)
        {
            // Rotate the cube so that its forward (local z-axis) points in the direction of the vector.
            cube.transform.rotation = Quaternion.LookRotation(vector);

            // Since a cube's pivot is in its center, offset the cube so that its base starts at the provided position.
            // We move the cube forward (in its local z direction) by half of its length.
            cube.transform.position = position + cube.transform.forward * (vectorLength / 2f);
        }
        else
        {
            // If the vector is zero, simply position the cube at the given location.
            cube.transform.position = position;
        }

        // Apply the specified color by modifying the cube's material.
        // Using 'material' here creates an instance so that the change affects only this cube.
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = color;
    }

    [Command(requiresAuthority = false)]
    public void SetTransform(Vector3 pos, Quaternion rot)
    {
        Debug.Log("Sending transform data");
        AllSetTransform(pos, rot);
    }

    [ClientRpc(includeOwner = true)]
    public void AllSetTransform(Vector3 pos, Quaternion rot)
    {
        Debug.Log("recieving transform Data");
        netTagTransform.transform.position = pos;
        netTagTransform.transform.eulerAngles = new Vector3(-90, rot.eulerAngles.y, 0);
    }

    [Command(requiresAuthority = false)]
    public void SetPlayerCalibrated(bool hasCalibrated)
    {
        AllSetPlayerCalibrated(hasCalibrated);
    }

    [ClientRpc(includeOwner = true)]
    public void AllSetPlayerCalibrated(bool hasCalibrated)
    {
        PlayerHasCalibrated = hasCalibrated;
    }

    public void StartCalibration()
    {
        isCalibrating = true;
        isSearching = false;

        if (PlayerHasCalibrated) // TODO
        {
            isSearching = true;
            isCalibrating = false;
            buttonController.UpdateStatus("Please look for your aprilTag");
            return;
            // Do search imediately
        }

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

    private void CalculatePlayerOrientation(Vector3 pt1, Vector3 pt2, Vector3 pt3)
    {
        Debug.Log("Selection confirmed. Processing room calibration...");

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
