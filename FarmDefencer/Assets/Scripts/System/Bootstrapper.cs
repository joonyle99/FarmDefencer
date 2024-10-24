using UnityEngine;

/// <summary>
/// Manager vs Supervisor ������ ������
/// </summary>
/// <remarks>
/// 
/// Manager�� �Ϲ������� �� ū ���̳� �μ��� �����ϴ� �ݸ�,
/// Supervisor�� �Ϲ������� �� ���� ���� �׷��� �����մϴ�.
/// 
/// Manager:
/// - Bootstrap�� ���� ���� �ε�Ǳ� ���� ����
/// - ���� ��ü�� ���� ������ ��ġ�� �۷ι� �ý����� �ǹ�
/// - �Ϲ������� �� ��ȯ �ÿ��� �����Ǹ�, ���� ��ü �����ֱ� ���� ����
/// 
/// - ��: InputManager, SoundManager
/// 
/// Supervisor:
/// - �ش� ���̳� ���ؽ�Ʈ �������� �ǹ̸� ����
/// - Ư�� ���̳� ���ѵ� ���� ������ �����ϴ� ���� �ý����� �ǹ�
/// 
/// - ��: PathSupervisor, BuildSupervisor
/// 
/// </remarks>
public static class ManagerVsSupervisor { }

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
