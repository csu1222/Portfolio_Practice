# Unity Offline Restore 학습 총정리

작성일: 2026-11-02  
대상: Unity·C# 기초를 이해한 학습자, 포트폴리오 검토자, 게임 클라이언트 개발자 면접관

> 이 문서는 사용자가 제공한 학습 인계 기록과 참조 대화의 마지막 문답을 근거로 작성했다. 세 모델의 구현, 수동 검증, Unity EditMode 테스트 전체 PASS는 **사용자가 수행하고 확인한 학습 결과**다. 문서 작성 과정에서 원본 프로젝트를 검사하거나 Unity 테스트를 재실행하지 않았다. 테스트 개수·Unity 버전·개별 실행 로그·성능 수치는 제공되지 않았으므로 기재하지 않는다. 아래의 수치 예시는 계산을 설명하기 위한 예시이며 개별 테스트 로그를 재현한 것이 아니다.

## 1. 배경과 학습 범위

기존 포트폴리오는 Unity 기반 방치형 무역 시뮬레이션이다. 핵심 흐름은 다음과 같다.

```text
무역 준비 → 무역 시작 → Traveling → 도착 → Completed → 정산 → 다음 무역 준비
```

게임을 종료한 동안에도 무역은 진행되어야 한다. 기존 프로젝트에는 저장 데이터 관리, 다중 캐러밴, 오프라인 복원, 정산 등이 있지만, 이번 별도 연습 프로젝트에서는 **단일 무역의 시간 진행 복원**에 집중했다.

목표는 기존 코드를 복사하는 대신 문제·저장 모델·불변조건을 이해하고 직접 다시 구현하는 것이었다. 학습자가 코드를 작성하고 AI는 설계 검토와 코드 리뷰를 담당했다.

```text
문제 정의 → 데이터 모델 → 불변조건 → 최소 구현 → 수동 검증
→ 예외 입력 처리 → 책임 분리 → EditMode 테스트 → 설계 비교 → 선택 근거 설명
```

학습한 구현 범위는 세 가지다.

1. Elapsed + LastAppliedUtc: 진행시간과 마지막 적용 UTC 저장.
2. RemainingTime + Snapshot UTC: 남은 시간과 해당 값의 기준 UTC 저장.
3. Absolute UTC: 시작 시각과 예상 종료 시각 저장.

시간 신뢰성, 부분적 Clock Rollback, 복원과 정산의 멱등성 차이, 정산 원자성이 필요한 이유까지 논의했다. 서버 시간 검증, 실제 정산 트랜잭션, 파일 저장 실패 복구는 구현 완료 범위에 포함하지 않는다.

## 2. 문제 정의와 공통 완료 조건

실행 중에는 다음과 같이 진행시간을 누적할 수 있다.

```csharp
elapsed += Time.deltaTime;
```

프로세스가 종료되면 Update가 실행되지 않으므로 이 방식만으로 종료 중 경과시간을 알 수 없다.

```text
전체 무역 시간 60초
저장 당시 진행시간 20초
종료 중 경과시간 30초
재실행 후 기대 진행시간 50초
```

진행값만 저장해서는 30초를 복원할 수 없다. 진행값에 대응하는 현실 시간 기준점이나 무역의 절대적인 시작·종료 시각이 필요하다.

세 모델의 공통 검증 목표는 다음과 같다.

- 저장 상태 적용과 현재 시간 반영을 분리한다.
- 정상 입력에서는 기대 진행 상태로 복원한다.
- 완료 경계를 넘으면 Completed로 전이하고 시간 값을 유효 범위에 둔다.
- 동일 시각의 반복 복원으로 진행량이 중복 누적되지 않는다.
- 모델에서 정의한 시간 역행 조건을 거부한다.
- 잘못된 Snapshot을 거부하고 기존 Runtime을 보존한다.
- 고정 UTC로 같은 입력과 기대 결과를 반복 검증할 수 있다.

이번 데모의 Duration은 60초 고정 설정이다. Elapsed·Remaining 모델의 SaveData에는 Duration을 저장하지 않았다. 설정값을 바꾼 뒤 예전 저장 데이터를 읽는 호환성 문제까지 해결한 것으로 해석해서는 안 된다.

## 3. 공통 구조와 데이터 소유권

### 3.1 TradeManager, SaveService, DebugPanel

**TradeManager**는 Runtime State와 시간 데이터를 소유하고 TradeStart, CreateSnapshot, ApplySnapshot, Restore를 담당한다. 상태와 시간의 도메인 규칙을 검증하는 주체다.

**SaveService**는 SaveData의 저장과 조회를 담당한다. 이번 데모는 JSON 파일이 아닌 **메모리 저장**이다. Save와 Load에서 객체 복사본을 만들어 외부 객체의 변경이 내부 저장 데이터로 전파되지 않도록 했다. 파일 I/O의 내구성이나 장애 복구를 검증한 구현은 아니다.

**TradeDebugPanel**은 버튼을 통해 두 객체를 연결하는 검증용 UI다.

```text
Save 버튼    → CreateSnapshot → Save
Load 버튼    → Load → ApplySnapshot
Restore 버튼 → Load → ApplySnapshot → Restore(CurrentUtc)
```

실제 연결에서는 Load 결과와 ApplySnapshot 성공 여부를 확인한 뒤 Restore로 진행해야 한다. 잘못된 저장 데이터를 적용하지 못했는데 기존 Runtime을 대상으로 복원을 이어가는 것은 별도 오류가 될 수 있다.

### 3.2 Snapshot과 Restore의 차이

- CreateSnapshot: 현재 Runtime을 저장 가능한 데이터로 표현한다. 모델에 따라 기준 UTC 정렬도 수행한다.
- Save: 전달된 Snapshot을 저장한다.
- Load: 저장된 Snapshot의 복사본을 반환한다.
- ApplySnapshot: 유효성을 검증한 뒤 저장 시점의 상태를 Runtime에 적용한다.
- Restore: 적용된 상태와 현재 UTC로 종료 중 시간 진행을 반영한다.

따라서 Load Only는 현재 시각까지의 복원이 아니다. 저장 당시 Traveling이었다면 실제 시간이 충분히 지났더라도 ApplySnapshot만으로 완료 판정까지 처리하는 계약은 아니다.

### 3.3 검증과 적용의 순서

ApplySnapshot은 모든 검증을 마친 다음 Runtime을 변경한다.

```text
입력 존재 여부 → 숫자·Tick·State 유효성 → 상태별 불변조건
→ 모두 성공한 경우에만 Runtime 적용
```

실패 시 false를 반환하고 기존 Runtime을 유지한다. 이는 **잘못된 입력에 의한 부분 적용 방지**다. 디스크 저장 원자성이나 스레드 안전성을 보장한다는 뜻은 아니다.

## 4. 모델 A: Elapsed + LastAppliedUtc

### 4.1 저장 데이터와 의미

```csharp
public class OfflineSaveData
{
    public float elapsed;
    public long lastAppliedUtcTicks;
    public int currentState;
}
```

Runtime은 duration, elapsed, lastAppliedUtc, currentState를 관리한다. elapsed는 초 단위이고 lastAppliedUtc는 **그 진행값까지 시간이 반영된 기준 시각**이다. 두 값은 같은 논리적 시점의 상태여야 한다.

### 4.2 상태 불변조건

```text
Prepare:   elapsed == 0
Traveling: 0 <= elapsed < duration
Completed: elapsed == duration
```

불변조건은 저장값이 범위 안에 있다는 것보다 강하다. elapsed가 60인데 Traveling이면 범위만으로는 정상처럼 보일 수 있지만 상태 계약을 위반한다.

### 4.3 복원 흐름

```text
Prepare 또는 Completed → 복원 거부
Traveling에서 CurrentUtc < LastAppliedUtc → false, Runtime 유지
그 외:
    offlineSeconds = (CurrentUtc - LastAppliedUtc).TotalSeconds
    newElapsed = elapsed + offlineSeconds
    elapsed = min(newElapsed, duration)
    완료 경계에 도달하면 Completed
    lastAppliedUtc = CurrentUtc
```

예를 들어 저장값 20초에 오프라인 10초를 더하면 30초다. 저장값 55초에 10초를 더하면 Runtime에는 60초를 남기고 Completed가 된다.

### 4.4 중복 복원과 시간 역행

복원 후 lastAppliedUtc를 갱신하므로 같은 CurrentUtc로 다시 호출하면 차이가 0이다. 아직 Traveling인 경우 정상 처리인 true를 반환할 수 있지만 실제 진행시간이 늘었다는 뜻은 아니다. 완료 후 호출은 Completed 분기에서 거부한다.

기준 시각을 갱신하지 않으면 같은 10초를 여러 번 더할 수 있다. elapsed만 바꾸거나 UTC만 바꾸는 것도 정합성을 깨뜨린다.

CurrentUtc가 LastAppliedUtc보다 과거이면 음수 시간을 더해 진행이 역행할 수 있으므로 거부한다. 다만 LastAppliedUtc 이후의 모든 관측 시각을 기억하는 것은 아니며, 미래 방향 시계 조작도 막지 못한다.

### 4.5 Snapshot 검증과 평가

검증 항목은 null, NaN·Infinity, elapsed 범위, DateTime Tick 범위, 정의되지 않은 State, State별 elapsed 조건이다. 실패 시 기존 Runtime을 보존한다.

기존 elapsed 누적 Runtime과 연결하기 쉽고 공식도 단순하다. 대신 **진행값·기준 UTC·State를 함께 일관되게 관리**해야 한다.

## 5. 모델 B: RemainingTime + Snapshot UTC

### 5.1 저장 데이터와 기준점

```csharp
public class RemainingTimeSaveData
{
    public float remainingTime;
    public long lastAppliedUtcTicks;
    public int currentState;
}
```

Runtime은 duration, remainingTime, lastAppliedUtc, currentState를 관리한다. 이름은 lastAppliedUtc이지만 **remainingTime이 어느 시점의 값인지 나타내는 Snapshot UTC 역할**을 한다. 파일을 기록한 시각이라는 의미만으로 이해하면 안 된다.

### 5.2 상태 불변조건

```text
Prepare:   remainingTime == 0
Traveling: 0 < remainingTime <= duration
Completed: remainingTime == 0
```

Prepare와 Completed 모두 0이므로 remainingTime만으로 상태를 판단할 수 없다. State가 두 의미를 구분한다.

### 5.3 복원 흐름

```text
Prepare 또는 Completed → 복원 거부
Traveling에서 CurrentUtc < LastAppliedUtc → false, Runtime 유지
그 외:
    offlineSeconds = (CurrentUtc - LastAppliedUtc).TotalSeconds
    newRemaining = remainingTime - offlineSeconds
    remainingTime = max(newRemaining, 0)
    0에 도달하면 Completed
    lastAppliedUtc = CurrentUtc
```

40초에서 오프라인 10초를 빼면 30초다. 8초에서 15초를 빼는 경우 계산값은 -7이지만 Runtime에는 0을 남기고 Completed로 바꾼다.

시간 역행을 허용하면 음수를 빼는 연산으로 남은 시간이 오히려 증가한다. 기준 UTC 이전 입력은 거부한다. 같은 시각의 반복 복원은 기준 UTC 갱신으로 차이가 0이 되어 중복 감소를 막는다.

### 5.4 CreateSnapshot 시각 정합성

학습 구현에서는 현재 UTC를 한 번만 얻어 Runtime과 Snapshot에 동일하게 사용했다.

```csharp
DateTime snapshotUtc = DateTime.UtcNow;
lastAppliedUtc = snapshotUtc;

snapshot.remainingTime = remainingTime;
snapshot.lastAppliedUtcTicks = snapshotUtc.Ticks;
```

이 코드는 **기준 시각을 동일한 값으로 맞추는 부분**을 보여준다. remainingTime 자체도 해당 시점까지 반영되어 있어야 완전한 정합성이 성립한다. 오래된 remainingTime에 새 UTC만 붙이면 중간 구간이 누락될 수 있다. 반대로 이미 반영된 구간을 다시 빼면 이중 반영된다. UTC를 한 번 조회한다는 사실만으로 모든 시간 정합성이 자동 보장되는 것은 아니다.

### 5.5 Snapshot 검증과 평가

null, NaN, 양·음 Infinity, remainingTime 범위, Tick 범위, enum, 상태별 remainingTime 관계를 검사했다. 거부 시 기존 Runtime을 유지한다.

카운트다운 중심 UI에 직관적이다. Elapsed와 수학적으로 대칭이지만 기준 UTC와 진행값을 동기화해야 하는 부담은 동일하다.

## 6. 모델 C: Absolute UTC

### 6.1 원본 데이터와 파생값

```csharp
public class AbsoluteUtcSaveData
{
    public long startUtcTicks;
    public long expectedEndUtcTicks;
    public int currentState;
}
```

Runtime은 duration, startUtc, expectedEndUtc, currentState를 관리한다. elapsed·remainingTime·progress는 원본 필드로 저장하지 않고 현재 UTC에서 계산한다. 같은 사실을 나타내는 누적값을 별도로 유지하지 않는 것이 핵심이다.

### 6.2 상태 불변조건

```text
Prepare:
    StartUtc == DateTime.MinValue
    ExpectedEndUtc == DateTime.MinValue

Traveling 또는 Completed:
    StartUtc > DateTime.MinValue
    ExpectedEndUtc > StartUtc
```

Completed에서도 해당 무역의 시작·종료 예정 시각을 보존한다. Traveling과 Completed의 시간 관계가 같은 것은 정상이다. 저장된 상태의 유효성과 현재 UTC 기준의 완료 판정은 별도 책임이기 때문이다.

### 6.3 TradeStart의 데모 정책

```text
Prepare에서 호출:
    StartUtc = 현재 UTC
    ExpectedEndUtc = StartUtc + Duration
    State = Traveling

Traveling에서 호출:
    진행 중 상태 유지

Completed에서 호출:
    StartUtc와 ExpectedEndUtc를 MinValue로 초기화
    State = Prepare
```

Completed에서의 호출은 즉시 새 무역을 출발시키는 동작이 아니라 준비 상태로 초기화하는 데모 정책이다. 이전 모델의 정산 후 초기화 흐름에 맞춘 것이며, 이 동작 자체가 실제 보상 지급이나 정산 트랜잭션 구현을 의미하지는 않는다.

### 6.4 Restore 계약

```text
Prepare → false
Completed → false

Traveling:
    CurrentUtc < StartUtc → false, 상태 유지
    StartUtc <= CurrentUtc < ExpectedEndUtc → true, Traveling 유지
    CurrentUtc >= ExpectedEndUtc → true, Completed 전이
```

Restore는 시작·종료 시각에 오프라인 시간을 더하지 않는다. 현재 시각으로 상태를 판정한다. 첫 호출에서 완료된 뒤 두 번째 호출이 false가 되더라도 진행량을 추가 누적하지 않는 성질은 유지된다.

### 6.5 파생 시간과 진행률

학습 데모의 60초 Duration과 시작·종료 간격이 일치하는 정상 무역에서 다음과 같이 계산한다.

```text
ElapsedTime   = Clamp((CurrentUtc - StartUtc).TotalSeconds, 0, Duration)
RemainingTime = Clamp((ExpectedEndUtc - CurrentUtc).TotalSeconds, 0, Duration)
ProgressRatio = Clamp(ElapsedTime / Duration, 0, 1)
```

Prepare에서는 모두 0이다. 메서드가 currentUtc를 인자로 받도록 작성해 고정 입력 테스트가 가능했다.

```csharp
CalculateElapsedTime(DateTime currentUtc)
CalculateRemainingTime(DateTime currentUtc)
CalculateProgressRatio(DateTime currentUtc)
```

```text
StartUtc = 12:00, ExpectedEndUtc = 13:00인 설명용 예시
현재 12:00 → 진행률 0
현재 12:30 → 진행률 0.5
현재 13:00 → 진행률 1, Restore 시 Completed
현재 13:10 → 진행률은 1을 넘지 않음
```

이 예시에서는 Duration도 1시간이라고 가정한다. 학습 데모의 실제 설정은 60초다.

**Duration 정합성 주의:** 제공된 ApplySnapshot 검증 목록에는 Start/End 간격이 현재 설정 Duration과 같은지 확인하는 조건까지 명시되어 있지 않다. 임의 Snapshot이나 설정 변경으로 두 값이 달라졌을 때 진행률·완료 판정까지 일관된다고 주장할 수 없다. 확장 시에는 간격을 Duration으로 삼을지, 설정과 일치하도록 검증할지 정책을 정해야 한다. 이번 구현이 이를 추가 해결했다고 기록하지 않는다.

### 6.6 Snapshot 검증

null, 두 Tick의 최소·최대 범위, enum 유효성, Prepare의 MinValue 규칙, Traveling·Completed의 Start/End 관계를 검사했다. 모든 검증 후 적용하여 실패 시 기존 Runtime을 보존했다.

### 6.7 중복 복원과 Clock Rollback 한계

같은 StartUtc, ExpectedEndUtc, CurrentUtc에서 진행률은 동일하다. 누적값을 변경하지 않으므로 중복 반영 방지 목적으로 LastAppliedUtc를 갱신할 필요가 없다.

기본형은 CurrentUtc < StartUtc를 거부하지만 마지막 관측 시각을 저장하지 않는다.

```text
StartUtc = 12:00, ExpectedEndUtc = 13:00
이전 관측 12:40 → 약 66.7%
새 관측   12:30 → 50%
```

12:30은 여전히 StartUtc 이후이므로 복원이 허용된다. 따라서 **부분적인 시간 역행을 감지하지 못하고 진행률이 감소할 수 있다.** 같은 시각에서의 비누적성과 서로 다른 시각 사이의 단조 증가는 다른 성질이다.

## 7. 실제 검증 범위와 결과

### 7.1 수동 Regression 검증

제공된 학습 기록에 따르면 세 모델 모두 DebugPanel, 고정 DateTime 시나리오, Unity 로그를 사용해 검증했고 기대 정책과 결과가 일치했다.

- Happy Path: 저장 상태에 정상 오프라인 시간을 반영.
- Load Only: 현재 시간이 아닌 저장 시점 상태만 적용.
- Offline Complete: 종료 중 완료 경계를 넘으면 Completed.
- Clock Rollback: 각 모델이 정한 기준 이전 시간 입력 거부.
- Duplicate Restore: 동일 시각 반복 호출로 진행량 중복 반영 방지.
- Prepare·Completed Restore: 해당 상태의 복원 거부.
- Invalid Snapshot: 잘못된 입력 거부.
- Runtime 불변성: 실패한 입력으로 기존 상태가 변경되지 않음.

Absolute UTC는 CurrentUtc == StartUtc에서 진행률 0·Traveling, CurrentUtc == ExpectedEndUtc에서 진행률 1·Completed도 확인했다.

Clock Rollback 검증 통과는 각 모델에 구현된 거부 조건의 통과를 뜻한다. Absolute UTC가 부분적 시간 역행까지 검출했다는 뜻은 아니다.

### 7.2 Unity EditMode 테스트 구성

Unity Test Framework와 NUnit을 사용했다. MonoBehaviour를 직접 new하지 않고 GameObject에 컴포넌트를 추가했다. 다음은 기록에 제시된 구성 예시다.

```csharp
[SetUp]
public void SetUp()
{
    gameObject = new GameObject();
    manager = gameObject.AddComponent<AbsoluteUtcTradeManager>();
}

[TearDown]
public void TearDown()
{
    UnityEngine.Object.DestroyImmediate(gameObject);
}
```

각 모델에 맞는 Manager 타입으로 초기화하고 Arrange → Act → Assert 순서로 검증했다. 반복되는 유효 Runtime 구성은 SetValidRuntime helper로 분리했다. 시간 입력은 DateTime.UtcNow 대신 고정 UTC를 사용했다.

Invalid Snapshot 테스트는 먼저 유효 Runtime을 만든 뒤 잘못된 데이터를 적용하고, false 반환과 기존 필드 보존을 함께 확인했다. 처음부터 기본값만 두면 잘못된 변경을 놓칠 수 있으므로 유효한 사전 상태가 중요하다.

### 7.3 자동화한 계약

ApplySnapshot 테스트는 공통으로 범위·enum·Tick·상태 불변조건·실패 시 Runtime 보존을 다뤘다. Elapsed와 RemainingTime은 NaN·Infinity, Absolute UTC는 Start/End 관계와 Prepare 규칙을 포함했다.

Restore 테스트는 정상 Traveling 복원, Offline Complete, Clock Rollback, Duplicate Restore, Prepare 거부, Completed 거부를 자동화했다. Absolute UTC는 시작·종료 경계, 파생 진행률·남은 시간, 시작·종료 시각 보존도 검증했다.

**결과: 사용자가 세 모델의 전체 EditMode 테스트를 실행해 모두 PASS했음을 확인했다.** 개별 테스트명이나 정확한 개수는 기록에 없으므로 추정하지 않는다.

### 7.4 검증 결과의 한계

이번 결과는 학습 데모의 메모리 저장과 시간·상태 계약에 대한 결과다. 다음은 이번 학습의 검증 완료 항목으로 주장하지 않는다.

- 실제 JSON 파일 기록, 중간 종료, 파일 손상과 복구.
- 디스크 저장 원자성·내구성, 정산 트랜잭션.
- 서버 시간 검증·클라이언트 시간 조작 방지.
- 성능 벤치마크·다중 캐러밴 부하 검증.
- 실제 빌드 환경 및 운영체제별 장기 동작.
- PlayMode 전체 정상 동작, 모든 Inspector Reference, 모든 Scene의 Console 오류 부재.

Unity 공식 문서는 EditMode와 PlayMode 테스트를 구분한다. EditMode 테스트 PASS를 모든 실행 환경의 검증으로 확대하면 안 된다. 참고: [Unity Test Framework 소개](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction).

## 8. 다섯 가지 Trade-off 기준

아래 다섯 항목은 제공된 학습 기록의 최종 비교 기준이다. 성능 측정에 따른 순위가 아니라 데이터와 복원 계약에 대한 설계 비교다.

### 8.1 저장 데이터 구조 / SSOT

SSOT는 같은 사실에 대한 기준 데이터를 하나로 명확히 정하는 관점이다.

- Elapsed: elapsed + 기준 UTC + State. 진행량이 원본이다.
- RemainingTime: remainingTime + 기준 UTC + State. 남은 시간이 원본이다.
- Absolute UTC: StartUtc + ExpectedEndUtc + State. 일정이 원본이며 진행량은 파생값이다.

이번 SaveData는 세 모델 모두 세 필드다. Absolute UTC가 필드 수나 저장 용량에서 가장 작다고 말할 근거는 없다. 장점은 누적 진행값을 별도 원본으로 관리하지 않는 데 있다.

### 8.2 Restore 계산 복잡도

- Elapsed: UTC 차이를 초로 변환해 더하고 Duration으로 Clamp.
- RemainingTime: UTC 차이를 초로 변환해 빼고 0으로 Clamp.
- Absolute UTC: 현재 시각을 시작·종료와 비교하고 표시값은 필요할 때 계산.

세 방식 모두 단일 무역에 대한 계산은 단순하다. Absolute UTC의 이점은 복원 시 갱신 대상과 정합성 부담이 줄어드는 것이며, 벤치마크 없이 더 빠르다고 단정하지 않는다.

### 8.3 Duplicate Restore 안정성

- Elapsed·RemainingTime: 반영한 진행값과 함께 기준 UTC를 갱신해야 한다.
- Absolute UTC: 같은 시각의 비누적 계산으로 동일 결과를 얻는다.

Elapsed·Remaining도 올바르게 구현하면 반복 복원에 안전하다. 다만 오래된 Snapshot을 다시 적용하면 기준점도 되돌아간다는 점을 구분해야 한다. 저장 상태부터 다시 계산하는 것과 이미 반영한 Runtime에 중복 가산하는 것은 다르다. 어느 방식도 반복 복원 뒤의 보상 지급을 자동으로 안전하게 만들지는 않는다.

### 8.4 Clock Rollback 대응

- Elapsed·RemainingTime: CurrentUtc < LastAppliedUtc를 거부.
- Absolute UTC 기본형: CurrentUtc < StartUtc를 거부. 마지막 관측 시각보다 과거이면서 StartUtc 이후인 입력은 검출하지 못함.

LastAppliedUtc도 기록된 기준점 이후의 모든 관측을 기억하는 장치는 아니다. 세 모델 모두 클라이언트 UTC가 미래로 이동하는 문제는 별도 대응이 필요하다.

### 8.5 데이터 정합성 관리 난이도

- Elapsed: 진행값·기준 UTC·State를 동기화.
- RemainingTime: 남은 시간·기준 UTC·State를 동기화. 0만으로 Prepare·Completed 구분 불가.
- Absolute UTC: 시작·종료·State 관계를 관리. 누적값 갱신은 줄지만 Duration과 일정의 관계, 시간 신뢰성은 여전히 중요.

Absolute UTC는 이번 요구사항에서 관리가 단순했지만 모든 요구사항에서 우월한 것은 아니다. 일시정지·가속·소요시간 변경이 추가되면 종료 예정시각을 어떻게 변경할지 별도 정책이 필요하다. 이는 이번에 구현한 기능이 아니라 선택을 재검토할 확장 조건이다.

## 9. 최종 선택: Absolute UTC

사용자는 다음 두 이유로 Absolute UTC를 선택했다.

1. **예상 종료시각이 시작 시점에 결정된다.** StartUtc + Duration으로 ExpectedEndUtc를 정할 수 있는 무역이므로 일정 자체를 저장하는 방식이 자연스럽다.
2. **Restore가 중복 호출될 수 있다.** 같은 현재 UTC를 기준으로 진행값을 다시 계산하므로 진행시간을 반복해서 더하거나 남은 시간을 반복해서 빼는 구조를 피한다.

감수한 제약은 클라이언트 UTC 의존성과 부분적인 Clock Rollback이다. 마지막 관측 시각을 저장하지 않아 StartUtc 이후에서의 시간 역행을 감지하지 못한다. 이 선택은 단점을 해결했다는 선언이 아니라, 현재 요구사항에 맞춰 장점과 한계를 함께 받아들인 결정이다.

## 10. 시간 소스와 Clock Rollback

### 10.1 UTC와 신뢰성은 별개

UTC는 시간대와 현지 시각 해석을 통일하는 기준이지 변조 방지 기능이 아니다. DateTime.UtcNow는 클라이언트 OS 시계에 의존하므로 기기 설정 변경이나 시간 보정의 영향을 받을 수 있다. API 정의와 시스템 타이머 의존성은 [Microsoft DateTime.UtcNow 문서](https://learn.microsoft.com/dotnet/api/system.datetime.utcnow)를 참고한다.

시계가 미래로 이동하면 무역이 조기 완료될 수 있다. 과거로 이동하면 모델의 기준점에 따라 복원이 거부되거나 진행률이 감소할 수 있다. 이 문제는 Absolute UTC만의 문제가 아니라 클라이언트 UTC를 사용하는 세 모델 모두의 한계다.

### 10.2 후속 보완 방안과 한계

아래는 고려사항이며 구현·검증 완료 항목이 아니다.

- 마지막 관측 UTC 영속화: 부분적 역행 판정의 기준을 남길 수 있지만 상태와 저장 부담이 증가한다. 메모리에만 두면 재실행 후 잃는다.
- 최대 진행률 유지: 표시나 진행량의 감소를 막을 수 있지만 잘못된 미래 시각으로 확보한 진행량을 정당화하지는 못한다.
- 신뢰 가능한 외부 시간: 클라이언트 시계 의존을 줄인다. 통신 실패·동기화·오프라인 허용 정책이 필요하다.
- 오프라인 반영 상한: 비정상적으로 큰 경과시간의 영향을 제한한다. 실제로 오래 접속하지 않은 사용자에게도 영향을 준다.
- 실행 중 단조 시간 기준: 세션 안의 경과시간 측정에 활용할 수 있지만 프로세스 종료·재시작을 넘는 오프라인 시간의 신뢰성을 단독으로 해결하지 않는다.

어떤 보완책을 선택할지는 재화 경제의 중요도, 완전 오프라인 지원 여부, 서버 권한 요구에 따라 달라진다.

## 11. 복원 멱등성, 정산 멱등성, 원자성

### 11.1 복원의 멱등성

고정된 시각 t에 대한 상태 변환을 F_t라고 할 때, 관심 상태에 대해 F_t(F_t(S)) = F_t(S)가 되면 같은 처리를 반복해도 추가 변화가 없다.

Absolute UTC에서 동일한 시작·종료·현재 시각으로 진행 계산을 반복하면 진행량이 추가 누적되지 않는다. 다만 완료 전 첫 Restore는 true, 완료 후 재호출은 false일 수 있다. **멱등성을 모든 반환값이 동일하다는 뜻으로 설명하면 부정확하다.**

시간이 달라진 호출에서 진행값이 달라지는 것은 정상이다. 부분적 Clock Rollback으로 진행률이 감소할 수 있다는 사실과 같은 시각의 반복 계산이 비누적이라는 사실도 양립한다.

### 11.2 완료와 지급 완료는 다르다

```text
CurrentState == Completed → 시간 진행이 끝남
SettlementClaimed == true → 해당 거래의 보상이 지급됨
TradeId                  → 어떤 거래의 정산인지 식별
```

Completed만 검사한 다음 골드를 지급하는 메서드를 두 번 호출하면 두 번 모두 조건이 참일 수 있다. Restore가 두 번째 호출을 거부해도 별도 정산 메서드가 안전해지는 것은 아니다.

```text
TradeId A123, Completed, SettlementClaimed false
→ 완료 여부 확인
→ 동일 TradeId의 지급 이력 확인
→ 미정산이면 지급과 정산 기록을 함께 반영
→ 같은 TradeId 재요청은 재지급하지 않음
```

TradeId와 SettlementClaimed는 이번 마지막 논의에서 다룬 정산 확장 개념이다. 세 시간 복원 SaveData에 이미 구현한 필드로 소개하지 않는다. 보상 책임을 분리하더라도 다른 객체의 내부 State에 직접 의존하기보다 완료 여부와 정산 식별자를 명확한 계약으로 전달하는 것이 좋다. 구체적 정산 API는 이번 구현 범위가 아니다.

### 11.3 정산 원자성이 필요한 이유

```text
골드 1,000 지급이 저장됨
→ SettlementClaimed 저장 전에 종료
→ 재실행 시 지급 기록은 false
→ 같은 거래에 골드 1,000 재지급 가능
```

지급 여부와 정산 기록이 어긋나는 부분 저장을 막아야 한다. 기준 골드를 G라고 하면 정상 영속 상태는 다음 두 가지다.

- 정산 전: Gold = G, Claimed = false.
- 정산 후: Gold = G + 1000, Claimed = true.

Gold만 증가하고 Claimed가 false면 중복 지급 위험이 있다. Claimed만 true이고 Gold가 그대로여도 보상을 받지 못하는 문제가 생긴다. 따라서 단순히 기록 순서를 바꾸는 것으로 해결할 수 없다.

```csharp
playerGold += 1000;
settlementClaimed = true;
SaveGame();
```

위 코드가 연속 실행된다는 사실은 영속 저장의 원자성을 보장하지 않는다. 하나의 JSON 파일에 담는 것만으로도 충분하지 않다.

후속 설계에서는 골드와 지급 이력을 함께 포함한 Snapshot을 저장 단위로 만들고, 실패 시 이전의 유효한 상태 또는 새로운 유효한 상태로 복구하는 정책을 고려할 수 있다. 임시 파일 작성 후 교체도 후보지만 플랫폼의 교체 동작과 실패 복구까지 확인해야 한다. 저장 실패 후 같은 프로세스에서 재시도할 때 메모리 상태를 어떻게 다룰지도 정해야 한다.

동시 정산 요청이 가능하다면 두 요청이 모두 미정산을 확인하는 경쟁도 별도로 막아야 한다. 하나의 변경 책임으로 직렬화하거나 적절한 트랜잭션·중복 키 정책 등을 선택할 수 있다. 이러한 동시성 대응 역시 이번 구현 결과가 아니다.

**학습 도달점:** 사용자는 1,000골드 중복 지급 가능성을 설명하고 지급과 정산 완료 기록이 원자적으로 저장되어야 함을 답했다. 실제 정산 트랜잭션·파일 교체 복구·서버 정산을 구현한 단계는 아니다.

## 12. 구현 과정의 실수와 교훈

### 12.1 Valid와 Invalid 조건

```text
Valid:   Min <= Value <= Max
Invalid: Value < Min OR Value > Max
```

정상 범위를 나타내는 조건을 거부 분기에 그대로 넣지 않는다. 경계값 포함 여부도 상태 불변조건과 맞춰야 한다.

### 12.2 논리 연산자 묶음

```csharp
if (state == SomeState &&
    (invalidA || invalidB || invalidC))
{
    return false;
}
```

&&와 ||를 의도대로 묶지 않으면 다른 State의 Snapshot까지 거부할 수 있다. State 조건이 어느 오류 조건에 적용되는지 괄호로 명확히 한다.

### 12.3 NaN·Infinity와 단위

NaN은 value == float.NaN으로 검출하지 않는다. float.IsNaN(value), float.IsInfinity(value)를 사용한다. 범위 비교만으로 NaN 처리를 대신하지 않는다.

TimeSpan.Ticks와 TotalSeconds는 단위가 다르다. elapsed와 remainingTime이 초라면 UTC 차이는 TotalSeconds로 변환한다. Tick은 저장된 시각 표현이고, 초는 진행량 계산 단위다.

### 12.4 테스트의 거짓 양성

Restore 직전 남은 시간이 5초인데 과거 설정값 10초보다 작다는 것만 검사하면, Restore가 전혀 동작하지 않아도 통과할 수 있다. 입력을 명시하고 예를 들어 2초 반영 후 3초라는 **정확한 기대값**을 검증해야 한다.

고정 UTC를 사용하고 실패 시 모든 관련 필드가 보존되는지 확인한다. 테스트 이름이 시나리오를 설명하더라도 Assert가 실제 계약을 확인하지 않으면 충분하지 않다.

### 12.5 Partial Apply와 저장 원자성 구분

ApplySnapshot에서 검증을 마친 뒤 값을 적용하는 것은 Runtime 오염을 막는다. 이것을 실제 파일 저장의 원자적 커밋이나 자동 롤백으로 확대 설명하지 않는다. 각 계약의 경계를 정확히 말하는 것이 이번 학습의 중요한 결과다.

## 13. 기술면접 대비

### 13.1 30초 핵심 설명

> 게임 종료 중에는 Update가 실행되지 않아 무역 진행을 별도로 복원해야 했습니다. Elapsed, RemainingTime, Absolute UTC를 연습 프로젝트에서 직접 구현해 비교했습니다. 종료 예정시각이 시작할 때 결정되고 복원이 반복 호출될 수 있어, 같은 시각에 진행량을 중복 누적하지 않는 Absolute UTC를 선택했습니다. 다만 클라이언트 UTC를 사용하므로 시작 이후의 부분적인 시간 역행을 감지하지 못한다는 한계는 남아 있습니다.

### 13.2 1~2분 상세 설명

> 방치형 무역 게임에서 종료 중에도 무역 시간이 흘러야 했습니다. 기존 구현을 이해하고 다시 만들기 위해 별도 연습 프로젝트에서 단일 무역의 복원을 세 방식으로 구현했습니다.
>
> Elapsed는 진행시간에 UTC 차이를 더하고 RemainingTime은 남은 시간에서 차이를 빼는 방식입니다. 둘 다 진행값과 마지막 적용 UTC를 함께 갱신해야 중복 반영을 막을 수 있습니다. Absolute UTC는 시작·종료 예정시각을 원본으로 두고 현재 UTC에서 진행량을 계산합니다.
>
> 저장·조회는 복사본을 사용하는 메모리 SaveService로 분리하고, Snapshot 검증과 시간 복원은 Manager가 담당하게 했습니다. 잘못된 Snapshot은 모든 검증이 끝나기 전에는 Runtime에 적용하지 않도록 했습니다.
>
> DebugPanel 수동 검증 후 고정 UTC를 사용하는 EditMode 테스트로 정상 복원, 완료 경계, 중복 호출, 시간 역행, 잘못된 저장값과 실패 시 Runtime 보존을 확인했습니다. 세 모델의 전체 테스트는 PASS했습니다.
>
> 최종적으로 종료시각이 정해진 무역에 자연스럽고 중복 Restore 시 누적값을 관리하지 않아도 되는 Absolute UTC를 선택했습니다. 다만 부분적 Clock Rollback과 클라이언트 시계 신뢰성은 해결하지 못합니다. 또한 완료 판정과 보상 지급은 별개라 정산에는 거래 식별자와 지급 이력, 지급과 기록의 원자적 저장이 필요하다는 점까지 학습했습니다. 실제 정산 트랜잭션과 파일 장애 복구는 이번 데모에서 구현하지 않았습니다.

### 13.3 예상 질문과 답변

#### Q1. 오프라인 진행 복원은 왜 필요한가요?

프로세스 종료 중에는 Update와 Time.deltaTime 누적이 실행되지 않습니다. 저장된 진행값만으로는 종료 중 시간을 알 수 없으므로 UTC 기준점이나 절대 종료시각으로 현실 시간의 경과를 반영해야 합니다.

#### Q2. Elapsed와 RemainingTime은 어떻게 다른가요?

Elapsed는 경과시간을 더하고 RemainingTime은 남은 시간을 줄입니다. 완료 시 각각 Duration과 0으로 Clamp합니다. 둘 다 값이 어느 시점의 것인지 나타내는 기준 UTC가 필요하고 복원 후 값과 UTC를 함께 갱신해야 합니다.

#### Q3. Absolute UTC를 선택한 이유는 무엇인가요?

무역의 예상 종료시각을 시작 시 정할 수 있고 Restore가 중복 호출될 수 있기 때문입니다. 저장된 일정에서 다시 계산하면 진행값을 누적 반영하지 않아 반복 호출의 관리가 단순해집니다. 기본형의 부분적 시간 역행 한계는 감수했습니다.

#### Q4. Absolute UTC에는 왜 LastAppliedUtc가 없어도 되나요?

저장된 진행량에 차이를 더하는 방식이 아니기 때문입니다. 같은 현재 시각에서 StartUtc와 ExpectedEndUtc로 계산하면 결과가 같아 중복 반영 방지용 기준점을 갱신할 필요가 없습니다. 시간 역행 감지까지 필요하면 별도 관측 기준을 추가할 수 있습니다.

#### Q5. Absolute UTC는 완전히 멱등적인가요?

같은 시각에 대한 진행 계산은 비누적입니다. 하지만 완료 전후 State에 따라 Restore 반환값은 달라질 수 있고 보상 지급 같은 별도 부작용까지 자동으로 멱등적이지는 않습니다. 어떤 상태와 부작용에 대한 멱등성인지 구분해야 합니다.

#### Q6. Clock Rollback을 어떻게 처리했나요?

Elapsed·RemainingTime은 CurrentUtc가 LastAppliedUtc보다 과거이면 거부했습니다. Absolute UTC 기본형은 StartUtc보다 과거이면 거부했습니다. 거부할 때 기존 Runtime을 유지하도록 검증했습니다.

#### Q7. Absolute UTC의 Clock Rollback 한계는 무엇인가요?

마지막 관측 시각을 저장하지 않아 시작 이후의 부분적 역행을 감지하지 못합니다. 12시 시작 후 12시 40분을 봤다가 12시 30분이 되어도 StartUtc 이후라 허용되고 진행률이 감소할 수 있습니다.

#### Q8. DateTime.UtcNow는 신뢰 가능한 시간인가요?

UTC 기준 시각이지만 클라이언트 OS 시계에 의존합니다. 사용자 변경이나 시간 보정의 영향을 받을 수 있습니다. UTC 사용과 시간의 신뢰성은 다른 문제이며 세 모델 모두 같은 한계가 있습니다.

#### Q9. Load하면 바로 현재 시각까지 복원되나요?

아닙니다. Load는 저장 데이터를 읽고 ApplySnapshot은 저장 상태를 검증해 적용합니다. 이후 Restore가 현재 UTC로 시간 진행을 반영합니다. 이 책임을 나눠 Load Only와 시간 복원을 각각 검증했습니다.

#### Q10. 잘못된 SaveData는 어떻게 처리하나요?

ApplySnapshot에서 null, 숫자·Tick 범위, enum, 상태 불변조건을 검사합니다. 모든 검증이 끝난 뒤 Runtime을 바꾸므로 실패하면 false를 반환하고 기존 상태를 보존합니다. Elapsed·RemainingTime에는 NaN·Infinity 검사도 포함했습니다.

#### Q11. EditMode 테스트를 사용한 이유는 무엇인가요?

현재 시간을 인자로 주고 고정 UTC로 경계값과 반복 호출을 결정적으로 검증하기 위해서입니다. GameObject에 Manager 컴포넌트를 추가해 테스트했고, 반환값뿐 아니라 시간값·State·실패 시 기존 값 보존을 확인했습니다. 파일 장애나 실제 운영 환경 전체를 검증한 것은 아닙니다.

#### Q12. 어느 모델이 항상 더 좋은가요?

항상 우수한 모델은 없습니다. 기존 Runtime이 누적시간 중심인지, 종료시각이 정해지는지, 어떤 역행 정책이 필요한지에 따라 다릅니다. 이번에는 다섯 기준으로 비교하고 고정된 무역 일정과 중복 복원 요구에 맞춰 Absolute UTC를 선택했습니다.

#### Q13. Restore가 멱등적이면 보상 중복 지급도 막히나요?

아닙니다. Completed는 시간이 끝났다는 뜻이지 지급 완료가 아닙니다. 정산은 TradeId와 지급 완료 기록 등을 통해 같은 거래의 반복 지급을 별도로 막아야 합니다. 이번에는 그 차이를 학습했으며 실제 정산 시스템을 구현하지는 않았습니다.

#### Q14. 골드 지급과 Claimed 저장 사이에 종료되면 어떻게 되나요?

골드만 저장되고 Claimed가 false로 남으면 재실행 후 다시 지급할 수 있습니다. 반대로 기록만 남으면 보상을 잃을 수 있습니다. 두 변경이 함께 반영되는 논리적 트랜잭션과 실패 복구가 필요하며, 코드 두 줄의 연속 실행이나 단일 JSON이라는 사실만으로 보장되지는 않습니다.

#### Q15. 상용 게임으로 확장한다면 무엇을 개선하겠습니까?

시간 신뢰성 요구를 먼저 정하고 외부 시간이나 역행 정책을 검토하겠습니다. 정산은 TradeId 기반 중복 방지와 지급·기록의 저장 원자성을 함께 설계하고 파일 손상·실패 복구를 검증하겠습니다. 다중 무역의 데이터 소유권과 변경 책임, Duration 변경의 저장 호환성도 별도 요구사항으로 다루겠습니다. 모두 이번 데모 이후의 확장 항목입니다.

#### Q16. RemainingTime이 0이면 완료 아닌가요?

이번 모델은 Prepare도 0입니다. 따라서 State와 함께 봐야 합니다. Traveling만 0 초과이고, Prepare와 Completed는 같은 시간값이라도 서로 다른 도메인 상태입니다.

#### Q17. SaveService에서 도메인 검증을 하지 않은 이유는 무엇인가요?

저장과 조회 책임을 시간·상태 규칙에서 분리하기 위해서입니다. Manager의 ApplySnapshot이 불변조건을 검증하고 Runtime 적용을 소유합니다. SaveService의 복사본 처리는 참조 공유를 통한 저장값 변경을 막는 역할이며 도메인 검증을 대신하지 않습니다.

#### Q18. 테스트 작성 중 배운 점은 무엇인가요?

과거 설정값과 단순 비교하면 실제 변화가 없어도 테스트가 통과할 수 있었습니다. 그래서 Act 직전 입력을 명확히 하고 정확한 기대값을 검증하는 것이 중요했습니다. Invalid Snapshot 테스트에서는 false만 확인하지 않고 기존 Runtime 필드가 보존되는지도 확인했습니다.

#### Q19. Absolute UTC가 저장 데이터를 가장 적게 쓰나요?

이번 SaveData는 세 방식 모두 세 필드라 그렇게 설명할 수 없습니다. 장점은 필드 수가 아니라 복원 시 누적 진행값과 기준 UTC를 동시에 갱신할 필요가 없다는 점입니다. 저장 용량이나 성능 우위는 측정하지 않았습니다.

### 13.4 답변 시 과장하지 않을 사항

- 세 모델의 구현·수동 검증·EditMode 전체 PASS는 학습 결과로 말할 수 있다. 원본 코드의 모든 동작이나 운영 환경까지 검증했다고 말하지 않는다.
- Absolute UTC의 비누적성을 모든 Clock Rollback 해결이나 보상 중복 지급 방지로 확대하지 않는다.
- 메모리 SaveService를 실제 JSON 영속 저장 구현으로 소개하지 않는다.
- 원자성의 필요성을 설명한 경험과 실제 트랜잭션을 구현한 경험을 구분한다.
- 필드 수·성능·테스트 개수에 근거 없는 우위를 붙이지 않는다.
- 이번 작업에서 테스트를 다시 실행했다는 표현을 사용하지 않는다.

### 13.5 마지막 핵심 메시지

> 오프라인 진행 복원의 핵심은 종료 중 경과시간 계산뿐 아니라 저장 데이터 정합성, 중복 복원 안전성, 시간 소스 신뢰성, 완료 후 부작용을 각각의 책임으로 구분하는 데 있습니다. 저는 단일 무역의 세 복원 모델을 직접 구현하고 수동 검증과 EditMode 테스트로 핵심 계약을 확인했습니다. 그 비교를 통해 Absolute UTC를 선택했고, 부분적 시간 역행과 정산 원자성은 별도로 해결해야 한다는 한계까지 설명할 수 있게 되었습니다.
