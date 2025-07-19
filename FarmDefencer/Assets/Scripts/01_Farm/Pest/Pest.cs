using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;

[Serializable]
public enum PestSize
{
    Small,
    Medium,
    Large
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
    Initialized,
    Dying,
    Running,
    Arrived
}

public sealed class Pest : MonoBehaviour
{
    public PestSize PestSize { get; private set; }
    public int Seed { get; private set; }
    public string TargetProduct { get; private set; }
    public Vector2 Destination { get; private set; }
    public float MoveSpeed { get; private set; }    
    public PestState State { get; private set; }
    
    private int _remainingCropEatCount;
    public int RemainingCropEatCount => _remainingCropEatCount;
    
    // 저장되지 않는 값들
    private float _remainingDieTime;
    private int _remainingClickCount;
    private float _beginDirectToDestinationCriterion;
    private float _originalDieTime;
    private SkeletonAnimation _skeletonAnimation;
    
    public void Init(PestSize pestSize, float dieTime, float beginDirectToDestinationCriterion)
    {
        PestSize = pestSize;
        _remainingCropEatCount = pestSize switch
        {
            PestSize.Large => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _remainingClickCount = pestSize switch
        {
            PestSize.Large => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _originalDieTime = dieTime;
        _beginDirectToDestinationCriterion = beginDirectToDestinationCriterion;
    }

    public void SetParameters(int seed, string targetProduct, Vector2 destination, float moveSpeed)
    {
        Seed = seed;
        TargetProduct = targetProduct;
        Destination = destination;
        MoveSpeed = moveSpeed;
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
            _remainingDieTime = _originalDieTime;
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

        var vectorToDestination =
            new Vector2(Destination.x - transform.position.x, Destination.y - transform.position.y);
        var remainingDistanceSquared = vectorToDestination.sqrMagnitude;
        var currentPosition = transform.position;

        if (remainingDistanceSquared < _beginDirectToDestinationCriterion * _beginDirectToDestinationCriterion)
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

        var origin = transform.position.x < Destination.x ? PestOrigin.Left : PestOrigin.Right;
        transform.position = new Vector3(
            transform.position.x + MoveSpeed * deltaTime * (origin == PestOrigin.Right ? -1.0f : 1.0f),
            GetDesiredZigZagHeight(Seed, transform.position.x),
            transform.position.z);
    }
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        jsonObject.Add("PestSize", (int)PestSize);
        jsonObject.Add("State", (int)State);
        jsonObject.Add("RemainingCropEatCount", _remainingCropEatCount);
        jsonObject.Add("Seed", Seed);
        jsonObject.Add("TargetProduct", TargetProduct);
        jsonObject.Add("X", transform.position.x);
        jsonObject.Add("DestinationX", Destination.x);
        jsonObject.Add("DestinationY", Destination.y);
        jsonObject.Add("MoveSpeed", MoveSpeed);
        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        // PestSize는 객체 생성 단계에서부터 알아야 하며, Init()에서 설정됨.
        State = (PestState)(json["State"]?.Value<int?>() ?? (int)PestState.Initialized);
        TargetProduct = json["TargetProduct"]?.Value<string>();
        MoveSpeed = json["MoveSpeed"]?.Value<float>() ?? 0.0f;
        var destinationX = json["DestinationX"]?.Value<float>() ?? 0.0f;
        var destinationY = json["DestinationY"]?.Value<float>() ?? 0.0f;
        Destination = new Vector2(destinationX, destinationY);
        
        _remainingCropEatCount = json["RemainingCropEatCount"]?.Value<int?>() ?? 0;
        var x = json["X"]?.Value<float?>() ?? 0.0f;
        var position = transform.position;
        position.x = x;
        transform.position = position;
    }

    private void Update()
    {
        if (State != PestState.Dying)
        {
            return;
        }
        
        _remainingDieTime -= Time.deltaTime;
        if (_remainingDieTime <= 0.0f)
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

    private static float GetDesiredZigZagHeight(int seed, float x) =>
        Mathf.Sin(x + seed);
}