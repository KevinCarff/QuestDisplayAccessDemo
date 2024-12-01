using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewInputs : MonoBehaviour
{
    private InputAction primaryButtonAction;
    private InputAction secondaryButtonAction;

    void Awake()
    {
        // Create Input Actions
        primaryButtonAction = new InputAction("PrimaryButton", InputActionType.Button, "<XRController>{RightHand}/primaryButton");
        secondaryButtonAction = new InputAction("SecondaryButton", InputActionType.Button, "<XRController>{RightHand}/secondaryButton");

        // Enable Actions
        primaryButtonAction.Enable();
        secondaryButtonAction.Enable();
    }

    void Update()
    {
        // Check if Primary Button is pressed
        if (primaryButtonAction.WasPressedThisFrame())
        {
            Debug.Log("Primary Button Pressed (A Button on Quest Controller)");
        }

        // Check if Secondary Button is pressed
        if (secondaryButtonAction.WasPressedThisFrame())
        {
            Debug.Log("Secondary Button Pressed (B Button on Quest Controller)");
        }
    }

    private void OnDestroy()
    {
        // Disable actions when the script is destroyed
        primaryButtonAction.Disable();
        secondaryButtonAction.Disable();
    }
}

