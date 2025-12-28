# ML-Agents 훈련 가이드

## 준비 단계

### 1. Python 환경 확인
```bash
# 명령 프롬프트에서 실행
python --version
# Python 3.8-3.10이어야 함

pip install mlagents
# 설치 완료 후
mlagents-learn --help
```

### 2. Unity 설정
1. **ML-Agents 패키지 설치** (Package Manager)
2. **레이어 설정**: DeadOrReload > Setup Layers and Tags
3. **훈련 씬 생성**: DeadOrReload > Setup Training Scene

## 훈련 시작

### 방법 1: 기본 훈련 (권장)

1. **명령 프롬프트 열기**
2. **Unity 프로젝트 폴더로 이동**:
   ```bash
   cd "D:\project\Dead or Reload\Assets"
   ```

3. **훈련 시작**:
   ```bash
   mlagents-learn DeadOrReload_Config.yaml --run-id=AIController_v1
   ```

4. **Unity에서 Play 버튼 클릭**
5. **훈련 진행 상황 확인**

### 방법 2: 빠른 테스트 훈련

```bash
mlagents-learn DeadOrReload_Config.yaml --run-id=test --time-scale=20
```

### 방법 3: 이어서 훈련

```bash
mlagents-learn DeadOrReload_Config.yaml --run-id=AIController_v1 --resume
```

## 훈련 모니터링

### TensorBoard로 진행 상황 확인:
```bash
# 새 명령 프롬프트 창에서
tensorboard --logdir results
```
브라우저에서 http://localhost:6006 접속

### 훈련 중 확인할 지표:
- **Cumulative Reward**: 점점 증가해야 함
- **Episode Length**: 안정화되어야 함
- **Policy Loss**: 점점 감소해야 함

## 훈련된 모델 사용

### 1. 모델 파일 위치:
```
results/AIController_v1/AIController/AIController-[숫자].onnx
```

### 2. Unity에서 모델 적용:
1. **모델 파일을 Assets 폴더로 복사**
2. **AI 오브젝트 선택**
3. **Behavior Parameters**에서:
   - Behavior Type: "Inference Only"
   - Model: 복사한 .onnx 파일 드래그

### 3. 테스트:
- Play 버튼 클릭
- AI가 학습된 행동을 보여줌

## 훈련 팁

### 빠른 훈련을 위해:
1. **Time Scale 증가**:
   ```bash
   mlagents-learn DeadOrReload_Config.yaml --run-id=fast --time-scale=20
   ```

2. **Unity에서 설정**:
   - Quality Settings: Fastest
   - Resolution: 낮게 설정
   - VSync 끄기

### 좋은 훈련을 위해:
1. **충분한 시간**: 최소 30분-1시간
2. **안정적인 환경**: 다른 프로그램 최소화
3. **여러 에이전트**: 4-16개 동시 훈련

## 문제 해결

### "No module named 'mlagents'" 오류:
```bash
pip install --upgrade mlagents
```

### Unity에서 연결 안됨:
1. 방화벽 확인
2. Unity Play 버튼이 눌러졌는지 확인
3. Behavior Parameters 설정 확인

### 훈련이 너무 느림:
1. Time Scale 증가
2. 그래픽 품질 낮추기
3. 에이전트 수 줄이기

### 보상이 증가하지 않음:
1. 보상 함수 확인
2. 학습률 조정
3. 네트워크 크기 조정

## 훈련 단계별 목표

### 1단계 (0-100k steps): 기본 이동 학습
- 벽에 부딪히지 않기
- 랜덤하게 움직이기

### 2단계 (100k-300k steps): 전투 기초 학습
- 적 발견하기
- 사격하기
- 기본적인 회피

### 3단계 (300k-500k steps): 전략 학습
- 엄폐물 활용
- 거리 조절
- 효과적인 사격

### 4단계 (500k+ steps): 고급 전술
- 예측 사격
- 복잡한 회피 패턴
- 상황 판단

## 성공적인 훈련의 신호

✅ **Cumulative Reward가 지속적으로 증가**
✅ **Episode Length가 안정화**
✅ **AI가 벽에 덜 부딪힘**
✅ **적을 향해 이동**
✅ **적절한 타이밍에 사격**

## 훈련 완료 후

1. **모델 백업**: results 폴더 전체 복사
2. **다양한 난이도 테스트**
3. **플레이어와 대전**
4. **필요시 추가 훈련**

훈련 시간: 보통 30분-2시간 (컴퓨터 성능에 따라)