using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI aiScoreText;
    
    public int playerScore = 0;
    public int aiScore = 0;
    
    private static ScoreUI instance;
    
    void Awake()
    {
        // 싱글톤 패턴
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateScoreDisplay();
    }
    
    public static ScoreUI Instance
    {
        get { return instance; }
    }
    
    public void AddPlayerScore()
    {
        playerScore++;
        UpdateScoreDisplay();
        Debug.Log($"[Score] Player: {playerScore} | AI: {aiScore}");
    }
    
    public void AddAIScore()
    {
        aiScore++;
        UpdateScoreDisplay();
        Debug.Log($"[Score] Player: {playerScore} | AI: {aiScore}");
    }
    
    private void UpdateScoreDisplay()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = $"Player: {playerScore}";
        }
        
        if (aiScoreText != null)
        {
            aiScoreText.text = $"AI: {aiScore}";
        }
    }
    
    public void ResetScores()
    {
        playerScore = 0;
        aiScore = 0;
        UpdateScoreDisplay();
    }
    
    public int GetPlayerScore()
    {
        return playerScore;
    }
    
    public int GetAIScore()
    {
        return aiScore;
    }
}
