using UnityEngine;

/// <summary>
/// Manager vs Supervisor 패턴의 차이점
/// </summary>
/// <remarks>
/// 
/// Manager는 일반적으로 더 큰 팀이나 부서를 감독하는 반면,
/// Supervisor는 일반적으로 더 작은 직원 그룹을 관리합니다.
/// 
/// Manager:
/// - Bootstrap에 의해 씬이 로드되기 전에 생성
/// - 게임 전체에 걸쳐 영향을 미치는 글로벌 시스템을 의미
/// - 일반적으로 씬 전환 시에도 유지되며, 게임 전체 수명주기 동안 존재
/// 
/// - 예: InputManager, SoundManager
/// 
/// Supervisor:
/// - 해당 씬이나 컨텍스트 내에서만 의미를 가짐
/// - 특정 씬이나 제한된 범위 내에서 동작하는 관리 시스템을 의미
/// 
/// - 예: PathSupervisor, BuildSupervisor
/// 
/// </remarks>
public static class ManagerVsSupervisor { }

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
