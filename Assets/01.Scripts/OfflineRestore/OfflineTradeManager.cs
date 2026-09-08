using System;
using UnityEditor.Overlays;
using UnityEngine;

/*
문제 정의 
플레이어가 60초가 걸리는 Trade를 시작합니다.
20초가 지난 시점에 게임을 종료합니다.
게임을 종료한 상태로 30초가 흐른 뒤 다시 실행합니다.
 */

// =========================================================================
// Step 1

// 가장 단순하게 Trade 진행 시간을 구현한다면?
// - float로 목표 무역 시간을 저장
// - float로 진행 한 시간을 저장
// - Update에서 Time.deltaTime으로 진행시간을 Tick당 증가 
// - 목표 무역시간에 도달하면 무역 진행 시간을 0으로 저장 후 무역 완료

// 이 방식으로 오프라인 진행을 구현하지 못하는 이유
// - Time.deltaTime을 이용한 진행도 누적은 게임 프로세스가 실행되는 동안에만 이루어진다.
// 따라서 애플리케이션이 종료된 시간 동안에는 상태가 갱신되지 않으며, 재실행 시 오프라인 경과시간을 추론할 근거가 없다. 

// 재실행 했을 때 시스템이 반드시 알아야 하는 정보는? 
// - Runtime이 끝나고 다시 온라인이 될때 까지 얼만큼의 시간이 진행되었는지

// 위 내용으로 게임을 50초 동안 종료했다가 다시 실행한다면 어떤 상태로 복원되어야 할까요?
// - 목표는 오프라인 시간동안 무역이 진행되어야 하겠지만 위 구조대로라면 오프라인동안은 무역이 진행되지 않을것입니다. 
// =========================================================================

// Step 2

// 요구사항과 불변 조건 

// A. 정상 복원 
// Trade Duration = 60초
// 종료 당시 진행 = 20초
// Offline = 10초 
// 재실행시 
// Elapsed = 30초
// Remaining = 30초 
// State = Traveling

// B. 오프라인 중 완료 
// Trade Duration = 60초
// 종료 당시 진행 = 20초
// Offline = 50초
// 재실행하면
// Elapsed = 70초 (Elapsed 실제 경과시간으로 사용하던지, 진행도 Clamp 값으로 사용할지 정할 수 있습니다.)
// Remaining = 0초
// State = Completed

// C. 이미 완료된 Trade
// Trade가 이미 Completed 상태 인데 게임을 재실행했을 때 무역 상태를 Enum으로 관리하면서 오프라인 복원시 Traveling상태가 아니라면 
// 복원 진행을 리턴

// D. 저장 데이터가 없음
// 최초 실행시 최초값을 설정한 저장데이터를 생성해야합니다. 
// 최초 실행시 RunTimeState를 생성하고 이후 저장 요청시 세이브 데이터를 생성하는 방법도 있다.

// E. 이상한 시간
// 방어 코드로 이상한 시간이 감지되었을대 예외 처리를 합니다.
// 위 대답은 너무 추상적
// 어떻게 예외 처리를 할지 후보
// 0으로 Clamp
// Restore 자체를 중단
// 저장 데이터를 Invalid 처리
// Exception 발생

// 불변 조건
// 1. 0 < Elapsed < Duration 이면 상태는 여전히 진행중이어야 한다.
// 2. 저장 데이터가 없더라도 예외 없이 기본 상태로 시작할 수 있어야 한다.
// 3. 시간 복원 때문에 Trade 진행도가 감소해서는 안 된다.

// =========================================================================
// Step 3 — 데이터 흐름 설계

/*
 Trade Start -> RunTimeData Validate 체크 -> InValid 라면 RuntimeData 생성 혹은 SaveData 생성(추후 정책에 따라) 
-> Valid 라면 Trade State 가 Traveling 인지 체크 -> Traveling이 아니라면 복원 진행 안하고 각 State에 맞는 입력을 처리
-> Traveling이라면 Elapsed 가 음수인지 체크 -> 음수라면 0으로 Clamp -> 양수라면 Duration 과 비교해 같거나 큰지 확인
-> Elapsed < Duration 라면 두 값을 비교해 Progress 값을 0 ~ 1 사이의 float 값으로 계산 -> Elapsed >= Duration 이라면 
Progress는 1, State를 Complete로 수정
 */

// 데이터 흐름은 
/*
 온라인 중에는 RuntimeData -> 오프라인 전 SaveData로 SnapShot -> 재실행 시 현실 시간을 Get -> SaveData를 RuntimeData에 Load
-> 현실시간과 RuntiemData의 Trade Start 시간을 비교 -> 오프라인 진행률 계산
 */

// 1. Trade 진행 상태의 원본은 어디에 있어야 할까요? : RuntimeData가 원본, SaveData는 오프라인전 SnapShot 으로 사용
// 2. 어떤 데이터가 저장되어야 오프라인 시간을 계산할 수 있을까요? : Trade 시작 시간, Elapsed, Duration
// 3. Load와 Restore는 같은 책임일까요, 다른 책임일까요? : 다른 책임 입니다. Load는 저장 데이터를 유효하게 불러오는것 까지,
//  Restore는 현재 상태 재구성 까지가 책입입니다. 
// 4. Restore가 끝난 뒤 어떤 값을 갱신해야 중복 복원을 막을 수 있을까요? : Trade State를 체크하고 Elapsed를 갱신

/*

 Trade Start
↓
Runtime State 생성
↓
Runtime 진행
↓
특정 시점에 Snapshot 저장
↓
게임 종료

──── Runtime 없음 ────

현실 시간 경과

──── 게임 재실행 ────

Snapshot Load
↓
현재 현실 시간 획득
↓
Offline Elapsed 계산
↓
현재 Trade 상태 재구성
↓
Runtime State 갱신
↓
새 기준점 확립
 */

enum OfflineTradeState
{
    Prepare = 0,
    Traveling = 1,
    Completed = 2,
}

public class OfflineTradeManager : MonoBehaviour
{
    [SerializeField] private float duration = 60f;
    private float elapsed;


    private DateTime lastAppliedUtc;

    private OfflineTradeState currentState;

    private OfflineSaveData currentSaveData;

    public float Elapsed => elapsed;
    public float Progress => elapsed / duration;
    public DateTime LastAppliedUtc => lastAppliedUtc;
    public OfflineTradeState CurrentState => currentState;
    public string CurrentStateString => currentState.ToString();

    private void Awake()
    {
        elapsed = 0;
        lastAppliedUtc = DateTime.UtcNow;
        currentState = OfflineTradeState.Prepare;
    }

    private void Update()
    {
        if (currentState == OfflineTradeState.Traveling)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= duration)
            {
                elapsed = duration;

                currentState = OfflineTradeState.Completed;
            }
        }
    }

    public void TradeStart()
    {
        if (currentState == OfflineTradeState.Prepare)
        {
            elapsed = 0;
            lastAppliedUtc = DateTime.UtcNow;
            currentState = OfflineTradeState.Traveling;
        }

        if (currentState == OfflineTradeState.Completed)
        {
            currentState = OfflineTradeState.Prepare;
        }
    }

    private bool Restore(DateTime currentUtc)
    {
        bool restored = false;

        if (currentUtc < lastAppliedUtc)
        {
            return restored;
        }

        if (currentState == OfflineTradeState.Traveling)
        {
            TimeSpan offlineElapsed = currentUtc - lastAppliedUtc;

            double offlineSeconds = offlineElapsed.TotalSeconds;

            if (offlineSeconds < 0)
            {
                offlineSeconds = 0;
            }

            elapsed += (float)offlineSeconds;

            lastAppliedUtc = currentUtc;

            if (duration <= elapsed)
            {
                elapsed = duration;
                currentState = OfflineTradeState.Completed;
            }

            restored = true;

            return restored;
        }

        return restored;
    }

    public void SaveButton()
    {
        Debug.Log("Save Button");
        SaveTrade();
    }

    public void LoadButton()
    {
        if (currentSaveData == null)
            return;

        Debug.Log("Load Button");
        Load(currentSaveData);
    }

    public void ResetButton()
    {
        Debug.Log("Reset Button");

        elapsed = 0;
        lastAppliedUtc = DateTime.UtcNow;
        currentState = OfflineTradeState.Prepare;
    }

    public OfflineSaveData SaveTrade()
    {
        OfflineSaveData savedata = new OfflineSaveData();

        savedata.elapsed = elapsed;
        savedata.lastAppliedUtcTicks = lastAppliedUtc.Ticks;
        savedata.currentState = (int)currentState;

        Debug.Log($"Save Result \n" +
            $" Elapsed = {savedata.elapsed}\n" +
            $"LastAppliedUtcTicks = {savedata.lastAppliedUtcTicks.ToString()} \n" +
            $"CurrentState = {savedata.currentState.ToString()}");

        currentSaveData = savedata;

        return savedata;
    }

    public void Load(OfflineSaveData saveData)
    {
        elapsed = saveData.elapsed;
        DateTime loadedLastAppliedUtc = new DateTime(saveData.lastAppliedUtcTicks, DateTimeKind.Utc);
        lastAppliedUtc = loadedLastAppliedUtc;
        currentState = (OfflineTradeState)saveData.currentState;


        Debug.Log($"Load Result \n" +
            $" Elapsed = {elapsed}\n" +
            $"LastAppliedUtcTicks = {lastAppliedUtc.ToString()} \n" +
            $"CurrentState = {currentState.ToString()}");
    }
}
