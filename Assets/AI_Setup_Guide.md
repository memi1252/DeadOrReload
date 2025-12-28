# AI 컨트롤러 설정 가이드

## 수정 완료 사항

### 1. 플레이어 이동 문제 해결
- **문제**: 카메라 방향에 따라 이동 방향이 바뀌는 문제
- **해결**: 월드 좌표 기준으로 직접 이동하도록 수정
- **결과**: W=앞, S=뒤, A=왼쪽, D=오른쪽으로 일관되게 이동

### 2. AI 컨트롤러 생성
- **파일**: `Scripts/AIController.cs`
- **특징**: ThirdPersonController와 동일한 기능 + ML-Agents 학습 기능
- **자동 조작**: 사용자 입력 없이 AI가 자동으로 움직임

## Unity에서 설정하기

### 플레이어 설정 (ThirdPersonController)

1. **Player 오브젝트 생성**:
   - Hierarchy > 3D Object > Capsule
   - 이름: "Player"
   - Position: (-5, 1, 0)

2. **컴포넌트 추가**:
   - Character Controller
   - ThirdPersonController (Scripts/ThirdPersonController.cs)
   - Player Input (Input System)
   - Starter Assets Inputs

3. **FirePoint 생성**:
   - Player의 자식으로 Empty GameObject 생성
   - 이름: "FirePoint"
   - Local Position: (0, 0, 0.5)

4. **Inspector 설정**:
   ```
   ThirdPersonController:
   - Move Speed: 5
   - Dash Speed: 10
   - Dash Duration: 0.3
   - Dash Cooldown: 2
   - Bullet Prefab: [총알 프리팹 드래그]
   - Fire Point: [FirePoint 드래그]
   - Bullet Speed: 20
   - Reload Time: 3
   - Reload Speed Penalty: 0.2
   - Ground Layers: Default
   ```

### AI 설정 (AIController)

1. **AI 오브젝트 생성**:
   - Hierarchy > 3D Object > Capsule
   - 이름: "AI"
   - Position: (5, 1, 0)

2. **컴포넌트 추가**:
   - Character Controller
   - AIController (Scripts/AIController.cs)
   - Behavior Parameters
   - Decision Requester
   - Ray Perception Sensor 3D (선택사항)

3. **FirePoint 생성**:
   - AI의 자식으로 Empty GameObject 생성
   - 이름: "FirePoint"
   - Local Position: (0, 0, 0.5)

4. **Behavior Parameters 설정**:
   ```
   Behavior Name: "AIController"
   Behavior Type: Default
   Vector Observation:
     - Space Size: 30
     - Stacked Vectors: 1
   Actions:
     - Continuous Actions: 2
     - Discrete Branches: 3
       - Branch 0 Size: 3 (회전: 없음/좌/우)
       - Branch 1 Size: 2 (사격: 안함/함)
       - Branch 2 Size: 2 (대시: 안함/함)
   ```

5. **Decision Requester 설정**:
   ```
   Decision Period: 5
   Take Actions Between Decisions: ✓
   ```

6. **AIController Inspector 설정**:
   ```
   AI Movement:
   - Move Speed: 5
   - Dash Speed: 10
   - Dash Duration: 0.3
   - Dash Cooldown: 2
   - Speed Change Rate: 10
   
   Combat:
   - Bullet Prefab: [총알 프리팹 드래그]
   - Fire Point: [FirePoint 드래그]
   - Bullet Speed: 20
   - Reload Time: 3
   - Reload Speed Penalty: 0.2
   
   AI Detection:
   - Detection Range: 15
   - Raycast Count: 16
   - Wall Layer: Wall
   - Enemy Layer: Player
   
   Grounded Check:
   - Ground Layers: Default
   ```

### 총알 프리팹 설정

1. **Bullet 프리팹 생성**:
   - Sphere (Scale: 0.1, 0.1, 0.1)
   - Rigidbody (Use Gravity: false)
   - Sphere Collider (Is Trigger: true)
   - Bullet 스크립트 (AI용)

2. **PlayerBullet 프리팹 생성**:
   - Sphere (Scale: 0.1, 0.1, 0.1)
   - Rigidbody (Use Gravity: false)
   - Sphere Collider (Is Trigger: true)
   - PlayerBullet 스크립트 (플레이어용)

### 레이어 설정

1. **레이어 생성**:
   - Layer 8: "Wall"
   - Layer 9: "Player"
   - Layer 10: "AI"

2. **오브젝트에 레이어 적용**:
   - Player 오브젝트 → Layer: Player
   - AI 오브젝트 → Layer: AI
   - 벽 오브젝트들 → Layer: Wall

### 카메라 설정

```
Main Camera:
- Position: (0, 15, 0)
- Rotation: (90, 0, 0)
- Projection: Orthographic
- Orthographic Size: 12
```

## 테스트 방법

### 1. 플레이어만 테스트
- Play 버튼 클릭
- WASD로 이동 (월드 좌표 기준)
- 마우스로 조준
- 마우스 좌클릭으로 사격
- Left Shift로 대시

### 2. AI 테스트 (수동 조작)
- AI 오브젝트의 Behavior Parameters에서:
  - Behavior Type을 "Heuristic Only"로 변경
- Play 버튼 클릭
- 화살표 키로 AI 이동
- Q/E로 회전
- Space로 사격
- Left Shift로 대시

### 3. AI 자동 조작 테스트
- AI 오브젝트의 Behavior Parameters에서:
  - Behavior Type을 "Default"로 변경
- Play 버튼 클릭
- AI가 자동으로 움직이고 플레이어를 추적하는지 확인

## ML-Agents 훈련

### 1. Python 환경 설정
```bash
conda create -n mlagents python=3.8
conda activate mlagents
pip install mlagents
```

### 2. 훈련 시작
```bash
mlagents-learn DeadOrReload_Config.yaml --run-id=AIController_v1
```

### 3. Unity에서 Play 버튼 클릭

## 조작법 요약

### 플레이어 (ThirdPersonController)
- **이동**: WASD (월드 좌표 기준)
- **조준**: 마우스 (자동 회전)
- **사격**: 마우스 좌클릭
- **대시**: Left Shift
- **재장전**: R (또는 자동)

### AI (AIController)
- **자동 모드**: AI가 자동으로 판단하여 행동
- **수동 모드**: 화살표 키 + Q/E + Space + Shift

## 문제 해결

### 플레이어가 이상하게 움직여요
- Character Controller가 제대로 설정되었는지 확인
- Ground Layers가 올바르게 설정되었는지 확인
- 카메라가 탑다운 뷰(위에서 아래)로 설정되었는지 확인

### AI가 움직이지 않아요
- Behavior Parameters의 Behavior Type 확인
- Decision Requester가 추가되었는지 확인
- AIController 스크립트가 제대로 연결되었는지 확인

### 총알이 발사되지 않아요
- Bullet Prefab이 설정되었는지 확인
- FirePoint가 올바른 위치에 있는지 확인
- Rigidbody가 총알 프리팹에 있는지 확인

이제 플레이어와 AI가 모두 정상적으로 작동합니다!