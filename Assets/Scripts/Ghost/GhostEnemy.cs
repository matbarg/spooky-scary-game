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

    [Header("Höhe über Spieler")]
    public Vector2 heightOffsetRange = new Vector2(0.2f, 1.3f);

    [Header("Spawn-Constraints")]
    public LayerMask spawnBlockMask;          // Wände, Möbel etc. die Spawn blockieren
    public float spawnCollisionCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;
    public Transform playerHead;              // optional: Kamera/Head, sonst wird player benutzt

    [Header("Vertreiben")]
    public float repelDisappearDelay = 1f;   // wie lange der Geist schreit, bevor er verschwindet

    [Header("Haus-Logik")]
    public bool requirePlayerInHouse = true;  // falls man abschalten will

    [Header("Angriffshöhe")]
    public float heightFollowSpeed = 3f;     // wie schnell er seine Höhe anpasst
    public float attackHeightOffset = 0.0f;  // z.B. 0 oder leicht über Kopf

    bool playerInHouse = false;
    bool isRepelling;
    int faceStateHash;

    // intern
    SkinnedMeshRenderer[] meshRenderers;
    Collider[] colliders;

    bool isActive;
    public bool IsActive => isActive;
    float lightTimer;
    bool hasKilledPlayer;
    bool isAttacking;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        if (animator != null && !string.IsNullOrEmpty(faceStateParam))
            faceStateHash = Animator.StringToHash(faceStateParam);
    }

    void LateUpdate()
    {
        // Nur wenn der Geist sichtbar/aktiv ist und gerade NICHT angreift
        if (!isActive || isAttacking || player == null)
            return;

        // Spielerposition: Kopf, wenn vorhanden, sonst Körper
        Vector3 targetPos = playerHead != null
            ? playerHead.position
            : player.position + Vector3.up * 1.6f;

        // Nur um Y drehen (kein „Umkippen“)
        Vector3 lookTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        transform.LookAt(lookTarget);
    }

    public void SetPlayerInHouse(bool inside)
    {
        playerInHouse = inside;

        // Wenn Spieler rausgeht: Geist sofort verschwinden lassen
        if (!inside && isActive)
        {
            SetVisible(false);
        }
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
            // Warten, bis der Spieler im Haus ist
            if (requirePlayerInHouse)
            {
                // so lange warten, bis playerInHouse true ist
                yield return new WaitUntil(() => playerInHouse);
            }

            // 1) warten bis zur nächsten Erscheinung
            float wait = Random.Range(appearDelayRange.x, appearDelayRange.y);
            float t = 0f;

            // Wartezeit runterzählen, aber abbrechen wenn Spieler in der Zeit wieder rausgeht
            while (t < wait)
            {
                if (requirePlayerInHouse && !playerInHouse)
                {
                    // rausgegangen -  zurück zum Schleifenanfang,
                    // es wird diesmal kein Spawn ausgelöst
                    t = 0f;
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            // wenn der Spieler während der Wartezeit raus ist - nächster Loop
            if (requirePlayerInHouse && !playerInHouse)
                continue;

            // 2) Spawn versuchen
            bool spawned = SpawnAroundPlayer();
            if (!spawned)
            {
                SetVisible(false);
                continue;
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

        // Kopfposition des Spielers (Kamera), als Basis für Y und Sichtlinie
        Vector3 headPos = playerHead != null
            ? playerHead.position
            : player.position + Vector3.up * 1.6f;

        bool found = false;
        Vector3 chosenPos = transform.position;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // zufälliger Winkel / Distanz um den Spieler herum
            float angleDeg = Random.Range(0f, 360f);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad),
                0f,
                Mathf.Sin(angleRad)
            ) * distance;

            // zufällige Schwebehöhe relativ zur Kopfposition
            float heightOffset = Random.Range(heightOffsetRange.x, heightOffsetRange.y);

            Vector3 candidatePos = new Vector3(
                headPos.x + offset.x,
                headPos.y + heightOffset,
                headPos.z + offset.z
            );

            // 1) Kollisionscheck – nicht in Wände / Möbel
            bool collides = Physics.CheckSphere(
                candidatePos + Vector3.up * 0.05f,   // minimal anheben, damit er nicht den Boden berührt
                spawnCollisionCheckRadius,
                spawnBlockMask,
                QueryTriggerInteraction.Ignore
            );

            if (collides)
                continue;

            // 2) Sichtlinie vom Kopf zum Geist
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
                // etwas blockiert die Sicht (Wand/Decke etc.)
                continue;
            }

            // gültigen Punkt gefunden
            chosenPos = candidatePos;
            found = true;
            break;
        }

        if (!found)
        {
            Debug.Log("GhostEnemy: kein gültiger Spawnpunkt gefunden, überspringe diese Erscheinung.");
            return false;
        }

        // Position setzen
        transform.position = chosenPos;

        // GhostFloat-Basis-Höhe an neue Position anpassen
        var ghostFloat = GetComponent<GhostFloat>();
        if (ghostFloat != null)
        {
            ghostFloat.ResetBaseHeight();
        }

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
        isAttacking = true;
        SetFaceState(FACE_EVIL);   // beim Zufliegen: Evil
        PlayClip(attackClip);

        // GhostFloat ausschalten, damit wir die Höhe kontrollieren können
        var ghostFloat = GetComponent<GhostFloat>();
        if (ghostFloat != null)
            ghostFloat.enabled = false;

        while (true)
        {
            if (player == null) yield break;

            // Zielposition: beim Spieler (oder Kopf, wenn vorhanden)
            Vector3 playerPos = player.position;
            if (playerHead != null)
                playerPos = playerHead.position;

            // gewünschte Höhe = Spieler/Kopf + optionaler Offset
            float targetY = playerPos.y + attackHeightOffset;

            Vector3 currentPos = transform.position;

            // X/Z: direkt auf den Spieler zu
            Vector3 targetPosXZ = new Vector3(playerPos.x, currentPos.y, playerPos.z);
            Vector3 dirXZ = (targetPosXZ - currentPos).normalized;

            // Bewegung auf X/Z
            currentPos += dirXZ * attackMoveSpeed * Time.deltaTime;

            // Höhe langsam anpassen in Richtung Zielhöhe
            currentPos.y = Mathf.Lerp(currentPos.y, targetY, heightFollowSpeed * Time.deltaTime);

            transform.position = currentPos;

            // Distanz zum Spieler prüfen (voll 3D)
            float dist = Vector3.Distance(currentPos, playerPos);
            if (dist <= attackStopDistance)
            {
                // HIER ist der "Hit" – Geist im Spieler
                KillPlayer();
                break;
            }

            // immer zum Spieler schauen (optional inkl. Höhe)
            Vector3 lookTarget = new Vector3(playerPos.x, currentPos.y, playerPos.z);
            transform.LookAt(lookTarget);

            yield return null;
        }


        // Nach Angriff GhostFloat wieder aktivieren (falls du ihn weiter nutzen willst)
        if (ghostFloat != null)
            ghostFloat.enabled = true;

        isAttacking = false;
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
    public void ExternalRepel()
    {
        if (!isActive) return;
        StartCoroutine(ExternalRepelRoutine());
    }

    IEnumerator ExternalRepelRoutine()
    {
        // Schrei / Scream + Sounds
        HandleRepelled();

        // 0.5 Sekunden warten
        yield return new WaitForSeconds(1f);

        // dann Geist verschwinden lassen
        SetVisible(false);
    }

}