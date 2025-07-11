using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTabs.Libs;
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
    [SerializeField] private float lengthOfDaytime = 300.0f;
    public float LengthOfDaytime => lengthOfDaytime;
    
    public float Daytime { get; private set; }

    public float RemainingDaytime => lengthOfDaytime - Daytime;

    public bool Stopped { get; private set; }

    private List<Func<bool>> _pauseConditions;

    private List<IFarmUpdatable> _farmUpdatables;

    public void AddPauseCondition(Func<bool> condition)
    {
        _pauseConditions.Add(condition);
    }

    public void SetDaytime(float daytime) => Daytime = Mathf.Clamp(daytime, 0.0f, lengthOfDaytime);

    public void RegisterFarmUpdatableObject(IFarmUpdatable farmUpdatable) => _farmUpdatables.Add(farmUpdatable);

    private void Awake()
    {
        _farmUpdatables = new();
        _pauseConditions = new();
    }

    private void Update()
    {
        Stopped = _pauseConditions.Any(cond => cond()) || RemainingDaytime == 0.0f;
        var deltaTime = Stopped ? 0.0f : Time.deltaTime;
        Daytime += deltaTime;
        _farmUpdatables.ForEach(f => f.OnFarmUpdate(deltaTime));
    }
}