using UnityEngine;

/// <summary>
/// 
/// --------- [DEPRECATED] Manager vs Supervisor ---------
/// 
/// </summary>
/// 
/// <remarks>
/// 
/// --------- [DEPRECATED] This class is no longer in use. ---------
/// 
/// Manager는 일반적으로 큰 팀이나 부서를 감독하는 반면,
/// Supervisor는 일반적으로 더 작은 직원 그룹을 관리합니다.
/// 
/// Manager:
/// - DontDestroy 객체인 Bootstrap의 자식으로 존재하며, 씬이 로드되기 전에 생성
/// - 씬 전환 시에도 유지되며, 게임 전체 수명주기 동안 존재
/// - 게임 전체에 걸쳐 영향을 미치는 글로벌 시스템을 의미
/// 
/// - 예: InputManager, SoundManager
/// 
/// Supervisor:
/// - 해당 씬이나 컨텍스트 내에서만 의미를 가짐
/// - 특정 씬이나 제한된 범위 내에서 동작하는 관리 시스템을 의미
/// 
/// - 예: PathSupervisor, BuildSystem, DialogSupervisor, QuestSupervisor
/// 
/// --------- [DEPRECATED] Consider removing this class or replacing it with a more relevant system. ---------
/// 
/// </remarks>
public class Guideline_ManagerClass { }

/// <summary>
/// 
/// 
/// 
/// </summary>
/// 
/// <remarks>
/// 
/// 1. Bootstrapper
///     - InputManager
///     - SoundManager
///     - ResourceManager
///     
/// 2. SceneContext (Tycoon, Defence Context)
///     - Tycoon Context
///         -
///     - Defence Context
///         - 
///
/// 1. SceneContext를 어느 타이밍에 생성할 것인가..? (애초에 생성해두면 안되는 걸까?)
/// 2. 스크립트 실행 순서에 따른 의존성 관계를 잘 고려해야 한다
///     - 각 스크립트에서도 Awake, Start, OnEnable, OnDisable
///     - 특히 Action과 같은 이벤트 처리를 메모리 할당 및 해제 시 주의해야 한다
/// 
/// </remarks>
public class Guideline_ScriptExecutionOrder { }

/// <summary>
/// RuntimeInitializer에 의해 씬이 로드되기 전에 생성되며,
/// 게임 시작 전에 필요한 매니저를 초기화합니다
/// </summary>
public class Bootstrapper : JoonyleGameDevKit.PersistentSingleton<Bootstrapper>
{
    public void InitializeGame()
    {
        Debug.Log("Initialize Game");
    }
}
