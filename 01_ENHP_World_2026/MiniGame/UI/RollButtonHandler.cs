using UnityEngine;
using UnityEngine.EventSystems;

public class RollButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool IsRolling { get; private set; }

    


    public void OnPointerDown(PointerEventData eventData)
    {
        IsRolling = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsRolling = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsRolling = false;
    }
}
