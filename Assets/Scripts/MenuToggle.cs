using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    public void ToggleMenu()
    {
        if (menu == null) return;
        menu.SetActive(!menu.activeSelf);
    }
}
