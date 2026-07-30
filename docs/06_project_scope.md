# 06. Project Scope

## 주요 문제와 해결

| 문제 | 원인 | 해결 방법 | 결과 |
|:---|:---|:---|:---|
| 장시간 기록 시 데이터와 메모리 사용량 증가 | 16개 오브젝트의 Transform을 매 프레임 계속 보관하면 실행 시간에 따라 기록 데이터가 누적됨 | `scr_TransformRecorder`에 프레임 보관 상한을 적용하고 오래된 프레임을 제거하여 최근 600프레임만 유지 | 장시간 실행 중에도 기록 데이터 크기를 일정 범위로 유지 |
| 여러 오브젝트의 기록 데이터 접근 방식이 분산됨 | 각 오브젝트가 개별 Recorder를 사용하여 화면마다 대상 검색과 데이터 접근 방식이 달라질 수 있음 | `scr_TransformRecorderManager`에서 기록 대상들을 수집하고 공통 접근 경로 제공 | 16개 기록 대상을 동일한 구조로 조회하고 관리 |
| 그래프 화면마다 불필요한 데이터 조회가 반복됨 | Overview·Detail·Focus 화면에서 전체 기록을 같은 방식으로 조회하면 그래프 갱신 부하가 증가함 | `scr_TransformGraphDataProvider`를 통해 데이터 접근을 통합하고 화면 단계에 필요한 기록만 조회 | 전체 현황부터 선택 오브젝트의 위치·회전 축까지 단계별 조회 가능 |
| 기록·저장·UI 책임이 하나의 흐름에 섞일 가능성 | Transform 기록과 JSON·CSV 저장, 파일 선택 UI를 함께 처리하면 기능별 동작과 저장 부하를 구분하기 어려움 | 기록·통합 관리·파일 저장·파일 선택 UI를 각각 `scr_TransformRecorder`, `scr_TransformRecorderManager`, `scr_TransformDataSaveLoadManager`, `scr_TransformFileBrowserUI`로 분리 | 실시간 기록과 저장 기능을 독립적으로 확인하고 유지보수할 수 있는 구조 확보 |
| JSON·CSV 저장 순간의 CPU·GC 상태 확인 필요 | 누적된 Transform 데이터를 직렬화하고 파일로 출력하는 순간 일시적인 연산과 메모리 할당이 발생할 수 있음 | Unity Profiler로 JSON·CSV 저장 시점의 CPU 사용량과 GC Alloc 변화를 확인 | 저장 동작의 성능 영향을 실제 실행 환경에서 점검 |

## 최종 구현 범위

- 16개 오브젝트의 Position·Rotation·Scale 실시간 기록
- 최근 600프레임 순환 보관
- Overview·Detail·Focus 단계별 그래프 구성
- 선택 오브젝트의 위치·회전 축별 데이터 조회
- JSON 형식 Transform 기록 저장
- CSV 형식 Transform 데이터 내보내기
- 저장 파일 선택과 JSON 역직렬화·CSV 행 파싱 결과 확인
- Unity Profiler 기반 저장 순간 CPU·GC 확인
- Transform 기록 샘플 JSON 제공

---

[문서 목차](README.md) · [프로젝트 README](../README.md)