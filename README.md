# PLT Monitor

## Production Line Transform Monitoring System

Unity 생산 라인에서 16개 오브젝트의 Transform 데이터를 실시간 기록하고 그래프로 시각화하며 JSON/CSV로 저장하는 모니터링 시스템입니다.

![PLT Monitor Overview](media/screenshots/overview.png)

## 프로젝트 목표

- 오브젝트를 제어하는 시스템이 아닌 관찰·기록·분석 시스템을 구현합니다.
- 교육 과정 과제 제출용으로 제작한 독립 프로젝트입니다.
- 생산 라인 모니터링, 실시간 데이터 수집, 그래프 시각화와 파일 기반 데이터 관리를 하나의 대시보드로 구성합니다.

## 핵심 결과

| 항목 | 결과 |
|---|---|
| 추적 대상 | Box 16개 |
| 기록 프레임 | 오브젝트당 최근 600개 |
| 기록값 | Position / Rotation / Scale의 X, Y, Z |
| 그래프 모드 | Overview / Detail / Focus |
| 저장 형식 | JSON / CSV |
| Unity | 6000.3.10f1 |
| Windows Build | 완료 |

## 주요 기능

- 16개 박스의 생산 라인 순환
- Object Pool을 이용한 박스 재사용
- 오브젝트별 Transform Recorder와 통합 Recorder Manager
- Overview / Detail / Focus의 3단계 그래프 분석
- 선택 대상을 추적하는 Live View와 카메라 시점 프리셋
- Transform 변화의 상태·의미·위험도를 설명하는 Meaning Analyzer
- 저장 파일을 조회하고 선택하는 File Browser
- JSON/CSV Save 및 파일 Parse/Load 성공 여부 확인

`Load Selected`는 선택한 JSON/CSV 파일을 파싱하고 성공 여부와 상태 메시지를 표시합니다. 로드한 세션을 Recorder에 다시 주입하거나 과거 그래프를 복원하는 기능은 현재 범위에 포함되지 않습니다.

## 시스템 구조

```text
Transform Recorder
→ Recorder Manager
→ Graph Data Provider
→ UI Controller / Meaning Analyzer
→ Overview / Detail / Focus

Recorder Manager
→ SaveLoad Manager
→ JSON / CSV
→ File Browser
```

## 데이터 규모

```text
16 Objects × 600 Frames = 9,600 Frames
9 Transform values per frame
= 86,400 Transform values
```

각 프레임은 `frameIndex`, `timeStamp`와 Position/Rotation/Scale의 XYZ 값을 보관합니다.

## 화면 구성

### Overview

16개 박스의 최근 변화를 카드형 미니 그래프로 비교하여 병목, 편차, 정지 여부를 빠르게 탐색합니다.

![Overview](media/screenshots/overview.png)

### Detail

선택한 박스의 X/Y/Z와 All 그래프를 4분면에 표시하여 축별 차이를 비교합니다.

![Detail](media/screenshots/detail.png)

### Focus

선택한 단일 축을 크게 표시하여 값의 변화, 튐, 정지, 노이즈를 자세히 확인합니다.

![Focus](media/screenshots/focus.png)

### Live View

선택된 박스를 카메라가 추적하고 Selected, Metric, Status와 해석 정보를 함께 표시합니다.

![Live View](media/screenshots/live-view.png)

### Save / Load

JSON/CSV 목록 조회, 저장, 새로고침, 파일 선택과 파싱 결과 확인을 지원합니다.

![Save and Load](media/screenshots/save-load.png)

## 기술 스택

- Unity 6000.3.10f1
- C#
- Universal Render Pipeline 17.3.0
- Unity uGUI 2.0.0
- Input System 1.18.0
- Newtonsoft.Json 3.2.2
- Windows Standalone

### 외부 에셋 안내

개발 과정에서 CleanFlatIcon 외부 아이콘 에셋을 참고했지만, 그래프 Point Marker는 프로젝트 내부에서 직접 생성한 `Assets/Art/Graph/point_marker.png` Sprite로 교체했습니다. CleanFlatIcon 원본은 재배포 조건과 저장소 용량을 고려해 GitHub 저장소에 포함하지 않으며, PLT Monitor의 핵심 기능 실행에도 필요하지 않습니다. 자세한 내용은 [Third-Party Notices](THIRD_PARTY_NOTICES.md)를 참고하세요.

## 실행 방법

1. Unity Hub에서 이 저장소의 프로젝트 폴더를 추가합니다.
2. Unity `6000.3.10f1`로 프로젝트를 엽니다.
3. `Assets/Scenes/PL_TransformMonitor.unity`를 엽니다.
4. Play를 실행합니다.

## Windows 빌드 방법

1. Unity의 Build Profiles를 엽니다.
2. Windows Standalone을 선택합니다.
3. `Assets/Scenes/PL_TransformMonitor.unity`가 Scene 목록에 포함됐는지 확인합니다.
4. Build를 실행합니다.

빌드 결과물은 소스 저장소에 포함하지 않고 GitHub Release로 제공할 예정입니다.

## QA 결과

- Unity Console 에러 0개
- 16개 박스 순환과 Recorder 동작 확인
- 일반 실행 60FPS 이상 충족
- 기존 QA 기록상 일반 실행 100FPS 이상
- Overview / Detail / Focus, 오브젝트·축 선택, Live View 동작 확인
- JSON/CSV 파일 생성과 파싱 성공 여부 확인
- Windows Standalone Build 실행 확인

상세 결과는 [QA 및 성능](docs/06-qa-performance.md)과 [최종 기능 테스트 체크리스트](docs/qa/final-test-checklist.md)를 참고하세요.

## Known Issue

- Save JSON/CSV 클릭 순간 CPU 및 GC spike가 발생합니다.
- 순간적으로 약 40FPS 전후까지 하락한 뒤 기존 수준으로 회복합니다.
- 16 × 600 프레임 집계, 메인 스레드 직렬화와 파일 I/O가 함께 수행되는 구조의 영향입니다.
- 저장 순간을 제외한 일반 모니터링 구간은 기존 QA 기준을 충족합니다.

![Profiler](media/screenshots/profiler.png)

## 향후 개선

- 비동기 또는 작업 분할 방식의 저장
- 직렬화 전용 데이터 버퍼링
- 로드된 세션의 그래프 재생과 비교
- Unity Profiler 원본 캡처 보관
- 외부 에셋 출처와 라이선스 고지 지속 관리

## 문서

- [프로젝트 개요](docs/01-project-overview.md)
- [시스템 아키텍처](docs/02-system-architecture.md)
- [핵심 기능](docs/03-core-features.md)
- [그래프 UI](docs/04-graph-ui.md)
- [데이터 저장 및 로드](docs/05-data-save-load.md)
- [QA 및 성능](docs/06-qa-performance.md)
- [실행 및 빌드](docs/07-build-and-run.md)
- [최종 기능 테스트 체크리스트](docs/qa/final-test-checklist.md)

## 포트폴리오 강조점

- Recorder / Provider / Renderer / UI의 책임 분리
- uGUI 기반 단일·다중 시리즈 그래프 렌더링 직접 구성
- Overview → Detail → Focus로 이어지는 3단계 분석 UX
- 16개 대상의 실시간 Transform 기록과 롤링 버퍼
- JSON/CSV 및 파일 브라우저 기반 데이터 관리
- QA 결과와 성능 병목을 숨기지 않는 Known Issue 문서화
