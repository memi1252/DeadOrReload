# ThirdPersonController를 데드 오어 리로드용으로 사용하기

## 수정된 기능들

### 1. 이동 시스템
- **기본 이동**: WASD 키로 탑다운 뷰 이동
- **대시**: Left Shift로 순간 가속 (쿨다운 2초)
- **재장전 페널티**: 재장전 중 이동 속도 20% 감소

### 2. 전투 시스템
- **조준**: 마우스 커서 방향으로 자동 회전
- **사격**: 마우스 좌클릭으로 총알 발사
- **재장전**: 사격 후 자동 재장전 (3초) 또는 R키로 수동 재장전

### 3. 설정 방법

#### Unity Inspector에서 설정:
1. **Movement 섹션**:
   - Move Speed: 5 (기본 이동 속도)
   - Dash Speed: 10 (대시 속도)
   - Dash Duration: 0.3 (대시 지속 시간)
   - Dash Cooldown: 2 (대시 쿨다운)

2. **Combat 섹션**:
   - Bullet Prefab: 총알 프리팹 드래그
   - Fire Point: 총구 위치 Transform 드래그
   - Bullet Speed: 20 (총알 속도)
   - Reload Time: 3 (재장전 시간)
   - Reload Speed Penalty: 0.2 (재장전 중 속도 감소율)

#### 필수 설정:
1. **Fire Point 생성**:
   ```
   Player GameObject
   └── FirePoint (Empty GameObject)
       └── Position: (0, 0, 0.5) - 캐릭터 앞쪽
   ```

2. **카메라 설정**:
   - Position: (0, 15, 0)
   - Rotation: (90, 0, 0)
   - Projection: Orthographic
   - Size: 12

3. **총알 프리팹**:
   - Sphere 오브젝트 (Scale: 0.1, 0.1, 0.1)
   - Rigidbody (Use Gravity: false)
   - Sphere Collider (Is Trigger: true)
   - PlayerBullet 스크립트

### 4. 조작법
- **이동**: WASD
- **조준**: 마우스 (자동으로 캐릭터가 마우스 방향을 바라봄)
- **사격**: 마우스 좌클릭
- **대시**: Left Shift
- **수동 재장전**: R키

### 5. 게임 로직 연동
- GameManager가 있으면 게임 상태를 확인하여 비활성 시 입력 무시
- 적 명중/피격 시 GameManager의 EndRound 호출
- AI 에이전트와의 상호작용 지원

### 6. 기존 ThirdPersonController와의 차이점
- 3인칭 → 탑다운 뷰로 변경
- 점프 제거, 대시 추가
- 카메라 회전 → 마우스 조준으로 변경
- 스프린트 → 전투 시스템으로 변경
- 애니메이션 시스템 유지 (선택사항)

이제 기존 Starter Assets의 ThirdPersonController를 그대로 사용하면서 데드 오어 리로드 게임을 플레이할 수 있습니다!