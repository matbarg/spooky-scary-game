using UnityEngine;

public class HoverAnimation : MonoBehaviour
{
    public float amplitude = 0.05f;   // How high/low it moves
    public float frequency = 1f;     // How fast it moves

    private Vector3 startPos;
    private float randomOffset;

    void Start()
    {
        startPos = transform.localPosition;
        randomOffset = Random.Range(0f, 1f);
    }

    void Update()
    {
        float yOffset = Mathf.Sin((Time.time + randomOffset) * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }
}
