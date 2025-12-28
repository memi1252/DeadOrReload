using UnityEngine;

// ML-Agents 없이도 작동하는 똑똑한 AI
public class SimpleAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 2f;
    
    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float shootCooldown = 3f;
    public float reloadSpeedPenalty = 0.2f;
    
    [Header("AI Behavior")]
    public float detectionRange = 15f;
    public float shootRange = 8f;
    public float optimalRange = 6f;
    public float avoidanceRange = 3f;
    
    [Header("AI Intelligence")]
    public float predictionAccuracy = 0.7f;
    public float reactionTime = 0.2f;
    
    private CharacterController controller;
    private Transform player;
    private float lastShootTime;
    private Vector3 randomDirection;
    private float directionChangeTime;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 lastPlayerPosition;
    private Vector3 predictedPlayerPosition;
    private float lastPlayerSeen = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // 랜덤 방향 설정
        ChangeRandomDirection();
    }
    
    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                // 플레이어 발견 - 추적 및 공격
                ChaseAndAttack();
            }
            else
            {
                // 플레이어 없음 - 랜덤 이동
                RandomMovement();
            }
        }
        else
        {
            // 플레이어 없음 - 랜덤 이동
            RandomMovement();
        }
    }
    
    void ChaseAndAttack()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        
        // 플레이어 방향으로 회전
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 적절한 거리 유지하며 이동
        if (distanceToPlayer > shootRange)
        {
            // 가까이 이동
            Vector3 moveDirection = directionToPlayer;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else if (distanceToPlayer < shootRange * 0.5f)
        {
            // 너무 가까우면 뒤로 이동
            Vector3 moveDirection = -directionToPlayer;
            controller.Move(moveDirection * moveSpeed * 0.5f * Time.deltaTime);
        }
        else
        {
            // 좌우로 이동 (회피)
            Vector3 sideDirection = Vector3.Cross(directionToPlayer, Vector3.up);
            if (Random.value > 0.5f) sideDirection = -sideDirection;
            controller.Move(sideDirection * moveSpeed * 0.7f * Time.deltaTime);
        }
        
        // 사격
        if (distanceToPlayer <= shootRange && Time.time - lastShootTime >= shootCooldown)
        {
            Shoot();
        }
    }
    
    void RandomMovement()
    {
        // 방향 변경 시간 체크
        if (Time.time - directionChangeTime >= Random.Range(2f, 5f))
        {
            ChangeRandomDirection();
        }
        
        // 랜덤 방향으로 이동
        controller.Move(randomDirection * moveSpeed * 0.5f * Time.deltaTime);
        
        // 랜덤 회전
        if (randomDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 0.5f * Time.deltaTime);
        }
    }
    
    void ChangeRandomDirection()
    {
        randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        directionChangeTime = Time.time;
    }
    
    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.tag = "AIBullet";
            
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
            }
            
            lastShootTime = Time.time;
            
            Debug.Log("AI Shot!");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 탐지 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 사격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}