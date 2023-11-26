using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TutorialRotateMana : Puzzle
{
    [Space(5), Header("Speed Mana flows"), Space(5)]
    public float m_speed = 1.0f;
    public float m_rewindSpeed = 1.0f;

    [Space(5), Header("Connected Objects"), Space(5)]
    public Puzzle m_outputObject;

    public MeshRenderer m_one;
    public MeshRenderer m_two;

    MeshRenderer m_in;
    MeshRenderer m_out;

    string m_shaderIn;
    string m_shaderOut;

    bool m_flipMana;

    public TutPositions m_tutInput;
    public TutPositions m_tutOutput;

    public enum TutPositions
    {
        ONE,
        TWO,
        THREE,
        FOUR,
    }

    private new void Start()
    {
        if (m_outputObject != null)
        {
            m_outputObject.SetInputObject(this);
        }

        m_input = Positions.TWO;
        m_output = Positions.TWO;

        m_degrees = 90;
        base.Start();
        List<Material> materials = new List<Material>();
        materials.Add(m_one.material);

        m_in = m_one;
        m_in.materials = materials.ToArray();
        m_shaderIn = "_ManaChannel1";

        m_out = m_two;
        m_out.materials = materials.ToArray();
        m_shaderOut = "_ManaChannel2";

        m_in.enabled = true;
        m_out.enabled = true;
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
        base.RotatePuzzle();
    }

    public override void StopRotation()
    {
        m_canInteract = !m_canInteract;
    }

    public override void UpdatePositions()
    {
        if (m_tutInput == TutPositions.ONE)
        {
            m_tutInput = TutPositions.FOUR;
            m_flipMana = true;
        }
        else
        {
            m_tutInput--;
        }
        if (m_tutOutput == TutPositions.ONE)
        {
            m_tutOutput = TutPositions.FOUR;
            m_flipMana = false;
        }
        else
        {
            m_tutOutput--;
        }
        if (m_tutInput == TutPositions.ONE || m_tutOutput == TutPositions.ONE)
        {
            m_input = Positions.ONE;
            m_output = Positions.ONE;
        }
        else
        {
            m_input = Positions.TWO;
            m_output = Positions.TWO;
        }
    }

    public override void UpdateMana()
    {
        if (m_rewindMana)
        {
            m_manaValue -= Time.deltaTime * m_speed;
           
            if (m_flipMana)
            {
                if (m_manaValue > 0.5f)
                {
                    m_in.material.SetFloat(m_shaderIn, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                }
                else
                {
                    m_out.material.SetFloat(m_shaderOut, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                }
            }
            else
            {
                if (m_manaValue > 0.5f)
                {
                    m_out.material.SetFloat(m_shaderOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                }
                else
                {
                    m_in.material.SetFloat(m_shaderIn, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                }
            }


            if (m_manaValue < 0.0f)
            {
                m_manaValue = 0.0f;
                m_updateMana = false;
                m_rewindMana = false;
                m_in.material.SetFloat(m_shaderIn, 1);
                m_out.material.SetFloat(m_shaderOut, 1);

                if (m_inputObject != null && m_inputObject.m_manaValue >= 1.0f)
                {
                    m_inputObject.RewindPuzzle();
                }
            }
        }
        else
        {
            m_manaValue += Time.deltaTime * m_speed;

            if (m_flipMana)
            {
                if (m_manaValue < 0.5f)
                {
                    m_out.material.SetFloat(m_shaderOut, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                }
                else
                {
                    m_in.material.SetFloat(m_shaderIn, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                }
            }
            else
            {
                if (m_manaValue < 0.5f)
                {
                    m_in.material.SetFloat(m_shaderIn, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                }
                else
                {
                    m_out.material.SetFloat(m_shaderOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                }
            }


            if (m_manaValue > 1.0f)
            {
                m_manaValue = 1.0f;
                m_in.material.SetFloat(m_shaderIn, 0);
                m_out.material.SetFloat(m_shaderOut, 0);

                m_updateMana = false;
                StartNextSequence();
            }
        }
    }

    void StartNextSequence()
    {
        if (m_tutInput == TutPositions.ONE)
        {
            if (m_outputObject != null && (m_outputObject.m_input == Positions.ONE || m_outputObject.m_output == Positions.ONE))
            {
                GameManager.Instance.m_audioManager.PlayOneShot(m_outputObject.m_manaFlowOn, m_outputObject.gameObject.transform.position);
                m_outputObject.m_updateMana = true;
            }
        }
        else if (m_tutOutput == TutPositions.ONE)
        {
            if (m_outputObject != null && (m_outputObject.m_input == Positions.ONE || m_outputObject.m_output == Positions.ONE))
            {
                GameManager.Instance.m_audioManager.PlayOneShot(m_outputObject.m_manaFlowOn, m_outputObject.gameObject.transform.position);
                m_outputObject.m_updateMana = true;
            }          
        }
    }

    public override void RewindPuzzle()
    {
        if (!m_rewindMana)
        {
            if (m_tutInput == TutPositions.ONE)
            {
                if (m_outputObject != null && m_outputObject.m_manaValue > 0)
                {
                    m_outputObject.RewindPuzzle();
                }
                else
                {                   
                    m_rewindMana = true;
                    m_updateMana = true;
                }
            }
            else if (m_tutOutput == TutPositions.ONE)
            {
                if (m_outputObject != null && m_outputObject.m_manaValue > 0)
                {
                    m_outputObject.RewindPuzzle();
                }
                else
                {                    
                    m_rewindMana = true;
                    m_updateMana = true;
                }
            }
        }
    }
    public override void ForwardMana()
    {
        m_rewindMana = false;
        if (m_outputObject != null)
        {
            m_outputObject.ForwardMana();
        }       
    }
}
