using Cysharp.Threading.Tasks.Triggers;
using Unity.Mathematics;
using UnityEngine;

public class ManaPipe : Puzzle
{
    [Space(5), Header("Speed Mana flows"), Space(5)]
    public float m_speed = 1.0f;
    public float m_rewindSpeed = 1.0f;

    [Space(5), Header("Connected Objects"), Space(5)]
    public Puzzle m_outputLeftObject;
    public Puzzle m_outputRightObject;

    bool m_twoOutputs = false;

    public ManaChannel m_channel;
    public ManaDirection m_direction;
    string m_shader;

    public bool m_murrayPuzzleOut;
    public bool m_rotationPuzzleOut;        

    MeshRenderer m_pipe;

    private new void Start()
    {
        m_pipe = GetComponent<MeshRenderer>();
        if (m_outputLeftObject != null)
        {
            m_outputLeftObject.SetInputObject(this);
        }
        if (m_outputRightObject != null)
        {
            m_outputRightObject.SetInputObject(this);
        }
        if (m_outputLeftObject != null && m_outputRightObject != null)
        {
            m_twoOutputs = true;
        }

        if (m_channel == ManaChannel.ONE)
        {
            m_shader = "_ManaChannel1";
        }
        else if (m_channel == ManaChannel.TWO)
        {
            m_shader = "_ManaChannel2";
        }
        else if (m_channel == ManaChannel.THREE)
        {
            m_shader = "_ManaChannel3";
        }
        else if (m_channel == ManaChannel.FOUR)
        {
            m_shader = "_ManaChannel4";
        }      
    }

    private void Update()
    {
        if (m_updateMana)
        {
            UpdateMana();
        }
    }

    public override void UpdateMana()
    {
        if (m_rewindMana)
        {
            m_manaValue -= Time.deltaTime * m_rewindSpeed;

            if (m_direction == ManaDirection.POS)
            {
                m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, 1, m_manaValue));
            }
            else
            {
                m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, -1, m_manaValue));
            }

            //Update material
            if (m_manaValue < 0.0f)
            {
                m_manaValue = 0.0f;
                m_rewindMana = false;
                m_updateMana = false;
                //Update material


                if (m_direction == ManaDirection.POS)
                {
                    m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, 1, m_manaValue));
                }
                else
                {
                    m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, -1, m_manaValue));
                }


                //STOP DELETE
                if (m_inputObject != null && !(m_inputObject is Lever))
                {
                    m_inputObject.RewindPuzzle();
                }
            }
        }
        else
        {
            m_manaValue += Time.deltaTime * m_speed;
            //Update material
            //DELETE THIS

            if (m_direction == ManaDirection.POS)
            {
                m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, 1, m_manaValue));
            }
            else
            {
                m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, -1, m_manaValue));
            }
            if (m_manaValue > 1.0f)
            {
                m_manaValue = 1.0f;
                m_updateMana = false;
                //Update material

                if (m_direction == ManaDirection.POS)
                {
                    m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, 1, m_manaValue));
                }
                else
                {
                    m_pipe.material.SetFloat(m_shader, math.remap(1, 0, 0, -1, m_manaValue));
                }

                if (m_murrayPuzzleOut)
                {
                    GameManager.Instance.m_audioManager.PlayMurrayPuzzleRoom();
                }
                else if (m_rotationPuzzleOut)
                {
                    GameManager.Instance.m_audioManager.PlayRoationPuzzleRoom();

                }

                StartNextSequence();
            }
        }
    }
    void StartNextSequence()
    {
        if (m_outputLeftObject != null)
        {
            if (m_outputLeftObject.m_input == Positions.ONE || m_outputLeftObject.m_output == Positions.ONE)
            {
                m_outputLeftObject.m_updateMana = true;
                GameManager.Instance.m_audioManager.PlayOneShot(m_outputLeftObject.m_manaFlowOn, m_outputLeftObject.gameObject.transform.position);
            }
        }
        if (m_outputRightObject != null)
        {
            if (m_outputRightObject.m_input == Positions.ONE || m_outputRightObject.m_output == Positions.ONE)
            {
                GameManager.Instance.m_audioManager.PlayOneShot(m_outputRightObject.m_manaFlowOn, m_outputRightObject.gameObject.transform.position);
                m_outputRightObject.m_updateMana = true;
            }
        }
        else
        {
            GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowFail, transform.position);
            // Activate futz graphic
        }
    }

    public override void RotatePuzzle()
    {

    }

    public override void StopRotation()
    {
        if (m_outputLeftObject != null)
        {
            m_outputLeftObject.StopRotation();
        }
        if (m_outputRightObject != null)
        {
            m_outputRightObject.StopRotation();
        }
    }

    public override void RewindPuzzle()
    {
        if (!m_rewindMana)
        {
            if (m_twoOutputs)
            {
                if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue <= 0 &&
                          m_outputRightObject != null && m_outputRightObject.m_manaValue <= 0)
                {
                    m_rewindMana = true;
                    m_updateMana = true;
                    GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowOff, transform.position);
                }
                else
                {
                    if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue > 0)
                    {
                        m_outputLeftObject.RewindPuzzle();
                    }
                    if (m_outputRightObject != null && m_outputRightObject.m_manaValue > 0)
                    {
                        m_outputRightObject.RewindPuzzle();
                    }
                }
            }
            else if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue > 0)
            {
                m_outputLeftObject.RewindPuzzle();
            }
            else
            {
                GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowOff, transform.position);
                m_rewindMana = true;
                m_updateMana = true;
            }
        }
        else
        {
            GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowOff, transform.position);
            m_rewindMana = true;
            m_updateMana = true;
        }
    }
}

public enum ManaChannel
{
    ONE,
    TWO,
    THREE,
    FOUR
}
public enum ManaDirection
{
    POS,
    NEG
}