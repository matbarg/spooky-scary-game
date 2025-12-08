using System.Collections;
using UnityEngine;
using TMPro;

public class HintUI : MonoBehaviour
{
    public Canvas canvas;              // dein Hint-Canvas oder Panel-Canvas
    public TextMeshProUGUI hintText;   // Text-Objekt
    public float showDuration = 2f;    // wie lange der Hinweis sichtbar ist

    Coroutine currentRoutine;

    void Awake()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        if (canvas != null)
            canvas.enabled = false; // am Anfang unsichtbar
    }

    public void ShowHint(string message, float showDuration)
    {
        if (hintText != null)
            hintText.text = message;
        this.showDuration = showDuration;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }
    public void ShowHint(string message)
    {
        if (hintText != null)
            hintText.text = message;
        this.showDuration = 2f;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        if (canvas == null)
            yield break;

        canvas.enabled = true;          // einblenden
        yield return new WaitForSeconds(showDuration);
        canvas.enabled = false;         // wieder ausblenden
    }
}