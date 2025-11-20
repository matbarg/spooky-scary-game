using UnityEngine;
using UnityEngine.EventSystems;

public class ShowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject icon;   // assign HoverIcon here

    void Awake()
    {
        if (icon != null)
            icon.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (icon != null)
            icon.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (icon != null)
            icon.SetActive(false);
    }
}
