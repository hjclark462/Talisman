using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue m_dialogue;
    public bool m_stopInteractions;
    AudioManager m_audioManager;
    public bool m_exploringMana = false;
    public bool m_exploringDarker = false;

    private void Start()
    {
        m_audioManager = GameManager.Instance.m_audioManager;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            m_audioManager.m_stopInteractions = m_stopInteractions;
            m_audioManager.PlayDialogue(m_dialogue);            
            if(m_exploringMana)
            {
                m_audioManager.m_currentZone = "Exploring_Mana";
                m_audioManager.StopCombatMusic();
            }
            if(m_exploringDarker)
            {
                m_audioManager.m_currentZone = "Exploring_Darker";
                m_audioManager.StopCombatMusic();
            }
            Destroy(gameObject);
        }
    }
}

