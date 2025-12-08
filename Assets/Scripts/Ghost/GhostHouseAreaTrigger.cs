using UnityEngine;

public class GhostHouseAreaTrigger : MonoBehaviour
{
    [Header("Ghost")]
    public GhostEnemy ghost;

    [Header("Erster Eintritt")]
    public AudioSource firstEnterAudio;   // AudioSource mit Voice/Sound
    public HintUI hintUI;                // zentrales HintUI
    [TextArea]
    public string firstEnterMessage = "Hint: Use your flashlight to repel the ghost.";
    public float hintDuration = 2f;      // wie lange der Hinweis sichtbar ist

    bool firstEnterDone = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Ghost aktivieren
        if (ghost != null)
            ghost.SetPlayerInHouse(true);

        // Nur beim ALLERERSTEN Mal Audio + Hint
        if (!firstEnterDone)
        {
            firstEnterDone = true;

            // Sound komplett abspielen lassen
            if (firstEnterAudio != null)
                firstEnterAudio.Play();         

            // Hint über dein HintUI anzeigen
            if (hintUI != null && !string.IsNullOrEmpty(firstEnterMessage))
                hintUI.ShowHint(firstEnterMessage, hintDuration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ghost != null)
            ghost.SetPlayerInHouse(false);

    }
}