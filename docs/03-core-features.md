# 핵심 기능

## Conveyor

`scr_ConveyorItemMover`가 StartPoint와 EndPoint를 기준으로 박스를 이동합니다. 이동, Spawn, 재활용의 책임을 분리하여 개별 박스는 자신의 이동만 담당합니다.

## Box Pool

`scr_ConveyorSpawnManager`가 16개 박스를 Pool로 관리합니다. Spawn 영역이 비어 있고 지정 간격이 지난 경우 비활성 박스를 재사용하며 EndPoint에 도달한 박스를 다시 비활성화합니다.

## Transform Recorder

각 Box에는 `scr_TransformRecorder`가 연결돼 있습니다. `Update`에서 월드 Position, Euler Rotation, Local Scale의 XYZ를 기록하고 최근 600프레임만 유지합니다.

## Recorder Manager

`scr_TransformRecorderManager`는 Scene의 Recorder를 수집하고 전체 Start, Stop, Clear/Restart를 제공합니다. 그래프와 저장 기능은 이 관리 계층을 통해 동일한 Recorder 목록을 사용합니다.

## Live View

선택된 Box를 카메라가 추적하며 정면, 측면, 사선 프리셋과 전체 확대 패널을 제공합니다. 화면에는 대상, 현재 Metric, Recording 상태가 함께 표시됩니다.

![Live View](../media/screenshots/live-view.png)

## Meaning Analyzer

프로젝트는 Position Z를 컨베이어의 전방 이동축으로 해석합니다. Position X/Y, Rotation XYZ, Scale XYZ의 변화량과 범위를 이용해 상태, 의미, 위험도와 인사이트 문구를 생성합니다. 이는 규칙 기반 해석이며 학습형 예측 모델은 아닙니다.

