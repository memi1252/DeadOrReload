# 데드 오어 리로드 (Dead or Reload)

## 프로젝트 개요
1:1 심리 저격 액션 게임으로, ML-Agents를 사용한 지능형 AI와의 대결을 구현합니다.

## 필요한 패키지
1. **ML-Agents** (Unity Package Manager에서 설치)
   - Window > Package Manager > Unity Registry
   - "ML-Agents" 검색 후 설치

2. **Input System** (이미 프로젝트에 포함됨)

## 프로젝트 설정

### 1. 씬 설정
1. 새로운 씬 생성: `GameScene`
2. 다음 오브젝트들을 생성:

#### 환경 설정
- **Ground**: Plane 오브젝트 (20x20 크기)
- **Walls**: Cube 오브젝트들로 경계 생성
- **Central Obstacle**: ㅁ자형 장애물 (4개의 Cube로 구성)

#### 플레이어 설정
- **Player**: Capsule 오브젝트
  - `PlayerController.cs` 스크립트 추가
  - `Rigidbody` 컴포넌트 추가
  - `Capsule Collider` 설정 (IsTrigger = false)
  - Tag: "Player"

#### AI 에이전트 설정
- **AI Agent**: Capsule 오브젝트
  - `DeadOrReloadAgent.cs` 스크립트 추가
  - `Rigidbody` 컴포넌트 추가
  - `Capsule Collider` 설정 (IsTrigger = false)
  - `Behavior Parameters` 컴포넌트 추가
    - Behavior Name: "DeadOrReloadAgent"
    - Vector Observation Space Size: 30
    - Continuous Actions: 2
    - Discrete Actions: 3 (각각 크기 3, 2, 2)
  - `Decision Requester` 컴포넌트 추가
    - Decision Period: 5
  - Tag: "AI"

#### 총알 프리팹
- **Bullet**: Sphere 오브젝트 (작은 크기)
  - `Bullet.cs` 또는 `PlayerBullet.cs` 스크립트 추가
  - `Rigidbody` 컴포넌트 추가
  - `Sphere Collider` 설정 (IsTrigger = true)
  - 프리팹으로 저장

#### 게임 매니저
- **GameManager**: Empty GameObject
  - `GameManager.cs` 스크립트 추가

#### 훈련 영역 (ML-Agents용)
- **Training Area**: Empty GameObject
  - `TrainingArea.cs` 스크립트 추가
  - 여러 개의 에이전트 복사본 배치 (훈련 가속화)

### 2. 레이어 설정
1. Edit > Project Settings > Tags and Layers
2. 다음 레이어 추가:
   - Layer 8: "Wall"
   - Layer 9: "Player"
   - Layer 10: "AI"
   - Layer 11: "Bullet"

### 3. 태그 설정
- "Player"
- "AI" 
- "Wall"
- "PlayerBullet"
- "AIBullet"

### 4. 물리 설정
1. Edit > Project Settings > Physics
2. Layer Collision Matrix에서 다음 설정:
   - Bullet과 Wall: 충돌 O
   - Bullet과 Player/AI: 충돌 O
   - Player와 AI: 충돌 X (서로 통과)

## ML-Agents 훈련

### 1. 환경 설정
```bash
# Python 환경 설정 (Anaconda 권장)
conda create -n mlagents python=3.8
conda activate mlagents

# ML-Agents 설치
pip install mlagents
```

### 2. 훈련 실행
```bash
# 프로젝트 루트 디렉토리에서 실행
mlagents-learn DeadOrReload_Config.yaml --run-id=DeadOrReload_v1

# Unity에서 Play 버튼 클릭하여 훈련 시작
```

### 3. 훈련 모니터링
```bash
# TensorBoard로 훈련 진행 상황 확인
tensorboard --logdir results
```

## 게임 플레이

### 플레이어 조작
- **이동**: WASD 키
- **조준**: 마우스
- **사격**: 마우스 좌클릭
- **대시**: Left Shift
- **재장전**: R 키

### 게임 규칙
- 원샷 원킬: 한 발로 승부 결정
- 재장전 시간: 3초 (이동 속도 20% 감소)
- 대시: 쿨다운 2초
- 제한 시간: 60초
- 승리 조건: 적 사살 또는 시간 내 생존

## 개발 일정 (3일)

### 1일차: 프로토타이핑
- [x] 기본 이동 및 사격 시스템
- [x] AI 에이전트 기본 구조
- [x] 보상 체계 설계

### 2일차: AI 훈련
- [ ] Self-Play 환경 구축
- [ ] 대량 에이전트 복제 (816개)
- [ ] 가속 학습 진행

### 3일차: 폴리싱
- [ ] 네온 그래픽 효과
- [ ] 사운드 시스템
- [ ] UI 완성
- [ ] 최종 빌드

## 파일 구조
```
Assets/
├── Scripts/
│   ├── DeadOrReloadAgent.cs      # ML-Agents AI 컨트롤러
│   ├── PlayerController.cs       # 플레이어 컨트롤러
│   ├── Bullet.cs                 # AI 총알
│   ├── PlayerBullet.cs          # 플레이어 총알
│   ├── GameManager.cs           # 게임 로직 관리
│   ├── TrainingArea.cs          # ML-Agents 훈련 환경
│   ├── VisualEffects.cs         # 네온 효과
│   ├── AudioManager.cs          # 사운드 관리
│   └── UIManager.cs             # UI 관리
├── Scenes/
│   └── GameScene.unity          # 메인 게임 씬
├── Prefabs/
│   ├── Player.prefab
│   ├── AIAgent.prefab
│   └── Bullet.prefab
└── DeadOrReload_Config.yaml     # ML-Agents 설정
```

## 문제 해결

### ML-Agents 관련
- **에이전트가 학습하지 않는 경우**: Behavior Parameters의 설정 확인
- **훈련이 느린 경우**: Time Scale을 높이거나 에이전트 수 증가
- **보상이 수렴하지 않는 경우**: 보상 함수 재조정

### 성능 최적화
- 훈련 시 그래픽 품질 낮추기
- 불필요한 렌더링 비활성화
- 물리 연산 최적화

## 추가 기능 아이디어
- 다양한 맵 레이아웃
- 무기 종류 추가
- 관전자 모드
- 리플레이 시스템
- 온라인 멀티플레이어