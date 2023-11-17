using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    PlayerController m_pc;
    public ParticleSystem m_endParticles;
    public List<ParticleSystem> m_endSconces = new();
    public List<GameObject> m_endPPs = new();

    void Start()
    {
        m_pc = gameObject.GetComponentInParent<PlayerController>();
    }

    public void EndParticles()
    {
        m_endParticles.Play();
        foreach (ParticleSystem ps in m_endSconces)
        {
            ps.Stop();
        }
        foreach (GameObject go in m_endPPs)
        {
            go.SetActive(true);
        }
    }
}
