using UnityEngine;
/// <summary>
/// Make a GO float like a ghost
/// </summary>
public class GhostFloat : MonoBehaviour
{
    public float floatAmplitude = 0.1f;
    public float floatSpeed = 1f;

    float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        float y = startY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        var pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}