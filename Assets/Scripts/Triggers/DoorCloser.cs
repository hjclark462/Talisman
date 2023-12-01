using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCloser : MonoBehaviour
{
    public Door m_door;
    public DoorOpener m_safetyNet;
    public bool m_isHallway = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            m_door.m_unlocked = !m_door.m_unlocked;
            if (!m_isHallway)
            {
                m_door.CheckState();
            }
            if (m_safetyNet != null)
            {
                m_safetyNet.gameObject.SetActive(true);
            }
            gameObject.SetActive(false);
        }
    }

    public void ResetDoor()
    {
        m_door.OpenDoor();
        gameObject.SetActive(true);
        m_safetyNet.gameObject.SetActive(false);
    }
}
