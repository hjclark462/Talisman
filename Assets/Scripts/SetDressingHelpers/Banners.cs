using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banners : MonoBehaviour
{    
    void Start()
    {        
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.SetFloat("_TimingOffset", Random.value);
    }    
}
