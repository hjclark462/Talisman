using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using AISystem.Systems;
using AISystem;
using AISystem.Contracts;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;
    public GameState m_lastState;
    public GameState m_gameState;
    public GameState m_controlsLastState = GameState.MENU;
    public event Action<GameState> OnGameStateChanged;
    public PlayerController m_player;
    public MenuManager m_menuManager;
    public AudioManager m_audioManager;
    public AIManager m_aiManager;

    public List<Interactable> m_cinematicTriggers;
    public List<Transform> m_cinematicPoints;
    public ParticleSystem m_sigilParticles;    
    public Vector3 m_teleportRumble = new Vector3(0.3f, 0.3f, 0.3f);
    public float m_explosionTime;
    public Vector3 m_leadInRumble = new Vector3(0.3f, 0f, 2f);
    public Vector3 m_explosionRumble = new Vector3(0.3f, 0.9f, 0.1f);
    public Vector3 m_lowRumble = new Vector3(0.3f, 0, 5f);
    public MeshRenderer m_sigilMesh;
    Material m_sigil;
    public float m_sigilSpeed;
    float m_meshActivationTime;
    public float m_talismanFadeWhite = 1f;
    public float m_talismanFadeBlack = 1f;
    public float m_talismanFadeClear = 1f;
    public float m_moveToCinematicSpeed = 1f;
    public float m_rotateToCinematicSpeed = 1f;

    public Collider m_combatTutorial;
    [HideInInspector]
    public bool m_firstEnemy;

    [HideInInspector]
    public Transform m_respawnPoint;
    [HideInInspector]
    public Transform m_initialSpawn;
    public float m_respawnHealth;
    public float m_respawnMana;
    
    public List<IBeing> m_activeBeings = new();

    public static GameManager Instance
    {
        get
        {
            m_instance = FindObjectOfType<GameManager>();
            if (m_instance == null)
            {
                Debug.LogError("Player Manager is Null!!");
            }
            return m_instance;
        }
    }

    private void Awake()
    {
        m_aiManager = new AIManager();
        OnGameStateChanged += GameStateChanged;
        m_firstEnemy = true;
    }

    private void Start()
    {
        m_sigil = m_sigilMesh.material;
        UpdateGameState(GameState.MENU);
    }

    void GameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MENU:
                MainMenu();
                break;
            case GameState.GAME:
                ResumeGame();
                break;
            case GameState.CINEMATIC:
                break;
            case GameState.PAUSE:
                PauseGame();
                break;
            case GameState.DEATH:
                DeathMenuStart();
                break;
            default:
                break;
        }
    }

    public void UpdateGameState(GameState newState)
    {
        if (newState == m_gameState)
            return;
        m_lastState = m_gameState;
        m_gameState = newState;
        switch (newState)
        {
            case GameState.CONTROLS:
                break;
            case GameState.MENU:
                break;
            case GameState.GAME:
                break;
            case GameState.CINEMATIC:
                break;
            case GameState.PAUSE:
                break;
            case GameState.OPTIONS:
                break;
            case GameState.CREDITS:
                break;
            case GameState.DEATH:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnGameStateChanged?.Invoke(m_gameState);
    }

    void MainMenu()
    {
        Time.timeScale = 0;
    }

    void StartGame()
    {
        Time.timeScale = 1;
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
    }

    void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void SetCheckPoint(Transform transform)
    {
        m_respawnPoint = transform;
    }

    public async UniTask FirstCinematic()
    {
        UpdateGameState(GameState.CINEMATIC);
        m_player.m_stopUpdate = true;
        bool rot = false;
        bool pos = false;
        while (!rot || !pos)
        {
            float moveStep = m_moveToCinematicSpeed * Time.deltaTime;
            float rotateStep = m_rotateToCinematicSpeed * Time.deltaTime;
            if (m_player.gameObject.transform.position != m_cinematicPoints[0].position)
            {
                Vector3 nextPos = Vector3.MoveTowards(m_player.gameObject.transform.position, m_cinematicPoints[0].position, moveStep);
                m_player.gameObject.transform.position = nextPos;
            }
            else
            {
                pos = true;
            }
            if (m_player.gameObject.transform.rotation.eulerAngles != m_cinematicPoints[0].rotation.eulerAngles)
            {
                Quaternion nextRot = Quaternion.RotateTowards(m_player.gameObject.transform.rotation, m_cinematicPoints[0].rotation, rotateStep);
                m_player.m_camera.SetRotation(nextRot.eulerAngles);
            }
            else
            {
                rot = true;
            }
            await UniTask.Yield();
        }
        TurnOnPlayer().Forget();
        m_player.m_animator.SetTrigger("TalismanCinematic");
        m_audioManager.PlayCinematic().Forget();
    }

    async UniTask TurnOnPlayer()
    {
        m_meshActivationTime = Time.time;
        while (Time.time <= m_meshActivationTime + 0.6f)
        {
            await UniTask.Yield();
        }
        m_player.m_skinnedMeshRenderer.enabled = true;
    }
    public ParticleSystem m_swordPodium;
    public async UniTask SecondCinematic()
    {
        m_swordPodium.Play();
        m_player.m_stopUpdate = false;
        m_menuManager.m_fadeWhite.gameObject.SetActive(true);
        Color w;
        Color f;
        Color b = new Color(0, 0, 0, 1);
        while (m_menuManager.m_fadeWhite.color.a <= 1)
        {
            w = new Color(1, 1, 1, m_menuManager.m_fadeWhite.color.a + Time.deltaTime * m_talismanFadeWhite);
            m_menuManager.m_fadeWhite.color = w;
            await UniTask.Yield();
        }
        w = new Color(1, 1, 1, 1);
        m_menuManager.m_fadeWhite.color = w;
        m_menuManager.m_fadeBlack.gameObject.SetActive(true);
        m_menuManager.m_fadeBlack.color = b;

        m_respawnPoint = m_cinematicPoints[1];
        m_player.transform.position = m_cinematicPoints[1].position;
        m_player.m_camera.SetRotation(m_cinematicPoints[1].rotation.eulerAngles);
        m_player.Rumble(m_teleportRumble).Forget();

        while (m_menuManager.m_fadeWhite.color.a >= 0)
        {
            f = new Color(1, 1, 1, m_menuManager.m_fadeWhite.color.a - Time.deltaTime * m_talismanFadeBlack);
            m_menuManager.m_fadeWhite.color = f;
            await UniTask.Yield();
        }
        f = new Color(1, 1, 1, 0);
        m_menuManager.m_fadeWhite.color = f;

        m_player.m_animator.SetTrigger("SwordCinematic");
        m_audioManager.PlayCinematic().Forget();


        while (m_menuManager.m_fadeBlack.color.a >= 0)
        {
            b = new Color(1, 1, 1, m_menuManager.m_fadeBlack.color.a - Time.deltaTime * m_talismanFadeClear);
            m_menuManager.m_fadeBlack.color = b;
            await UniTask.Yield();
        }
        b = new Color();
        m_menuManager.m_fadeWhite.color = b;
        m_menuManager.m_fadeWhite.gameObject.SetActive(false);
        m_menuManager.m_fadeBlack.gameObject.SetActive(false);
        FadeSigil().Forget();
        m_sigilParticles.Play();
    }

    async UniTask FadeSigil()
    {
        while (m_sigil.GetColor("_ColourAlpha") != Color.black)
        {
            float step = Time.deltaTime * m_sigilSpeed;
            m_sigil.SetColor("_ColourAlpha", Color.Lerp(m_sigil.GetColor("_ColourAlpha"), Color.black, step));
            await UniTask.Yield();
        }
    }

    public async UniTask LastCinematic()
    {
        UpdateGameState(GameState.CINEMATIC);
        m_player.m_stopUpdate = true;
        EndRumbles().Forget();
        bool rot = false;
        bool pos = false;
        while (!rot || !pos)
        {
            float moveStep = m_moveToCinematicSpeed * Time.deltaTime;
            float rotateStep = m_rotateToCinematicSpeed * Time.deltaTime;
            if (m_player.gameObject.transform.position != m_cinematicPoints[2].position)
            {
                Vector3 nextPos = Vector3.MoveTowards(m_player.gameObject.transform.position, m_cinematicPoints[2].position, moveStep);
                m_player.gameObject.transform.position = nextPos;
            }
            else
            {
                pos = true;
            }
            if (m_player.gameObject.transform.rotation.eulerAngles != m_cinematicPoints[2].rotation.eulerAngles)
            {
                Quaternion nextRot = Quaternion.RotateTowards(m_player.gameObject.transform.rotation, m_cinematicPoints[2].rotation, rotateStep);
                m_player.m_camera.SetRotation(nextRot.eulerAngles);
            }
            else
            {
                rot = true;
            }
            await UniTask.Yield();
        }
        m_player.m_animator.SetTrigger("AltarCinematic");
        m_player.m_overlayCamera.enabled = false;
        m_audioManager.PlayCinematic().Forget();
    }

    async UniTask EndRumbles()
    {
        float time = Time.time;
        while (Time.time < time + m_explosionTime)
        {
            await UniTask.Yield();
        }
        m_player.Rumble(m_leadInRumble).Forget();
        time = Time.time;
        while (Time.time < time + m_leadInRumble.z)
        {
            await UniTask.Yield();
        }
        m_player.Rumble(m_explosionRumble).Forget();
        time = Time.time;
        while (Time.time < time + m_explosionRumble.z)
        {
            await UniTask.Yield();
        }
        m_player.Rumble(m_lowRumble).Forget();
    }


    public bool m_isEnd = false;
    public void EndGame()
    {
        m_isEnd = true;
        m_player.m_stopUpdate = false;        
        UpdateGameState(GameState.DEATH);
    }

    void DeathMenuStart()
    {
        if (!m_isEnd)
        {
            m_player.m_animator.SetTrigger("Die");           
            m_audioManager.PlayDeathDialogue();
            if (m_firstEnemy)
            {
                m_combatTutorial.gameObject.SetActive(false);
            }
        }
        m_menuManager.FadeDeathScreen(m_isEnd).Forget();
    }

    public void Respawn()
    {
        UpdateGameState(GameState.GAME);
        m_player.m_animator.SetTrigger("Alive");
        m_player.transform.position = m_respawnPoint.position;
        m_player.m_camera.SetRotation(m_respawnPoint.rotation.eulerAngles);
        m_player.m_currentHealth = m_player.m_health;
        m_player.m_currentMana = m_player.m_startMana;
        if (m_firstEnemy)
        {
            m_combatTutorial.gameObject.SetActive(true);
        }
        m_menuManager.UpdateHealth();
        m_menuManager.UpdateMana();
        m_audioManager.StopCombatMusic();
        m_aiManager.ResetEnemies();
    }

    public void ActivateEnemy(IBeing being)
    {
        m_activeBeings.Add(being);        
        if(m_activeBeings.Count == 1)
        {
            m_audioManager.StartCombatMusic();
        }
    }

    public void DeactivateEnemy(IBeing being)
    {
        m_activeBeings.Remove(being);        
        if (m_activeBeings.Count == 0)
        {
            m_audioManager.StopCombatMusic();
        }
    }
}

public enum GameState
{
    CONTROLS,
    MENU,
    GAME,
    CINEMATIC,
    PAUSE,
    OPTIONS,
    CREDITS,
    DEATH,
}