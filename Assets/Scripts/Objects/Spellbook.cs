using UnityEngine;
using UnityEngine.UI;

public class Spellbook : MonoBehaviour
{
    public HintUI hintUI;
    [TextArea]
    public string bookText = "Lightning spell banishing evil spirits:";
    public float showDuration = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ShowBookText()
    {
        if (hintUI != null)
            hintUI.ShowHint(bookText, showDuration);
    }
}
