using UnityEngine;

public class HideUI : MonoBehaviour
{
    public Transform player;           // VR Camera / HMD
    public float showDistance = 2f;    // Distanz in Metern

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= showDistance)
            canvas.enabled = true;
        else
            canvas.enabled = false;
    }
}
