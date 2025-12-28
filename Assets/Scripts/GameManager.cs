using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float roundTime = 60f;
    public int maxRounds = 10;
    
    [Header("UI References")]
    public Text timerText;
    public Text roundText;
    public Text scoreText;
    public GameObject gameOverPanel;
    public Text gameOverText;
    
    [Header("Spawn Points")]
    public Transform[] playerSpawnPoints;
    public Transform[] aiSpawnPoints;

    public GameObject playerWin;
    public GameObject aiWin;
    
    private float currentTime;
    private int currentRound = 1;
    private int playerWins = 0;
    private int aiWins = 0;
    private bool gameActive = false;
    
    private DeadOrReloadAgent[] agents;
    
    private void Start()
    {
        SoundManager.Instance.PlayMusic("BGM");
        playerWin.SetActive(false);
        aiWin.SetActive(false);
        
        // 게임 시작
        agents = FindObjectsByType<DeadOrReloadAgent>(FindObjectsSortMode.None);
        StartNewRound();
    }
    
    private void Update()
    {
        if(!gameActive) return;
       
        if(ScoreUI.Instance.aiScore >= 5)
        {
            EndGameAI();
        }
        if(ScoreUI.Instance.playerScore >= 10)
        {
            EndGamePlayer();
        }
    }
    
    public void StartNewRound()
    {
        currentTime = roundTime;
        gameActive = true;
        
        // // 에이전트들 위치 초기화
        // if (agents.Length >= 2)
        // {
        //     if (playerSpawnPoints.Length > 0)
        //         agents[0].transform.position = playerSpawnPoints[Random.Range(0, playerSpawnPoints.Length)].position;
            
        //     if (aiSpawnPoints.Length > 0)
        //         agents[1].transform.position = aiSpawnPoints[Random.Range(0, aiSpawnPoints.Length)].position;
        // }
        
        // 에이전트 에피소드 시작
        foreach (var agent in agents)
        {
            agent.OnEpisodeBegin();
        }
        
        //UpdateUI();
    }
    
    public void EndRound(bool playerWon)
    {
        gameActive = false;
        
        if (playerWon)
        {
            playerWins++;
        }
        else
        {
            aiWins++;
        }
        
        currentRound++;
        
        if (currentRound > maxRounds)
        {
            EndGame();
        }
        else
        {
            // 다음 라운드 시작 (2초 후)
            Invoke(nameof(StartNewRound), 2f);
        }
        
        UpdateUI();
    }
    
    private void EndGame()
    {
        gameActive = false;
        if (playerWins >= 10)
        {
            playerWin.SetActive(true);
            playerWin.SetActive(true);
        }
        else if (aiWins >= 5)
        {
            aiWin.SetActive(true);
            aiWin.SetActive(true);
        }
        StartCoroutine(Exit());
    }

    private void EndGameAI()
    {
        gameActive = false;
        aiWin.SetActive(true);
        aiWin.SetActive(true);
        StartCoroutine(Exit());
    }

    private void EndGamePlayer()
    {
        gameActive = false;
        playerWin.SetActive(true);
        playerWin.SetActive(true);
        StartCoroutine(Exit());
    }   

    IEnumerator Exit()
    {
        yield return new WaitForSeconds(3f);
        Application.Quit();
    }
    
    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = $"시간: {currentTime:F1}";
        
        if (roundText != null)
            roundText.text = $"라운드: {currentRound}/{maxRounds}";
        
        if (scoreText != null)
            scoreText.text = $"플레이어: {playerWins} | AI: {aiWins}";
    }
    
    public void RestartGame()
    {
        currentRound = 1;
        playerWins = 0;
        aiWins = 0;
        gameOverPanel.SetActive(false);
        StartNewRound();
    }
    
    public bool IsGameActive()
    {
        return gameActive;
    }
    
    public float GetRemainingTime()
    {
        return currentTime;
    }
}