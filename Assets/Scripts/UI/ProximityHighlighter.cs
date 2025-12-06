using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Highlights nearby objects 
/// </summary>
public class ProximityHighlighter : MonoBehaviour
{
    [Header("Settings")]
    public float highlightRadius = 2f;
    public LayerMask interactableLayer;

    private readonly List<Outline> currentlyHighlighted = new List<Outline>();

    void Update()
    {
        // Alte Highlights ausschalten
        foreach (var outline in currentlyHighlighted)
        {
            if (outline != null)
                outline.enabled = false;
        }
        currentlyHighlighted.Clear();

        // Collider im Radius finden
        Collider[] hits = Physics.OverlapSphere(transform.position, highlightRadius, interactableLayer);

        // -> Zum Testen: siehst du das im Console-Log?
        // Debug.Log($"ProximityHighlighter: {hits.Length} Treffer");

        foreach (Collider hit in hits)
        {
            // Versuche Child ODER Parent zu finden
            Outline outline = hit.GetComponentInChildren<Outline>();
            if (outline == null)
                outline = hit.GetComponentInParent<Outline>();

            if (outline != null)
            {
                outline.enabled = true;
                currentlyHighlighted.Add(outline);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, highlightRadius);
    }
}