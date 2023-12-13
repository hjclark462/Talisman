using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemsOnOff : MonoBehaviour
{
    public List<ParticleSystem> m_particles;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == (int)Mathf.Log(LayerMask.GetMask("Player"), 2))
        {
            foreach (ParticleSystem p in m_particles)
            {
                p.Play();
            }
        }
    }
    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == (int)Mathf.Log(LayerMask.GetMask("Player"), 2))
        {
            foreach (ParticleSystem p in m_particles)
            {
                if (p.isStopped)
                {
                    p.Play();
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == (int)Mathf.Log(LayerMask.GetMask("Player"), 2))
        {
            foreach (ParticleSystem p in m_particles)
            {
                p.Stop();
            }
        }
    }
}
