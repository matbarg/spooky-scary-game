using System.Collections;
using UnityEngine;

public class GhostEnemy : MonoBehaviour
{
    [Header("Referenzen")]
    public Transform player;        // XR Origin (VR)
    public Light flashLight;        // Light der Taschenlampe
    public Transform eyesTarget;    // Punkt zwischen den Augen
    public Animator animator;       // Face-Animator (optional)
    public AudioSource audioSource; // AudioSource am Geist

    [Header("Spawn / Timing")]
    public Vector2 appearDelayRange = new Vector2(15f, 40f); // Zeit bis zum nächsten Erscheinen
    public float minSpawnDistance = 3f;   // Nah an Spieler
    public float maxSpawnDistance = 6f;   // Weiter weg
    public float timeUntilAttack = 5f;    // Reaktionszeit für Spieler

    [Header("Taschenlampen-Logik")]
    public float requiredLightTime = 2f;  // wie lange in die Augen leuchten
    public float lightLoseSpeed = 0.5f;   // wie schnell der Fortschritt wieder sinkt
    public float angleTolerance = 5f;     // Zusatzwinkel zum SpotAngle
    public LayerMask occlusionMask;       // Wände, die den Strahl blockieren

    [Header("Angriffsbewegung")]
    public float attackMoveSpeed = 3f;    // Geschwindigkeit beim Zufliegen
    public float attackStopDistance = 0.1f; // wie nah er an den Spieler ranfliegt (in m)

    [Header("Audio Clips (optional)")]
    public AudioClip appearClip;
    public AudioClip warningClip;
    public AudioClip hurtClip;
    public AudioClip attackClip;
    public AudioClip disappearClip;

    [Header("Animator")]
    public string faceStateParam = "FaceState";

    const int FACE_IDLE = 0;
    const int FACE_ANGRY = 1;
    const int FACE_EVIL = 2;
    const int FACE_SCREAM = 3;

    [Header("Spawn-Constraints")]
    public LayerMask spawnBlockMask;          // Wände, Möbel etc. die Spawn blockieren
    public float spawnCollisionCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;
    public Transform playerHead;              // optional: Kamera/Head, sonst wird player benutzt

    int faceStateHash;

    // intern
    SkinnedMeshRenderer[] meshRenderers;
    Collider[] colliders;

    bool isActive;
    float lightTimer;
    bool hasKilledPlayer;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        if (animator != null && !string.IsNullOrEmpty(faceStateParam))
            faceStateHash = Animator.StringToHash(faceStateParam);
    }

    void SetFaceState(int state)
    {
        if (animator == null) return;
        animator.SetInteger(faceStateHash, state);
    }

    void Start()
    {
        SetVisible(false);
        StartCoroutine(GhostLoop());
    }

    void SetVisible(bool visible)
    {
        isActive = visible;

        foreach (var r in meshRenderers)
            if (r != null) r.enabled = visible;

        foreach (var c in colliders)
            if (c != null) c.enabled = visible;

        // Gesicht:
        // wenn sichtbar -> erstmal EVIL
        // wenn unsichtbar -> Idle (neutral, für nächste Erscheinung)
        if (visible)
        {
            hasKilledPlayer = false;       // neuen Versuch starten
            SetFaceState(FACE_EVIL);
        }
        else
        {
            SetFaceState(FACE_IDLE);
        }
    }

    IEnumerator GhostLoop()
    {
        while (true)
        {
            // 1) warten bis zur nächsten Erscheinung
            float wait = Random.Range(appearDelayRange.x, appearDelayRange.y);
            yield return new WaitForSeconds(wait);

            // 2) um den Spieler herum erscheinen (nur X/Z, Y bleibt von GhostFloat)
            // wenn kein gültiger Spawn gefunden -> diese Runde aussetzen
            bool spawned = SpawnAroundPlayer();
            if (!spawned)
            {
                SetVisible(false);
                continue; // zurück an den Anfang der while(true)-Schleife -> nächster Loop
            }

            SetVisible(true);
            lightTimer = 0f;

            PlayClip(appearClip);
            PlayClip(warningClip);

            float aliveTimer = 0f;

            // 3) Phase, in der der Spieler reagieren muss
            while (aliveTimer < timeUntilAttack)
            {
                bool hitByLight = IsHitByFlashlight();

                if (hitByLight)
                {
                    // Lampe trifft: Angriffstimer PAUSIERT
                    lightTimer += Time.deltaTime;
                    SetFaceState(FACE_ANGRY);   // solange Licht: Angry
                }
                else
                {
                    // Kein Licht: Angriffstimer läuft weiter
                    aliveTimer += Time.deltaTime;

                    // optional: Fortschritt vom "Wegbrennen" wieder verlieren
                    lightTimer = Mathf.Max(0f, lightTimer - Time.deltaTime * lightLoseSpeed);

                    SetFaceState(FACE_EVIL);    // kein Licht: Evil
                }

                // Hat der Spieler lang genug in die Augen geleuchtet?
                if (lightTimer >= requiredLightTime)
                {
                    HandleRepelled();
                    yield return new WaitForSeconds(0.5f);
                    SetVisible(false);
                    break; // raus aus der inneren while -> zurück zur Wartephase
                }

                yield return null;
            }

            // 4) wenn noch aktiv -> Angriff mit Zufliegen
            if (isActive)
            {
                yield return StartCoroutine(AttackPlayer());
                SetVisible(false);
            }
        }
    }

    // zufällig um den Spieler herum spawnen (0–360°), mit Kollisions- und Sicht-Check
    // return: true = Spawn erfolgreich, false = kein gültiger Punkt gefunden
    bool SpawnAroundPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("GhostEnemy: Player reference not set!");
            return false;
        }

        // Kopfposition für Sichtlinie (Head oder grob über dem Player)
        Vector3 headPos = playerHead != null
            ? playerHead.position
            : player.position + Vector3.up * 1.6f;

        bool found = false;
        Vector3 chosenPos = transform.position;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // zufälliger Winkel / Distanz
            float angleDeg = Random.Range(0f, 360f);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad),
                0f,
                Mathf.Sin(angleRad)
            ) * distance;

            // Höhe beibehalten
            Vector3 candidatePos = transform.position;
            candidatePos.x = player.position.x + offset.x;
            candidatePos.z = player.position.z + offset.z;

            // 1) Kollisionscheck – steht der Geist in einer Wand / einem Möbel?
            bool collides = Physics.CheckSphere(
                candidatePos,
                spawnCollisionCheckRadius,
                spawnBlockMask,
                QueryTriggerInteraction.Ignore
            );

            if (collides)
                continue; // ungültiger Punkt, nächster Versuch

            // 2) Sichtlinie vom Spieler zum Geist (ob Wände dazwischen sind)
            Vector3 dir = candidatePos - headPos;
            float dist = dir.magnitude;

            if (dist <= 0.01f)
                continue;

            if (Physics.Raycast(
                    headPos,
                    dir.normalized,
                    out RaycastHit hit,
                    dist,
                    occlusionMask,
                    QueryTriggerInteraction.Ignore))
            {
                // etwas blockiert die Sicht -> ungültig
                continue;
            }

            // gültigen Punkt gefunden
            chosenPos = candidatePos;
            found = true;
            break;
        }

        if (!found)
        {
            // Dieses Mal kein guter Spawn -> aussetzen
            Debug.Log("GhostEnemy: kein gültiger Spawnpunkt gefunden, überspringe diese Erscheinung.");
            return false;
        }

        // Position setzen
        transform.position = chosenPos;

        // zum Spieler schauen (nur um Y drehen)
        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookTarget);

        return true;
    }

    bool IsHitByFlashlight()
    {
        if (!isActive || flashLight == null || !flashLight.enabled || eyesTarget == null)
            return false;

        Vector3 fromLight = eyesTarget.position - flashLight.transform.position;
        float distance = fromLight.magnitude;
        if (distance > flashLight.range) return false;

        float angle = Vector3.Angle(flashLight.transform.forward, fromLight.normalized);
        if (angle > flashLight.spotAngle * 0.5f + angleTolerance) return false;

        // Raycast: blockiert etwas?
        if (Physics.Raycast(
                flashLight.transform.position,
                fromLight.normalized,
                out RaycastHit hit,
                distance,
                occlusionMask,
                QueryTriggerInteraction.Ignore))
        {
            if (!hit.transform.IsChildOf(transform))
                return false;
        }

        return true;
    }

    void HandleRepelled()
    {
        // Gesicht auf Scream, bis SetVisible(false) kommt
        SetFaceState(FACE_SCREAM);

        PlayClip(hurtClip);
        PlayClip(disappearClip);
    }

    // Coroutine: Geist fliegt auf den Spieler zu und "trifft" ihn
    IEnumerator AttackPlayer()
    {
        SetFaceState(FACE_EVIL);   // beim Zufliegen: Evil

        PlayClip(attackClip);

        while (true)
        {
            if (player == null) yield break;

            // nur auf X/Z zubewegen, Y bleibt von GhostFloat
            Vector3 currentPos = transform.position;
            Vector3 targetPos = new Vector3(player.position.x, currentPos.y, player.position.z);

            float dist = Vector3.Distance(currentPos, targetPos);

            if (dist <= attackStopDistance)
            {
                // HIER ist der "Hit" – Geist im Spieler
                KillPlayer();
                break;
            }

            Vector3 dir = (targetPos - currentPos).normalized;
            transform.position += dir * attackMoveSpeed * Time.deltaTime;

            // immer zum Spieler schauen
            transform.LookAt(targetPos);

            yield return null;
        }
    }

    void KillPlayer()
    {
        if (hasKilledPlayer) return;   // nur einmal killen
        hasKilledPlayer = true;

        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(9999);   // instant kill

        // Geist sofort unsichtbar machen
        SetVisible(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        KillPlayer();
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
    }
}