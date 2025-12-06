using UnityEngine;
using UnityEngine.InputSystem;

public class DistanceInteractionLimiter : MonoBehaviour
{
    [Header("Ray / Interaktion")]
    public Transform rayOrigin;            // z.B. RightHand Controller
    public LayerMask interactableMask;     // Layer der interactables
    public float maxGrabDistance = 2f;     // echte Reichweite
    public float maxCheckDistance = 10f;   // wie weit wir "Fehler" prüfen
    public InputActionReference interactAction;

    [Header("UI")]
    public HintUI hintUI;

    void Update()
    {
        // Input nehmen, mit dem man "Greifen" / Interagieren auslöst.
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            CheckDistanceAndShowHint();
        }
    }

    void CheckDistanceAndShowHint()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxCheckDistance, interactableMask))
        {
            float distance = hit.distance;

            // Wenn Objekt außerhalb der erlaubten Greif-Reichweite liegt:
            if (distance > maxGrabDistance)
            {
                if (hintUI != null)
                    hintUI.ShowHint("Please move closer to object");

                // hier KEINE Interaktion ausführen
                return;
            }

            // Wenn nah genug: Interaktion, falls nicht nur über XR Ray Interactor 
        }
    }
}