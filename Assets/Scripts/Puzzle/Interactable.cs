using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool m_spritesFirst;
    public List<string> m_interactStrings = new List<string>();
    public List<ControlSprites> m_interactSprites = new List<ControlSprites>();
}
