using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Talisman/DialogueObject", fileName = "New Dialogue Object")]
public class DialogueObject : Dialogue
{
    public FMODUnity.EventReference m_eventReference;
    public string m_subtitle;
    public bool m_isPlayer;
}
