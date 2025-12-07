using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    // Generic toggle for any menu
    public void ToggleMenu(GameObject menu)
    {
        if (menu == null) return;
        menu.SetActive(!menu.activeSelf);
    }
}
