using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public List<Puzzle> m_puzzleList;
    [HideInInspector]
    public bool m_unlocked = false;
    public float m_speed;
    MeshRenderer m_mesh;
    float m_alpha;
    public Collider m_colliders;
    public FMODUnity.EventReference m_appearing;
    public FMODUnity.EventReference m_disappearing;

    private void Start()
    {
        foreach (Puzzle p in m_puzzleList)
        {
            p.m_bridge = this;
        }
        m_mesh = GetComponentInChildren<MeshRenderer>();
        foreach (Material m in m_mesh.materials)
        {
            m.SetFloat("_ArmorFade", 0);
        }
        m_alpha = 0.0f;
    }

    public void CheckState()
    {
        foreach (Puzzle p in m_puzzleList)
        {
            if (p.m_unlocked == false)
            {
                if (m_alpha == 1f)
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_disappearing, gameObject.transform.position);
                }
                SetBridgeState(false);
                return;
            }
        }
        if (m_alpha == 0f)
        {
            GameManager.Instance.m_audioManager.PlayOneShot(m_appearing, gameObject.transform.position);
        }
        SetBridgeState(true);
    }

    public void SetBridgeState(bool state)
    {
        m_unlocked = state;
        m_colliders.enabled = state;
    }

    private void Update()
    {
        var step = m_speed * Time.deltaTime;
        if (m_unlocked && m_alpha < 1f)
        {
            m_alpha += step;
        }
        else if (!m_unlocked && m_alpha > 0f)
        {
            m_alpha -= step;
        }
        foreach (Material m in m_mesh.materials)
        {
            m.SetFloat("_ArmorFade", m_alpha);
        }
    }
}
