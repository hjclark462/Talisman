using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using System.Threading;

public class MenuManager : MonoBehaviour
{
    public event Action<ControllerType> OnControllerChanged;
    public ControllerType m_currentController;
    InputDevice m_lastDevice = null;

    public ControllerImages m_keyboardImages;
    public ControllerImages m_xBoxImages;
    public ControllerImages m_pSImages;
    public ControllerImages m_nintendoImages;
    public ControllerImages m_genericImages;
    ControllerImages m_currentImages;
    bool m_tutorialSpriteFirst = false;
    List<string> m_currentTutorialStrings = new List<String>();
    List<ControlSprites> m_currentTutorialSprites = new List<ControlSprites>();
    bool m_interactSpriteFirst = false;
    List<string> m_currentInteractStrings = new List<string>();
    List<ControlSprites> m_currentInteractSprites = new List<ControlSprites>();

    #region UI Category Objects
    public GameObject m_mainMenu;
    public GameObject m_hud;
    public GameObject m_cinematics;
    public GameObject m_pauseMenu;
    public GameObject m_optionsMenu;
    public GameObject m_controls;
    public GameObject m_deathScreen;
    public GameObject m_creditsScreen;
    #endregion

    #region Main Menu Fields
    [Header("Main Menu"), Space(5)]
    public Button m_newGame;
    public Button m_menuOptions;
    public Button m_credits;
    public Button m_controlsMain;
    public Button m_quit;
    #endregion

    #region HUD Fields
    [Header("HUD"), Space(5)]
    public TextMeshProUGUI m_subtitles;
    public Color m_playerColour = Color.blue;
    public Color m_swordColour = Color.red;
    public Slider m_health;
    public Slider m_mana;
    public TextMeshProUGUI m_tutorial;
    public Image m_interactImage;
    public TextMeshProUGUI m_interactText;
    [Space(5)]
    public float m_subtitleTime = 2;
    bool m_showSubtitle = true;
    public Image m_reticleHit;
    public float m_reticleHitTime = 0.5f;
    public Image m_damageVignette;
    public float m_damageAlphaMax = 70;
    public float m_damageUpSpeed;
    public float m_damageWaitSpeed;
    public float m_damageDownSpeed;
    #endregion

    #region Cinematic Fields
    [Header("Cinematics"), Space(5)]
    public Image m_topBlackBar;
    public Image m_bottomBlackBar;
    public Image m_fadeBlack;
    public Image m_fadeWhite;
    #endregion

    #region Pause Menu Fields
    [Header("Pause Menu"), Space(5)]
    public Button m_resume;
    public Button m_pauseOptions;
    public Button m_controllerImage;
    public Button m_quitMenu;
    #endregion

    #region Option Fields
    [Header("Options"), Space(5)]
    public Slider m_camSensitivitySlider;
    public Toggle m_subtitlesToggle;
    public Slider m_masterVolumeSlider;
    public Slider m_musicVolumeSlider;
    public Slider m_sfxSlider;
    public Slider m_dialogueSlider;
    public Button m_optionsBack;
    #endregion

    #region Controller Screen Fields
    [Space(5), Header("Controller Screen"), Space(5)]
    public Button m_controlsBackButton;
    public Image m_controlsImage;
    #endregion

    #region Death Fields
    public Image m_deathImage;
    public Button m_respawnButton;
    public Button m_deathQuit;
    public float m_deathFade = 0.33f;
    public float m_endFade = 0.33f;
    #endregion

    //Credits
    public Button m_creditsBack;

    GameManager m_game;
    public PlayerController m_player;
    public EventSystem m_eventSystem;
    
    private void Start()
    {
        m_game = GameManager.Instance;
        m_eventSystem = FindObjectOfType<EventSystem>();

        InputSystem.onEvent += InputDeviceChanged;
        OnControllerChanged += SwapControls;
        m_currentImages = m_keyboardImages;

        m_game.OnGameStateChanged += OnGameStateChanged;

        //Main Menu Setup
        m_newGame.onClick.AddListener(delegate () { StartGame(); });
        m_menuOptions.onClick.AddListener(delegate () { Options(); });
        m_quit.onClick.AddListener(delegate () { QuitGame(); });

        //HUD Setup
        m_health.maxValue = m_player.m_health;
        m_mana.maxValue = m_player.m_maxMana;

        //Cinematic Setup
        m_creditsBack.onClick.AddListener(delegate () { Options(); });

        //Pause Menu Setup
        m_resume.onClick.AddListener(delegate () { Resume(); });
        m_pauseOptions.onClick.AddListener(delegate () { Options(); });
        m_controllerImage.onClick.AddListener(delegate () { ControlScreen(); });
        m_quitMenu.onClick.AddListener(delegate () { TitleScreen(); });

        //Options Setup        
        m_camSensitivitySlider.onValueChanged.AddListener(m_player.ChangeSensitivity);
        m_camSensitivitySlider.SetValueWithoutNotify(m_player.m_cameraSensitivity);
        m_subtitlesToggle.onValueChanged.AddListener(ShowSubtitles);
        m_subtitlesToggle.SetIsOnWithoutNotify(m_showSubtitle);
        m_masterVolumeSlider.onValueChanged.AddListener(m_game.m_audioManager.MasterVolumeLevel);
        m_masterVolumeSlider.SetValueWithoutNotify(1);
        m_musicVolumeSlider.onValueChanged.AddListener(m_game.m_audioManager.MusicVolumeLevel);
        m_musicVolumeSlider.SetValueWithoutNotify(0.5f);
        m_sfxSlider.onValueChanged.AddListener(m_game.m_audioManager.SFXVolumeLevel);
        m_sfxSlider.SetValueWithoutNotify(0.5f);
        m_dialogueSlider.onValueChanged.AddListener(m_game.m_audioManager.DialogueVolumeLevel);
        m_dialogueSlider.SetValueWithoutNotify(0.5f);
        m_controlsMain.onClick.AddListener(delegate () { ControlScreen(); });
        m_credits.onClick.AddListener(delegate () { Credits(); });
        m_optionsBack.onClick.AddListener(delegate () { OptionsBack(); });

        //Controls Screen Setup
        m_controlsBackButton.onClick.AddListener(delegate () { Options(); });

        //Death Setup
        m_respawnButton.onClick.AddListener(delegate () { Respawn(); });
        m_respawnButton.gameObject.SetActive(false);
        m_deathQuit.onClick.AddListener(delegate () { QuitGame(); });
        m_deathQuit.gameObject.SetActive(false);
    }

    public void InputDeviceChanged(InputEventPtr eventPtr, InputDevice device)
    {
        if (m_lastDevice == device) return;

        if (eventPtr.type != StateEvent.Type) return;

        bool validPress = false;
        foreach (InputControl control in eventPtr.EnumerateChangedControls(device, 0.01F))
        {
            validPress = true;
            break;
        }
        if (validPress is false) return;

        if (device is Keyboard || device is Mouse)
        {
            if (m_currentController == ControllerType.KEYBOARD) return;
            OnControllerChanged?.Invoke(ControllerType.KEYBOARD);
        }
        if (device is XInputController)
        {
            OnControllerChanged?.Invoke(ControllerType.XBOX);
        }
        else if (device is DualShockGamepad)
        {
            OnControllerChanged?.Invoke(ControllerType.PS);
        }
        else if (device is SwitchProControllerHID)
        {
            OnControllerChanged?.Invoke(ControllerType.NINTENDO);
        }
        else if (device is Gamepad)
        {
            OnControllerChanged?.Invoke(ControllerType.GENERIC);
        }
    }

    void SwapControls(ControllerType controls)
    {
        m_currentController = controls;
        switch (controls)
        {
            case ControllerType.KEYBOARD:
                UpdateUIImages(m_keyboardImages);
                break;
            case ControllerType.XBOX:
                UpdateUIImages(m_xBoxImages);
                break;
            case ControllerType.PS:
                UpdateUIImages(m_pSImages);
                break;
            case ControllerType.NINTENDO:
                UpdateUIImages(m_nintendoImages);
                break;
            case ControllerType.GENERIC:
                UpdateUIImages(m_genericImages);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(controls), controls, null);
        }
    }

    void UpdateUIImages(ControllerImages ci)
    {
        m_currentImages = ci;
        if (m_tutorial.isActiveAndEnabled)
        {
            SetTutorial(m_currentTutorialStrings, m_currentTutorialSprites, m_tutorialSpriteFirst);
        }
        if (m_interactText.enabled)
        {
            SetInteractMessage(m_currentInteractStrings, m_currentInteractSprites, m_interactSpriteFirst);
        }
        m_controlsImage.sprite = m_currentImages.m_controlsDisplay;
    }

    void OnGameStateChanged(GameState state)
    {
        UpdateUI(state);
        switch (state)
        {
            case GameState.CONTROLS:
                ControlScreen();
                break;
            case GameState.MENU:
                MainMenu();
                break;
            case GameState.GAME:
                if (m_game.m_lastState == GameState.PAUSE || m_game.m_lastState == GameState.DEATH)
                {
                    Resume();
                }
                break;
            case GameState.PAUSE:
                Pause();
                break;
            case GameState.CINEMATIC:
                break;
            case GameState.CREDITS:
                break;
        }
    }

    void UpdateUI(GameState state)
    {
        m_mainMenu.SetActive(state == GameState.MENU);
        m_hud.SetActive(state == GameState.GAME);
        m_cinematics.SetActive(state == GameState.CINEMATIC);
        m_pauseMenu.SetActive(state == GameState.PAUSE);
        m_optionsMenu.SetActive(state == GameState.OPTIONS);
        m_controls.SetActive(state == GameState.CONTROLS);
        m_deathScreen.SetActive(state == GameState.DEATH);
        m_creditsScreen.SetActive(state == GameState.CREDITS);
        if (state == GameState.CINEMATIC || state == GameState.GAME)
        {
            m_subtitles.gameObject.SetActive(true);
        }
        else
        {
            m_subtitles.gameObject.SetActive(false);
        }
    }
    void TitleScreen()
    {
        m_game.m_audioManager.EndFmodLoop(m_game.m_audioManager.m_menuMusicInstance);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void MainMenu()
    {
        m_game.UpdateGameState(GameState.MENU);
        m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);
        Cursor.lockState = CursorLockMode.Confined;
    }
    void StartGame()
    {
        m_game.m_audioManager.PlayIntroDialogue();
        m_game.UpdateGameState(GameState.GAME);
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Resume()
    {
        m_game.UpdateGameState(GameState.GAME);
        m_game.m_audioManager.m_dialogueInstance.setPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    void QuitGame()
    {
        m_game.m_audioManager.EndFmodLoop(m_game.m_audioManager.m_menuMusicInstance);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void Options()
    {
        if (m_game.m_gameState == GameState.PAUSE)
        {
            m_game.m_controlsLastState = GameState.PAUSE;
        }
        else if (m_game.m_gameState == GameState.MENU)
        {
            m_game.m_controlsLastState = GameState.MENU;
        }
        m_game.UpdateGameState(GameState.OPTIONS);
        m_eventSystem.SetSelectedGameObject(m_camSensitivitySlider.gameObject);
    }

    void OptionsBack()
    {
        if (m_game.m_controlsLastState == GameState.PAUSE)
        {
            Pause();
        }
        else if (m_game.m_controlsLastState == GameState.MENU)
        {
            MainMenu();
        }
    }

    void ControlScreen()
    {
        m_game.UpdateGameState(GameState.CONTROLS);
        m_eventSystem.SetSelectedGameObject(m_controlsBackButton.gameObject);
        m_controlsImage.sprite = m_currentImages.m_controlsDisplay;
    }


    void Pause()
    {
        m_game.UpdateGameState(GameState.PAUSE);
        m_eventSystem.SetSelectedGameObject(m_resume.gameObject);
        m_game.m_audioManager.m_dialogueInstance.setPaused(true);
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void Cancel(InputAction.CallbackContext obj)
    {
        switch (m_game.m_gameState)
        {
            case GameState.OPTIONS:
                if (m_game.m_lastState == GameState.PAUSE)
                {
                    Pause();
                }
                else if (m_game.m_lastState == GameState.MENU)
                {
                    MainMenu();
                }
                break;
            case GameState.PAUSE:
                Resume();
                break;
            default:
                break;
        }
    }

    void Credits()
    {
        m_game.UpdateGameState(GameState.CREDITS);
        m_eventSystem.SetSelectedGameObject(m_creditsBack.gameObject);
    }

    public void SetTutorial(List<string> text, List<ControlSprites> sprites, bool spriteFirst)
    {
        if (text.Count == 0 && sprites.Count == 0)
        {
            return;
        }
        m_currentTutorialStrings = text;
        m_currentTutorialSprites = sprites;
        m_tutorialSpriteFirst = spriteFirst;
        m_tutorial.gameObject.SetActive(true);
        string message = "";
        int index = 0;
        if (spriteFirst)
        {
            while (index < sprites.Count)
            {
                message += SpriteToString(sprites[index]);
                if (index < text.Count)
                {
                    message += text[index];
                }
                index++;
            }
        }
        else
        {
            while (index < text.Count)
            {
                message += text[index];
                if (index < sprites.Count)
                {
                    message += SpriteToString(sprites[index]);
                }
                index++;
            }
        }
        m_tutorial.text = message;
    }
    public void SetInteractMessage(List<string> text, List<ControlSprites> sprites, bool spriteFirst)
    {
        if (text.Count == 0 && sprites.Count == 0)
        {
            return;
        }
        m_currentInteractStrings = text;
        m_currentInteractSprites = sprites;
        m_interactSpriteFirst = spriteFirst;
        string message = "";
        int index = 0;
        if (spriteFirst)
        {
            while (index < sprites.Count)
            {
                message += SpriteToString(sprites[index]);
                if (index < text.Count)
                {
                    message += text[index];
                }
                index++;
            }
        }
        else
        {
            while (index < text.Count)
            {
                message += text[index];
                if (index < sprites.Count)
                {
                    message += SpriteToString(sprites[index]);
                }
                index++;
            }
        }
        m_interactText.enabled = true;
        m_interactText.text = message;
    }

    string SpriteToString(ControlSprites cs)
    {
        string sprite = "";
        switch (cs)
        {
            case ControlSprites.MENU_NAV:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_menuNavigation + "> ";
                break;
            case ControlSprites.MENU_SELECT:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_menuSelect + "> ";
                break;
            case ControlSprites.MENU_BACK:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_menuBack + "> ";
                break;
            case ControlSprites.MOVEMENT:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_movement + "> ";
                break;
            case ControlSprites.CAMERA:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_camera + "> ";
                break;
            case ControlSprites.JUMP:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_jump + "> ";
                break;
            case ControlSprites.INTERACT_ONE:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_interactOne + "> ";
                break;
            case ControlSprites.INTERACT_TWO:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_interactTwo + "> ";
                break;
            case ControlSprites.ATTACK:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_attack + "> ";
                break;
            case ControlSprites.BLOCK:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_block + "> ";
                break;
            case ControlSprites.HEAL:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_heal + "> ";
                break;
            case ControlSprites.PAUSE:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_pause + "> ";
                break;
            case ControlSprites.SPRINT:
                sprite = "<sprite=\"SS_" + m_currentImages.m_spriteAsset + "\" index= " + m_currentImages.m_sprint + "> ";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cs), cs, null);
        }
        return sprite;
    }

    public void ClearTutorial()
    {
        m_tutorial.gameObject.SetActive(false);
    }

    public void SetInteract(RaycastHit hit)
    {
        if (m_interactText.enabled)
            return;
        else
        {
            List<string> strings = new List<string>();
            List<ControlSprites> sprites = new List<ControlSprites>();
            bool spriteFirst = false;
            Puzzle puzzle = hit.transform.gameObject.GetComponentInParent<Puzzle>();
            if (puzzle != null)
            {
                spriteFirst = puzzle.m_spritesFirst;
                strings = puzzle.m_interactStrings;
                sprites = puzzle.m_interactSprites;
            }
            Interactable interactable = hit.transform.gameObject.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                spriteFirst = interactable.m_spritesFirst;
                strings = interactable.m_interactStrings;
                sprites = interactable.m_interactSprites;
            }
            ManaPool manaPool = hit.transform.gameObject.GetComponent<ManaPool>();
            if (manaPool != null)
            {
                spriteFirst = manaPool.m_spritesFirst;
                strings = manaPool.m_interactStrings;
                sprites = manaPool.m_interactSprites;
            }
            SetInteractMessage(strings, sprites, spriteFirst);
        }
    }
    public void StopInteract()
    {
        m_interactText.enabled = false;
    }

    public void UpdateHealth()
    {
        m_health.value = m_player.m_currentHealth;
        if (m_health.value <= 0)
        {
            m_game.m_audioManager.PlayOneShot(m_game.m_player.m_deathSound, m_game.m_player.gameObject.transform.position);
            m_game.m_isEnd = false;
            m_game.UpdateGameState(GameState.DEATH);
        }
    }

    public void UpdateMana()
    {
        m_mana.value = m_player.m_currentMana;
    }

    public async UniTask FadeDeathScreen(bool isEnd)
    {
        m_deathImage.color = Color.clear;
        float alpha = 0;
        while (alpha < 1)
        {
            m_deathImage.color = new Color(0, 0, 0, alpha);
            alpha += Time.deltaTime * (isEnd ? m_endFade : m_deathFade);
            await UniTask.Yield();
        }
        m_deathImage.color = Color.black;
        SetDeathScreen();
    }

    void SetDeathScreen()
    {
        m_respawnButton.gameObject.SetActive(true);
        if (m_game.m_isEnd)
        {
            m_respawnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Main Menu";
        }
        else
        {
            m_respawnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Respawn";
        }
        m_deathQuit.gameObject.SetActive(true);
        m_eventSystem.SetSelectedGameObject(m_respawnButton.gameObject);
    }


    void Respawn()
    {
        m_respawnButton.gameObject.SetActive(false);
        m_deathQuit.gameObject.SetActive(false);
        m_deathImage.color = new Color(0, 0, 0, 0);
        if (m_game.m_isEnd)
        {
            m_game.m_isEnd = false;
            m_game.m_audioManager.EndFmodLoop(m_game.m_audioManager.m_menuMusicInstance);
            TitleScreen();
        }
        else
        {
            var zero = m_damageVignette.color;
            zero.a = 0;
            m_damageVignette.color = zero;
            m_game.Respawn();
        }
    }

    void ShowSubtitles(bool show)
    {
        m_showSubtitle = show;
    }

    public bool m_stopSubtitle = false;
    public void SetSubtitle(string subtitile, bool isPlayer)
    {       
        m_subtitles.gameObject.SetActive(m_showSubtitle);
        if (isPlayer) 
        {
            m_subtitles.color = m_playerColour;
        }
        else
        {
            m_subtitles.color = m_swordColour;
        }
        m_subtitles.text = subtitile;        
    }    

    public async UniTask HitReticle()
    {
        float startTime = Time.time;
        m_reticleHit.enabled = true;
        while (Time.time <= startTime + m_reticleHitTime)
        {
            await UniTask.Yield();
        }
        if (m_reticleHit.enabled == true)
        {
            m_reticleHit.enabled = false;
        }
    }
    public async UniTask DamageVignette()
    {
        float alpha = 0;
        while (alpha <= m_damageAlphaMax)
        {
            float upStep = m_damageUpSpeed * Time.deltaTime;
            alpha += upStep;
            var temp = m_damageVignette.color;
            temp.a = alpha;
            m_damageVignette.color = temp;
            await UniTask.Yield();
        }
        float wait = m_damageWaitSpeed;
        while (wait >= 0)
        {
            wait -= Time.deltaTime;
            await UniTask.Yield();
        }
        alpha = m_damageAlphaMax;
        while (alpha > 0)
        {
            float downStep = m_damageDownSpeed * Time.deltaTime;
            alpha -= downStep;
            var temp = m_damageVignette.color;
            temp.a = alpha;
            m_damageVignette.color = temp;
            await UniTask.Yield();
        }
        var zero = m_damageVignette.color;
        zero.a = 0;
        m_damageVignette.color = zero;
    }
}

public enum ControllerType
{
    KEYBOARD,
    PS,
    XBOX,
    NINTENDO,
    GENERIC
}