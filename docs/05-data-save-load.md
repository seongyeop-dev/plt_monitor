# 데이터 저장 및 로드

## TransformFrameData

한 프레임은 다음 필드를 가집니다.

| 구분 | 필드 |
|---|---|
| 식별 | `frameIndex`, `timeStamp` |
| Position | `posX`, `posY`, `posZ` |
| Rotation | `rotX`, `rotY`, `rotZ` |
| Scale | `scaleX`, `scaleY`, `scaleZ` |

`TransformObjectRecordData`는 오브젝트 이름과 프레임 목록을, `TransformMonitoringSaveData`는 세션 이름, 생성 시각과 전체 오브젝트 목록을 보관합니다.

## JSON 구조

```text
Session
├─ sessionName
├─ createdTimeText
└─ objects[16]
   ├─ objectName
   └─ frames[600]
```

JSON은 Newtonsoft.Json을 사용해 들여쓰기된 텍스트로 저장합니다.

## CSV 구조

```text
SessionName,CreatedTime,ObjectName,FrameIndex,TimeStamp,
PosX,PosY,PosZ,RotX,RotY,RotZ,ScaleX,ScaleY,ScaleZ
```

샘플 전체 세션은 헤더 1줄과 데이터 9,600줄로 구성됩니다.

## File Browser 흐름

```text
Show JSON / Show CSV
→ 저장 폴더 검색
→ 파일 버튼 생성
→ 파일 선택
→ Load Selected
→ 확장자에 맞는 JSON/CSV 파서 실행
→ 성공 또는 실패 상태 메시지 표시
```

![Save and Load](../media/screenshots/save-load.png)

## Load Selected의 실제 범위

현재 구현은 다음 범위까지 수행합니다.

- 선택 경로와 파일 존재 여부 확인
- JSON을 `TransformMonitoringSaveData`로 역직렬화
- CSV를 행 Dictionary 목록으로 파싱
- 성공 또는 실패 상태 메시지 표시

로드 결과는 메서드 안에서 확인되며 Recorder, DataProvider 또는 Graph UI에 다시 전달되지 않습니다. 따라서 과거 세션 복원과 그래프 재생은 구현된 기능으로 설명하지 않습니다.

## 저장 경로

Windows 실행 시 `Application.persistentDataPath` 아래의 다음 구조를 사용합니다.

```text
PLT_Monitor/TransformRecords/
├─ transform_record.json
└─ transform_record_yyyyMMdd_HHmmss.csv
```

저장 파일 내용에는 사용자 이름이나 절대 로컬 경로를 기록하지 않습니다.

