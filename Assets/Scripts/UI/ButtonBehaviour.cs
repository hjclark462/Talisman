using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    EventSystem m_eventSystem;
    // Start is called before the first frame update
    void Start()
    {
        m_eventSystem = EventSystem.current;    
    }

    public void OnSelect(BaseEventData eventData)
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_eventSystem.SetSelectedGameObject(eventData.pointerEnter);
    }
}
