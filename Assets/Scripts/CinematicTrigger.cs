using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    PlayerController m_pc;
    public ParticleSystem m_endParticles;
    void Start()
    {
        m_pc = gameObject.GetComponentInParent<PlayerController>();
    }

    public void EndParticles()
    {
        m_endParticles.Play();
    }
}
