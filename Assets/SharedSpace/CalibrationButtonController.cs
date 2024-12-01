using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CalibrationButtonController : MonoBehaviour
{
    public RoomCalibration roomCalibration; // Reference to the RoomCalibration script
    public Text statusText; // Reference to the TextMeshPro text object


    // Method to trigger calibration sequence
    public void ActivateCalibration()
    {
        if (roomCalibration != null && statusText != null)
        {
            statusText.text = "Calibration Started...";
            roomCalibration.StartCalibration();
        }
        else
        {
            Debug.LogError("RoomCalibration or statusText reference is missing.");
        }
    }

    // Method to update the status text
    public void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        else
        {
            Debug.LogError("statusText reference is missing.");
        }
    }
}
