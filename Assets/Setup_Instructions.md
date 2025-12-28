# Unity 프로젝트 설정 가이드

## 1. ML-Agents 패키지 설치
1. Unity Editor에서 Window > Package Manager 열기
2. 좌상단 드롭다운에서 "Unity Registry" 선택
3. 검색창에 "ML-Agents" 입력
4. "ML-Agents" 패키지 찾아서 Install 클릭
5. 설치 완료까지 대기

## 2. 씬 설정
1. Scenes 폴더의 GameScene.unity 열기
2. 기존 오브젝트들 삭제 (Main Camera, Directional Light 제외)

## 3. 환경 오브젝트 생성

### Ground (바닥)
1. Hierarchy에서 우클릭 > 3D Object > Plane
2. 이름을 "Ground"로 변경
3. Transform: Position (0, 0, 0), Scale (2, 1, 2)

### 경계 벽들
1. Hierarchy에서 우클릭 > 3D Object > Cube
2. 이름을 "Wall_North"로 변경
3. Transform: Position (0, 1, 10), Scale (20, 2, 1)
4. 같은 방식으로 3개 더 생성:
   - Wall_South: Position (0, 1, -10), Scale (20, 2, 1)
   - Wall_East: Position (10, 1, 0), Scale (1, 2, 20)
   - Wall_West: Position (-10, 1, 0), Scale (1, 2, 20)

### 중앙 장애물 (ㅁ자형)
1. 4개의 Cube 생성하여 ㅁ자 모양 배치:
   - Obstacle_1: Position (-2, 0.5, 2), Scale (4, 1, 1)
   - Obstacle_2: Position (2, 0.5, 2), Scale (4, 1, 1)
   - Obstacle_3: Position (-2, 0.5, -2), Scale (4, 1, 1)
   - Obstacle_4: Position (2, 0.5, -2), Scale (4, 1, 1)

## 4. 레이어 및 태그 설정

### 레이어 생성
1. Edit > Project Settings > Tags and Layers
2. Layers에 다음 추가:
   - Layer 8: "Wall"
   - Layer 9: "Player" 
   - Layer 10: "AI"
   - Layer 11: "Bullet"

### 태그 생성
1. 같은 창에서 Tags에 다음 추가:
   - "Player"
   - "AI"
   - "Wall"
   - "PlayerBullet"
   - "AIBullet"

### 오브젝트에 레이어/태그 적용
1. 모든 Wall 오브젝트들 선택
2. Inspector에서 Layer를 "Wall"로, Tag를 "Wall"로 설정

## 5. 플레이어 생성

### 플레이어 오브젝트
1. Hierarchy에서 우클릭 > 3D Object > Capsule
2. 이름을 "Player"로 변경
3. Transform: Position (-5, 1, 0)
4. Layer를 "Player"로, Tag를 "Player"로 설정

### 플레이어 컴포넌트 추가
1. Player 오브젝트 선택
2. Inspector에서 Add Component 클릭
3. 다음 컴포넌트들 추가:
   - Rigidbody (Freeze Rotation X, Z 체크)
   - PlayerController (Scripts/PlayerController.cs)

### Fire Point 생성
1. Player의 자식으로 Empty GameObject 생성
2. 이름을 "FirePoint"로 변경
3. Transform: Position (0, 0, 0.5) - 캡슐 앞쪽

## 6. AI 에이전트 생성

### AI 오브젝트
1. Hierarchy에서 우클릭 > 3D Object > Capsule
2. 이름을 "AIAgent"로 변경
3. Transform: Position (5, 1, 0)
4. Layer를 "AI"로, Tag를 "AI"로 설정

### AI 컴포넌트 추가
1. AIAgent 오브젝트 선택
2. Inspector에서 다음 컴포넌트들 추가:
   - Rigidbody (Freeze Rotation X, Z 체크)
   - DeadOrReloadAgent (Scripts/DeadOrReloadAgent.cs)
   - Behavior Parameters
   - Decision Requester

### Behavior Parameters 설정
1. Behavior Name: "DeadOrReloadAgent"
2. Vector Observation Space Size: 30
3. Stacked Vectors: 1
4. Continuous Actions: 2
5. Discrete Actions: 3개 브랜치 (크기: 3, 2, 2)

### Decision Requester 설정
1. Decision Period: 5
2. Take Actions Between Decisions: 체크

### AI Fire Point 생성
1. AIAgent의 자식으로 Empty GameObject 생성
2. 이름을 "FirePoint"로 변경
3. Transform: Position (0, 0, 0.5)

## 7. 총알 프리팹 생성

### 총알 오브젝트
1. Hierarchy에서 우클릭 > 3D Object > Sphere
2. 이름을 "Bullet"로 변경
3. Transform: Scale (0.1, 0.1, 0.1)
4. Layer를 "Bullet"로 설정

### 총알 컴포넌트
1. Bullet 오브젝트 선택
2. 다음 컴포넌트들 추가:
   - Rigidbody (Use Gravity 체크 해제)
   - Sphere Collider (Is Trigger 체크)
   - Bullet (Scripts/Bullet.cs)

### 프리팹 생성
1. Bullet 오브젝트를 Project 창으로 드래그
2. Prefabs 폴더 생성 후 그 안에 저장
3. Hierarchy에서 Bullet 오브젝트 삭제

## 8. 게임 매니저 설정

### 게임 매니저 오브젝트
1. Hierarchy에서 우클릭 > Create Empty
2. 이름을 "GameManager"로 변경
3. GameManager (Scripts/GameManager.cs) 컴포넌트 추가

## 9. 스크립트 연결

### PlayerController 설정
1. Player 오브젝트 선택
2. PlayerController 컴포넌트에서:
   - Bullet Prefab: 생성한 Bullet 프리팹 드래그
   - Fire Point: Player의 FirePoint 자식 오브젝트 드래그

### DeadOrReloadAgent 설정
1. AIAgent 오브젝트 선택
2. DeadOrReloadAgent 컴포넌트에서:
   - Bullet Prefab: 생성한 Bullet 프리팹 드래그
   - Fire Point: AIAgent의 FirePoint 자식 오브젝트 드래그
   - Wall Layer: "Wall" 레이어 선택
   - Enemy Layer: "Player" 레이어 선택

## 10. 카메라 설정
1. Main Camera 선택
2. Transform: Position (0, 15, 0), Rotation (90, 0, 0)
3. Camera 컴포넌트에서 Projection을 "Orthographic"으로 변경
4. Size: 12

## 11. 테스트
1. Play 버튼 클릭
2. WASD로 플레이어 이동 확인
3. 마우스 클릭으로 총알 발사 확인
4. AI 에이전트가 랜덤하게 움직이는지 확인

## 다음 단계: ML-Agents 훈련
프로젝트가 정상 작동하면 Python 환경에서 ML-Agents 훈련을 시작할 수 있습니다.