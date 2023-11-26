using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FMODUnity;

public class RotateCircularMana : Puzzle
{
    [Space(5), Header("Type of visual"), Space(5)]
    public bool m_isThreeWay;
    public bool m_twoInputs;
    public bool m_isLeftBent;

    [Space(5), Header("Speed Mana flows"), Space(5)]
    public float m_speed = 1.0f;
    public float m_rewindSpeed = 1.0f;

    [Space(5), Header("Connected Objects"), Space(5)]
    public Puzzle m_outputLeftObject;
    public Puzzle m_outputRightObject;

    public MeshRenderer m_one;
    public MeshRenderer m_two;
    public MeshRenderer m_three;

    MeshRenderer m_in;
    MeshRenderer m_out;
    MeshRenderer m_inOut;

    string m_shaderIn;
    string m_shaderOut;
    string m_shaderInOut;

    public Transform m_posOneFail;
    public Transform m_posTwoFail;
    public Transform m_posThreeFail;
    Transform m_failParticlesPos;
    Transform m_failParticlesNeg;
    public ParticleSystem m_failParticlesOne;
    public ParticleSystem m_failParticlesTwo;

    FMOD.Studio.EventInstance m_manaFailOne;
    FMOD.Studio.EventInstance m_manaFailTwo;

    bool m_flipMana;

    private new void Start()
    {
        if (m_outputRightObject != null)
        {
            m_outputRightObject.SetInputObject(this);
        }
        if (m_outputLeftObject != null)
        {
            if (m_outputLeftObject is RotateCircularMana)
            {
                var left = (RotateCircularMana)m_outputLeftObject;
                if (left.m_twoInputs)
                {
                    left.SetSecondInputObject(this);
                }
                else
                {
                    left.SetInputObject(this);
                }
            }
            else
            {
                m_outputLeftObject.SetInputObject(this);
            }
        }

        if (m_isThreeWay)
        {
            m_isLeftBent = false;
        }
        else if (m_isLeftBent)
        {
            if (m_input == Positions.ONE)
            {
                m_output = Positions.THREE;
            }
            else
            {
                m_output = m_input - 1;
            }
        }
        else
        {
            if (m_input == Positions.THREE)
            {
                m_output = Positions.ONE;
            }
            else
            {
                m_output = m_input + 1;
            }
        }
        base.Start();
        List<Material> materials = new List<Material>();
        materials.Add(m_one.material);


        if (m_isThreeWay)
        {
            m_in = m_one;
            m_out = m_two;
            m_inOut = m_three;
            m_in.materials = materials.ToArray();
            m_inOut.materials = materials.ToArray();
            m_out.materials = materials.ToArray();
            m_shaderIn = "_ManaChannel1";
            m_shaderOut = "_ManaChannel2";
            m_shaderInOut = "_ManaChannel3";
            m_inOut.enabled = true;
            m_canInteract = false;
        }
        else
        {
            if (m_input == Positions.ONE)
            {
                m_in = m_one;
                m_in.materials = materials.ToArray();
                m_shaderIn = "_ManaChannel1";
                m_failParticlesNeg = m_posOneFail;
            }
            else if (m_input == Positions.TWO)
            {
                m_in = m_two;
                m_in.materials = materials.ToArray();
                m_shaderIn = "_ManaChannel2";
                m_failParticlesNeg = m_posTwoFail;
            }
            else
            {
                m_in = m_three;
                m_in.materials = materials.ToArray();
                m_shaderIn = "_ManaChannel3";
                m_failParticlesNeg = m_posThreeFail;
            }
            if (m_output == Positions.ONE)
            {
                m_out = m_one;
                m_out.materials = materials.ToArray();
                m_shaderOut = "_ManaChannel1";
                m_failParticlesPos = m_posOneFail;
            }
            else if (m_output == Positions.TWO)
            {
                m_out = m_two;
                m_out.materials = materials.ToArray();
                m_shaderOut = "_ManaChannel2";
                m_failParticlesPos = m_posTwoFail;
            }
            else
            {
                m_out = m_three;
                m_out.materials = materials.ToArray();
                m_shaderOut = "_ManaChannel3";
                m_failParticlesPos = m_posThreeFail;
            }
        }
        m_in.enabled = true;
        m_out.enabled = true;
        m_manaFailOne = RuntimeManager.CreateInstance(m_manaFlowFail);
        m_manaFailTwo = RuntimeManager.CreateInstance(m_manaFlowFail);
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
        if (!m_isThreeWay)
        {
            base.RotatePuzzle();
        }
    }
    bool m_firstOfTwo = true;
    public override void StopRotation()
    {
        m_canInteract = !m_canInteract;
        if (m_isThreeWay)
        {
            m_canInteract = false;
            if (m_twoInputs)
            {
                if (m_outputLeftObject != null)
                {
                    if (m_firstOfTwo)
                    {
                        m_firstOfTwo = false;
                    }
                    else
                    {
                        m_firstOfTwo = true;
                        m_outputLeftObject.StopRotation();
                    }
                }
            }
            else
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
        }
        else if (m_input == Positions.ONE)
        {
            if (m_isLeftBent)
            {
                if (m_outputLeftObject != null)
                {
                    m_outputLeftObject.StopRotation();
                }
            }
            else
            {
                if (m_outputRightObject != null)
                {
                    m_outputRightObject.StopRotation();
                }
            }
        }
        else if (m_output == Positions.ONE)
        {
            if (m_isLeftBent)
            {
                if (m_outputRightObject != null)
                {
                    m_outputRightObject.StopRotation();
                }
            }
            else
            {
                if (m_outputLeftObject != null)
                {
                    m_outputLeftObject.StopRotation();
                }
            }
        }
    }

    public override void UpdatePositions()
    {
        if (!m_isThreeWay)
        {
            if (m_input == Positions.ONE)
            {
                m_input = Positions.THREE;
                m_flipMana = true;
            }
            else
            {
                m_input--;
            }
            if (m_output == Positions.ONE)
            {
                m_output = Positions.THREE;
                m_flipMana = false;
            }
            else
            {
                m_output--;
            }
        }
    }

    public override void UpdateMana()
    {
        if (m_rewindMana)
        {
            if (m_failParticlesOne.isPlaying)
            {
                m_failParticlesOne.Stop();
                StopOneFailSounds();
            }
            if (m_failParticlesTwo.isPlaying)
            {
                m_failParticlesTwo.Stop();
                StopTwoFailSounds();
            }
            m_manaValue -= Time.deltaTime * m_speed;

            if (m_isThreeWay)
            {
                if (m_twoInputs)
                {
                    if (m_manaValue > 0.5f)
                    {
                        m_inOut.material.SetFloat(m_shaderInOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                    }
                    else
                    {
                        m_in.material.SetFloat(m_shaderIn, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                        m_out.material.SetFloat(m_shaderOut, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                    }
                }
                else
                {
                    if (m_manaValue > 0.5f)
                    {
                        m_inOut.material.SetFloat(m_shaderInOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                        m_out.material.SetFloat(m_shaderOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                    }
                    else
                    {
                        m_in.material.SetFloat(m_shaderIn, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                    }
                }

            }
            else
            {
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
            }

            if (m_manaValue < 0.0f)
            {
                m_manaValue = 0.0f;
                m_updateMana = false;
                m_rewindMana = false;
                m_in.material.SetFloat(m_shaderIn, 1);
                m_out.material.SetFloat(m_shaderOut, 1);
                if (m_isThreeWay)
                {
                    m_inOut.material.SetFloat(m_shaderInOut, 1);
                }
                if (m_inputObject != null && m_inputObject.m_manaValue >= 1.0f)
                {
                    m_inputObject.RewindPuzzle();
                }
                if (m_secondInputObject != null && m_secondInputObject.m_manaValue >= 1.0f)
                {
                    m_secondInputObject.RewindPuzzle();
                }
            }
        }
        else
        {
            m_manaValue += Time.deltaTime * m_speed;

            if (m_isThreeWay)
            {
                if (m_twoInputs)
                {
                    if (m_manaValue < 0.5f)
                    {
                        m_in.material.SetFloat(m_shaderIn, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                        m_out.material.SetFloat(m_shaderOut, math.remap(0f, 0.5f, -1f, 0f, m_manaValue));
                    }
                    else
                    {
                        m_inOut.material.SetFloat(m_shaderInOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
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
                        m_inOut.material.SetFloat(m_shaderInOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                        m_out.material.SetFloat(m_shaderOut, math.remap(0.5f, 1f, 1f, 0f, m_manaValue));
                    }
                }

            }
            else
            {
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

            }

            if (m_manaValue > 1.0f)
            {
                m_manaValue = 1.0f;
                m_in.material.SetFloat(m_shaderIn, 0);
                m_out.material.SetFloat(m_shaderOut, 0);
                if (m_isThreeWay)
                {
                    m_inOut.material.SetFloat(m_shaderInOut, 0);
                }
                m_updateMana = false;
                StartNextSequence();
            }
        }
    }

    void StartNextSequence()
    {
        if (m_isThreeWay)
        {
            if (m_twoInputs)
            {
                if ((m_inputObject.m_manaValue >= 1.0f && (m_inputObject.m_input == Positions.TWO || m_inputObject.m_output == Positions.TWO))
                    && (m_secondInputObject.m_manaValue >= 1.0f && (m_inputObject.m_input == Positions.THREE || m_secondInputObject.m_output == Positions.THREE)))
                {
                    if (m_outputLeftObject != null)
                    {
                        GameManager.Instance.m_audioManager.PlayOneShot(m_outputLeftObject.m_manaFlowOn, m_outputLeftObject.gameObject.transform.position);
                        m_outputLeftObject.m_updateMana = true;
                    }
                }
            }
            else
            {
                if (m_outputLeftObject != null && (m_outputLeftObject.m_input == Positions.ONE || m_outputLeftObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputLeftObject.m_manaFlowOn, m_outputLeftObject.gameObject.transform.position);
                    m_outputLeftObject.m_updateMana = true;
                }
                else
                {
                    //Activate futz graphic                    
                    m_failParticlesOne.gameObject.transform.position = m_posTwoFail.position;
                    m_failParticlesOne.Play();
                    StartOneFailSounds(m_failParticlesOne.transform);
                }
                if (m_outputRightObject != null && (m_outputRightObject.m_input == Positions.ONE || m_outputRightObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputRightObject.m_manaFlowOn, m_outputRightObject.gameObject.transform.position);
                    m_outputRightObject.m_updateMana = true;
                }
                else
                {
                    //Activate futz graphic                                        
                    m_failParticlesTwo.gameObject.transform.position = m_posThreeFail.position;
                    m_failParticlesTwo.Play();
                    StartTwoFailSounds(m_failParticlesTwo.transform);
                }
            }
        }
        else if (m_input == Positions.ONE)
        {
            if (m_isLeftBent)
            {
                if (m_outputLeftObject != null && (m_outputLeftObject.m_input == Positions.ONE || m_outputLeftObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputLeftObject.m_manaFlowOn, m_outputLeftObject.gameObject.transform.position);
                    m_outputLeftObject.m_updateMana = true;
                }
                else
                {
                    //Activate futz graphic                    
                    m_failParticlesOne.gameObject.transform.position = m_failParticlesPos.position;
                    m_failParticlesOne.Play();
                    StartOneFailSounds(m_failParticlesOne.transform);
                }
            }
            else
            {
                if (m_outputRightObject != null && (m_outputRightObject.m_input == Positions.ONE || m_outputRightObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputRightObject.m_manaFlowOn, m_outputRightObject.gameObject.transform.position);
                    m_outputRightObject.m_updateMana = true;
                }
                else
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowFail, transform.position);

                    //Activate futz graphic                    
                    m_failParticlesOne.gameObject.transform.position = m_failParticlesPos.position;
                    m_failParticlesOne.Play();
                    StartOneFailSounds(m_failParticlesOne.transform);
                }
            }
        }
        else if (m_output == Positions.ONE)
        {
            if (m_isLeftBent)
            {
                if (m_outputRightObject != null && (m_outputRightObject.m_input == Positions.ONE || m_outputRightObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputRightObject.m_manaFlowOn, m_outputRightObject.gameObject.transform.position);
                    m_outputRightObject.m_updateMana = true;
                }
                else
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowFail, transform.position);

                    //Activate futz graphic                    
                    m_failParticlesOne.gameObject.transform.position = m_failParticlesNeg.position;
                    m_failParticlesOne.Play();
                    StartOneFailSounds(m_failParticlesOne.transform);
                }
            }
            else
            {
                if (m_outputLeftObject != null && (m_outputLeftObject.m_input == Positions.ONE || m_outputLeftObject.m_output == Positions.ONE))
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_outputLeftObject.m_manaFlowOn, m_outputLeftObject.gameObject.transform.position);
                    m_outputLeftObject.m_updateMana = true;
                }
                else
                {
                    GameManager.Instance.m_audioManager.PlayOneShot(m_manaFlowFail, transform.position);

                    //Activate futz graphic                    
                    m_failParticlesOne.gameObject.transform.position = m_failParticlesNeg.position;
                    m_failParticlesOne.Play();
                    StartOneFailSounds(m_failParticlesOne.transform);
                }
            }
        }
    }

    public override void RewindPuzzle()
    {
        if (!m_rewindMana)
        {
            if (m_isThreeWay)
            {
                if (m_twoInputs)
                {
                    if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue > 0)
                    {
                        m_outputLeftObject.RewindPuzzle();
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
                else
                {
                    if (m_outputLeftObject != null && m_outputRightObject != null && (m_outputLeftObject.m_manaValue > 0 || m_outputRightObject.m_manaValue > 0))
                    {
                        if (m_outputLeftObject.m_manaValue > 0)
                        {
                            m_outputLeftObject.RewindPuzzle();
                        }
                        if (m_outputRightObject.m_manaValue > 0)
                        {
                            m_outputRightObject.RewindPuzzle();
                        }
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
            }
            else if (m_input == Positions.ONE)
            {
                if (m_isLeftBent)
                {
                    if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue > 0)
                    {
                        m_outputLeftObject.RewindPuzzle();
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
                else
                {
                    if (m_outputRightObject != null && m_outputRightObject.m_manaValue > 0)
                    {
                        m_outputRightObject.RewindPuzzle();
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
            }
            else if (m_output == Positions.ONE)
            {
                if (m_isLeftBent)
                {
                    if (m_outputRightObject != null && m_outputRightObject.m_manaValue > 0)
                    {
                        m_outputRightObject.RewindPuzzle();
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
                else
                {
                    if (m_outputLeftObject != null && m_outputLeftObject.m_manaValue > 0)
                    {
                        m_outputLeftObject.RewindPuzzle();
                    }
                    else
                    {
                        m_rewindMana = true;
                        m_updateMana = true;
                    }
                }
            }
        }
    }
    void StartOneFailSounds(Transform position)
    {
        RuntimeManager.AttachInstanceToGameObject(m_manaFailOne, position);
        m_manaFailOne.start();
    }

    void StopOneFailSounds()
    {
        m_manaFailOne.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    void StartTwoFailSounds(Transform position)
    {
        RuntimeManager.AttachInstanceToGameObject(m_manaFailTwo, position);
        m_manaFailTwo.start();
    }

    void StopTwoFailSounds()
    {
        m_manaFailTwo.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public override void ForwardMana()
    {
        m_rewindMana = false;
        if (m_outputLeftObject != null)
        {
            m_outputLeftObject.ForwardMana();
        }
        if (m_outputRightObject != null)
        {
            m_outputRightObject.ForwardMana();
        }
    }
}