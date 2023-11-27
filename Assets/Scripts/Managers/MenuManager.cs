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
using AISystem.Systems;

public class MenuManager : MonoBehaviour
{
    public event Action<ControllerType> OnControllerChanged;
    public ControllerType m_currentController;
    public InputDevice m_currentDevice = null;

    public ControllerImages m_keyboardImages;
    public ControllerImages m_xBoxImages;
    public ControllerImages m_pSImages;
    public ControllerImages m_nintendoImages;
    public ControllerImages m_genericImages;
    ControllerImages m_currentImages;
    bool m_tutorialSpriteFirst = false;
    List<string> m_currentTutorialStrings = new List<string>();
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
    public ParticleSystem m_menuParticles;
    public Camera m_menuCamera;
    public List<Image> m_images;
    public List<TextMeshProUGUI> m_labels;
    public float m_startTimeDown = 2f;
    #endregion

    #region HUD Fields
    [Header("HUD"), Space(5)]
    public TextMeshProUGUI m_subtitlesBackground;
    public TextMeshProUGUI m_subtitles;
    public Color m_playerColour = Color.blue;
    public Color m_swordColour = Color.red;
    public Slider m_health;
    public Slider m_mana;
    public GameObject m_tutorialImages;
    public TextMeshProUGUI m_tutorial;
    public TextMeshProUGUI m_interactText;
    [Space(5)]
    public bool m_showSubtitle = true;
    public Image m_reticleHit;
    public float m_reticleHitTime = 0.5f;
    public Image m_damageVignette;
    public float m_damageAlphaMax = 70;
    public float m_damageUpSpeed;
    public float m_damageWaitSpeed;
    public float m_damageDownSpeed;
    public float m_manaUpSpeed;
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
    public Toggle m_vibrationsToggle;
    public Slider m_masterVolumeSlider;
    public Slider m_musicVolumeSlider;
    public Slider m_sfxSlider;
    public Slider m_dialogueSlider;
    public Button m_optionsBack;

    public Button m_resetToDefault;
    public float m_defaultSensitivity = 150;
    public float m_defaultMusicVolume = 0.5f;
    public float m_defaultSFXVolume = 0.5f;
    public float m_defaultDialogueVolume = 0.5f;
    public float m_defaultMasterVolume = 1f;
    public bool m_defaultSubtitle = true;
    public bool m_defaultVibrations = true;
    public bool m_vibrations = true;
    #endregion

    #region Controller Screen Fields
    [Space(5), Header("Controller Screen"), Space(5)]
    public Button m_controlsBackButton;
    public Image m_controlsImage;
    #endregion

    #region Death Fields
    public Image m_deathImage;
    public RawImage m_deathParticles;
    public Image m_endTitle;
    public TextMeshProUGUI m_thanksMessage;
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
        LoadSettings();

        //Main Menu Setup
        m_newGame.onClick.AddListener(delegate () { StartGame().Forget(); });
        m_menuOptions.onClick.AddListener(delegate () { Options(); });
        m_quit.onClick.AddListener(delegate () { QuitGame(); });

        //HUD Setup
        m_health.maxValue = m_player.m_health;
        m_mana.maxValue = m_player.m_maxMana;

        //Cinematic Setup
        m_creditsBack.onClick.AddListener(delegate () { OptionsBack(); });

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
        m_vibrationsToggle.onValueChanged.AddListener(AllowVibrations);
        m_vibrationsToggle.SetIsOnWithoutNotify(m_vibrations);
        m_masterVolumeSlider.onValueChanged.AddListener(m_game.m_audioManager.MasterVolumeLevel);
        m_masterVolumeSlider.SetValueWithoutNotify(m_game.m_audioManager.m_masterVolume);
        m_musicVolumeSlider.onValueChanged.AddListener(m_game.m_audioManager.MusicVolumeLevel);
        m_musicVolumeSlider.SetValueWithoutNotify(m_game.m_audioManager.m_musicVolume);
        m_sfxSlider.onValueChanged.AddListener(m_game.m_audioManager.SFXVolumeLevel);
        m_sfxSlider.SetValueWithoutNotify(m_game.m_audioManager.m_sFXVolume);
        m_dialogueSlider.onValueChanged.AddListener(m_game.m_audioManager.DialogueVolumeLevel);
        m_dialogueSlider.SetValueWithoutNotify(m_game.m_audioManager.m_dialogueVolume);
        m_controlsMain.onClick.AddListener(delegate () { ControlScreen(); });
        m_credits.onClick.AddListener(delegate () { Credits(); });
        m_optionsBack.onClick.AddListener(delegate () { OptionsBack(); });
        m_resetToDefault.onClick.AddListener(delegate () { ResetToDefaults(); });

        //Controls Screen Setup
        m_controlsBackButton.onClick.AddListener(delegate () { OptionsBack(); });

        //Death Setup
        m_respawnButton.onClick.AddListener(delegate () { Respawn(); });
        m_respawnButton.gameObject.SetActive(false);
        m_deathQuit.onClick.AddListener(delegate () { QuitGame(); });
        m_deathQuit.gameObject.SetActive(false);
    }
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("showSubtitle"))
        {
            m_showSubtitle = PlayerPrefs.GetInt("showSubtitle") == 1;
        }
        else
        {
            m_showSubtitle = true;
        }
        if (PlayerPrefs.HasKey("vibrations"))
        {
            m_vibrations = PlayerPrefs.GetInt("vibrations") == 1;
        }
        else
        {
            m_vibrations = true;
        }
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("showSubtitle", m_showSubtitle ? 1 : 0);
        PlayerPrefs.SetInt("vibrations", m_vibrations ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        SaveSettings();
    }

    public void InputDeviceChanged(InputEventPtr eventPtr, InputDevice device)
    {
        if (m_currentDevice == device)
        {
            return;
        }

        if (eventPtr.type != StateEvent.Type)
        {
            return;
        }
        bool validPress = false;
        foreach (InputControl control in eventPtr.EnumerateChangedControls(device, 0.01F))
        {
            validPress = true;
            break;
        }
        if (validPress is false) return;

        if (device is Keyboard || device is Mouse)
        {
            m_currentDevice = device;
            if (m_currentController == ControllerType.KEYBOARD) return;
            OnControllerChanged?.Invoke(ControllerType.KEYBOARD);
        }
        if (device is XInputController)
        {
            m_currentDevice = device;
            OnControllerChanged?.Invoke(ControllerType.XBOX);
        }
        else if (device is DualShockGamepad)
        {
            m_currentDevice = device;
            OnControllerChanged?.Invoke(ControllerType.PS);
        }
        else if (device is SwitchProControllerHID)
        {
            m_currentDevice = device;
            OnControllerChanged?.Invoke(ControllerType.NINTENDO);
        }
        else if (device is Gamepad)
        {
            m_currentDevice = device;
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
        if (m_tutorialImages.activeSelf)
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
        if (state == GameState.MENU || state == GameState.DEATH)
        {
            m_menuCamera.gameObject.SetActive(true);
            m_menuParticles.Play();
        }
        else
        {
            m_menuCamera.gameObject.SetActive(false);
            m_menuParticles.Stop();
        }
        m_hud.SetActive(state == GameState.GAME);
        m_cinematics.SetActive(state == GameState.CINEMATIC);
        m_pauseMenu.SetActive(state == GameState.PAUSE);
        m_optionsMenu.SetActive(state == GameState.OPTIONS);
        m_controls.SetActive(state == GameState.CONTROLS);
        m_deathScreen.SetActive(state == GameState.DEATH);
        m_creditsScreen.SetActive(state == GameState.CREDITS);
        if (state == GameState.CINEMATIC || state == GameState.GAME)
        {
            m_subtitles.gameObject.SetActive(m_showSubtitle);
            m_subtitlesBackground.gameObject.SetActive(m_showSubtitle);
        }
        else
        {
            m_subtitles.gameObject.SetActive(false);
            m_subtitlesBackground.gameObject.SetActive(false);
        }
    }
    void TitleScreen()
    {
        m_game.m_audioManager.EndFmodLoop(m_game.m_audioManager.m_menuMusicInstance);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    bool first = true;
    void MainMenu()
    {
        m_game.UpdateGameState(GameState.MENU);
        if (first)
        {
            first = false;
            m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);
        }
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_newGame.gameObject);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
    async UniTask StartGame()
    {        
        Color background = Color.white;
        float alpha = 1f;
        while(alpha > 0f) 
        {
            float step = Time.unscaledDeltaTime * m_startTimeDown;
            alpha -= step;
            background.a = alpha;
            foreach(Image i in m_images)
            {
                i.color = background;
            }
            foreach(TextMeshProUGUI text in m_labels)
            {
                text.color = background;
            }
            Time.timeScale = 1 - alpha;
            await UniTask.Yield();
        }
        alpha = 0;
        background.a = alpha;
        foreach (Image i in m_images)
        {
            i.color = background;
        }
        foreach (TextMeshProUGUI text in m_labels)
        {
            text.color = background;
        }
        m_subtitles.gameObject.SetActive(m_showSubtitle);
        m_subtitlesBackground.gameObject.SetActive(m_showSubtitle);
        m_game.m_audioManager.PlayIntroDialogue();
        m_game.UpdateGameState(GameState.GAME);
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Resume()
    {
        m_game.m_audioManager.OnMenuSelect();
        m_game.UpdateGameState(GameState.GAME);
        m_game.m_audioManager.m_dialogueInstance.setPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    void QuitGame()
    {
        m_game.m_audioManager.OnMenuSelect();
        m_game.m_audioManager.EndFmodLoop(m_game.m_audioManager.m_menuMusicInstance);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void Options()
    {
        m_game.m_audioManager.OnMenuSelect();
        m_game.UpdateGameState(GameState.OPTIONS);
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_camSensitivitySlider.gameObject);
        }
    }

    void OptionsBack()
    {
        if (m_game.m_lastState == GameState.PAUSE)
        {
            Pause();
        }
        else if (m_game.m_lastState == GameState.MENU)
        {
            MainMenu();
        }
        m_game.m_audioManager.OnMenuBack();
    }

    void ResetToDefaults()
    {
        m_camSensitivitySlider.value = m_defaultSensitivity;
        m_player.ChangeSensitivity(m_defaultSensitivity);

        m_subtitlesToggle.SetIsOnWithoutNotify(m_defaultSubtitle);
        m_showSubtitle = m_defaultSubtitle;
        
        m_vibrationsToggle.SetIsOnWithoutNotify(m_defaultVibrations);
        m_vibrations = m_defaultVibrations;

        m_masterVolumeSlider.value = m_defaultMasterVolume;
        m_game.m_audioManager.MasterVolumeLevel(m_defaultMasterVolume);

        m_musicVolumeSlider.value = m_defaultMusicVolume;
        m_game.m_audioManager.MusicVolumeLevel(m_defaultMusicVolume);

        m_sfxSlider.value = m_defaultSFXVolume;
        m_game.m_audioManager.SFXVolumeLevel(m_defaultSFXVolume);

        m_dialogueSlider.value = m_defaultDialogueVolume;
        m_game.m_audioManager.DialogueVolumeLevel(m_defaultDialogueVolume);
    }

    void ControlScreen()
    {
        m_game.m_audioManager.OnMenuSelect();
        m_game.UpdateGameState(GameState.CONTROLS);
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_controlsBackButton.gameObject);
        }
        m_controlsImage.sprite = m_currentImages.m_controlsDisplay;
    }


    void Pause()
    {
        m_game.UpdateGameState(GameState.PAUSE);
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_resume.gameObject);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        m_game.m_audioManager.m_dialogueInstance.setPaused(true);
    }
    public void Cancel(InputAction.CallbackContext obj)
    {
        switch (m_game.m_gameState)
        {
            case GameState.OPTIONS:
            case GameState.CREDITS:
            case GameState.CONTROLS:
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
        m_game.m_audioManager.OnMenuSelect();
        m_game.UpdateGameState(GameState.CREDITS);
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_creditsBack.gameObject);
        }
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
        m_tutorialImages.SetActive(true);
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
        m_tutorialImages.SetActive(false);
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

    public async UniTask LerpMana()
    {
        float mana = m_mana.value;
        while (mana < m_player.m_currentMana)
        {
            mana += Time.deltaTime * m_manaUpSpeed;
            m_mana.value = mana;
            await UniTask.Yield();
        }
        m_mana.value = m_player.m_currentMana;
    }

    public async UniTask FadeDeathScreen(bool isEnd)
    {
        Color colourA = m_deathImage.color;
        Color colourB = m_deathParticles.color;
        Color colourC = m_endTitle.color;
        Color colourD = m_thanksMessage.color;
        float alpha = 0;
        colourA.a = alpha;
        colourB.a = alpha;
        colourC.a = alpha;
        colourD.a = alpha;
        m_deathImage.color = colourA;
        m_deathParticles.color = colourB;
        m_endTitle.color = colourC;
        m_thanksMessage.color = colourD;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * (isEnd ? m_endFade : m_deathFade);
            colourA.a = alpha;
            colourB.a = alpha;
            colourC.a = alpha;
            colourD.a = alpha;
            m_deathImage.color = colourA;
            m_deathParticles.color = colourB;
            if (isEnd)
            {
                m_endTitle.color = colourC;
                m_thanksMessage.color = colourD;
            }
            await UniTask.Yield();
        }
        m_game.m_aiManager.ResetEnemies();
        m_game.m_activeBeings.Clear();
        colourA.a = 1;
        colourB.a = 1;
        colourC.a = 1;
        colourD.a = 1;
        m_deathImage.color = colourA;
        m_deathParticles.color = colourB;
        if (isEnd)
        {
            m_endTitle.color = colourC;
            m_thanksMessage.color = colourD;
        }
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
        if (!(m_currentDevice is Keyboard || m_currentDevice is Mouse) && m_currentDevice != null)
        {
            m_eventSystem.SetSelectedGameObject(m_respawnButton.gameObject);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }


    void Respawn()
    {
        m_game.m_audioManager.OnMenuSelect();
        m_respawnButton.gameObject.SetActive(false);
        m_deathQuit.gameObject.SetActive(false);
        Color colourA = m_deathImage.color;
        Color colourB = m_deathParticles.color;
        Color colourC = m_endTitle.color;
        Color colourD = m_thanksMessage.color;
        float alpha = 0;
        colourA.a = alpha;
        colourB.a = alpha;
        colourC.a = alpha;
        colourD.a = alpha;
        m_deathImage.color = colourA;
        m_deathParticles.color = colourB;
        m_endTitle.color = colourC;
        m_thanksMessage.color = colourD;
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

    void AllowVibrations(bool vibes)
    {
        m_vibrations = vibes;
    }

    void ShowSubtitles(bool show)
    {
        m_showSubtitle = show;
    }

    public void SetSubtitle(string subtitile, bool isPlayer)
    {
        m_subtitles.gameObject.SetActive(m_showSubtitle);
        m_subtitlesBackground.gameObject.SetActive(m_showSubtitle);
        if (isPlayer)
        {
            m_subtitles.color = m_playerColour;
        }
        else
        {
            m_subtitles.color = m_swordColour;
        }
        m_subtitles.text = "<mark=#00000000 padding=\"2,2,15,12\">" + subtitile + "</mark>";
        m_subtitlesBackground.text = "<mark=#000000C8 padding=\"2,2,15,12\">" + subtitile + "</mark>";
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