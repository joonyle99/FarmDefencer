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
/// Manager�� �Ϲ������� ū ���̳� �μ��� �����ϴ� �ݸ�,
/// Supervisor�� �Ϲ������� �� ���� ���� �׷��� �����մϴ�.
/// 
/// Manager:
/// - DontDestroy ��ü�� Bootstrap�� �ڽ����� �����ϸ�, ���� �ε�Ǳ� ���� ����
/// - �� ��ȯ �ÿ��� �����Ǹ�, ���� ��ü �����ֱ� ���� ����
/// - ���� ��ü�� ���� ������ ��ġ�� �۷ι� �ý����� �ǹ�
/// 
/// - ��: InputManager, SoundManager
/// 
/// Supervisor:
/// - �ش� ���̳� ���ؽ�Ʈ �������� �ǹ̸� ����
/// - Ư�� ���̳� ���ѵ� ���� ������ �����ϴ� ���� �ý����� �ǹ�
/// 
/// - ��: PathSupervisor, TowerBuildSystem, DialogSupervisor, QuestSupervisor
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
/// 3. 
/// 
/// 
/// TODO:
/// 1. SceneContext�� ��� Ÿ�ֿ̹� ������ ���ΰ�..? (���ʿ� �����صθ� �ȵǴ� �ɱ�?)
/// 2. ��ũ��Ʈ ���� ������ ���� ������ ���踦 �� ����ؾ� �Ѵ�
///     - �� ��ũ��Ʈ������ Awake, Start, OnEnable, OnDisable
///     - Ư�� Action�� ���� �̺�Ʈ ó���� �޸� �Ҵ� �� ���� �� �����ؾ� �Ѵ�
/// 
/// </remarks>
public class Guideline_ScriptExecutionOrder { }

/// <summary>
/// RuntimeInitializer�� ���� ���� �ε�Ǳ� ���� �����Ǹ�,
/// ���� ���� ���� �ʿ��� �Ŵ����� �ʱ�ȭ�մϴ�
/// </summary>
public class Bootstrapper : JoonyleGameDevKit.PersistentSingleton<Bootstrapper>
{
    public void InitializeGame()
    {
        Debug.Log("Initialize Game");
    }
}
