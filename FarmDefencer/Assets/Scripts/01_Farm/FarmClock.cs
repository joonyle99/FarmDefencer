using UnityEngine;

/// <summary>
/// FarmClock에 종속적으로 업데이트될 객체를 의미합니다.
/// </summary>
public interface IFarmUpdatable
{
	/// <summary>
	/// 항상 매 프레임 호출되지만, 일시정지되거나 낮 시간이 다 되었다면 deltaTime이 0.0f입니다.
	/// </summary>
	/// <param name="deltaTime"></param>
	void OnFarmUpdate(float deltaTime);
}

/// <summary>
/// 남은 낮 시간을 담당하며 IFarmUpdatable 객체의 업데이트를 담당합니다.
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
	/// 낮 시간이 종료되어 시간이 멈췄거나, 아직 낮 시간이 남았어도 IsManuallyPaused가 true로 설정되어 일시정지 상태가 되었음을 의미합니다.
	/// </summary>
	public bool IsPaused { get; private set; }

	/// <summary>
	/// 이번에 설정된 낮의 총 길이입니다.
	/// </summary>
	public float LengthOfDaytime { get; private set; }

	/// <summary>
	/// 유저의 일시정지 요청 여부 의미하는 변수입니다.
	/// 이 값이 true면 RemainingDaytime > 0.0f여도 멈춥니다. 
	/// 이미 RemainingDaytime이 0.0f라면 아무 효과도 없습니다.
	/// </summary>
	public bool IsManuallyPaused;

	public IFarmUpdatable _farmUpdatable;

	public void SetRemainingDaytimeBy(float daytime)
	{
		LengthOfDaytime = daytime;
		_remainingDaytime = daytime;
	}
	public void SetRemainingDaytimeRandom(float min, float max) => SetRemainingDaytimeBy(Random.Range(min, max));
	public void RegisterFarmUpdatableObject(IFarmUpdatable farmUpdatable) => _farmUpdatable = farmUpdatable;
	private void Update()
	{
		IsPaused = IsManuallyPaused || RemainingDaytime == 0.0f;
		var deltaTime = IsPaused ? 0.0f : Time.deltaTime;
		RemainingDaytime -= deltaTime;
		_farmUpdatable?.OnFarmUpdate(deltaTime);
    }
}