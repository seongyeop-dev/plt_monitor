# 01. Overview

## 개발 배경

생산 라인 위에서 이동하는 여러 오브젝트의 상태를 한 화면에서 비교하고, 특정 대상의 변화 원인을 단계적으로 좁혀보기 위해 제작한 Unity 기반 Transform 모니터링 프로젝트입니다. 오브젝트 제어보다 데이터 기록, 비교, 해석과 파일 보존에 초점을 두었습니다.

## 프로젝트 목적

- Box 16개의 Position·Rotation·Scale을 실시간 기록
- 대상별 최근 600프레임 Rolling Buffer 유지
- Overview·Detail·Focus의 단계별 그래프 분석
- 선택 대상을 추적하는 Live View 구성
- 최근 데이터 구간의 상태·의미·위험도 표시
- JSON·CSV 저장, 파일 목록 조회와 파싱 결과 확인
- Windows Standalone 실행과 성능 검증

## 구현 범위

| 구분 | 구현 내용 |
|---|---|
| Production Line | Box 16개 Object Pool, Spawn·이동·재활용 |
| Recorder | Position·Rotation·Scale XYZ 프레임 기록 |
| Buffer | 대상별 최대 600프레임 유지 |
| Graph | Overview·Detail·Focus, Position·Rotation·Scale, All·X·Y·Z |
| Live View | 선택 대상 추적과 카메라 프리셋 |
| Interpretation | 최근 변화량·범위 기반 규칙형 상태 해석 |
| File | JSON·CSV Save, 목록 조회, 선택과 파싱 결과 |
| Build | Windows Intel 64-bit Standalone 실행 |

## 데이터 규모

```text
16 Objects × 600 Frames = 최대 9,600 Frames
9 Transform Values per Frame
= 최대 86,400 Transform Values
```

각 프레임은 `frameIndex`, `timeStamp`와 Position·Rotation·Scale의 X·Y·Z를 보관합니다.

## 구현하지 않은 범위

- 로드한 세션을 Recorder에 다시 주입
- 과거 세션 그래프 재생과 현재 데이터 비교
- 서버·데이터베이스 업로드
- 학습형 AI 기반 이상 탐지
- 대규모 대상과 장시간 무중단 환경의 부하 검증

## 최종 결과

생산 라인 시뮬레이션, 16개 대상의 Transform 기록, 단계별 그래프, Live View, 규칙 기반 해석과 JSON·CSV 파일 관리를 하나의 대시보드로 통합했습니다. 일반 모니터링과 핵심 기능은 최종 체크리스트를 통과했으며, 저장 순간의 CPU·GC Spike는 Known Issue로 분리해 기록했습니다.

[문서 목차로 돌아가기](README.md)
