# 01. 프로젝트 요약

[← 포트폴리오 목차](README.md)

## 한 줄 소개

PLT Monitor는 생산 라인을 순환하는 16개 오브젝트의 Transform을 프레임 단위로 기록하고, 단계형 그래프 UI와 JSON·CSV 파일로 관찰·비교·분석하는 Unity 기반 모니터링 시스템입니다.

## 배경과 목적

Unity 오브젝트가 움직이는 장면은 Game View만으로 현재 상태를 볼 수 있지만, 시간에 따른 위치·회전·크기 변화와 오브젝트 간 차이를 체계적으로 확인하기 어렵습니다. 이 프로젝트는 생산 라인의 움직임을 단순 연출로 끝내지 않고 다음 흐름으로 연결하는 것을 목표로 했습니다.

```text
생산 라인 이동
  → Transform 기록
  → 그래프 비교
  → 상태 해석
  → JSON/CSV 저장
  → 파일 검토
```

제어 시스템이나 설비 예측 모델을 구현하는 것이 아니라, **관찰·기록·분석 중심의 모니터링 도구**를 완성하는 것이 개발 범위입니다.

## 핵심 요구사항

| 요구사항 | 구현 결과 |
| --- | --- |
| 16개 이상 오브젝트 추적 | `Box_01`~`Box_16`을 오브젝트 풀로 순환 |
| Transform 전체 기록 | Position·Rotation·Scale의 XYZ 9개 값 기록 |
| 일정 범위 데이터 유지 | 오브젝트별 최근 600 Frames rolling buffer |
| 전체와 상세 비교 | Overview·Detail·Focus 3단계 그래프 |
| 현장 화면 연계 | 별도 카메라 기반 Live View |
| 파일 저장 | JSON·CSV 두 형식 지원 |
| 저장 파일 검토 | 파일 브라우저와 선택 파일 파싱 |
| 실행 가능 결과물 | Windows Standalone 빌드 완료 |

## 데이터 규모

- 추적 오브젝트: 16개
- 오브젝트별 유지 프레임: 600개
- 한 프레임의 Transform 값: 9개
- 한 시점의 최대 보유 프레임: 9,600개
- Transform 수치: 86,400개

프레임에는 `frameIndex`, `timeStamp`, Position XYZ, Rotation XYZ, Scale XYZ가 포함됩니다. 저장 샘플에서도 16개 오브젝트가 각각 600개 프레임을 가진 구조를 확인할 수 있습니다.

## 주요 결과

1. 생산 라인, 추적 오브젝트, 기록기, 그래프, 파일 브라우저를 한 Scene에 통합했습니다.
2. 16개 전체 흐름을 보고 관심 오브젝트와 축으로 좁혀 가는 분석 UX를 구성했습니다.
3. Recorder와 시각화 사이에 Manager·DataProvider 계층을 두어 데이터 수집과 표현을 분리했습니다.
4. JSON·CSV 저장 결과와 샘플 파일을 제공했습니다.
5. 최종 QA에서 일반 실행, 그래프 전환, Live View, Save/Load 파싱, Windows 빌드를 확인했습니다.

![Overview 화면](../media/screenshots/overview.png)

## 범위와 한계

- 상태 해석은 데이터 범위와 변화량을 이용한 규칙 기반 문구이며 AI·ML 분석이 아닙니다.
- `Load Selected`는 파일 존재 여부를 확인하고 JSON 역직렬화 또는 CSV 파싱 후 성공·실패 상태를 표시합니다.
- 파싱한 데이터를 현재 Recorder, DataProvider, 그래프 UI에 다시 주입하지 않으므로 기록 재생이나 그래프 복원 기능으로 표현하지 않습니다.
- Save JSON/CSV는 동기 직렬화 과정에서 순간적인 CPU·GC 병목이 발생할 수 있습니다.

