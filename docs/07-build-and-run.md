# 실행 및 빌드

## 요구 환경

- Unity Editor `6000.3.10f1`
- Windows 개발 환경
- 프로젝트 Package 복원을 위한 Unity Package Manager

## 프로젝트 실행

1. 저장소를 내려받습니다.
2. Unity Hub에서 저장소 루트를 프로젝트로 추가합니다.
3. Unity `6000.3.10f1`로 엽니다.
4. `Assets/Scenes/PL_TransformMonitor.unity`를 엽니다.
5. Play를 실행합니다.

Unity가 `Library`, `Temp`, `obj`, IDE 프로젝트 파일을 다시 생성할 수 있습니다. 이 항목들은 Git에서 제외됩니다.

## Windows Standalone Build

1. Unity 메뉴에서 Build Profiles를 엽니다.
2. Windows 플랫폼을 선택합니다.
3. `Assets/Scenes/PL_TransformMonitor.unity`가 포함됐는지 확인합니다.
4. 출력 폴더를 선택하고 Build를 실행합니다.
5. 생성된 `PLT_Monitor.exe`를 실행합니다.

## 배포 계획

Build 결과물은 소스 저장소에 commit하지 않고 ZIP으로 패키징하여 GitHub Release에서 제공할 예정입니다.

Release ZIP에는 실행에 필요한 EXE, `PLT_Monitor_Data`, `MonoBleedingEdge`, Unity DLL만 포함해야 합니다. 다음 디버그 폴더는 이름 그대로 배포 대상이 아닙니다.

```text
PLT_Monitor_BurstDebugInformation_DoNotShip/
```

현재 저장소의 `.gitignore`는 `Build/`, `Builds/`와 모든 `*_BurstDebugInformation_DoNotShip/` 폴더를 제외합니다.

