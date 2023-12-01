using UnityEngine;

public class KillZone : MonoBehaviour
{
    public Transform m_returnPoint;
    private void OnTriggerEnter(Collider other)
    {        
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.transform.position = m_returnPoint.position;
            other.gameObject.transform.rotation = m_returnPoint.rotation;            
        }
    }
}
