using Cysharp.Threading.Tasks;
using UnityEngine;

public class Lever : Puzzle
{
    public bool m_isOn = true;

    public Vector3 m_onAngle = new Vector3(295f, 0, 0);
    public Vector3 m_offAngle = new Vector3(245f, 0, 0);

    public float m_leverSpeed = 10;
    public float m_manaSpeed = 10;

    public Puzzle m_connectedPuzzle;
    public bool m_canBeInteracted = true;

    private new void Start()
    {
        if (m_connectedPuzzle != null)
        {
            m_connectedPuzzle.SetInputObject(this);
        }

        base.Start();
    }

    private void FixedUpdate()
    {
        if (m_isOn && Quaternion.Angle(transform.localRotation, Quaternion.Euler(m_onAngle)) >= 1)
        {
            float step = m_leverSpeed * Time.deltaTime;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(m_onAngle), step);
        }
        else if (m_isOn == false && Quaternion.Angle(transform.localRotation, Quaternion.Euler(m_offAngle)) >= 1)
        {
            float step = m_leverSpeed * Time.deltaTime;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(m_offAngle), step);
        }
        if (!m_canBeInteracted)
        {
            if (m_isOn && Quaternion.Angle(transform.localRotation, Quaternion.Euler(m_onAngle)) < 1)
            {
                m_canBeInteracted = true;
            }
            else if (!m_isOn && Quaternion.Angle(transform.localRotation, Quaternion.Euler(m_offAngle)) < 1)
            {
                m_canBeInteracted = true;
            }
        }
    }


    private void Update()
    {
        if (m_updateMana)
        {
            UpdateMana();
        }
    }

    public override void RotatePuzzle()
    {
        if (m_canBeInteracted)
        {
            GameManager.Instance.m_audioManager.PlayOneShot(m_interact, transform.position);
            GameManager.Instance.m_player.Rumble(m_rumble).Forget();
            if (!m_isOn)
            {
                m_canBeInteracted = false;
                m_isOn = true;
                if (m_connectedPuzzle != null)
                {
                    ForwardMana();
                    m_connectedPuzzle.m_updateMana = true;
                    m_connectedPuzzle.StopRotation();
                }
            }
            else
            {
                m_canBeInteracted = false;
                m_isOn = false;
                if (m_connectedPuzzle != null)
                {
                    m_connectedPuzzle.StopRotation();
                    m_connectedPuzzle.RewindPuzzle();
                }
            }
            m_unlocked = m_isOn;
            foreach (Door door in m_doors)
            {
                door.CheckState();
            }
        }
    }
    public override void ForwardMana()
    {
        m_rewindMana = false;
        if (m_connectedPuzzle != null)
        {
            m_connectedPuzzle.ForwardMana();
        }        
    }
}
