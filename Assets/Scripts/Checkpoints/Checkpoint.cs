using UnityEngine;
/// <summary>
/// Attach to a GO with Collider to make a Checkpoint at this place
/// </summary>
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.SetCheckpoint(other.transform.position, other.transform.rotation);
    }
}
