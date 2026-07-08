# Fantasy Town Guard

판타지 마을의 입구를 지키는 검문소 시뮬레이션 게임입니다.  
방문자가 제출한 방문증과 소지품을 확인하고, 정보 일치 여부와 인증 절차, 지불 금액을 판단해 하루의 결과를 정산합니다.

## 프로젝트 개요

- 장르: 2D 검사/판정 시뮬레이션
- 엔진: Unity 6000.3.14f1
- 렌더 파이프라인: Universal Render Pipeline
- 플랫폼: WebGL 빌드 기준
- 주요 구현: 방문자 데이터 검증, 드래그 앤 드롭 상호작용, CSV 기반 대사/데이터 로딩, 튜토리얼 진행, 점수 정산

## 게임 흐름

1. 종을 눌러 다음 방문자를 호출합니다.
2. 방문자가 제출한 방문증, 돈, 문서 등의 아이템을 작업 공간으로 가져옵니다.
3. 방문증에 적힌 이름, 직업, 국가, 방문 목적을 실제 방문자 데이터와 비교합니다.
4. 필요하면 방문자에게 잘못된 정보를 설명하고, 인증 마크와 요금을 확인합니다.
5. 방문자 처리가 끝나면 성공/실패, 확인한 정보 수, 받은 금액, 누락 사항을 바탕으로 최종 급여가 계산됩니다.

## 주요 기능

### 방문자 검문 시스템

- `VisitorData.csv`에서 방문자 정보를 로드합니다.
- 방문자마다 이름, 직업, 국가, 방문 목적 정보를 보유합니다.
- 일정 확률로 방문증의 일부 정보를 실제 데이터와 다르게 표시해 플레이어가 오류를 찾아내도록 구성했습니다.

### 드래그 앤 드롭 상호작용

- 마우스 입력 기반으로 클릭 가능한 오브젝트와 드래그 가능한 아이템을 분리해 처리합니다.
- 작업 공간 안팎에 따라 아이템의 열린 상태와 닫힌 상태가 전환됩니다.
- `RaycastManager`에서 2D Collider, Sorting Order, UI 입력 예외 처리를 통합 관리합니다.

### CSV 기반 대사 시스템

- 대사 데이터를 CSV로 관리해 코드 수정 없이 대사 흐름을 조정할 수 있습니다.
- 일반 대사, 플레이어 대사, 방문자 대사, 튜토리얼 안내 대사를 타입별 프리팹으로 출력합니다.
- `nextId`를 이용해 연속 대사를 연결하고, 변수 치환을 지원합니다.

### 튜토리얼 시스템

- 튜토리얼 단계를 데이터화해 특정 오브젝트 클릭, 포인터 입력, 대사 종료 조건을 순차적으로 처리합니다.
- 강조 대상과 입력 필터를 사용해 플레이어가 현재 필요한 조작에 집중할 수 있도록 구성했습니다.
- 튜토리얼 완료 여부와 현재 레벨은 `PlayerPrefs`로 저장합니다.

### 결과 정산

- 처리한 방문자 수, 통과/실패 수, 확인한 정보 수, 수령 금액, 누락된 절차를 종합해 점수를 계산합니다.
- 실수 항목은 메모 형태로 누적되어 결과 화면에서 확인할 수 있습니다.

## 기술 스택

- Unity 6000.3.14f1
- C#
- Universal Render Pipeline 17.3.0
- Unity Input System 1.19.0
- TextMesh Pro
- Unity UI
- CSV 데이터 로딩

## 프로젝트 구조

```text
Assets/
  Scenes/
    MainScene.unity
  Scripts/
    GameManager.cs
    RaycastManager.cs
    VisitorController.cs
    TrayController.cs
    TutorialManager.cs
    CSV/
      DialogCsvLoader.cs
      VisitorDataCsvLoader.cs
    Items/
      ItemManager.cs
      ItemBookManager.cs
      ItemMemoManager.cs
      ItemMoneyManager.cs
      ItemScrollManager.cs
    Reciever/
      ItemReceiver.cs
      TraySlotReceiver.cs
      VisitorReceiver.cs
    ScriptableObject/
      LevelData.cs
      LevelDatabase.cs
  StreamingAssets/
    Data/
      VisitorData.csv
    Dialog/
      Dialog1.csv
      TutorialDialog.csv
Packages/
  manifest.json
ProjectSettings/
```

## 실행 방법

1. Unity Hub에서 `Unity 6000.3.14f1` 버전을 설치합니다.
2. 이 저장소를 클론합니다.

```bash
git clone <repository-url>
```

3. Unity Hub에서 프로젝트 폴더를 열어줍니다.
4. `Assets/Scenes/MainScene.unity` 씬을 실행합니다.

## 빌드

이 프로젝트는 WebGL 빌드 산출물을 포함할 수 있는 구조로 구성되어 있습니다.  
Unity Editor에서 `File > Build Profiles` 또는 `Build Settings`를 열고 WebGL 플랫폼을 선택한 뒤 빌드하면 됩니다.

## 구현 포인트

- 방문자 원본 데이터와 표시 데이터를 분리해 검문 판단 로직을 구성했습니다.
- CSV 파서를 직접 구현해 따옴표와 쉼표가 포함된 대사 데이터를 처리할 수 있도록 했습니다.
- 클릭, 드래그, 호버 입력을 하나의 매니저에서 관리해 상호작용 충돌을 줄였습니다.
- ScriptableObject 기반 레벨 데이터로 레벨명과 방문자 수를 관리합니다.
- 튜토리얼 진행 상태와 레벨 진행도를 저장해 재실행 시 이어지는 흐름을 만들었습니다.

## 스크린샷

GitHub에 업로드할 때 아래 영역에 플레이 화면 이미지를 추가하면 좋습니다.

```markdown
![Gameplay](Docs/Images/gameplay.png)
![Inspection](Docs/Images/inspection.png)
![Result](Docs/Images/result.png)
```

## 라이선스

프로젝트에 포함된 에셋의 라이선스를 확인한 뒤 공개 범위를 결정해야 합니다.  
외부 에셋이 포함되어 있다면 저장소 공개 전 출처와 사용 조건을 별도로 정리하는 것을 권장합니다.
