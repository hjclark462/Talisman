using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEmission : MonoBehaviour
{
    public float m_emission;
    public float m_smoothness;
    public float m_opacity;
    MeshRenderer m_mesh;
    void Start()
    {
        m_mesh = GetComponent<MeshRenderer>();
        m_mesh.material.SetFloat("_EmissionIntensity", m_emission);
        m_mesh.material.SetFloat("_Smoothness", m_smoothness);
        m_mesh.material.SetFloat("_Opacity", m_opacity);
    }   
}
