# QA 및 성능

## 최종 QA 요약

| 영역 | 결과 |
|---|---|
| 기본 실행 | 통과 |
| 16개 Box 순환 및 Transform 기록 | 통과 |
| Overview / Detail / Focus | 통과 |
| Live View 및 상태 해석 | 통과 |
| JSON/CSV 파일 생성 | 통과 |
| JSON/CSV 파싱 및 상태 표시 | 통과 |
| Windows Build | 통과 |
| 저장 순간 성능 | Known Issue |

전체 체크 항목은 [최종 기능 테스트 체크리스트](qa/final-test-checklist.md)에 정리했습니다.

## 일반 실행 FPS

- 요구 기준인 60FPS 이상을 충족했습니다.
- 기존 QA 기록에서는 일반 실행 중 100FPS 이상으로 안정적으로 유지됐습니다.
- 그래프 모드 전환, Box 선택과 Live View 사용에서 큰 끊김이 없다고 기록돼 있습니다.

## Save JSON/CSV 병목

Save JSON 또는 Save CSV를 클릭하면 순간적으로 CPU spike와 GC spike가 발생하고 FPS가 약 40 전후까지 하락한 뒤 회복합니다.

현재 저장 과정은 다음 작업을 메인 스레드에서 연속 수행합니다.

1. 16개 Recorder에서 최대 600프레임씩 복사
2. 세션 DTO 구성
3. JSON 직렬화 또는 CSV Dictionary 행 9,600개 생성
4. 문자열 생성과 파일 I/O

![Profiler](../media/screenshots/profiler.png)

PPT에는 Profiler 화면 이미지가 포함돼 있지만 `ProfilerCaptures` 폴더에는 Unity Profiler 원본 캡처 파일이 없습니다.

## Known Issue 판단

- 저장 순간에 한정된 일시적 병목입니다.
- 저장 완료 후 기존 FPS 수준으로 회복합니다.
- 일반 모니터링과 그래프 탐색은 QA 기준을 충족했습니다.
- 기존에 코루틴 저장을 시험했으나 장시간 실행 시 FPS 유지가 어려워 최종 버전에는 적용하지 않았습니다.

## 향후 최적화

- 프레임 복사와 직렬화 작업의 분할
- 백그라운드 직렬화가 가능한 순수 데이터 스냅샷 구성
- CSV 문자열을 한 번에 메모리에 구성하지 않는 스트리밍 기록
- 재사용 가능한 행/문자열 버퍼 검토
- 저장 전후 Profiler 원본 캡처와 GC Alloc 수치 보관

