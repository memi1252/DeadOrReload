using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 2f;
    
    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float reloadTime = 3f;
    public float reloadSpeedPenalty = 0.2f;
    
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dashSound;
    
    private Rigidbody rb;
    private bool isReloading = false;
    private bool canShoot = true;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float reloadTimer = 0f;
    
    private AudioSource audioSource;
    private GameManager gameManager;
    
    // UI 업데이트용
    public UnityEngine.UI.Image dashCooldownImage;
    public UnityEngine.UI.Image reloadProgressImage;
    public UnityEngine.UI.Text ammoText;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Update()
    {
        if (!gameManager.IsGameActive()) return;
        
        HandleInput();
        UpdateTimers();
        UpdateUI();
    }
    
    private void HandleInput()
    {
        // 이동 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        float currentSpeed = moveSpeed;
        
        // 재장전 중 속도 감소
        if (isReloading)
        {
            currentSpeed *= (1f - reloadSpeedPenalty);
        }
        
        // 대시 입력
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            StartDash();
        }
        
        if (isDashing)
        {
            currentSpeed = dashSpeed;
        }
        
        // 이동 적용
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
        
        // 마우스 방향으로 회전 (탑다운 뷰용)
        if (Camera.main != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            
            // 바닥 평면과의 교차점 계산
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            
            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 worldMousePosition = ray.GetPoint(distance);
                Vector3 lookDirection = (worldMousePosition - transform.position).normalized;
                lookDirection.y = 0;
                
                if (lookDirection.magnitude > 0.1f) // 최소 거리 체크
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
        
        // 사격 입력
        if (Input.GetMouseButtonDown(0) && canShoot && !isReloading)
        {
            Shoot();
        }
        
        // 수동 재장전 (R키)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && !canShoot)
        {
            StartReload();
        }
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }
    
    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            // 총알에 플레이어 태그 설정
            bullet.tag = "PlayerBullet";
            
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
            }
            
            // 총알 스크립트 초기화 (AI 에이전트용이 아닌 플레이어용)
            PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(this, bulletSpeed);
            }
        }
        
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        canShoot = false;
        StartReload();
    }
    
    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
    }
    
    private void UpdateTimers()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                canShoot = true;
            }
        }
    }
    
    private void UpdateUI()
    {
        // 대시 쿨다운 UI
        if (dashCooldownImage != null)
        {
            dashCooldownImage.fillAmount = dashCooldownTimer / dashCooldown;
        }
        
        // 재장전 진행도 UI
        if (reloadProgressImage != null)
        {
            if (isReloading)
            {
                reloadProgressImage.fillAmount = 1f - (reloadTimer / reloadTime);
            }
            else
            {
                reloadProgressImage.fillAmount = canShoot ? 1f : 0f;
            }
        }
        
        // 탄약 상태 UI
        if (ammoText != null)
        {
            if (isReloading)
            {
                ammoText.text = "재장전 중...";
            }
            else if (canShoot)
            {
                ammoText.text = "준비됨";
            }
            else
            {
                ammoText.text = "재장전 필요";
            }
        }
    }
    
    public void OnHit()
    {
        // 플레이어가 맞았을 때
        Debug.Log("Player Hit!");
        if (gameManager != null)
        {
            gameManager.EndRound(false); // AI 승리
        }
    }
    
    public void OnEnemyHit()
    {
        // 플레이어가 적을 맞췄을 때
        Debug.Log("Enemy Hit by Player!");
        if (gameManager != null)
        {
            gameManager.EndRound(true); // 플레이어 승리
        }
    }
}