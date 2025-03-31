using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// FarmClock에 의해 Update될 객체에 대한 인터페이스.
/// </summary>
public interface IFarmUpdatable
{
	/// <summary>
	/// FarmClock이 일시정지 상태일 경우에도 지속적으로 호출되며 이 경우는 deltaTime이 0.0f로 전달됨.
	/// </summary>
	/// <param name="deltaTime"></param>
	void OnFarmUpdate(float deltaTime);
}

/// <summary>
/// 일시정지될 수 있고 IFarmUpdatable 객체에 대해 OnFarmUpdate()를 호출하는 클래스.
/// </summary>
public sealed class FarmClock : MonoBehaviour
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
	
	public bool Stopped { get; private set; }
	
	public float LengthOfDaytime { get; private set; }

	private List<Func<bool>> _pauseConditions;

	// 일단은 하나만 가질 수 있도록 구현하였음
	private IFarmUpdatable _farmUpdatable;
	
	public void AddPauseCondition(Func<bool> condition)
	{
		_pauseConditions.Add(condition);
	}

	public void SetRemainingDaytimeBy(float daytime)
	{
		LengthOfDaytime = daytime;
		_remainingDaytime = daytime;
	}
	public void SetRemainingDaytimeRandom(float min, float max) => SetRemainingDaytimeBy(Random.Range(min, max));
	public void RegisterFarmUpdatableObject(IFarmUpdatable farmUpdatable) => _farmUpdatable = farmUpdatable;

	private void Awake()
	{
		_pauseConditions = new();
	}
	
	private void Update()
	{
		Stopped = _pauseConditions.Any(cond => cond()) || RemainingDaytime == 0.0f;
		var deltaTime = Stopped ? 0.0f : Time.deltaTime;
		RemainingDaytime -= deltaTime;
		_farmUpdatable?.OnFarmUpdate(deltaTime);
    }
}