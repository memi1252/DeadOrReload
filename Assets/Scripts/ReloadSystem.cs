using UnityEngine;
using TMPro;
using System.Collections.Generic;
using StarterAssets;

public class ReloadSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI reloadPromptText;
    public GameObject reloadPanel;
    
    [Header("Reload Settings")]
    public int keysToPress = 3; // 눌러야 하는 키 개수
    
    private List<KeyCode> availableKeys = new List<KeyCode>
    {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V
    };
    
    private List<KeyCode> requiredKeys = new List<KeyCode>();
    private int currentKeyIndex = 0;
    private bool isReloading = false;
    private ThirdPersonController playerController;
    
    private static ReloadSystem instance;
    
    void Awake()
    {
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
        playerController = FindFirstObjectByType<ThirdPersonController>();
        
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isReloading)
        {
            CheckKeyPress();
        }
    }
    
    public static ReloadSystem Instance
    {
        get { return instance; }
    }
    
    public void StartReload()
    {
        isReloading = true;
        currentKeyIndex = 0;
        
        // 랜덤하게 키 선택
        requiredKeys.Clear();
        List<KeyCode> tempKeys = new List<KeyCode>(availableKeys);
        
        for (int i = 0; i < keysToPress; i++)
        {
            int randomIndex = Random.Range(0, tempKeys.Count);
            requiredKeys.Add(tempKeys[randomIndex]);
            tempKeys.RemoveAt(randomIndex);
        }
        
        // UI 표시
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(true);
        }
        
        UpdatePromptText();
        
        Debug.Log($"[Reload] Press keys: {string.Join(", ", requiredKeys)}");
    }
    
    private void CheckKeyPress()
    {
        if (currentKeyIndex >= requiredKeys.Count)
        {
            return;
        }
        
        KeyCode requiredKey = requiredKeys[currentKeyIndex];
        
        if (Input.GetKeyDown(requiredKey))
        {
            // 올바른 키 입력
            currentKeyIndex++;
            UpdatePromptText();
            
            if (currentKeyIndex >= requiredKeys.Count)
            {
                // 장전 완료
                CompleteReload();
            }
        }
        else
        {
            // 잘못된 키 입력 확인
            foreach (KeyCode key in availableKeys)
            {
                if (Input.GetKeyDown(key) && key != requiredKey)
                {
                    // 잘못된 키 - 처음부터 다시
                    Debug.Log("[Reload] Wrong key! Starting over...");
                    currentKeyIndex = 0;
                    UpdatePromptText();
                    break;
                }
            }
        }
    }
    
    private void UpdatePromptText()
    {
        if (reloadPromptText == null) return;
        
        string promptText = "Reload: ";
        
        for (int i = 0; i < requiredKeys.Count; i++)
        {
            if (i < currentKeyIndex)
            {
                // 이미 누른 키 (체크 표시)
                promptText += $"<color=green>[{requiredKeys[i]}]</color> ";
            }
            else if (i == currentKeyIndex)
            {
                // 현재 눌러야 하는 키 (강조)
                promptText += $"<color=yellow><b>[{requiredKeys[i]}]</b></color> ";
            }
            else
            {
                // 아직 안 누른 키
                promptText += $"<color=white>[{requiredKeys[i]}]</color> ";
            }
        }
        
        reloadPromptText.text = promptText;
    }
    
    private void CompleteReload()
    {
        isReloading = false;
        
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(false);
        }
        
        // 플레이어 컨트롤러에 장전 완료 알림
        if (playerController != null)
        {
            playerController.CompleteReload();
        }
        
        Debug.Log("[Reload] Reload complete!");
    }
    
    public bool IsReloading()
    {
        return isReloading;
    }
    
    public void CancelReload()
    {
        isReloading = false;
        currentKeyIndex = 0;
        
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(false);
        }
    }
}
