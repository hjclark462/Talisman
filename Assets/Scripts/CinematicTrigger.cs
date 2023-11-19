using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    PlayerController m_pc;
    public ParticleSystem m_endParticles;
    public List<ParticleSystem> m_endSconces = new();
    public List<GameObject> m_endPPs = new();
    public List<Light> m_endLights = new();
    public float m_lightOffSpeed = 2f;

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
        LightsDown().Forget();
    }

    public async UniTask LightsDown()
    {
        foreach (Light light in m_endLights)
        {
            if(light.intensity > 0)
            {
                light.intensity -= Time.deltaTime * m_lightOffSpeed;
            }
        }
        await UniTask.Yield();  
    }
}
