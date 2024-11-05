using UnityEngine;

/// <summary>
/// Manager vs Supervisor
/// </summary>
/// <remarks>
/// 
/// Manager�� �Ϲ������� �� ū ���̳� �μ��� �����ϴ� �ݸ�,
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
/// - ��: PathSupervisor, BuildSupervisor, DialogSupervisor, QuestSupervisor
/// 
/// </remarks>
public class ManagerClassGuideline { }

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
