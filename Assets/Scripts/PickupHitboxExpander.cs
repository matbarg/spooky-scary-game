using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupHitboxExpander : MonoBehaviour
{
    [SerializeField] private float sizeMultiplier = 2f;   // how much bigger in assist mode

    private Collider col;

    // original values
    private Vector3 originalBoxSize;
    private float originalCapsuleRadius;
    private float originalCapsuleHeight;

    private bool isExpanded;

    void Awake()
    {
        col = GetComponent<Collider>();

        if (col is BoxCollider box)
        {
            originalBoxSize = box.size;
        }
        else if (col is CapsuleCollider capsule)
        {
            originalCapsuleRadius = capsule.radius;
            originalCapsuleHeight = capsule.height;
        }
        else
        {
            Debug.LogWarning($"{name}: PickupHitboxExpander only tested with BoxCollider and CapsuleCollider.");
        }
    }

    public void Toggle()
    {
        isExpanded = !isExpanded;
        ApplySize();
    }

    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        ApplySize();
    }

    private void ApplySize()
    {
        if (col is BoxCollider box)
        {
            box.size = isExpanded ? originalBoxSize * sizeMultiplier : originalBoxSize;
        }
        else if (col is CapsuleCollider capsule)
        {
            capsule.radius = isExpanded ? originalCapsuleRadius * sizeMultiplier : originalCapsuleRadius;
            capsule.height = isExpanded ? originalCapsuleHeight * sizeMultiplier : originalCapsuleHeight;
        }
    }
}
