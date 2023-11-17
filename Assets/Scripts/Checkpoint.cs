using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{    
    public Bridge m_bridge;
    public Transform m_respawnPosition;
    public List<Door> m_door;    
    public ManaPool m_manaPool;
    public Dialogue m_dialogue;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponentInParent<PlayerController>();
        if (pc != null)
        {               
            pc.m_game.SetCheckPoint(m_respawnPosition);
            if (m_bridge != null)
            {
                m_bridge.SetBridgeState(false);
            }
            if(m_door.Count > 0)
            {
                foreach (Door d in m_door)
                {
                    d.CloseDoor();
                }
            }
            if(m_manaPool != null)
            {
                m_manaPool.ResetPool();
            }
            if(m_dialogue != null)
            {

            }
            Destroy(this.gameObject);
        }
    }
}