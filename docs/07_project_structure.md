# 07. Project Structure

## 저장소 구조

```text
plt_monitor
├─ Assets
│  ├─ Art
│  ├─ Docs
│  ├─ Materials
│  ├─ Models
│  ├─ Prefabs
│  ├─ Scenes
│  ├─ Scripts
│  └─ Settings
├─ Packages
├─ ProjectSettings
├─ docs
│  ├─ README.md
│  ├─ 01_overview.md
│  ├─ 02_architecture.md
│  ├─ 03_features.md
│  ├─ 04_data_flow.md
│  ├─ 05_validation.md
│  ├─ 06_project_scope.md
│  ├─ 07_project_structure.md
│  ├─ images
│  ├─ presentation
│  │  └─ plt_monitor.pptx
│  └─ qa
│     └─ final_test_checklist.md
├─ THIRD_PARTY_NOTICES.md
└─ README.md
```

## Unity 프로젝트 구조

```text
Assets
├─ Art
│  └─ Graph
│     └─ point_marker.png
├─ Docs
│  ├─ PLT_Monitor_.pptx
│  └─ 프로젝트 참고 문서
├─ Materials
├─ Models
│  ├─ Box.fbx
│  ├─ Conveyor.fbx
│  ├─ Ground.fbx
│  ├─ Palette.fbx
│  └─ Wall.fbx
├─ Prefabs
│  ├─ Basic
│  ├─ Control
│  ├─ Scne Conveyor
│  └─ UI
├─ Scenes
│  └─ PL_TransformMonitor.unity
├─ Scripts
│  ├─ Data
│  ├─ ProductionLine
│  ├─ SaveLoad
│  └─ UI
└─ Settings
```

## 주요 Prefab

| 폴더 | 주요 Prefab | 역할 |
|:---|:---|:---|
| `Basic` | `PF_Box`, `PF_Conveyor`, `PF_GraphArea`, `PF_OverviewGraphCard` | 생산 라인 기본 오브젝트와 그래프 카드 |
| `Control` | `PF_GraphDataProvider`, `PF_GraphUIController`, `PF_OverviewController` | 그래프 데이터·상태·Overview 제어 |
| `Scne Conveyor` | `PF_ProductionLine`, `PF_TrackObjects`, `PF_CameraRig`, `PF_Debug_FPS` | 생산 라인, Box Pool, 카메라와 FPS |
| `UI/Panel` | Axis·Center·CurrentState·DetailQuad·FileBrowser·LiveView Panel | 기능별 UI 패널 |
| Root | `PF_PL_Root`, `PF_GraphSystem`, `PF_GraphCanvas` | Scene과 그래프 시스템 상위 구조 |

## Data Scripts

| 파일 | 역할 |
|:---|:---|
| `TransformFrameData.cs` | 프레임 인덱스·시간과 9개 Transform 값 |
| `TransformObjectRecordData.cs` | 대상명과 프레임 목록 |
| `TransformMonitoringSaveData.cs` | 세션 정보와 전체 대상 목록 |
| `scr_TransformRecorder.cs` | 대상별 최대 600프레임 기록 |
| `scr_TransformRecorderManager.cs` | Recorder 검색과 일괄 제어 |
| `NST_Json.cs` | Newtonsoft.Json 기반 저장·복원 |
| `NST_CSV.cs` | CSV 생성·파싱과 UTF-8 파일 처리 |
| `FPSText.cs` | 런타임 FPS 표시 |

## ProductionLine Scripts

| 파일 | 역할 |
|:---|:---|
| `scr_ConveyorSpawnManager.cs` | 16개 Box Pool 초기화, Spawn과 회수 |
| `scr_ConveyorItemMover.cs` | 개별 Box 이동과 EndPoint 판정 |
| `scr_AutoRotateObject.cs` | Rotation 변화 테스트 |
| `scr_AutoScaleObject.cs` | Scale 변화 테스트 |

## SaveLoad Scripts

| 파일 | 역할 |
|:---|:---|
| `scr_TransformDataSaveLoadManager.cs` | 세션 집계, JSON·CSV 저장과 파싱 |
| `scr_TransformFileBrowserUI.cs` | 파일 목록, 선택, 저장·로드 버튼과 상태 메시지 |

## UI Scripts

| 파일 | 역할 |
|:---|:---|
| `scr_TransformGraphDataProvider.cs` | Recorder 데이터를 GraphPoint로 변환 |
| `scr_TransformGraphUIController.cs` | 모드·대상·카테고리·축 상태 총괄 |
| `scr_TransformGraphOverviewController.cs` | Overview 카드 생성과 갱신 |
| `scr_TransformOverviewGraphCard.cs` | 대상별 미니 그래프와 상태 |
| `scr_TransformGraphDetailQuadController.cs` | X·Y·Z·All 4분면 |
| `scr_TransformGraphRenderer.cs` | Grid, 축과 그래프 시리즈 렌더링 |
| `scr_TransformGraphLiveViewController.cs` | 선택 대상 추적과 Live View |
| `scr_TransformGraphMeaningAnalyzer.cs` | 규칙 기반 상태·위험·인사이트 |
| `scr_TransformGraphTestRunner.cs` | 그래프 연결 테스트 |
| `scr_TimeDisplay.cs` | 날짜와 시간 표시 |

## 주요 패키지

| 패키지 | 버전 | 용도 |
|:---|---:|:---|
| Universal Render Pipeline | 17.3.0 | 렌더링 |
| Unity uGUI | 2.0.0 | 모니터링 UI |
| Input System | 1.18.0 | 입력 처리 |
| Newtonsoft Json | 3.2.2 | JSON 직렬화·역직렬화 |

외부 에셋과 라이선스 범위는 [Third-Party Notices](../THIRD_PARTY_NOTICES.md)에서 확인할 수 있습니다.

---

[문서 목차](README.md) · [프로젝트 README](../README.md)
