using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDecalSorter : MonoBehaviour
{
    public int m_sortPriority;
    Material m_material;
    // Start is called before the first frame update
    void Start()
    {
        m_material = GetComponent<Material>();
        m_material.renderQueue = m_sortPriority;
    }   
}
