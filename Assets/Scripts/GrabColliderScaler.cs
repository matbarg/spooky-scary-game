using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public CapsuleCollider grabCollider;
    public float radiusMultiplier = 1.5f;
    public float heightMultiplier = 1.5f;

    private float originalRadius;
    private float originalHeight;

    private bool enlarged = false;

    void Start()
    {
        originalRadius = grabCollider.radius;
        originalHeight = grabCollider.height;
    }

    public void ToggleHitbox()
    {
        enlarged = !enlarged;

        if (enlarged)
        {
            grabCollider.radius = originalRadius * radiusMultiplier;
            grabCollider.height = originalHeight * heightMultiplier;
        }
        else
        {
            grabCollider.radius = originalRadius;
            grabCollider.height = originalHeight;
        }
    }
}
