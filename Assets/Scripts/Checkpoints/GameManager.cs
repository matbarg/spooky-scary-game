using System.Collections;
using UnityEngine;
/// <summary>
/// Make checkpoints in the game possible and handle Game Over
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public Transform player;   // XR Origin (VR)

    [Header("Game Over UI")]
    public GameObject gameOverPanel;  // Canvas/Panel mit Text+Bild
    public float gameOverDuration = 2f;

    Vector3 spawnPosition;
    Quaternion spawnRotation;
    bool isGameOverRunning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (player == null)
        {
            // versucht automatisch einen XROrigin zu finden
            var origin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null)
                player = origin.transform;
        }

        // Startposition als erster Checkpoint
        if (player != null)
        {
            spawnPosition = player.position;
            spawnRotation = player.rotation;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        spawnPosition = pos;
        spawnRotation = rot;
        Debug.Log("Checkpoint gesetzt bei: " + pos);
    }

    public void RespawnPlayer()
    {
        if (player == null) return;

        // bei CharController kurz deaktivieren sonst klemmt er manchmal
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = spawnPosition;
        player.rotation = spawnRotation;

        if (cc != null) cc.enabled = true;
    }
    public void TriggerGameOver()
    {
        if (!isGameOverRunning)
            StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isGameOverRunning = true;

        // UI zeigen
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // hier könnte man Bewegung / Input deaktivieren, wenn nötig

        yield return new WaitForSeconds(gameOverDuration);

        // UI ausblenden
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        RespawnPlayer();

        isGameOverRunning = false;
    }
}