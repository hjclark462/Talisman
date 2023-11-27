using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    EventSystem m_eventSystem;
    AudioManager m_audioManager;

    // Start is called before the first frame update
    void Awake()
    {        
        m_eventSystem = EventSystem.current;
        m_audioManager = GameManager.Instance.m_audioManager;
    }

    public void OnSelect(BaseEventData eventData)
    {
       
        m_audioManager.OnMenuNavigation();      
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_eventSystem.SetSelectedGameObject(eventData.pointerEnter);
    }
}
