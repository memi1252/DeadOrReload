using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class StataUI : MonoBehaviour
{
    [Header("Dash Cooldown")]
    public Image dashCooldownImage;
    
    [Header("Reload")]
    public Image reloadImage;
    
    private ThirdPersonController playerController;
    
    void Start()
    {
        // 플레이어 컨트롤러 찾기
        playerController = FindFirstObjectByType<ThirdPersonController>();
        
        // 초기 상태 설정
        if (dashCooldownImage != null)
        {
            dashCooldownImage.fillAmount = 1f; // 대시 사용 가능
        }
        
        if (reloadImage != null)
        {
            reloadImage.fillAmount = 0f; // 재장전 안 함
        }
    }

    void Update()
    {
        if (playerController == null) return;
        
        UpdateDashCooldown();
        UpdateReloadStatus();
    }
    
    private void UpdateDashCooldown()
    {
        if (dashCooldownImage == null) return;
        
        float dashCooldownTimer = playerController.GetDashCooldownTimer();
        float dashCooldown = playerController.DashCooldown;
        
        if (dashCooldownTimer > 0f)
        {
            // 쿨타임 중 - 0에서 1로 채워짐
            dashCooldownImage.fillAmount = 1f - (dashCooldownTimer / dashCooldown);
        }
        else
        {
            // 대시 사용 가능
            dashCooldownImage.fillAmount = 1f;
        }
    }
    
    private void UpdateReloadStatus()
    {
        if (reloadImage == null) return;
        
        float reloadTimer = playerController.GetReloadTimer();
        float reloadTime = playerController.ReloadTime;
        bool isReloading = playerController.IsReloading();
        
        if (isReloading)
        {
            // 재장전 중 - 1에서 0으로 감소
            reloadImage.fillAmount = reloadTimer / reloadTime;
        }
        else
        {
            // 재장전 안 함
            reloadImage.fillAmount = 0f;
        }
    }
}
