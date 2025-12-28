using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: Modified for Dead or Reload - Top-down tactical shooter
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player Movement")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 5.0f;

        [Tooltip("Dash speed of the character in m/s")]
        public float DashSpeed = 10.0f;
        
        [Tooltip("Dash duration in seconds")]
        public float DashDuration = 0.3f;
        
        [Tooltip("Dash cooldown in seconds")]
        public float DashCooldown = 2.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

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

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        
        // combat system
        private bool _canShoot = true;
        private bool _isReloading = false;
        private float _reloadTimer = 0f;
        
        // dash system
        private bool _isDashing = false;
        private float _dashTimer = 0f;
        private float _dashCooldownTimer = 0f;
        
        // game manager reference
        private GameManager _gameManager;
        
        // Respawn system
        private Vector3 _initialSpawnPosition;
        private Quaternion _initialSpawnRotation;
        private bool _hasInitializedSpawn = false;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            
            // 게임 시작 시 최초 1회만 초기 위치 저장
            if (!_hasInitializedSpawn)
            {
                _initialSpawnPosition = transform.position;
                _initialSpawnRotation = transform.rotation;
                _hasInitializedSpawn = true;
                Debug.Log($"[Player] {gameObject.name} initial spawn saved at {_initialSpawnPosition}");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _gameManager = FindFirstObjectByType<GameManager>();
            
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // 디버그: 컨트롤러 상태 확인
            if (_controller == null || !_controller.enabled)
            {
                Debug.LogWarning("[Player] CharacterController is null or disabled!");
                if (_controller != null && !_controller.enabled)
                {
                    _controller.enabled = true;
                    Debug.Log("[Player] CharacterController re-enabled");
                }
            }

            // Skip input if game is not active
            if (_gameManager != null && !_gameManager.IsGameActive())
            {
                Debug.Log("[Player] Game is not active, skipping input");
                return;
            }

            // 디버그: 입력 상태 확인
            if (_input.move != Vector2.zero)
            {
                Debug.Log($"[Player] Movement input: {_input.move}");
            }

            HandleCombatInput();
            HandleDashInput();
            UpdateTimers();
            JumpAndGravity();
            GroundedCheck();
            Move();
            HandleMouseRotation();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on dash state and reload penalty
            float targetSpeed = MoveSpeed;
            
            if (_isDashing)
            {
                targetSpeed = DashSpeed;
            }
            else if (_isReloading)
            {
                targetSpeed *= (1f - ReloadSpeedPenalty);
            }

            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 카메라 방향 기준 이동
            Vector3 moveDirection = Vector3.zero;
            
            if (_input.move != Vector2.zero)
            {
                // 카메라 트랜스폼 가져오기
                Transform cameraTransform = _mainCamera != null ? _mainCamera.transform : Camera.main.transform;
                
                if (cameraTransform != null)
                {
                    // 카메라의 forward와 right 벡터 가져오기 (Y축 제거하여 지면 평면에서만 이동)
                    Vector3 cameraForward = cameraTransform.forward;
                    Vector3 cameraRight = cameraTransform.right;
                    
                    cameraForward.y = 0f;
                    cameraRight.y = 0f;
                    
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    
                    // 카메라 방향을 기준으로 이동 방향 계산
                    moveDirection = (cameraForward * _input.move.y + cameraRight * _input.move.x).normalized;
                }
                else
                {
                    // 카메라를 찾을 수 없으면 월드 좌표 사용 (폴백)
                    moveDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
                }
            }

            // move the player
            _controller.Move(moveDirection * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }
        
        private void HandleMouseRotation()
        {
            // Mouse rotation for top-down view
            if (Camera.main != null)
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                
                // Calculate intersection with ground plane at character's height
                Plane groundPlane = new Plane(Vector3.up, transform.position);
                float distance;
                
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 worldMousePosition = ray.GetPoint(distance);
                    Vector3 lookDirection = (worldMousePosition - transform.position).normalized;
                    lookDirection.y = 0;
                    
                    if (lookDirection.magnitude > 0.01f)
                    {
                        // 즉시 회전 (부드러운 회전 없이)
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        transform.rotation = targetRotation;
                    }
                }
            }
        }
        
        private void HandleCombatInput()
        {
            // Shooting with left mouse button
            if (Input.GetMouseButtonDown(0) && _canShoot && !_isReloading)
            {
                Shoot();
            }
            
            // 장전은 자동으로 시작되며, ReloadSystem에서 키 입력 처리
        }
        
        private void HandleDashInput()
        {
            // Dash with Left Shift
            if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing && _dashCooldownTimer <= 0f)
            {
                StartDash();
            }
        }
        
        private void Shoot()
        {
            if (BulletPrefab != null && FirePoint != null)
            {
                // 총알 생성 (FirePoint의 forward 방향으로 발사)
                GameObject bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);
                
                // Set bullet tag
                bullet.tag = "PlayerBullet";
                
                // Apply velocity to bullet
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = FirePoint.forward * BulletSpeed;
                }
                
                // Initialize bullet script if exists
                PlayerBullet bulletScript = bullet.GetComponent<PlayerBullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(this, BulletSpeed);
                }
                
                // 총소리 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound("Shoot");
                }
            }
            
            _canShoot = false;
            StartReload();
        }
        
        private void StartReload()
        {
            _isReloading = true;
            
            // 새로운 장전 시스템 시작
            if (ReloadSystem.Instance != null)
            {
                ReloadSystem.Instance.StartReload();
            }
            else
            {
                // ReloadSystem이 없으면 기존 방식 사용
                _reloadTimer = ReloadTime;
            }
        }
        
        public void CompleteReload()
        {
            // 장전 완료 (ReloadSystem에서 호출)
            _isReloading = false;
            _canShoot = true;
            _reloadTimer = 0f;
        }
        
        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
            
            // 대쉬 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("Dash");
            }
        }
        
        private void UpdateTimers()
        {
            // Dash timer
            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                }
            }
            
            // Dash cooldown timer
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.deltaTime;
            }
            
            // Reload timer (ReloadSystem이 없을 때만 사용)
            if (_isReloading && ReloadSystem.Instance == null)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                {
                    _isReloading = false;
                    _canShoot = true;
                }
            }
        }
        
        public void OnHit()
        {
            // Player got hit
            Debug.Log("[Player] Got hit! Respawning...");
            
            // 피격 사운드 재생
            if (SoundManager.Instance != null)
            {
                //SoundManager.Instance.PlaySound("PlayerHit");
            }
            
            // 플레이어 리스폰
            Respawn();
            
            // AI도 리스폰
            AIController aiController = FindFirstObjectByType<AIController>();
            if (aiController != null)
            {
                aiController.Respawn();
            }
        }
        
        public void OnEnemyHit()
        {
            // Player hit enemy
            Debug.Log("[Player] Hit enemy! Respawning...");
            
            // 플레이어 점수 추가
            if (ScoreUI.Instance != null)
            {
                
            }
            
            // 플레이어 리스폰
            Respawn();
            
            // AI도 리스폰
            AIController aiController = FindFirstObjectByType<AIController>();
            if (aiController != null)
            {
                aiController.Respawn();
            }
        }
        
        private void Respawn()
        {
            // CharacterController를 비활성화하고 위치 변경 후 다시 활성화
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
            ResetPlayerState();
            
            Debug.Log($"[Player] {gameObject.name} respawned at initial position {_initialSpawnPosition}");
        }
        
        public void ResetPlayerState()
        {
            // 플레이어 상태 초기화
            _isReloading = false;
            _canShoot = true;
            _isDashing = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
            _reloadTimer = 0f;
            _speed = 0f;
            _verticalVelocity = 0f;
            _animationBlend = 0f;
            
            // 애니메이터 리셋
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        
        // Getter methods for UI
        public float GetDashCooldownTimer()
        {
            return _dashCooldownTimer;
        }
        
        public float GetReloadTimer()
        {
            return _reloadTimer;
        }
        
        public bool IsReloading()
        {
            return _isReloading;
        }
        
        public bool CanShoot()
        {
            return _canShoot;
        }
    }
}