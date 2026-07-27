# 시스템 아키텍처

## Scene 구조

메인 Scene은 `Assets/Scenes/PL_TransformMonitor.unity`이며 Build Settings에 활성화돼 있습니다.

```text
PL_TransformMonitor
├─ EventSystem
└─ PL_Root
   ├─ Environment
   ├─ ProductionLine
   ├─ TrackObjects
   ├─ Props
   ├─ CameraRig
   ├─ Debug FPS
   └─ GraphSystem
```

`PL_Root`는 `PF_PL_Root.prefab` 인스턴스입니다. ProductionLine, TrackObjects, GraphSystem 등 주요 영역도 중첩 Prefab으로 분리돼 있습니다.

## PL_Root 하위 구조

```text
ProductionLine
├─ Conveyor
├─ StartPoint
├─ EndPoint
└─ FallBox

TrackObjects
├─ Box_01 ... Box_16
└─ BoxPool

GraphSystem
├─ GraphDataProvider
├─ GraphUIController
├─ GraphLiveViewCamera
├─ OverviewController
├─ GraphTestRunner
└─ GraphCanvas
```

## Recorder에서 UI까지의 데이터 흐름

```text
Box Transform
→ scr_TransformRecorder
→ scr_TransformRecorderManager
→ scr_TransformGraphDataProvider
→ scr_TransformGraphUIController
├─ scr_TransformGraphOverviewController
├─ scr_TransformGraphDetailQuadController
├─ scr_TransformGraphRenderer
├─ scr_TransformGraphMeaningAnalyzer
└─ scr_TransformGraphLiveViewController
```

저장은 Recorder Manager의 데이터를 별도 DTO로 집계한 후 JSON 또는 CSV로 직렬화합니다.

```text
Recorder Manager
→ TransformMonitoringSaveData
→ scr_TransformDataSaveLoadManager
├─ NST_Json
└─ NST_CSV
→ scr_TransformFileBrowserUI
```

## 핵심 스크립트 역할

| 영역 | 스크립트 | 역할 |
|---|---|---|
| Data | `TransformFrameData` | 한 프레임의 시간과 Transform XYZ 저장 |
| Data | `TransformObjectRecordData` | 오브젝트 이름과 프레임 목록 저장 |
| Data | `TransformMonitoringSaveData` | 전체 세션과 오브젝트 목록 저장 |
| Data | `scr_TransformRecorder` | 오브젝트별 600프레임 롤링 기록 |
| Data | `scr_TransformRecorderManager` | Recorder 검색 및 일괄 제어 |
| Graph | `scr_TransformGraphDataProvider` | Recorder 데이터를 GraphPoint로 변환 |
| Graph | `scr_TransformGraphRenderer` | 단일·다중 시리즈와 축·Grid 렌더링 |
| Graph | `scr_TransformGraphUIController` | 모드·대상·카테고리·축 상태 총괄 |
| Graph | `scr_TransformGraphOverviewController` | Overview 카드 생성과 갱신 |
| Graph | `scr_TransformOverviewGraphCard` | 개별 박스 미니 그래프 표시 |
| Graph | `scr_TransformGraphDetailQuadController` | X/Y/Z/All 4분면 표시 |
| Graph | `scr_TransformGraphMeaningAnalyzer` | 값 변화의 상태·위험·인사이트 해석 |
| Live | `scr_TransformGraphLiveViewController` | 추적 카메라와 확대 화면 제어 |
| Save | `scr_TransformDataSaveLoadManager` | 세션 집계, JSON/CSV 저장 및 파싱 |
| Save | `scr_TransformFileBrowserUI` | 파일 목록, 선택, 상태 메시지 |
| Save | `NST_Json` | JSON 직렬화·역직렬화 |
| Save | `NST_CSV` | CSV 생성·파싱과 UTF-8 파일 처리 |
| Conveyor | `scr_ConveyorSpawnManager` | Pool 초기화, Spawn과 재활용 |
| Conveyor | `scr_ConveyorItemMover` | 개별 Box의 구간 이동 |
| Conveyor | `scr_AutoRotateObject` | 테스트용 회전 변화 |
| Conveyor | `scr_AutoScaleObject` | 테스트용 Scale 변화 |
| Utility | `FPSText` | FPS 표시 |
| Utility | `scr_TimeDisplay` | 날짜와 시간 표시 |
| Test | `scr_TransformGraphTestRunner` | 그래프 연결 임시 테스트 |

![Project scripts](../media/screenshots/project-assets.png)

