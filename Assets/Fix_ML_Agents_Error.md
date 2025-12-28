# ML-Agents 에러 수정 가이드

## 발생한 에러
```
IndexOutOfRangeException: Index was outside the bounds of the array.
```

## 원인
Behavior Parameters의 Discrete Actions 설정이 잘못되었습니다.

## 해결 방법

### 1단계: AI 오브젝트 선택
Unity에서 AI 오브젝트를 선택합니다.

### 2단계: Behavior Parameters 설정 수정

**Behavior Parameters** 컴포넌트에서 다음과 같이 설정:

```
Behavior Name: AIController
Behavior Type: Default

Vector Observation:
├─ Space Size: 30
└─ Stacked Vectors: 1

Actions:
├─ Continuous Actions: 2
└─ Discrete Actions: 
    ├─ Size: 3 (브랜치 개수)
    ├─ Branch 0 Size: 3 (회전: 없음/좌/우)
    ├─ Branch 1 Size: 2 (사격: 안함/함)  
    └─ Branch 2 Size: 2 (대시: 안함/함)
```

### 3단계: 상세 설정 방법

1. **AI 오브젝트 선택**
2. **Inspector**에서 **Behavior Parameters** 찾기
3. **Actions** 섹션 확장
4. **Continuous Actions**: `2`로 설정
5. **Discrete Actions**: `3`으로 설정
6. **Discrete Actions** 아래 나타나는 **Branch 0, 1, 2**를 각각:
   - **Branch 0 Size**: `3`
   - **Branch 1 Size**: `2`
   - **Branch 2 Size**: `2`

### 4단계: 설정 확인

올바른 설정:
```
Behavior Parameters:
├─ Behavior Name: "AIController"
├─ Behavior Type: Default
├─ Team ID: 0
├─ Use Child Sensors: ✓
├─ Use Child Actuators: ✓
├─ Observable Attribute Handling: Ignore
├─ Vector Observation:
│   ├─ Space Size: 30
│   └─ Stacked Vectors: 1
└─ Actions:
    ├─ Continuous Actions: 2
    └─ Discrete Actions: 3
        ├─ Branch 0 Size: 3
        ├─ Branch 1 Size: 2
        └─ Branch 2 Size: 2
```

### 5단계: Decision Requester 설정

**Decision Requester** 컴포넌트:
```
Decision Period: 5
Take Actions Between Decisions: ✓
```

## 테스트 방법

### 수동 조작 테스트:
1. **Behavior Type**을 **"Heuristic Only"**로 변경
2. **Play** 버튼 클릭
3. **화살표 키**로 이동 테스트
4. **Q/E**로 회전 테스트
5. **Space**로 사격 테스트
6. **Left Shift**로 대시 테스트

### 자동 AI 테스트:
1. **Behavior Type**을 **"Default"**로 변경
2. **Play** 버튼 클릭
3. AI가 랜덤하게 움직이는지 확인

## 추가 문제 해결

### "No Behavior Parameters found" 에러:
1. AI 오브젝트에 **Behavior Parameters** 컴포넌트 추가
2. **Decision Requester** 컴포넌트 추가

### AI가 움직이지 않는 경우:
1. **Character Controller** 컴포넌트 확인
2. **AIController** 스크립트 확인
3. **Ground Layers** 설정 확인

### 훈련이 시작되지 않는 경우:
1. **Behavior Name**이 YAML 파일과 일치하는지 확인
2. **mlagents-learn** 명령어 다시 실행
3. Unity **Play** 버튼 다시 클릭

## 올바른 YAML 설정

`DeadOrReload_Config.yaml` 파일:
```yaml
behaviors:
  AIController:  # Behavior Name과 일치해야 함
    trainer_type: ppo
    hyperparameters:
      batch_size: 512
      buffer_size: 5120
      learning_rate: 3.0e-4
    network_settings:
      hidden_units: 128
      num_layers: 2
    max_steps: 1000000
    time_horizon: 500
```

## 성공 확인

✅ **에러 메시지 사라짐**
✅ **AI가 수동 조작됨** (Heuristic Only 모드)
✅ **AI가 자동으로 움직임** (Default 모드)
✅ **훈련 시작됨** (mlagents-learn 실행 후)

이제 에러 없이 AI 훈련을 시작할 수 있습니다!