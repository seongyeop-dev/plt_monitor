# 06. 결과와 QA

[← 포트폴리오 목차](README.md)

## 최종 결과

| 영역 | 확인 결과 |
| --- | --- |
| Unity 실행 | 시스템 정상 시작 |
| Console | 최종 체크리스트 기준 컴파일 에러 0 |
| 생산 라인 | 16개 Box 순환 이동 |
| 데이터 기록 | 오브젝트별 최근 600 Frames 유지 |
| 그래프 UI | Overview·Detail·Focus 및 Category·Axis 전환 |
| 상태 패널 | Selected·Metric·Status와 규칙 기반 해석 문구 |
| Live View | 확대, 시점 전환, 닫기와 복귀 |
| Save | JSON·CSV 파일 생성과 목록 갱신 |
| Load Selected | 선택 JSON 역직렬화·CSV 파싱 및 상태 표시 |
| Build | Windows Standalone 빌드 완료 |

## 데이터 결과 검증

포트폴리오 샘플 파일로 저장 구조를 다시 확인했습니다.

| 샘플 | 검증 내용 |
| --- | --- |
| `transform_record_sample.json` | Objects 16개, 각 Frames 600개 |
| `transform_record_sample.csv` | 헤더 1행 + 데이터 9,600행 |
| 각 Frame | FrameIndex, TimeStamp, Position/Rotation/Scale XYZ |

샘플은 저장 형식과 데이터 규모를 보여 주는 근거이며, 불러온 데이터를 그래프에 재생하는 기능의 근거로 사용하지 않습니다.

## 성능 확인

- 최종 요구 기준: 일반 실행 60 FPS 이상
- QA·Profiler 기록: 일반 실행에서 100 FPS 이상으로 유지된 구간 확인
- 그래프 모드 전환, Box 선택, Live View 확대에서 큰 끊김 없음
- Save JSON/CSV 클릭 순간 CPU spike와 GC spike 발생
- 저장 순간 약 40 FPS 수준까지 하락한 뒤 회복한 기록이 있음

환경에 따라 절대 FPS는 달라질 수 있으므로 포트폴리오에는 “일반 실행 60 FPS 이상 요구 충족”을 기준 결과로 사용하고, 100 FPS 이상은 해당 QA 환경에서 관찰한 값으로 구분합니다.

![Profiler](../media/screenshots/profiler.png)

## QA 판정

기능과 빌드는 완료됐고 일반 실행 성능도 목표를 충족했습니다. 저장 순간 병목은 파일 생성 실패나 지속적인 프레임 저하로 이어지지는 않았지만, 실시간성이 중요한 제품 수준에서는 개선해야 할 known issue입니다.

| 판정 | 범위 |
| --- | --- |
| 통과 | 실행, 16개 순환, 기록, 그래프 3모드, Live View, 저장, 파일 파싱, Windows 빌드 |
| 부분 통과 | 성능 — 일반 실행 안정, 저장 클릭 순간 병목 존재 |
| 미구현 범위 | 로드 데이터의 Recorder 재주입, 그래프 복원, Replay |

## 검증 근거

- [기존 QA·성능 문서](../docs/06-qa-performance.md)
- [최종 기능 테스트 체크리스트](../docs/qa/final-test-checklist.md)
- [JSON 샘플](../samples/transform_record_sample.json)
- [CSV 샘플](../samples/transform_record_sample.csv)

