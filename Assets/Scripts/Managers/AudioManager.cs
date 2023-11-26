using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FMODUnity;
using System;
using UnityEngine.Timeline;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    GameManager m_game;

    FMOD.Studio.Bus m_music;
    FMOD.Studio.Bus m_sFX;
    FMOD.Studio.Bus m_dialogue;
    FMOD.Studio.Bus m_master;
    public float m_musicVolume = 0.5f;
    public float m_sFXVolume = 0.5f;
    public float m_dialogueVolume = 0.5f;
    public float m_masterVolume = 1f;

    [Header("FMOD Music Event References")]
    public FMODUnity.EventReference m_menuMusic;
    [HideInInspector]
    public string m_currentZone;

    [Space(5), Header("UI SFX")]
    public FMODUnity.EventReference m_navigatingMenu;
    public FMODUnity.EventReference m_selectedButton;
    public FMODUnity.EventReference m_menuBack;
    public FMODUnity.EventReference m_sliderSound;
    public FMODUnity.EventReference m_startGame;

    [Space(5), Header("Murray Puzzle Dialogue References")]
    public Dialogue m_murrayHalfSolve;
    public Dialogue m_murrayFullSolve;
    public Dialogue m_murrayFirstFail;
    public Dialogue m_murraySecondFail;
    int m_murraySolveInstances = 0;
    int m_murrayFailInstances = 0;

    [Space(5), Header("Rotation Puzzle Dialogue References")]
    public Dialogue m_rotationFirstFail;
    public Dialogue m_rotationSecondFail;
    int m_rotationFailInstances = 0;

    [Space(5), Header("Sword Room")]
    public Dialogue m_swordRoomEnd;

    public FMOD.Studio.EventInstance m_dialogueInstance;
    public FMOD.Studio.EventInstance m_menuMusicInstance;
    public FMOD.Studio.EventInstance m_talismanInstance;

    [Header("Dialogue Sections")]
    public Dialogue m_intro;
    public Dialogue m_playerDeath;

    public Dialogue m_talismanGrab;
    public Dialogue m_teleport;
    public Dialogue m_ending;
    public FMODUnity.EventReference m_talismanSFX;
    public FMODUnity.EventReference m_endSFX;
    int m_cinematics = 0;
    bool m_nextCinematic = false;

    bool m_playLines = true;
    [HideInInspector]
    public bool m_stopInteractions = false;
    FMOD.Studio.Bank m_bank;

    bool isBothSolved;
    public Dialogue m_allPuzzlesSolved;
    public void MainRoom(Dialogue dialogue)
    {
        if (!isBothSolved)
        {
            PlayDialogue(dialogue);
            isBothSolved = true;
        }
        else
        {
            PlayDialogue(m_allPuzzlesSolved);
        }
    }

    private void Awake()
    {
        m_music = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        m_sFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
        m_master = FMODUnity.RuntimeManager.GetBus("bus:/Master");
        m_dialogue = FMODUnity.RuntimeManager.GetBus("bus:/Master/Dialogue");
        m_currentZone = "Exploring";
        LoadSettings();
    }

    void Start()
    {
        m_game = GameManager.Instance;
        m_game.m_audioManager = this;


        m_menuMusicInstance = FMODUnity.RuntimeManager.CreateInstance(m_menuMusic);
        StartFmodLoop(m_menuMusicInstance);
    }

    void Update()
    {
        m_master.setVolume(m_masterVolume);
        m_music.setVolume(m_musicVolume);
        m_dialogue.setVolume(m_dialogueVolume);
        m_sFX.setVolume(m_sFXVolume);
    }

    void OnDestroy()
    {
        SaveSettings();
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("masterVolume"))
        {
            m_masterVolume = PlayerPrefs.GetFloat("masterVolume");
        }
        else
        {
            m_masterVolume = 1f;
        }
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            m_musicVolume = PlayerPrefs.GetFloat("musicVolume");
        }
        else
        {
            m_musicVolume = 0.6f;
        }
        if (PlayerPrefs.HasKey("dialogueVolume"))
        {
            m_dialogueVolume = PlayerPrefs.GetFloat("dialogueVolume");
        }
        else
        {
            m_dialogueVolume = 0.5f;
        }
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            m_sFXVolume = PlayerPrefs.GetFloat("sfxVolume");
        }
        else
        {
            m_sFXVolume = 1f;
        }
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("masterVolume", m_masterVolume);
        PlayerPrefs.SetFloat("musicVolume", m_musicVolume);
        PlayerPrefs.SetFloat("dialogueVolume", m_dialogueVolume);
        PlayerPrefs.SetFloat("sfxVolume", m_sFXVolume);
        PlayerPrefs.Save();
    }
    public void MasterVolumeLevel(float newMasterVolume)
    {
        PlayOneShot(m_sliderSound, m_game.m_player.gameObject.transform.position);
        m_masterVolume = newMasterVolume;
    }

    public void MusicVolumeLevel(float newMusicVolume)
    {
        PlayOneShot(m_sliderSound, m_game.m_player.gameObject.transform.position);
        m_musicVolume = newMusicVolume;
    }

    public void SFXVolumeLevel(float newSFXVolume)
    {
        PlayOneShot(m_sliderSound, m_game.m_player.gameObject.transform.position);
        m_sFXVolume = newSFXVolume;
    }
    public void DialogueVolumeLevel(float newDialogueVolume)
    {
        PlayOneShot(m_sliderSound, m_game.m_player.gameObject.transform.position);
        m_dialogueVolume = newDialogueVolume;
    }

    public void PlayOneShot(EventReference fmodEvent, Vector3 pos)
    {
        RuntimeManager.PlayOneShot(fmodEvent, pos);
    }

    public void EndFmodLoop(FMOD.Studio.EventInstance eventInstance)
    {
        eventInstance.release();
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void StartFmodLoop(FMOD.Studio.EventInstance eventInstance)
    {
        eventInstance.start();
    }

    public void PlayDialogue(Dialogue reference)
    {
        if (reference is DialogueSequence)
        {
            PlayDialogueSequence((DialogueSequence)reference).Forget();
        }
        else if (reference is DialogueObject)
        {
            PlayOneShotAttachedDialogue((DialogueObject)reference, m_game.m_player.gameObject).Forget();
        }
    }

    public async UniTask PlayDialogueSequence(DialogueSequence reference)
    {
        FMOD.Studio.PLAYBACK_STATE current;
        m_dialogueInstance.getPlaybackState(out current);

        if (current == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            m_dialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            m_game.m_menuManager.SetSubtitle(string.Empty, true);
        }

        int index = 0;
        m_playLines = true;
        if (m_stopInteractions)
        {
            m_game.m_player.m_stopInteracts = true;
        }
        while (m_playLines)
        {
            m_dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(reference.m_sequence[index].m_eventReference);
            RuntimeManager.AttachInstanceToGameObject(m_dialogueInstance, m_game.m_player.gameObject.transform);
            m_dialogueInstance.start();
            m_dialogueInstance.release();
            m_dialogueInstance.getPlaybackState(out current);

            m_game.m_menuManager.SetSubtitle(reference.m_sequence[index].m_subtitle, reference.m_sequence[index].m_isPlayer);
            index++;
            while (current != FMOD.Studio.PLAYBACK_STATE.STOPPING)
            {
                m_dialogueInstance.getPlaybackState(out current);
                await UniTask.Yield();
            }
            m_game.m_menuManager.SetSubtitle(string.Empty, true);

            if (index > reference.m_sequence.Count - 1)
            {
                m_playLines = false;
                StopInteractions(m_dialogueInstance).Forget();
            }
        }
        m_nextCinematic = false;
    }

    public async UniTask PlayOneShotAttachedDialogue(DialogueObject fmodEvent, GameObject go)
    {
        FMOD.Studio.PLAYBACK_STATE current;
        m_dialogueInstance.getPlaybackState(out current);

        if (current == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            m_dialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            m_game.m_menuManager.SetSubtitle(string.Empty, fmodEvent.m_isPlayer);
        }
        m_dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent.m_eventReference);
        RuntimeManager.AttachInstanceToGameObject(m_dialogueInstance, go.transform);
        m_dialogueInstance.start();
        m_game.m_menuManager.SetSubtitle(fmodEvent.m_subtitle, fmodEvent.m_isPlayer);

        if (m_stopInteractions)
        {
            StopInteractions(m_dialogueInstance).Forget();
        }
        while (current != FMOD.Studio.PLAYBACK_STATE.STOPPING)
        {
            m_dialogueInstance.getPlaybackState(out current);
            await UniTask.Yield();
        }
        m_game.m_menuManager.SetSubtitle(string.Empty, true);
    }

    async UniTask StopInteractions(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE current;
        instance.getPlaybackState(out current);
        m_game.m_player.m_stopInteracts = true;
        while (current != FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            instance.getPlaybackState(out current);
            await UniTask.Yield();
        }
        m_game.m_player.m_stopInteracts = false;
        m_stopInteractions = false;
    }

    public void PlayIntroDialogue()
    {
        PlayOneShot(m_startGame, m_game.m_player.gameObject.transform.position);
        PlayDialogue(m_intro);
    }

    public void PlayDeathDialogue()
    {
        PlayDialogue(m_playerDeath);
    }

    public void PlayMurrayPuzzleDialogue()
    {
        if (m_murraySolveInstances == 0)
        {
            m_murraySolveInstances++;
            PlayDialogue(m_murrayHalfSolve);
        }
        else if (m_murraySolveInstances == 1)
        {
            m_murraySolveInstances++;
            PlayDialogue(m_murrayFullSolve);
        }
    }

    public void PlayMurrayPuzzleRoom()
    {
        FMOD.Studio.PLAYBACK_STATE current;
        m_dialogueInstance.getPlaybackState(out current);

        if (m_murrayFailInstances == 0)
        {
            m_murrayFailInstances++;
            PlayDialogue(m_murrayFirstFail);
            m_dialogueInstance.getPlaybackState(out current);
        }
        else if (m_murrayFailInstances == 1 && current == FMOD.Studio.PLAYBACK_STATE.STOPPED)
        {
            m_murrayFailInstances++;
            PlayDialogue(m_murraySecondFail);
        }
    }

    public void PlayRoationPuzzleRoom()
    {
        if (m_rotationFailInstances == 0)
        {
            m_rotationFailInstances++;
            PlayDialogue(m_rotationFirstFail);
        }
        else if (m_rotationFailInstances == 1)
        {
            m_rotationFailInstances++;
            PlayDialogue(m_rotationSecondFail);
        }
    }

    public void PlaySwordRoomEndDialogue()
    {
        m_stopInteractions = true;
        PlayDialogue(m_swordRoomEnd);
    }

    public async UniTask PlayCinematic()
    {
        m_nextCinematic = true;
        if (m_cinematics > 2)
        {
            return;
        }
        else if (m_cinematics == 0)
        {
            PlayDialogue(m_talismanGrab);
            PlayOneShot(m_talismanSFX, m_game.m_player.transform.position);
            while (m_nextCinematic)
            {
                await UniTask.Yield();
            }
            m_game.SecondCinematic().Forget();
        }
        else if (m_cinematics == 1)
        {
            PlayDialogue(m_teleport);
            while (m_nextCinematic)
            {
                await UniTask.Yield();
            }
            m_game.m_player.FinishCinematic();
            m_game.m_combatTutorial.gameObject.SetActive(true);
        }
        else if (m_cinematics == 2)
        {
            PlayDialogue(m_ending);
            PlayOneShot(m_endSFX, m_game.m_player.transform.position);
            while (m_nextCinematic)
            {
                await UniTask.Yield();
            }
            m_game.EndGame();
        }
        m_cinematics++;
    }

    public void OnMenuNavigation()
    {
        PlayOneShot(m_navigatingMenu, m_game.m_player.gameObject.transform.position);
    }

    public void OnMenuSelect()
    {
        PlayOneShot(m_selectedButton, m_game.m_player.gameObject.transform.position);
    }

    public void OnMenuSlider()
    {
        PlayOneShot(m_sliderSound, m_game.m_player.gameObject.transform.position);
    }

    public void OnMenuBack()
    {
        PlayOneShot(m_menuBack, m_game.m_player.gameObject.transform.position);
    }

    public void StartCombatMusic()
    {
        m_menuMusicInstance.setParameterByNameWithLabel("Music_Zone", "Combat");
    }

    public void StopCombatMusic()
    {
        m_menuMusicInstance.setParameterByNameWithLabel("Music_Zone", m_currentZone);
    }
}
