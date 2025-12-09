using System.Collections;
using UnityEngine;

public class CanvasTimedActivator : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;   // the canvas you want to show/hide
    [SerializeField] private float duration = 2f;   // how long it stays visible

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (targetCanvas == null)
            targetCanvas = GetComponent<Canvas>();

        if (targetCanvas != null)
            targetCanvas.enabled = false;  // start hidden
    }

    // Call this from your button / OnButtonPress / event
    public void ShowForAWhile()
    {
        if (targetCanvas == null)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        targetCanvas.enabled = true;               // show canvas
        yield return new WaitForSeconds(duration);
        targetCanvas.enabled = false;              // hide canvas
        currentRoutine = null;
    }
}
