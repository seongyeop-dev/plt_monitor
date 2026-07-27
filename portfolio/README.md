# PLT Monitor 포트폴리오

생산 라인의 16개 오브젝트에서 `Position / Rotation / Scale`의 XYZ 값을 실시간 기록하고, Overview·Detail·Focus 그래프와 JSON·CSV 파일로 분석할 수 있도록 만든 Unity 모니터링 프로젝트입니다.

![PLT Monitor Overview](../media/screenshots/overview.png)

## 프로젝트 한눈에 보기

| 항목 | 내용 |
| --- | --- |
| 프로젝트명 | PLT Monitor |
| 영문명 | Production Line Transform Monitoring System |
| 형태 | 교육 과정 과제용 독립 프로젝트 |
| 담당 | 기획, Unity 구조 설계, C# 구현, UI, 데이터 저장·불러오기, QA |
| 핵심 기술 | Unity 6000.3.10f1, C#, URP, uGUI, Newtonsoft.Json |
| 데이터 규모 | 16 Objects × 최근 600 Frames × Transform 9개 값 |
| 실행 환경 | Windows Standalone |
| GitHub | [kimseongyeop2811-ux/PLT_Monitor](https://github.com/kimseongyeop2811-ux/PLT_Monitor) |

## 핵심 구현

- 16개 생산 오브젝트를 오브젝트 풀로 순환시키며 Transform을 프레임 단위로 수집
- 오브젝트별 최근 600개 프레임만 유지하는 rolling buffer
- 전체 탐색에서 단일 축 분석까지 이어지는 Overview → Detail → Focus UX
- Recorder, Manager, DataProvider, Renderer, UI Controller의 책임 분리
- 현재값·최솟값·최댓값과 규칙 기반 상태 해석을 그래프와 함께 표시
- JSON·CSV 저장, 파일 목록 탐색, 선택 파일 파싱과 결과 상태 표시
- 별도 카메라와 RenderTexture를 이용한 생산 라인 Live View

## 검증 범위

최종 QA 체크리스트에서 Console 컴파일 에러 0, 16개 오브젝트 순환, 세 그래프 모드, Live View, JSON·CSV 저장과 선택 파일 파싱, Windows 빌드를 확인했습니다. 일반 실행은 요구 기준인 60 FPS 이상을 충족했습니다. 다만 Save JSON/CSV 클릭 시 대량 직렬화로 순간적인 CPU·GC spike가 발생하는 known issue가 남아 있습니다.

> `Load Selected`는 선택한 JSON 또는 CSV 파일을 읽고 파싱 성공 여부와 상태를 표시합니다. 불러온 데이터를 Recorder나 그래프에 다시 주입하는 재생·복원 기능은 현재 범위에 포함되지 않습니다.

## 문서

| 문서 | 내용 |
| --- | --- |
| [01. 프로젝트 요약](01-project-summary.md) | 배경, 목표, 범위와 결과 |
| [02. 역할과 기여](02-role-and-contribution.md) | 직접 담당한 설계·구현·검증 |
| [03. 기술 아키텍처](03-technical-architecture.md) | Scene 구조, 데이터 흐름, 스크립트 책임 |
| [04. 핵심 기능](04-key-features.md) | 기록, 그래프 UX, Live View, Save/Load |
| [05. 문제 해결](05-problem-solving.md) | rolling buffer, UX, 저장 병목, 에셋 경량화 |
| [06. 결과와 QA](06-results-and-qa.md) | 기능·성능·빌드 검증과 한계 |
| [07. 회고](07-retrospective.md) | 배운 점과 개선 방향 |
| [프로젝트 카드](project-card.md) | 통합 포트폴리오용 요약 |
| [프로젝트 데이터](project-data.json) | 통합 인덱스용 구조화 데이터 |

## 근거 자료

- [기존 기술 문서](../docs/01-project-overview.md)
- [최종 QA 체크리스트](../docs/qa/final-test-checklist.md)
- [발표 자료](../docs/presentation/PLT_Monitor.pptx)
- [샘플 JSON](../samples/transform_record_sample.json)
- [샘플 CSV](../samples/transform_record_sample.csv)

