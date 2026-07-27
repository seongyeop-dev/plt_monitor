# 프로젝트 개요

## 배경

PLT Monitor는 생산 라인 위에서 이동하는 여러 오브젝트의 상태를 관찰하기 위해 제작한 Unity 기반 Transform 모니터링 프로젝트입니다. 오브젝트 제어보다 데이터 기록, 비교, 해석과 보존에 초점을 둡니다.

## 목표

- 이동 중인 16개 오브젝트의 Transform을 실시간 수집
- Position / Rotation / Scale의 XYZ를 프레임 단위로 기록
- 전체 흐름과 개별 축을 단계적으로 분석할 수 있는 그래프 UI 구성
- JSON/CSV 파일 저장과 파싱 결과 확인
- 실제 실행과 성능을 QA 체크리스트로 검증

## 범위

프로젝트 범위에는 Conveyor 시뮬레이션, Object Pool, Transform Recorder, 3단계 그래프 UI, Live View, Meaning Analyzer, File Browser와 Windows Build가 포함됩니다.

파일 Load는 JSON/CSV 파싱 및 성공 상태 표시까지 지원합니다. 로드 데이터를 실시간 Recorder에 재주입하거나 과거 세션 그래프를 재생하는 기능은 후속 범위입니다.

## 주요 요구사항

| 요구사항 | 구현 결과 |
|---|---|
| 추적 오브젝트 | Box_01~Box_16 |
| 기록 버퍼 | 오브젝트당 최근 600프레임 |
| 기록 항목 | Position / Rotation / Scale XYZ |
| 그래프 | Overview / Detail / Focus |
| 파일 | JSON / CSV Save 및 Parse |
| 부가 UI | Live View, 상태 해석, File Browser |
| 성능 | 일반 실행 60FPS 이상, QA 기록상 100FPS 이상 |
| 배포 | Windows Standalone Build 완료 |

## 최종 결과

16개 박스의 순환 이동과 Transform 기록, 그래프 시각화, 상태 해석, 파일 저장·파싱을 하나의 산업용 대시보드 형태로 통합했습니다. 저장 버튼 클릭 순간의 CPU/GC spike는 Known Issue로 남아 있으며 기능과 일반 모니터링 구간은 최종 QA를 통과했습니다.

