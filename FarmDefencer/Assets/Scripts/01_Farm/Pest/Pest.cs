using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

[Serializable]
public enum PestSize
{
    Small,
    Medium,
    Big
}

[Serializable]
public enum PestOrigin
{
    Left,
    Right
}

[Serializable]
public enum PestState
{
    Uninitialized,
    Initialized,
    Dying,
    Running,
    Arrived
}

public sealed class Pest : MonoBehaviour
{
    public PestSize PestSize { get; private set; }
    public PestOrigin PestOrigin { get; private set; }
    public int Seed { get; private set; }
    public ProductEntry TargetProduct { get; private set; }
    public Vector2 Destination { get; private set; }
    public float MoveSpeed { get; private set; }
    public PestState State { get; private set; }
    private int _remainingCropEatCount;
    public int RemainingCropEatCount => _remainingCropEatCount;

    private float _pestBeginDirectToDestinationCriterion;
    private float _elapsedTime;
    private Action _onArrived;
    private float _originalDieTime;
    private float _remainingDieTime;
    private int _remainingClickCount;
    private SkeletonAnimation _skeletonAnimation;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pestSize"></param>
    /// <param name="pestOrigin"></param>
    /// <param name="seed"></param>
    /// <param name="targetProduct"></param>
    /// <param name="destination"></param>
    /// <param name="moveSpeed"></param>
    /// <param name="pestBeginDirectToDestinationCriterion">현재 위치와 도착 위치를 비교했을 때 이 값보다 작으면 더 이상 지그재그 횡보하지 않고 목적지 이동을 시작함.</param>
    /// <param name="dieTime"></param>
    public void Init(
        PestSize pestSize, 
        PestOrigin pestOrigin, 
        int seed, 
        ProductEntry targetProduct, 
        Vector2 destination,
        float moveSpeed,
        float pestBeginDirectToDestinationCriterion,
        float dieTime)
    {
        if (State != PestState.Uninitialized)
        {
            return;
        }

        PestSize = pestSize;
        PestOrigin = pestOrigin;
        Seed = seed;
        TargetProduct = targetProduct;
        Destination = destination;
        MoveSpeed = moveSpeed;

        _remainingCropEatCount = PestSize switch
        {
            PestSize.Big => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _remainingClickCount = PestSize switch
        {
            PestSize.Big => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _pestBeginDirectToDestinationCriterion = pestBeginDirectToDestinationCriterion;
        _originalDieTime = _remainingDieTime = dieTime;
        
        State = PestState.Initialized;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>먹어서 할당량 채운 후 남은 개수.</returns>
    public int Eat(int amount)
    {
        var edible = Math.Min(_remainingCropEatCount, amount);
        var remainder = amount - edible;

        _remainingCropEatCount -= edible;

        if (_remainingCropEatCount <= 0)
        {
            Destroy(gameObject);
        }

        return remainder;
    }

    public void Hit()
    {
        if (State != PestState.Running)
        {
            return;
        }
        
        EffectPlayer.PlayTabEffect(transform.position);
        _remainingClickCount -= 1;
        if (_remainingClickCount <= 0)
        {
            State = PestState.Dying;
        }
    }

    public void Run()
    {
        if (State != PestState.Initialized)
        {
            return;
        }

        State = PestState.Running;
    }

    public void ManualUpdate(float deltaTime)
    {
        if (State != PestState.Running)
        {
            return;
        }

        _elapsedTime += deltaTime;

        var vectorToDestination =
            new Vector2(Destination.x - transform.position.x, Destination.y - transform.position.y);
        var remainingDistanceSquared = vectorToDestination.sqrMagnitude;
        var currentPosition = transform.position;

        if (remainingDistanceSquared < _pestBeginDirectToDestinationCriterion * _pestBeginDirectToDestinationCriterion)
        {

            if (MoveSpeed * MoveSpeed * deltaTime * deltaTime > remainingDistanceSquared)
            {
                transform.position = new Vector3(Destination.x, Destination.y, transform.position.z);
                State = PestState.Arrived;
                return;
            }

            var stepForThisFrame = vectorToDestination.normalized * (MoveSpeed * deltaTime);
            var nextPosition = new Vector3(
                currentPosition.x + stepForThisFrame.x,
                currentPosition.y + stepForThisFrame.y, 
                currentPosition.z);
            transform.position = nextPosition;
            return;
        }

        transform.position = new Vector3(
            transform.position.x + MoveSpeed * deltaTime * (PestOrigin == PestOrigin.Right ? -1.0f : 1.0f),
            GetDesiredZigZagHeight(Seed, _elapsedTime, MoveSpeed),
            transform.position.z);
    }

    private void Update()
    {
        if (State != PestState.Dying)
        {
            return;
        }
        
        _remainingDieTime -= Time.deltaTime;
        if (_remainingDieTime < 0.0f)
        {
            Destroy(gameObject);
            return;
        }

        var alpha = _remainingDieTime / _originalDieTime;
        _skeletonAnimation.skeleton.A = alpha;
    }

    private void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private static float GetDesiredZigZagHeight(int seed, float elapsedTime, float moveSpeed) =>
        Mathf.Sin(elapsedTime * moveSpeed + seed);
}