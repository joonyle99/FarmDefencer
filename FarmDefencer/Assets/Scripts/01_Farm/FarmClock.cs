using UnityEngine;

/// <summary>
/// FarmClock�� ���������� ������Ʈ�� ��ü�� �ǹ��մϴ�.
/// </summary>
public interface IFarmUpdatable
{
	/// <summary>
	/// �׻� �� ������ ȣ�������, �Ͻ������ǰų� �� �ð��� �� �Ǿ��ٸ� deltaTime�� 0.0f�Դϴ�.
	/// </summary>
	/// <param name="deltaTime"></param>
	void OnFarmUpdate(float deltaTime);
}

/// <summary>
/// ���� �� �ð��� ����ϸ� IFarmUpdatable ��ü�� ������Ʈ�� ����մϴ�.
/// </summary>
public class FarmClock : MonoBehaviour
{
	private float _remainingDaytime;
	public float RemainingDaytime
	{
		get => _remainingDaytime;
		private set
		{
			_remainingDaytime = value;
			if (_remainingDaytime < 0.0f)
			{
				_remainingDaytime = 0.0f;
			}
		}
	}
	/// <summary>
	/// ������ �Ͻ����� ��û�� �ǹ��ϴ� �����Դϴ�.
	/// �� ���� true�� RemainingDaytime > 0.0f���� ����ϴ�. 
	/// �̹� RemainingDaytime�� 0.0f��� �ƹ� ȿ���� �����ϴ�.
	/// </summary>
	public bool IsManuallyPaused;

	public IFarmUpdatable _farmUpdatable;

	public void SetRemainingDaytimeBy(float daytime) => _remainingDaytime = daytime;
	public void SetRemainingDaytimeRandom(float min, float max) => _remainingDaytime = Random.Range(min, max);
	public void RegisterFarmUpdatableObject(IFarmUpdatable farmUpdatable) => _farmUpdatable = farmUpdatable;
	private void Update()
	{
		var deltaTime = IsManuallyPaused || RemainingDaytime == 0.0f ? 0.0f : Time.deltaTime;
		RemainingDaytime -= deltaTime;
		_farmUpdatable?.OnFarmUpdate(deltaTime);
    }
}