# 그래프 UI

## Overview

16개 Box를 카드형 미니 그래프로 동시에 표시합니다. 전체 흐름을 비교하고 정지, 지연, 편차가 의심되는 대상을 탐색하는 첫 단계입니다. 카드를 선택하면 해당 Box의 Detail로 전환할 수 있습니다.

![Overview](../media/screenshots/overview.png)

## Detail

선택한 Box의 X, Y, Z와 All 시리즈를 4분면에 표시합니다. 동일한 카테고리 안에서 어느 축의 변화가 큰지 비교하기 위한 모드입니다.

![Detail](../media/screenshots/detail.png)

## Focus

단일 축을 큰 그래프로 표시합니다. 최근 프레임 구간의 변화, 튐, 정지와 노이즈를 더 자세히 확인합니다.

![Focus](../media/screenshots/focus.png)

## 패널 구성

| 영역 | 역할 |
|---|---|
| Top Panel | Position/Rotation/Scale, Target, Axis, Current/Min/Max/Frame Range |
| Left Panel | Overview/Detail/Focus와 Box_01~Box_16 선택 |
| Center Panel | 현재 모드의 메인 그래프 |
| Right Panel | Live View, State/Meaning/Risk/Insight, All/X/Y/Z |
| Bottom Panel | JSON/CSV File Browser와 Save/Load 버튼 |

## 모드 사용 목적

```text
Overview: 전체 대상 비교와 이상 후보 탐색
→ Detail: 선택 대상의 XYZ 비교
→ Focus: 특정 축의 정밀 확인
```

이 단계 구조를 통해 한 화면에 모든 정보를 과도하게 표시하지 않고 탐색 범위를 좁힐 수 있습니다.

