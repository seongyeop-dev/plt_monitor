# PLT Monitor

![PLT Monitor 대표 화면](../media/screenshots/overview.png)

## 한 줄 요약

16개 생산 라인 오브젝트의 Transform을 600 프레임 rolling buffer로 기록하고 Overview·Detail·Focus 그래프와 JSON·CSV로 분석하는 Unity 모니터링 시스템

## 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 형태 | 교육 과정 과제용 독립 프로젝트 |
| 기간 | 확인 가능한 자료 없음 |
| 역할 | 기획, Unity/C# 구현, UI, Save/Load, QA |
| 기술 | Unity, C#, URP, uGUI, Newtonsoft.Json |
| 데이터 | 16 Objects × 600 Frames × 9 Transform Values |

## 해결한 문제

1. 실시간 Transform 기록이 장시간 실행에서 무한히 증가하는 문제
2. 16개 오브젝트와 9개 축을 한 화면에서 비교하기 어려운 문제
3. 대량 JSON·CSV 직렬화의 순간 CPU·GC 병목
4. Sprite 하나 때문에 400MB 이상 외부 에셋에 의존하던 문제

## 핵심 기여

1. Recorder–Manager–DataProvider–Renderer/UI 데이터 파이프라인 설계
2. 16개 오브젝트의 600 Frames rolling buffer 구현
3. Overview → Detail → Focus 단계형 그래프 UX 구현
4. Live View, 규칙 기반 상태 해석과 JSON·CSV 파일 관리 구현
5. 외부 point sprite를 내부 생성 자산으로 대체해 저장소 경량화

## 결과

- Position·Rotation·Scale XYZ 실시간 기록과 그래프 시각화
- 일반 실행 60 FPS 이상 요구 충족
- Console 컴파일 에러 0과 Windows Standalone 빌드 확인
- 저장 시 순간 병목을 known issue로 분리하고 개선 방향 제시
- 예상 Git 추적 크기를 약 252MB에서 약 16MB로 축소

## 면접용 30초 설명

PLT Monitor는 Unity 생산 라인의 16개 오브젝트에서 Position, Rotation, Scale의 XYZ를 실시간 기록하고 그래프로 분석하는 프로젝트입니다. 오브젝트별 최근 600 프레임만 유지하고, Overview에서 전체를 탐색한 뒤 Detail과 Focus로 좁히는 UI를 구현했습니다. Recorder와 그래프 표현 계층을 분리하고 JSON·CSV 저장까지 연결했으며, Profiler로 저장 순간의 CPU·GC 병목도 확인해 known issue와 개선안을 문서화했습니다.

## 링크

- [GitHub 저장소](https://github.com/kimseongyeop2811-ux/PLT_Monitor)
- [상세 포트폴리오](README.md)
- [기술 아키텍처](03-technical-architecture.md)
- [문제 해결](05-problem-solving.md)
