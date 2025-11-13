using UnityEngine;

public class SpinAndPulseReticle : MonoBehaviour
{
    [Header("Rotate")]
    public Vector3 axis = new Vector3(0f, 1f, 0f);   // Y-axis
    public float rpm = 90f;                           // rotations per minute
    public Space rotateSpace = Space.Self;

    [Header("Pulse")]
    public bool pulse = true;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float pulseSpeed = 3f;                     // cycles per second

    Vector3 _baseScale;

    void Awake() => _baseScale = transform.localScale;

    void Update()
    {
        // Spin
        float degPerSec = rpm * 6f;                   // 360/60
        transform.Rotate(axis.normalized, degPerSec * Time.deltaTime, rotateSpace);

        // Pulse
        if (pulse)
        {
            float t = (Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) + 1f) * 0.5f; // 0..1
            float s = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = _baseScale * s;
        }
    }
}

