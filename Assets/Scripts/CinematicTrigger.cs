using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    PlayerController m_pc;
    void Start()
    {
        m_pc = gameObject.GetComponentInParent<PlayerController>();
    }

    public void FinishCinematic()
    {
        m_pc.FinishCinematic();
        Debug.Log("This runs");
    }
}