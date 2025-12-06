using UnityEngine;

/// <summary>
/// Make a GO float like a ghost
/// </summary>
public class GhostFloat : MonoBehaviour
{
    public float floatAmplitude = 0.1f;
    public float floatSpeed = 1f;

    float baseY;

    // von außen aufrufbar, um die neue Basis-Höhe zu setzen
    public void ResetBaseHeight()
    {
        baseY = transform.position.y;
    }

    void OnEnable()
    {
        ResetBaseHeight();   // beim ersten Aktivieren
    }

    void LateUpdate()
    {
        float y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        var pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}