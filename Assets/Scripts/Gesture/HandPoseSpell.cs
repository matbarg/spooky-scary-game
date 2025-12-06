using UnityEngine;
using UnityEngine.InputSystem;

public class HandPoseSpell : MonoBehaviour
{
    [Header("Referenzen")]
    public GhostEnemy ghost;               // dein Geist
    public Transform head;                 // Main Camera (Spieler-Kopf)

    [Header("Eingabe")]
    public InputActionReference spellButton; // z.B. XRI Left/Right Interaction -> Activate oder Select

    [Header("Spell-Einstellungen")]
    public float spellRange = 5f;          // Reichweite
    public float cooldown = 6f;            // Zeit zwischen zwei Zaubern
    public float requiredHoldTime = 0.5f;  // wie lange Pose gehalten werden muss

    [Header("Pose-Bedingungen")]
    public float minHeightAboveHead = 0.15f; // Hand mindestens so viel über Kopf
    public float minUpDot = 0.7f;            // wie stark Controller nach oben zeigen muss (0..1)

    [Header("Effekte (optional)")]
    public ParticleSystem lightningVfx;
    public AudioSource audioSource;
    public AudioClip lightningClip;

    [Header("Debug")]
    public bool debugLog = false;

    float holdTimer;
    float lastCastTime;

    void OnEnable()
    {
        if (spellButton != null)
            spellButton.action.Enable();
    }

    void OnDisable()
    {
        if (spellButton != null)
            spellButton.action.Disable();
    }

    void Update()
    {
        if (spellButton == null || head == null)
            return;

        // Button muss gehalten werden (Activate / Select o.ä.)
        bool buttonHeld = spellButton.action.IsPressed();
        if (!buttonHeld)
        {
            holdTimer = 0f;
            return;
        }

        Vector3 handPos = transform.position;
        Vector3 headPos = head.position;

        // 1) Hand hoch genug?
        float heightDiff = handPos.y - headPos.y;
        bool highEnough = heightDiff >= minHeightAboveHead;

        // 2) Controller zeigt nach oben?
        float upDot = Vector3.Dot(transform.up, Vector3.up);
        bool pointingUp = upDot >= minUpDot;

        if (debugLog)
        {
            Debug.Log($"[HandPoseSpell] highEnough={highEnough} diff={heightDiff:F2}, pointingUp={pointingUp} dot={upDot:F2}");
        }

        if (!highEnough || !pointingUp)
        {
            holdTimer = 0f;
            return;
        }

        // Cooldown?
        if (Time.time - lastCastTime < cooldown)
            return;

        // Pose wird korrekt gehalten Zeit zählen
        holdTimer += Time.deltaTime;
        if (holdTimer >= requiredHoldTime)
        {
            CastSpell();
            lastCastTime = Time.time;
            holdTimer = 0f;
        }
    }

    void CastSpell()
    {
        if (debugLog)
            Debug.Log("[HandPoseSpell] SPELL CAST");

        // Effekte
        if (lightningVfx != null)
            lightningVfx.Play();

        if (audioSource != null)
        {
            if (lightningClip != null)
                audioSource.PlayOneShot(lightningClip);
            else
                audioSource.Play();
        }

        // Geist vertreiben
        if (ghost == null || !ghost.IsActive)
            return;

        float dist = Vector3.Distance(transform.position, ghost.transform.position);
        if (debugLog)
            Debug.Log($"[HandPoseSpell] dist={dist:F2}");

        if (dist <= spellRange)
        {
            ghost.ExternalRepel();
        }
    }
}