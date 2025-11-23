using UnityEngine;
using UnityEngine.InputSystem;

public class BothThumbsticksPress : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference leftThumbPress;   // z.B. LeftHand / thumbstickClick
    public InputActionReference rightThumbPress;  // z.B. RightHand / thumbstickClick

    [Header("Target UI")]
    public GameObject highContrastUI;             // dein High_Contrast_UI

    private bool leftDown;
    private bool rightDown;

    private void OnEnable()
    {
        if (leftThumbPress != null)
        {
            leftThumbPress.action.performed += OnLeftDown;
            leftThumbPress.action.canceled += OnLeftUp;
            leftThumbPress.action.Enable();
        }

        if (rightThumbPress != null)
        {
            rightThumbPress.action.performed += OnRightDown;
            rightThumbPress.action.canceled += OnRightUp;
            rightThumbPress.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (leftThumbPress != null)
        {
            leftThumbPress.action.performed -= OnLeftDown;
            leftThumbPress.action.canceled -= OnLeftUp;
            leftThumbPress.action.Disable();
        }

        if (rightThumbPress != null)
        {
            rightThumbPress.action.performed -= OnRightDown;
            rightThumbPress.action.canceled -= OnRightUp;
            rightThumbPress.action.Disable();
        }
    }

    private void OnLeftDown(InputAction.CallbackContext ctx)
    {
        leftDown = true;
        CheckBothPressed();
    }

    private void OnLeftUp(InputAction.CallbackContext ctx)
    {
        leftDown = false;
    }

    private void OnRightDown(InputAction.CallbackContext ctx)
    {
        rightDown = true;
        CheckBothPressed();
    }

    private void OnRightUp(InputAction.CallbackContext ctx)
    {
        rightDown = false;
    }

    private void CheckBothPressed()
    {
        if (leftDown && rightDown && highContrastUI != null)
        {
            // Toggle High-Contrast-UI an/aus
            highContrastUI.SetActive(!highContrastUI.activeSelf);
        }
    }
}
