using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button playButton;
    public Button trainingButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Game UI")]
    public GameObject gameUIPanel;
    public Text timerText;
    public Text roundText;
    public Text scoreText;
    public Image healthBar;
    public Image dashCooldownBar;
    public Image reloadProgressBar;
    public Text ammoText;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Settings")]
    public GameObject settingsPanel;
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public Button backButton;
    
    [Header("Training UI")]
    public GameObject trainingPanel;
    public Text trainingStatusText;
    public Button startTrainingButton;
    public Button stopTrainingButton;
    public Text episodeCountText;
    public Text rewardText;
    
    private GameManager gameManager;
    private bool isInGame = false;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        SetupUI();
        ShowMainMenu();
    }
    
    private void SetupUI()
    {
        // 버튼 이벤트 연결
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (trainingButton != null)
            trainingButton.onClick.AddListener(StartTraining);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ShowMainMenu);
        
        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
        
        if (startTrainingButton != null)
            startTrainingButton.onClick.AddListener(StartMLTraining);
        
        if (stopTrainingButton != null)
            stopTrainingButton.onClick.AddListener(StopMLTraining);
        
        // 볼륨 슬라이더 설정
        SetupVolumeSliders();
    }
    
    private void SetupVolumeSliders()
    {
        if (AudioManager.Instance != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.masterVolume;
                masterVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }
        }
    }
    
    public void ShowMainMenu()
    {
        isInGame = false;
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gameUIPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(trainingPanel, false);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    public void StartGame()
    {
        isInGame = true;
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gameUIPanel, true);
        
        if (gameManager != null)
            gameManager.StartNewRound();
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameStart();
    }
    
    public void StartTraining()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(trainingPanel, true);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    public void ShowSettings()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(settingsPanel, true);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    public void RestartGame()
    {
        if (gameManager != null)
            gameManager.RestartGame();
        
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(gameUIPanel, true);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    public void QuitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ShowGameOver(string message)
    {
        SetPanelActive(gameUIPanel, false);
        SetPanelActive(gameOverPanel, true);
        
        if (gameOverText != null)
            gameOverText.text = message;
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();
    }
    
    private void StartMLTraining()
    {
        // ML-Agents 훈련 시작 로직
        Debug.Log("ML Training Started");
        
        if (trainingStatusText != null)
            trainingStatusText.text = "훈련 중...";
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    private void StopMLTraining()
    {
        // ML-Agents 훈련 중지 로직
        Debug.Log("ML Training Stopped");
        
        if (trainingStatusText != null)
            trainingStatusText.text = "훈련 중지됨";
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    private void Update()
    {
        if (isInGame && gameManager != null)
        {
            UpdateGameUI();
        }
        
        // ESC 키로 메뉴 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInGame)
            {
                ShowMainMenu();
            }
        }
    }
    
    private void UpdateGameUI()
    {
        // 타이머 업데이트
        if (timerText != null)
        {
            float remainingTime = gameManager.GetRemainingTime();
            timerText.text = $"시간: {remainingTime:F1}";
        }
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}