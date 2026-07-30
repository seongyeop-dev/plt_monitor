# 최종 기능 테스트 체크리스트

표기:

- ✅ 검증 완료

## A. 실행 기본 상태

- ✅ Unity Console 에러 0개
- ✅ Play 직후 시스템 정상 시작
- ✅ Box 16개 생산 라인 순환
- ✅ FPS 표시
- ✅ Game View 해상도와 레이아웃

## B. 오브젝트와 데이터

- ✅ 추적 오브젝트 16개 이상 유지
- ✅ 각 오브젝트 Recorder 동작
- ✅ 오브젝트당 600프레임 유지 구조
- ✅ Position / Rotation / Scale 기록
- ✅ X/Y/Z 축 데이터 기록

## C. 그래프 UI

- ✅ Overview / Detail / Focus 버튼
- ✅ Position / Rotation / Scale 버튼
- ✅ All / X / Y / Z 버튼
- ✅ Box_01~Box_16 선택 버튼
- ✅ 선택된 Box 강조 표시

## D. Overview

- ✅ 16개 카드 그래프
- ✅ Box 이름과 Current 값
- ✅ 상태 해석 문구
- ✅ 카드 클릭 시 Detail 전환
- ✅ 그래프 표시 안정성

## E. Detail

- ✅ 선택 Box의 X/Y/Z/All 그래프
- ✅ 4분면 제목과 범례
- ✅ Y축 라벨 배치
- ✅ Current / Min / Max / Frame Range

## F. Focus

- ✅ Position / Rotation / Scale의 X/Y/Z 단일 축 확대
- ✅ Current / Min / Max / Frame Range
- ✅ 확대 그래프 가독성

## G. Live View와 Right Panel

- ✅ Live Conveyor 표시
- ✅ Selected / Metric / Status 표시
- ✅ State / Meaning / Risk / Insight 표시
- ✅ Risk 색상
- ✅ 그래프 모드에 따른 해석 문구

## H. 파일 브라우저와 저장/로드

- ✅ Show JSON / Show CSV / Refresh
- ✅ Save JSON / Save CSV
- ✅ 파일 목록과 선택 파일 표시
- ✅ 상태 메시지
- ✅ Load Selected
- ✅ JSON/CSV 실제 파일 생성
- ✅ JSON 역직렬화와 CSV 파싱

Load Selected는 JSON 역직렬화·CSV 행 파싱과 성공 상태 표시까지 검증했습니다.

## I. 성능과 Profiler

- ✅ 일반 실행 FPS 안정
- ✅ 그래프 전환과 Box 선택 시 큰 끊김 없음
- ✅ Overview / Detail / Focus 전환 시 큰 끊김 없음
- ✅ Save JSON 클릭 순간 CPU·GC 사용량과 일시적 FPS 하락 측정
- ✅ Save CSV 클릭 순간 CPU·GC 사용량과 일시적 FPS 하락 측정
- ✅ 저장 후 FPS 회복
- ✅ 메모리 폭주 없음

## 성능 측정 기록

- 일반 실행 중 100FPS 이상
- Save 클릭 순간 약 40FPS 전후까지 일시 하락
- 저장 완료 후 100FPS 이상으로 회복
- 최종 저장 방식은 코루틴 대신 동기식 파일 저장 방식으로 확정

최종 판단: 일반 실행과 핵심 기능을 검증했으며, JSON·CSV 저장 순간의 CPU·GC 사용량과 FPS 변화를 측정하고 저장 완료 후 기존 수준으로 회복되는 것을 확인했습니다.

---

[문서 목차](../README.md) · [프로젝트 README](../../README.md)
