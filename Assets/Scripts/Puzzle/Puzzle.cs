using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    // Rotation safety
    protected bool m_rotate = false;
    protected float m_nextY;
    protected int m_degrees = 120;
    protected Quaternion m_targetRotation;
    protected int m_rotations = 0;
    [HideInInspector]
    public bool m_unlocked = false;
    [HideInInspector]
    public List<Door> m_doors;

    [HideInInspector]
    public bool m_updateMana = false;
    [HideInInspector]
    public float m_manaValue = 0f;

    protected Puzzle m_inputObject;
    protected Puzzle m_secondInputObject;

    // Set the first face and the last face
    [Space(5), Header("Starting and End Symbols"), Space(5)]
    public Positions m_input;
    public Positions m_output;    
    
    [SerializeField]
    protected bool m_rewindMana;
    protected bool m_canInteract = true;

    public bool m_spritesFirst = true;
    public List<string> m_interactStrings = new List<string>();
    public List<ControlSprites> m_interactSprites = new List<ControlSprites>();

    public FMODUnity.EventReference m_interact;

    public FMODUnity.EventReference m_manaFlowOn;
    public FMODUnity.EventReference m_secondOnSound;
    public FMODUnity.EventReference m_manaFlowFail;

    public Vector3 m_rumble = new Vector3(0.5f, 0.3f, 0.5f);

    public Bridge m_bridge;

    public void Start()
    {
        m_nextY = transform.rotation.eulerAngles.y + m_degrees;
    }

    private void FixedUpdate()
    {
        if (m_rotate)
        {
            transform.Rotate(0, m_degrees * Time.deltaTime, 0);
            if (Quaternion.Angle(transform.rotation, m_targetRotation) <= 5f)
            {
                transform.rotation = Quaternion.Euler(0, m_nextY, 0);
                m_nextY += m_degrees;
                m_rotate = false;
                UpdatePositions();
            }
        }
    }

    public bool CanInteract()
    {
        return m_canInteract;
    }

    public virtual void RotatePuzzle()
    {
        if (m_canInteract)
        {
            m_targetRotation = Quaternion.Euler(0, m_nextY, 0);
            m_rotations++;
            m_rotate = true;
            GameManager.Instance.m_audioManager.PlayOneShot(m_interact, transform.position);
            GameManager.Instance.m_player.Rumble(m_rumble).Forget();
        }
    }

    public virtual void StopRotation() { }

    public virtual void UpdatePositions() { }

    public virtual void UpdateMana() { }

    public virtual void RewindPuzzle() { }

    public void SetInputObject(Puzzle p)
    {
        m_inputObject = p;
    }

    public void SetSecondInputObject(Puzzle p)
    {
        m_secondInputObject = p;
    }
  
    public virtual void ForwardMana() { }
}

public enum Positions
{
    ONE,
    TWO,
    THREE,
}




