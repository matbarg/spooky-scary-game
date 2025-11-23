using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighContrastMode : MonoBehaviour
{
    [Header("UI Elements")]
    public Image[] backgrounds;
    public Image[] buttons;
    public TMP_Text[] texts;

    [Header("Normal Colors")]
    public Color normalBackground = new Color(0.1f, 0.1f, 0.1f);
    public Color normalButton = new Color(0.8f, 0.8f, 0.8f);
    public Color normalText = Color.white;

    [Header("High Contrast Colors")]
    public Color hcBackground = Color.black;
    public Color hcButton = Color.white;
    public Color hcText = Color.yellow;

    bool highContrast;

    public void ToggleHighContrast()
    {
        highContrast = !highContrast;
        ApplyColors();
    }

    void ApplyColors()
    {
        Color bg = highContrast ? hcBackground : normalBackground;
        Color bt = highContrast ? hcButton : normalButton;
        Color tx = highContrast ? hcText : normalText;

        foreach (var img in backgrounds)
            if (img) img.color = bg;

        foreach (var img in buttons)
            if (img) img.color = bt;

        foreach (var t in texts)
            if (t) t.color = tx;
    }
}
