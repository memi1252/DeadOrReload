using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class AIController : Agent
    {
        [Header("AI Movement")]
        [Tooltip("Move speed of the AI in m/s")]
        public float MoveSpeed = 5.0f;

        [Tooltip("Dash speed of the AI in m/s")]
        public float DashSpeed = 10.0f;

        [Tooltip("Dash duration in seconds")]
        public float DashDuration = 0.3f;

        [Tooltip("Dash cooldown in seconds")]
        public float DashCooldown = 2.0f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("Combat")]
        [Tooltip("Bullet prefab to shoot")]
        public GameObject BulletPrefab;

        [Tooltip("Point where bullets are fired from")]
        public Transform FirePoint;

        [Tooltip("Speed of fired bullets")]
        public float BulletSpeed = 20.0f;

        [Tooltip("Time to reload after shooting")]
        public float ReloadTime = 3.0f;

        [Tooltip("Movement speed penalty while reloading")]
        [Range(0.0f, 1.0f)]
        public float ReloadSpeedPenalty = 0.2f;

        [Header("AI Detection")]
        [Tooltip("Detection range for raycasting")]
        public float DetectionRange = 15f;

        [Tooltip("Number of rays for 360 degree detection")]
        public int RaycastCount = 16;

        [Tooltip("Layer mask for walls")]
        public LayerMask WallLayer;

        [Tooltip("Layer mask for enemies")]
        public LayerMask EnemyLayer;

        [Header("Grounded Check")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        // AI state
        private float _speed;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float Gravity = -15.0f;

        // combat system
        private bool _canShoot = true;
        private bool _isReloading = false;
        private float _reloadTimer = 0f;

        // dash system
        private bool _isDashing = false;
        private float _dashTimer = 0f;
        private float _dashCooldownTimer = 0f;

        // components
        private CharacterController _controller;
        private GameManager _gameManager;
        private Animator _animator;
        private bool _hasAnimator;
        
        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDMotionSpeed;
        
        // animation blend
        private float _animationBlend;

        // AI target data
        public Transform _enemyTransform; // 타겟 (자동으로 찾음)
        
        [Header("Target Settings")]
        [Tooltip("Tag to search for target (Player or AI)")]
        public string TargetTag = "Player";
        
        [Header("Obstacle Avoidance")]
        [Tooltip("Distance to check for obstacles ahead")]
        public float ObstacleCheckDistance = 3f;
        
        [Tooltip("Number of rays for obstacle detection")]
        public int ObstacleRayCount = 5;

        // Action smoothing to prevent jittery behavior
        private Vector3 _lastMoveDirection;
        private float _actionChangeTimer = 0f;
        private const float ACTION_CHANGE_DELAY = 0.1f;

        // Respawn system
        private Vector3 _initialSpawnPosition;
        private Quaternion _initialSpawnRotation;
        private float _targetSearchTimer = 0f;
        private const float TARGET_SEARCH_INTERVAL = 1f; // 1초마다 타겟 재검색
        private bool _hasInitializedSpawn = false; // 초기 위치 저장 여부
        
        // Visibility tracking
        private float _targetNotVisibleTime = 0f; // 타겟이 안 보인 시간
        private const float MAX_INVISIBLE_TIME = 3f; // 3초 이상 안 보이면 패널티
        
        // Shooting tracking
        private bool _justShot = false; // 방금 총을 쐈는지
        private float _timeSinceLastShot = 0f;
        private const float ADVANCE_AFTER_SHOT_TIME = 1.5f; // 총 쏜 후 1.5초 동안 전진
        private float _timeSinceLastAttack = 0f; // 마지막 공격 이후 시간
        private const float MAX_NO_ATTACK_TIME = 5f; // 5초 이상 공격 안 하면 패널티
        
        // Obstacle avoidance
        private bool _isStuck = false; // 막혔는지 여부
        private float _stuckTimer = 0f; // 막힌 시간
        private Vector3 _lastPosition; // 이전 위치
        private float _positionCheckTimer = 0f;
        private const float STUCK_CHECK_INTERVAL = 0.5f; // 0.5초마다 위치 확인
        private const float STUCK_THRESHOLD = 0.5f; // 0.5m 이하 이동 시 막힌 것으로 판단
        private Vector3 _avoidanceDirection = Vector3.zero; // 회피 방향
        private float _wallProximityTime = 0f; // 벽 근처에 있는 시간
        private const float MAX_WALL_PROXIMITY_TIME = 3f; // 3초 이상 벽 근처에 있으면 패널티
        
        // AI 개별 행동 패턴
        private int _aiRandomSeed; // 각 AI마다 다른 랜덤 시드
        private float _movementBias; // 움직임 편향 (-1 ~ 1)
        private float _movementChangeTimer = 0f; // 움직임 변경 타이머
        private Vector3 _currentMoveDirection = Vector3.zero; // 현재 움직임 방향
        private const float MOVEMENT_CHANGE_INTERVAL = 0.5f; // 0.5초마다 움직임 변경

        void Awake()
        {
            // 게임 시작 시 최초 1회만 초기 위치 저장
            if (!_hasInitializedSpawn)
            {
                _initialSpawnPosition = transform.position;
                _initialSpawnRotation = transform.rotation;
                _hasInitializedSpawn = true;
                
                // 각 AI마다 고유한 랜덤 시드 생성 (위치 기반)
                _aiRandomSeed = Mathf.RoundToInt(transform.position.x * 1000 + transform.position.z * 1000);
                Random.InitState(_aiRandomSeed);
                _movementBias = Random.Range(-1f, 1f); // 각 AI마다 다른 움직임 편향
                
                Debug.Log($"[AI] {gameObject.name} initial spawn saved at {_initialSpawnPosition}, seed: {_aiRandomSeed}, bias: {_movementBias}");
            }
            
            // 애니메이터 컴포넌트 확인
            _hasAnimator = TryGetComponent(out _animator);
        }

        void Start()
        {
            // 애니메이션 ID 할당
            AssignAnimationIDs();
            
            // 초기 위치 저장
            _lastPosition = transform.position;
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        
        void Update()
        {
            // 막힘 감지
            CheckIfStuck();
        }
        
        private void CheckIfStuck()
        {
            _positionCheckTimer += Time.deltaTime;
            
            if (_positionCheckTimer >= STUCK_CHECK_INTERVAL)
            {
                _positionCheckTimer = 0f;
                
                // 이동 거리 계산
                float distanceMoved = Vector3.Distance(transform.position, _lastPosition);
                
                if (distanceMoved < STUCK_THRESHOLD)
                {
                    // 막힌 것으로 판단
                    _stuckTimer += STUCK_CHECK_INTERVAL;
                    _isStuck = true;
                    
                    // 회피 방향 계산
                    CalculateAvoidanceDirection();
                }
                else
                {
                    // 정상적으로 이동 중
                    _stuckTimer = 0f;
                    _isStuck = false;
                }
                
                _lastPosition = transform.position;
            }
        }
        
        private void CalculateAvoidanceDirection()
        {
            // 여러 방향으로 레이캐스트하여 가장 열린 방향 찾기
            float maxDistance = 0f;
            Vector3 bestDirection = transform.right; // 기본값: 오른쪽
            
            for (int i = 0; i < ObstacleRayCount; i++)
            {
                float angle = (i - ObstacleRayCount / 2) * 45f; // -90도 ~ +90도
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                
                if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, ObstacleCheckDistance, WallLayer))
                {
                    // 장애물 없음 - 이 방향이 가장 좋음
                    _avoidanceDirection = direction;
                    return;
                }
                else if (hit.distance > maxDistance)
                {
                    // 가장 먼 장애물 방향 저장
                    maxDistance = hit.distance;
                    bestDirection = direction;
                }
            }
            
            _avoidanceDirection = bestDirection;
        }

        public override void Initialize()
        {
            _controller = GetComponent<CharacterController>();
            _gameManager = FindFirstObjectByType<GameManager>();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Self state (6 observations)
            sensor.AddObservation(transform.position); // 3
            sensor.AddObservation(transform.forward); // 3

            // Target information (타겟 기반) (6 observations)
            if (_enemyTransform != null)
            {
                // 타겟 위치
                sensor.AddObservation(_enemyTransform.position); // 3
                // 타겟까지의 방향
                Vector3 dirToTarget = (_enemyTransform.position - transform.position).normalized;
                sensor.AddObservation(dirToTarget); // 3
            }
            else
            {
                sensor.AddObservation(Vector3.zero); // 3
                sensor.AddObservation(Vector3.zero); // 3
            }

            // Combat state (4 observations)
            sensor.AddObservation(_canShoot ? 1f : 0f); // 1
            sensor.AddObservation(_isReloading ? 1f : 0f); // 1
            sensor.AddObservation(_isDashing ? 1f : 0f); // 1
            sensor.AddObservation(_dashCooldownTimer / DashCooldown); // 1

            // Simplified wall detection (8 observations - 8방향만)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 45도씩 8방향
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

                float wallDistance = 1f;
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, DetectionRange, WallLayer))
                {
                    wallDistance = hit.distance / DetectionRange;
                }
                sensor.AddObservation(wallDistance); // 1 per ray = 8
            }

            // Target distance and angle (2 observations)
            if (_enemyTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _enemyTransform.position);
                sensor.AddObservation(distance / DetectionRange); // 1 - 정규화된 거리

                Vector3 dirToEnemy = (_enemyTransform.position - transform.position).normalized;
                float angleToEnemy = Vector3.Dot(transform.forward, dirToEnemy); // -1 to 1
                sensor.AddObservation(angleToEnemy); // 1 - 타겟을 바라보는 정도
            }
            else
            {
                sensor.AddObservation(1f); // 2
                sensor.AddObservation(0f);
            }

            // Total: 6 + 6 + 4 + 8 + 2 = 26 observations
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // 기본 AI 행동 추가 (학습 전에도 똑똑하게 행동)
            BasicAIBehavior();
            
            // Continuous actions: movement (2)
            float moveX = actions.ContinuousActions[0];
            float moveZ = actions.ContinuousActions[1];

            // Discrete actions: rotation, shoot, dash (3)
            int rotateAction = actions.DiscreteActions[0]; // 0: none, 1: left, 2: right
            int shootAction = actions.DiscreteActions[1];  // 0: no, 1: yes
            int dashAction = actions.DiscreteActions[2];   // 0: no, 1: yes

            // Handle movement with action smoothing
            Vector3 moveDirection = new Vector3(moveX, 0, moveZ);

            // Action smoothing to prevent jittery movements
            _actionChangeTimer += Time.fixedDeltaTime;
            if (_actionChangeTimer >= ACTION_CHANGE_DELAY || Vector3.Distance(moveDirection, _lastMoveDirection) > 0.5f)
            {
                _lastMoveDirection = moveDirection;
                _actionChangeTimer = 0f;
            }
            else
            {
                // Use smoothed direction
                moveDirection = Vector3.Lerp(moveDirection, _lastMoveDirection, 0.5f);
            }

            HandleMovement(moveDirection);

            // Handle rotation (더 부드럽고 자연스러운 회전)
            if (rotateAction == 1) // left
            {
                transform.Rotate(0, -90f * Time.fixedDeltaTime, 0);
            }
            else if (rotateAction == 2) // right
            {
                transform.Rotate(0, 90f * Time.fixedDeltaTime, 0);
            }

            // Handle shooting
            if (shootAction == 1 && _canShoot && !_isReloading)
            {
                Shoot();
            }

            // Handle dash
            if (dashAction == 1 && !_isDashing && _dashCooldownTimer <= 0f)
            {
                StartDash();
            }

            // Update timers
            UpdateTimers();

            // Apply gravity
            ApplyGravity();

            // Calculate rewards
            CalculateRewards();
        }

        private void HandleMovement(Vector3 moveDirection)
        {
            float targetSpeed = MoveSpeed;

            if (_isDashing)
            {
                targetSpeed = DashSpeed;
            }
            else if (_isReloading)
            {
                targetSpeed *= (1f - ReloadSpeedPenalty);
            }

            // 더 엄격한 움직임 필터링으로 지터링 방지
            if (moveDirection.magnitude < 0.2f)
            {
                targetSpeed = 0.0f;
                moveDirection = Vector3.zero;
            }
            else
            {
                // 움직임 방향 정규화 확실히 하기
                moveDirection = moveDirection.normalized;
            }

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            // 더 부드러운 가속/감속
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.fixedDeltaTime * SpeedChangeRate);

            // 매우 작은 속도는 0으로 처리하여 미세한 움직임 방지
            if (_speed < 0.1f) _speed = 0f;
            
            // 애니메이션 블렌드 업데이트
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.fixedDeltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Move the AI
            Vector3 moveVector = moveDirection * (_speed * Time.fixedDeltaTime) +
                               new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.fixedDeltaTime;

            _controller.Move(moveVector);
            
            // 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, moveDirection.magnitude);
            }
        }

        private void Shoot()
        {
            if (BulletPrefab != null && FirePoint != null)
            {
                GameObject bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);
                bullet.tag = "AIBullet";

                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = FirePoint.forward * BulletSpeed;
                }

                // Initialize AI bullet
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    // Convert to DeadOrReloadAgent for compatibility
                    DeadOrReloadAgent agentScript = GetComponent<DeadOrReloadAgent>();
                    if (agentScript != null)
                    {
                        bulletScript.Initialize(agentScript, BulletSpeed);
                    }
                }
                
                // AI 총소리 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound("Shoot");
                }
            }

            _canShoot = false;
            _justShot = true; // 총을 쐈음을 표시
            _timeSinceLastShot = 0f;
            _timeSinceLastAttack = 0f; // 공격 시간 리셋
            StartReload();
        }

        private void StartReload()
        {
            _isReloading = true;
            _reloadTimer = ReloadTime;
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
        }

        private void UpdateTimers()
        {
            if (_isDashing)
            {
                _dashTimer -= Time.fixedDeltaTime;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                }
            }

            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.fixedDeltaTime;
            }

            if (_isReloading)
            {
                _reloadTimer -= Time.fixedDeltaTime;
                if (_reloadTimer <= 0f)
                {
                    _isReloading = false;
                    _canShoot = true;
                }
            }
            
            // 총 쏜 후 시간 추적
            if (_justShot)
            {
                _timeSinceLastShot += Time.fixedDeltaTime;
                if (_timeSinceLastShot >= ADVANCE_AFTER_SHOT_TIME)
                {
                    _justShot = false;
                }
            }
        }

        private void ApplyGravity()
        {
            // Ground check
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            // 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }

            if (Grounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * Time.fixedDeltaTime;
                }
            }
        }

        private void CalculateRewards()
        {
            if (_enemyTransform != null)
            {
                float distance = Vector3.Distance(transform.position, _enemyTransform.position);
                Vector3 dirToTarget = (_enemyTransform.position - transform.position).normalized;
                float aimAccuracy = Vector3.Dot(transform.forward, dirToTarget);
                
                bool canSeeEnemy = CanSeeTarget(_enemyTransform.position);

                // 타겟 가시성 추적
                if (canSeeEnemy)
                {
                    _targetNotVisibleTime = 0f; // 보이면 타이머 리셋
                }
                else
                {
                    _targetNotVisibleTime += Time.fixedDeltaTime;
                    
                    // 3초 이상 타겟이 안 보이면 패널티
                    if (_targetNotVisibleTime >= MAX_INVISIBLE_TIME)
                    {
                        AddReward(-0.01f); // 큰 패널티
                    }
                }
                
                // 공격 시간 추적
                _timeSinceLastAttack += Time.fixedDeltaTime;
                
                // 5초 이상 공격 안 하면 패널티
                if (_timeSinceLastAttack >= MAX_NO_ATTACK_TIME)
                {
                    AddReward(-0.015f); // 매우 큰 패널티
                }

                // 최적 전투 거리 보상 - 더 가까운 거리 선호
                float optimalDistance = 7f;
                float distanceScore = 1f - Mathf.Abs(distance - optimalDistance) / optimalDistance;
                AddReward(distanceScore * 0.01f);
                
                // 너무 멀면 큰 패널티
                if (distance > 15f)
                {
                    AddReward(-0.01f);
                }

                // 타겟을 바라보고 있을 때 큰 보상 (조준)
                if (aimAccuracy > 0.7f)
                {
                    AddReward(0.02f * aimAccuracy);
                }
                
                // 타겟이 가까울수록 큰 보상 (적극적인 교전 유도)
                if (distance < 12f)
                {
                    float proximityReward = (12f - distance) / 12f;
                    AddReward(proximityReward * 0.01f);
                }
                
                // 타겟을 직접 볼 수 있을 때 큰 보상 (시야 확보)
                if (canSeeEnemy)
                {
                    AddReward(0.015f);
                }

                // 은폐 보상 (전투 보상보다 훨씬 작게 유지)
                AIController enemyAI = _enemyTransform.GetComponent<AIController>();
                if (enemyAI != null)
                {
                    if (!enemyAI.CanSeeTarget(transform.position))
                    {
                        AddReward(0.0003f);
                    }
                }
                
                // 벽 근처 시간 추적
                if (IsNearWall(2f))
                {
                    _wallProximityTime += Time.fixedDeltaTime;
                    
                    // 3초 이상 벽 근처에 있으면 큰 패널티
                    if (_wallProximityTime >= MAX_WALL_PROXIMITY_TIME)
                    {
                        AddReward(-0.02f); // 매우 큰 패널티
                    }
                    else
                    {
                        AddReward(-0.008f); // 벽 근처 패널티
                    }
                }
                else
                {
                    _wallProximityTime = 0f; // 벽에서 멀어지면 리셋
                }
                
                // 벽에 아주 가까이 있으면 더 큰 패널티
                if (IsNearWall(1f))
                {
                    AddReward(-0.015f);
                }
                
                // 막혔을 때 패널티
                if (_isStuck && _stuckTimer > 2f)
                {
                    AddReward(-0.025f); // 2초 이상 막히면 매우 큰 패널티
                }
            }
        }
        
        // 벽 근처 체크
        private bool IsNearWall(float radius)
        {
            return Physics.CheckSphere(transform.position, radius, WallLayer);
        }
        
        // 외부에서 호출 가능하도록 public으로 변경
        public bool CanSeeTarget(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget, out RaycastHit hit, distanceToTarget, WallLayer | EnemyLayer))
            {
                // 적을 직접 볼 수 있는지 확인
                return ((1 << hit.collider.gameObject.layer) & EnemyLayer) != 0;
            }
            
            return false;
        }
        
        private void ResetState()
        {
            _isReloading = false;
            _canShoot = true;
            _isDashing = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
            _reloadTimer = 0f;
            _speed = 0f;
            _verticalVelocity = 0f;
            _lastMoveDirection = Vector3.zero;
            _actionChangeTimer = 0f;
            _targetNotVisibleTime = 0f;
            _justShot = false;
            _timeSinceLastShot = 0f;
            _timeSinceLastAttack = 0f;
            _animationBlend = 0f;
            _movementChangeTimer = 0f;
            _currentMoveDirection = Vector3.zero;
            _isStuck = false;
            _stuckTimer = 0f;
            _positionCheckTimer = 0f;
            _lastPosition = transform.position;
            _wallProximityTime = 0f;
            
            // 애니메이터 리셋
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
        }

        public void OnBulletDodged()
        {
            AddReward(0.3f);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Manual control for testing (arrow keys + numpad)
            var continuousActions = actionsOut.ContinuousActions;
            var discreteActions = actionsOut.DiscreteActions;

            // Continuous actions (movement)
            if (continuousActions.Length >= 2)
            {
                continuousActions[0] = Input.GetAxis("Horizontal");
                continuousActions[1] = Input.GetAxis("Vertical");
            }

            // Discrete actions (rotation, shoot, dash)
            if (discreteActions.Length >= 3)
            {
                discreteActions[0] = 0; // rotation
                if (Input.GetKey(KeyCode.Q)) discreteActions[0] = 1;
                if (Input.GetKey(KeyCode.E)) discreteActions[0] = 2;

                discreteActions[1] = Input.GetKey(KeyCode.Space) ? 1 : 0; // shoot
                discreteActions[2] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0; // dash
            }
        }

        public void RespawnBothPlayers()
        {
            // 자신 리스폰
            Respawn();

            // 타겟도 리스폰 (타겟이 있고 AIController나 ThirdPersonController를 가지고 있으면)
            if (_enemyTransform != null)
            {
                AIController targetAI = _enemyTransform.GetComponent<AIController>();
                if (targetAI != null)
                {
                    targetAI.Respawn();
                }
                
                ThirdPersonController playerController = _enemyTransform.GetComponent<ThirdPersonController>();
                if (playerController != null)
                {
                    playerController.ResetPlayerState();
                }
            }
        }

        private void BasicAIBehavior()
        {
            // 타겟이 없으면 찾기
            if (_enemyTransform == null)
            {
                SearchForEnemy();
                return;
            }
            
            float distanceToEnemy = Vector3.Distance(transform.position, _enemyTransform.position);
            
            // 항상 적을 바라보기
            LookAtTarget(_enemyTransform.position);
            
            // 거리에 따른 행동
            if (distanceToEnemy <= DetectionRange)
            {
                // 적 발견 - 추적 및 공격
                ChaseAndAttack(distanceToEnemy);
            }
            else
            {
                // 적을 찾아 이동
                SearchForEnemy();
            }
        }
        
        private void LookAtTarget(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            directionToTarget.y = 0; // Y축 회전 방지
            
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 120f * Time.fixedDeltaTime);
            }
        }
        
        private void ChaseAndAttack(float distanceToEnemy)
        {
            Vector3 directionToEnemy = (_enemyTransform.position - transform.position).normalized;
            directionToEnemy.y = 0;
            
            float optimalDistance = 7f; // 최적 교전 거리
            
            // 움직임 변경 타이머 업데이트
            _movementChangeTimer += Time.fixedDeltaTime;
            
            // 막혔으면 회피 방향으로 이동 (더 빨리 반응)
            if (_isStuck && _stuckTimer > 0.5f) // 1초 -> 0.5초 (더 빠르게 반응)
            {
                Debug.Log($"[AI] {gameObject.name} is stuck! Using avoidance direction.");
                
                // 회피 방향 재계산 (계속 새로운 길 찾기)
                if (_movementChangeTimer >= 0.3f)
                {
                    _movementChangeTimer = 0f;
                    CalculateAvoidanceDirection();
                }
                
                HandleMovement(_avoidanceDirection * 0.9f); // 0.7 -> 0.9 (더 빠르게)
                
                // 회피 방향으로 회전
                if (_avoidanceDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_avoidanceDirection);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 240f * Time.fixedDeltaTime); // 180 -> 240 (더 빠르게)
                }
                return;
            }
            
            // 앞에 장애물이 있는지 확인
            bool obstacleAhead = Physics.Raycast(transform.position, directionToEnemy, ObstacleCheckDistance, WallLayer);
            
            if (obstacleAhead)
            {
                // 장애물이 있으면 우회 경로 사용 (더 자주 재계산)
                if (_movementChangeTimer >= 0.3f)
                {
                    _movementChangeTimer = 0f;
                    CalculateAvoidanceDirection();
                }
                
                HandleMovement(_avoidanceDirection * 0.8f); // 0.6 -> 0.8 (더 빠르게)
                return;
            }
            
            // 총을 쏜 직후이고 아직 안 맞았으면 전진 (1.5초 동안)
            if (_justShot && _timeSinceLastShot < ADVANCE_AFTER_SHOT_TIME)
            {
                _currentMoveDirection = directionToEnemy;
                HandleMovement(_currentMoveDirection * 0.8f);
            }
            else if (distanceToEnemy > optimalDistance + 2f)
            {
                // 너무 멀면 적극적으로 가까이 이동
                _currentMoveDirection = directionToEnemy;
                HandleMovement(_currentMoveDirection * 0.8f);
            }
            else if (distanceToEnemy < optimalDistance - 2f)
            {
                // 너무 가까우면 뒤로 이동하면서 좌우로도 움직임
                Vector3 backDirection = -directionToEnemy;
                Vector3 sideDirection = Vector3.Cross(directionToEnemy, Vector3.up);
                
                if (_movementBias < 0)
                {
                    sideDirection = -sideDirection;
                }
                
                // 뒤로 가면서 옆으로도 이동 (더 자연스러운 회피)
                _currentMoveDirection = (backDirection * 0.7f + sideDirection * 0.3f).normalized;
                HandleMovement(_currentMoveDirection * 0.6f);
            }
            else
            {
                // 적절한 거리에서 좌우로 이동 (회피)
                // 0.5초마다 움직임 방향 변경
                if (_movementChangeTimer >= MOVEMENT_CHANGE_INTERVAL)
                {
                    _movementChangeTimer = 0f;
                    
                    Vector3 sideDirection = Vector3.Cross(directionToEnemy, Vector3.up);
                    
                    // 랜덤하게 좌우 선택 (편향 적용)
                    float randomValue = Random.value;
                    if (_movementBias < 0)
                    {
                        // 왼쪽 선호
                        if (randomValue < 0.7f)
                        {
                            sideDirection = -sideDirection;
                        }
                    }
                    else
                    {
                        // 오른쪽 선호
                        if (randomValue < 0.3f)
                        {
                            sideDirection = -sideDirection;
                        }
                    }
                    
                    // 가끔 전진/후진도 섞음
                    float forwardBias = Random.Range(-0.3f, 0.3f);
                    _currentMoveDirection = (sideDirection + directionToEnemy * forwardBias).normalized;
                }
                
                HandleMovement(_currentMoveDirection * 0.5f);
            }
            
            // 사격 판단
            if (distanceToEnemy <= 10f && _canShoot && !_isReloading)
            {
                // 적이 시야에 있는지 확인
                if (CanSeeTarget(_enemyTransform.position))
                {
                    Shoot();
                }
            }
            
            // 대시 판단 (위험할 때)
            if (distanceToEnemy < 4f && !_isDashing && _dashCooldownTimer <= 0f)
            {
                StartDash();
            }
        }
        
        private void SearchForEnemy()
        {
            // 적을 찾기 위해 천천히 회전
            transform.Rotate(0, 45f * Time.fixedDeltaTime, 0); // 30 -> 45 (더 빠르게)
            
            // 전진
            HandleMovement(transform.forward * 0.3f); // 0.15 -> 0.3 (더 빠르게)
        }
        
        public void OnHit()
        {
            Debug.Log($"[AI] {gameObject.name} got hit! Respawning both players...");
            AddReward(-1.0f);
            
            // AI 피격 사운드 재생
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySound("AIHit");
            }
            
            // 플레이어 점수 추가 (AI가 맞았으므로)
            if (ScoreUI.Instance != null)
            {
                
            }

            // 맞은 AI와 맞춘 AI 둘 다 리스폰
            RespawnBothPlayers();
        }

        public void OnEnemyHit()
        {
            Debug.Log($"[AI] {gameObject.name} hit enemy! Respawning both players...");
            AddReward(1.0f);
            
            // AI 점수 추가 (AI가 적을 맞췄으므로)
            if (ScoreUI.Instance != null)
            {
                ScoreUI.Instance.AddAIScore();
            }
            
            // 적중했으므로 전진 플래그 리셋 (안 맞았을 때만 전진하므로)
            _justShot = false;
            _timeSinceLastShot = 0f;

            // 맞은 AI와 맞춘 AI 둘 다 리스폰
            RespawnBothPlayers();
        }
        
        public void Respawn()
        {
            // CharacterController를 비활성화하고 위치 변경 후 다시 활성화
            // (CharacterController가 활성화된 상태에서 transform.position 변경 시 문제가 생길 수 있음)
            if (_controller != null)
            {
                _controller.enabled = false;
            }
            
            // 처음 시작할 때 저장된 위치로 이동
            transform.position = _initialSpawnPosition;
            transform.rotation = _initialSpawnRotation;
            
            if (_controller != null)
            {
                _controller.enabled = true;
            }
            
            // 상태 초기화
            ResetState();
            
            Debug.Log($"[AI] {gameObject.name} respawned at initial position {_initialSpawnPosition}");
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);

            // Draw ground check
            Color groundColor = Grounded ? Color.green : Color.red;
            groundColor.a = 0.35f;
            Gizmos.color = groundColor;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}