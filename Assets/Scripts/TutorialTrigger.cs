using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    MenuManager m_manager;
    public List<string> m_tutorialText;
    public List<ControlSprites> m_controlSprites;
    public bool m_spriteFirst;
    public TutorialTrigger m_secondText;
    public List<EnemyActivator> m_enemies;
    public bool m_isHallway;

    private void Start()
    {
        m_manager = GameManager.Instance.m_menuManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            m_manager.SetTutorial(m_tutorialText, m_controlSprites, m_spriteFirst);
            for(int i = 0; i < m_enemies.Count; i++)
            {
                if (!m_enemies[i].m_updateMana)
                {
                    m_enemies[i].m_updateMana = true;
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            m_manager.ClearTutorial();
        }
    }

    public void SecondTutorial()
    {
        if(m_isHallway)
        {
            return;
        }
        m_secondText.gameObject.SetActive(true);      
        m_manager.ClearTutorial();
        GameManager.Instance.m_firstEnemy = false;
        Destroy(this);
    }
}

public enum ControlSprites
{
    MENU_NAV,
    MENU_SELECT,
    MENU_BACK,
    MOVEMENT,
    CAMERA,
    JUMP,
    INTERACT_ONE,
    INTERACT_TWO,
    ATTACK,
    BLOCK,
    HEAL,
    PAUSE,
    SPRINT
}