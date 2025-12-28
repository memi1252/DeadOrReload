using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DeadOrReloadAgent : Agent
{
    [Header("Agent Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 2f;
    public float reloadTime = 3f;
    public float reloadSpeedPenalty = 0.2f;
    
    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public LayerMask wallLayer;
    public LayerMask enemyLayer;
    
    [Header("Detection")]
    public float detectionRange = 15f;
    public int raycastCount = 16; // 360도를 16개 레이로 나눔
    
    private Rigidbody rb;
    private bool isReloading = false;
    private bool canShoot = true;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float reloadTimer = 0f;
    
    private DeadOrReloadAgent enemyAgent;
    private GameManager gameManager;
    
    // 관찰 데이터
    private Vector3 lastEnemyPosition;
    private float timeSinceLastEnemySeen = 0f;
    
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        
        // 적 에이전트 찾기
        DeadOrReloadAgent[] agents = FindObjectsOfType<DeadOrReloadAgent>();
        foreach (var agent in agents)
        {
            if (agent != this)
            {
                enemyAgent = agent;
                break;
            }
        }
    }
    
    public override void OnEpisodeBegin()
    {
        // 에피소드 시작 시 초기화
        transform.position = GetRandomSpawnPosition();
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        isReloading = false;
        canShoot = true;
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        reloadTimer = 0f;
        timeSinceLastEnemySeen = 0f;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // 자신의 상태 (6개)
        sensor.AddObservation(transform.position); // 3개
        sensor.AddObservation(transform.forward); // 3개
        
        // 적의 위치 정보 (3개)
        if (enemyAgent != null)
        {
            Vector3 enemyDirection = (enemyAgent.transform.position - transform.position).normalized;
            sensor.AddObservation(enemyDirection);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }
        
        // 전투 상태 (4개)
        sensor.AddObservation(canShoot ? 1f : 0f);
        sensor.AddObservation(isReloading ? 1f : 0f);
        sensor.AddObservation(isDashing ? 1f : 0f);
        sensor.AddObservation(dashCooldownTimer / dashCooldown);
        
        // 360도 레이캐스트 (16개)
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = i * (360f / raycastCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            
            RaycastHit hit;
            float distance = 1f; // 정규화된 거리
            
            if (Physics.Raycast(transform.position, direction, out hit, detectionRange, wallLayer | enemyLayer))
            {
                distance = hit.distance / detectionRange;
                
                // 적을 발견했을 때
                if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
                {
                    lastEnemyPosition = hit.point;
                    timeSinceLastEnemySeen = 0f;
                }
            }
            
            sensor.AddObservation(distance);
        }
        
        // 시간 정보 (1개)
        timeSinceLastEnemySeen += Time.fixedDeltaTime;
        sensor.AddObservation(Mathf.Clamp01(timeSinceLastEnemySeen / 5f));
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 연속 액션: 이동 (2개)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        
        // 이산 액션: 회전, 사격, 대시 (3개)
        int rotateAction = actions.DiscreteActions[0]; // 0: 정지, 1: 좌회전, 2: 우회전
        int shootAction = actions.DiscreteActions[1];  // 0: 안쏨, 1: 쏨
        int dashAction = actions.DiscreteActions[2];   // 0: 안함, 1: 대시
        
        // 이동 처리
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        float currentSpeed = moveSpeed;
        
        // 재장전 중 속도 감소
        if (isReloading)
        {
            currentSpeed *= (1f - reloadSpeedPenalty);
        }
        
        // 대시 처리
        if (dashAction == 1 && !isDashing && dashCooldownTimer <= 0f)
        {
            StartDash();
        }
        
        if (isDashing)
        {
            currentSpeed = dashSpeed;
        }
        
        // 이동 적용
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y; // Y축 속도 유지
        rb.linearVelocity = velocity;
        
        // 회전 처리
        if (rotateAction == 1) // 좌회전
        {
            transform.Rotate(0, -180f * Time.fixedDeltaTime, 0);
        }
        else if (rotateAction == 2) // 우회전
        {
            transform.Rotate(0, 180f * Time.fixedDeltaTime, 0);
        }
        
        // 사격 처리
        if (shootAction == 1 && canShoot && !isReloading)
        {
            Shoot();
        }
        
        // 타이머 업데이트
        UpdateTimers();
        
        // 보상 계산
        CalculateRewards();
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
    }
    
    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(this, bulletSpeed);
            }
        }
        
        canShoot = false;
        isReloading = true;
        reloadTimer = reloadTime;
    }
    
    private void UpdateTimers()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
        }
        
        if (isReloading)
        {
            reloadTimer -= Time.fixedDeltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                canShoot = true;
            }
        }
    }
    
    private void CalculateRewards()
    {
        // 생존 보상
        AddReward(0.001f);
        
        // 적과의 거리에 따른 보상 (너무 가깝거나 멀지 않게)
        if (enemyAgent != null)
        {
            float distance = Vector3.Distance(transform.position, enemyAgent.transform.position);
            float optimalDistance = 8f;
            float distanceReward = -Mathf.Abs(distance - optimalDistance) * 0.0001f;
            AddReward(distanceReward);
        }
        
        // 벽 근처에서 엄폐 보상
        if (IsNearWall())
        {
            AddReward(0.002f);
        }
    }
    
    private bool IsNearWall()
    {
        return Physics.CheckSphere(transform.position, 2f, wallLayer);
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // 맵 경계 내에서 랜덤 위치 생성
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 1f, z);
    }
    
    public void OnHit()
    {
        // 피격 시 큰 음의 보상
        AddReward(-1.0f);
        EndEpisode();
    }
    
    public void OnEnemyHit()
    {
        // 적 명중 시 큰 양의 보상
        AddReward(1.0f);
        EndEpisode();
    }
    
    public void OnBulletDodged()
    {
        // 탄환 회피 시 보상
        AddReward(0.3f);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        UnityEngine.Profiling.Profiler.BeginSample("DeadOrReloadAgent.Heuristic");
        
        try
        {
            // 수동 조작을 위한 휴리스틱 (테스트용)
            var continuousActions = actionsOut.ContinuousActions;
            var discreteActions = actionsOut.DiscreteActions;
            
            continuousActions[0] = Input.GetAxis("Horizontal");
            continuousActions[1] = Input.GetAxis("Vertical");
            
            discreteActions[0] = 0; // 회전
            if (Input.GetKey(KeyCode.Q)) discreteActions[0] = 1;
            if (Input.GetKey(KeyCode.E)) discreteActions[0] = 2;
            
            discreteActions[1] = Input.GetKey(KeyCode.Space) ? 1 : 0; // 사격
            discreteActions[2] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0; // 대시
        }
        finally
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}