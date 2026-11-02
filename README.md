# Unity Offline Restore 구현 연습

게임 종료 중의 무역 진행을 복원하는 세 가지 모델을 직접 구현하고 비교한 Unity 학습 프로젝트입니다. 기존 방치형 무역 시뮬레이션의 기능을 이해하고, 원본 코드를 복사하지 않고 다시 구현하며 설계 선택을 설명하는 것을 목표로 했습니다.

이번 연습의 범위는 **단일 무역의 시간 진행 복원**입니다. 완성된 상용 저장·정산 시스템을 제공하는 프로젝트는 아닙니다.

## 먼저 볼 코드

1. **[AbsoluteUtcTradeManager.cs](Assets/Scripts/OfflineRestore/AbsoluteUtc/AbsoluteUtcTradeManager.cs)** — 최종 선택한 모델입니다. 시작·종료 예정시각을 저장하고 현재 UTC로 완료 여부와 진행량을 계산합니다.
2. **[AbsoluteUtcTradeManagerTests.cs](Assets/Test/EditMode/Tests/AbsoluteUtcTradeManagerTests.cs)** — 고정 UTC를 사용해 Snapshot 검증, 복원, 시간 경계, 반복 호출과 실패 시 Runtime 보존을 검증합니다.
3. **[RemainingTimeTradeManager.cs](Assets/Scripts/OfflineRestore/RemainingTime/RemainingTimeTradeManager.cs)** / **[대응 테스트](Assets/Test/EditMode/Tests/RemainingTimeTradeManagerTests.cs)** — 남은 시간과 기준 UTC를 함께 갱신하는 비교 모델입니다.
4. **[OfflineTradeManager.cs](Assets/Scripts/OfflineRestore/Elapsed/OfflineTradeManager.cs)** / **[대응 테스트](Assets/Test/EditMode/Tests/OfflineTradeManagerTests.cs)** — 경과시간을 누적하는 비교 모델입니다.

상세한 불변조건, 학습 과정과 면접 대비 내용은 [학습 총정리](https://github.com/csu1222/Portfolio_Practice/tree/main/Assets/Scripts/OfflineRestore/Offline_Restore_Study_Summary.md)를 참고하세요. 총정리 문서는 학습 당시의 기록이며, 이후 코드 수정과 최신 검증 범위는 이 README를 기준으로 확인할 수 있습니다.

## 학습 목적

- 실행 중의 시간 누적만으로는 게임 종료 중 진행을 복원할 수 없는 이유를 이해합니다.
- Runtime과 SaveData, Snapshot 생성·적용, 시간 복원의 책임을 구분합니다.
- 상태별 불변조건을 정의하고 잘못된 Snapshot이 기존 Runtime을 변경하지 않도록 검증합니다.
- 중복 Restore와 Clock Rollback의 차이를 이해합니다.
- 수동 검증과 Unity EditMode 테스트로 계약을 확인하고 설계의 장단점을 설명합니다.

## 세 방식의 차이

### Elapsed + LastAppliedUtc

진행시간, 마지막 적용 UTC, State를 저장합니다.

```text
새 진행시간 = 저장된 진행시간 + (현재 UTC - 마지막 적용 UTC)
```

완료 시 Duration으로 Clamp합니다. 기존의 진행시간 누적 방식과 연결하기 쉽지만, 진행시간과 기준 UTC를 함께 갱신해야 중복 반영을 막을 수 있습니다. 현재 UTC가 마지막 적용 UTC보다 과거이면 복원을 거부합니다.

### RemainingTime + Snapshot UTC

남은 시간, 그 값의 기준 UTC, State를 저장합니다. 코드의 `lastAppliedUtc`가 Snapshot UTC 역할을 합니다.

```text
새 남은 시간 = 저장된 남은 시간 - (현재 UTC - 기준 UTC)
```

완료 시 0으로 Clamp합니다. 카운트다운 중심으로 이해하기 쉽지만 남은 시간과 기준 UTC의 동기화가 필요합니다. Prepare와 Completed가 모두 남은 시간 0이므로 State로 구분합니다. 기준 UTC 이전의 복원 입력은 거부합니다.

### Absolute UTC — 최종 선택

시작 UTC, 종료 예정 UTC, State를 저장합니다. 누적 진행값을 저장하지 않고 현재 UTC에서 경과시간·남은 시간·진행률을 계산합니다.

```text
기존 무역의 전체 시간 = 종료 예정 UTC - 시작 UTC
진행률 = Clamp((현재 UTC - 시작 UTC) / 전체 시간, 0, 1)
완료 조건 = 현재 UTC >= 종료 예정 UTC
```

무역 시작 시 종료시각이 정해지고 Restore가 반복 호출될 수 있어 이 방식을 선택했습니다. 같은 시각으로 재계산해도 진행량이 추가 누적되지 않습니다. 세 방식 모두 저장 필드는 세 개이므로, 선택 이유가 저장 필드 수의 감소는 아닙니다.

현재 코드는 기존 무역의 계산에 저장된 시작·종료 간격을 사용합니다. Inspector의 `duration`은 새 무역의 종료 예정시각을 만들 때 사용하므로, 현재 설정과 저장 간격이 달라도 기존 일정의 계산 기준을 유지합니다.

기본형은 현재 UTC가 StartUtc보다 과거인 입력을 거부하지만, 마지막 관측 시각은 저장하지 않습니다. 예를 들어 12:00 시작 후 12:40을 관측했다가 12:30으로 돌아가는 부분적 시간 역행은 감지하지 못합니다.

### 비교한 다섯 기준

1. **저장 구조 / SSOT:** 누적 진행값을 기준으로 삼을지 시작·종료 일정을 기준으로 삼을지 비교했습니다.
2. **Restore 계산:** 경과시간 더하기, 남은 시간 빼기, 절대시각 비교를 비교했습니다.
3. **중복 Restore:** Elapsed·Remaining은 값과 UTC 갱신이 필요하고, Absolute UTC는 같은 시각에 비누적 계산을 합니다.
4. **Clock Rollback:** 마지막 적용 UTC 기준 검사와 StartUtc 기준 검사의 차이를 확인했습니다.
5. **정합성 관리:** 진행값·UTC·State의 동기화와 시작·종료·State의 관계 관리를 비교했습니다.

성능 벤치마크에 따른 우열을 측정한 것은 아닙니다. 클라이언트 UTC를 사용한다는 시간 신뢰성의 한계는 세 모델에 공통으로 남습니다.

## 직접 작성한 부분과 AI 피드백 범위

**직접 작성·수행한 부분**

- 세 모델의 Manager, SaveData, 메모리 SaveService, 표시·디버그 UI 코드.
- Snapshot 생성·적용, 상태별 검증과 시간 복원 로직.
- Unity Test Framework와 NUnit을 사용하는 EditMode 테스트.
- DebugPanel과 고정 시간 입력을 통한 수동 검증 및 Test Runner 실행.
- 세 모델 비교, Absolute UTC 선택, 리뷰에서 발견한 계산 기준 불일치 수정.

**AI의 도움을 받은 부분**

- 문제 분해, 데이터 모델·불변조건·책임 분리에 대한 피드백.
- 코드 리뷰와 오류 조건·경계값·테스트 기대값에 대한 검토.
- 비교 기준 정리, Clock Rollback·멱등성·정산 원자성 설명.
- 제출 코드 안내와 학습 총정리·README 문서 작성 지원.

구현 코드는 학습자가 작성했으며, AI는 리뷰와 수정 방향·예시를 제안했습니다. AI가 Unity 테스트나 Player 빌드를 실행해 검증한 것은 아닙니다.

## 실행 방법

### 환경

- 프로젝트에 기록된 Unity Editor: **6000.6.0f1**.
- 패키지 구성은 저장소의 `Packages`를 사용합니다.
- 테스트는 Unity Test Framework와 NUnit을 사용합니다.

### 데모 실행

1. 저장소를 내려받고 Unity Hub에서 `Assets`, `Packages`, `ProjectSettings`가 있는 루트 폴더를 프로젝트로 추가합니다.
2. 기록된 Editor 버전으로 열고 패키지 로드와 컴파일이 끝날 때까지 기다립니다.
3. `Assets/Scenes/AbsoluteUtcScene.unity`를 열고 Play를 실행합니다.
4. 무역 시작 버튼으로 Traveling을 시작하고 Save 버튼으로 Snapshot을 저장합니다.
5. 시간이 지난 뒤 Load만 실행하면 저장 당시 상태를 적용합니다. Restore 버튼은 Load → ApplySnapshot → 현재 UTC 기준 Restore를 수행합니다.
6. `RemainingTimeScene.unity`와 `ElapsedScene.unity`에서 비교 모델을 확인할 수 있습니다.

**저장소는 메모리 저장 데모입니다.** Play 종료나 프로세스 재시작 후 저장 데이터가 유지되는 파일 저장 기능은 없습니다. 실제 종료·재실행 대신 같은 실행 세션의 저장·재적용과 고정 UTC 테스트로 복원 규칙을 확인합니다.

Absolute UTC의 TradeStart는 Prepare에서 출발하고, Traveling에서는 유지하며, Completed에서는 Prepare로 초기화합니다. Completed에서 한 번 눌렀을 때 곧바로 다음 무역을 출발시키는 동작은 아닙니다.

### EditMode 테스트 실행

1. Unity에서 `Window > General > Test Runner`를 엽니다.
2. `EditMode` 탭을 선택합니다.
3. 먼저 `AbsoluteUtcTradeManagerTests`를 실행하고, `Run All`로 전체 테스트를 확인합니다.
4. 실패 시 결과와 Console 로그를 함께 확인합니다.

테스트 코드는 `Assets/Test/EditMode/Tests`에 있습니다. GameObject에 Manager 컴포넌트를 추가하고 테스트 후 정리하며, 복원 입력에는 고정 UTC를 사용합니다.

## 실제 검증 범위

### 수행한 검증

- 학습 과정에서 세 모델의 DebugPanel과 고정 시간 시나리오로 정상 복원, Load Only, 오프라인 완료, 시간 역행, 중복 Restore와 상태별 거부 동작을 확인했습니다.
- EditMode 테스트로 Snapshot 유효성, 잘못된 입력 거부와 기존 Runtime 보존, 정상 복원, 완료 처리, 중복 호출 등을 확인했습니다.
- Absolute UTC 테스트에는 시작·종료시각 경계와 파생 진행률·남은 시간 검증이 포함됩니다.
- `using PlasticGui;`를 제거하고 기존 무역의 시간 계산을 저장된 시작·종료 간격 기준으로 수정했습니다.
- **해당 수정 후 작성자가 Test Runner에서 기존 테스트 전체 통과를 확인했습니다.**

### 검증하지 않은 범위

- 저장 간격이 현재 `duration`보다 긴 경우·짧은 경우, 설정이 0인 경우에 대한 **새 테스트는 추가하지 않았습니다.** 계산 기준 통일은 코드 리뷰로 확인했으며, 기존 테스트 PASS가 이 시나리오까지 자동 검증했다는 뜻은 아닙니다.
- 실제 Player 빌드 성공은 확인하지 않았습니다.
- 파일 I/O, 프로세스 종료 중 저장 실패, 파일 손상·복구, 영속 저장 원자성은 검증하지 않았습니다.
- 서버 시간 검증, 시간 조작 방지, 성능 벤치마크, 다중 무역의 통합 검증은 범위 밖입니다.

기존 테스트 전체 PASS는 해당 테스트가 확인하는 계약의 결과이며, 운영 환경의 모든 문제에 대한 보장은 아닙니다.

## 남아 있는 한계와 확장 주제

- `DateTime.UtcNow`는 클라이언트 OS 시계에 의존합니다. 미래 방향 시계 변경으로 조기 완료될 수 있으며, Absolute UTC 기본형은 부분적 시간 역행에 따른 진행률 감소를 막지 않습니다.
- Restore의 비누적성은 보상 지급의 멱등성을 보장하지 않습니다. Completed는 시간 진행 완료이며 지급 완료와는 다릅니다.
- TradeId 기반 지급 이력과 골드·정산 기록의 원자적 저장 필요성은 개념을 학습했습니다. 실제 정산 트랜잭션은 구현하지 않았습니다.
- 파일 영속 저장, 실패 복구, 신뢰할 수 있는 외부 시간, 다중 무역 처리는 후속 확장 주제입니다.
