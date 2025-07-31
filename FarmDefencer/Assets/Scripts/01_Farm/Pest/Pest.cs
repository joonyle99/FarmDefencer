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
    Arrived,
    Fleeing,
}

public sealed class Pest : MonoBehaviour
{
    [SpineAnimation] [SerializeField] private string idleAnimation;
    [SpineAnimation] [SerializeField] private string runningAnimation;
    [SpineAnimation] [SerializeField] private string dyingAnimation;
    [SpineAnimation] [SerializeField] private string idleLiftingAnimation;
    [SpineAnimation] [SerializeField] private string runningLiftingAnimation;

    public PestSize PestSize { get; private set; }
    public int Seed { get; private set; }
    public string TargetProduct { get; private set; }
    public Vector2 Destination { get; private set; }
    public float MoveSpeed { get; private set; }

    private PestState _state;

    public PestState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            OnStateChanged();
        }
    }

    private int _remainingCropEatCount;
    public int RemainingCropEatCount => _remainingCropEatCount;

    // 저장되지 않는 값들
    private int _originalCropEatCount;
    private int _remainingClickCount;
    private float _beginDirectToDestinationCriterion;
    private float _dieTime;
    private float _fleeTime;
    private float _wavelength;
    private float _amplitude;
    private SkeletonAnimation _skeletonAnimation;
    private MeshRenderer _meshRenderer;
    private Action<string, Vector2, Vector2> _playPestStealFromFieldAnimation;

    // 저장과는 무관한 초기 설정값들을 설정함.
    // 참고: PestSize는 예외적으로 저장됨. 객체 생성과 밀접하게 관련되어 있으므로.
    public void Init(
        PestSize pestSize,
        float dieTime,
        float fleeTime,
        float beginDirectToDestinationCriterion,
        float wavelength,
        float amplitude,
        Action<string, Vector2, Vector2> playPestStealFromFieldAnimation)
    {
        PestSize = pestSize;
        _originalCropEatCount = pestSize switch
        {
            PestSize.Large => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _remainingCropEatCount = _originalCropEatCount;
        
        _remainingClickCount = pestSize switch
        {
            PestSize.Large => 3,
            PestSize.Medium => 2,
            _ => 1
        };
        _dieTime = dieTime;
        _beginDirectToDestinationCriterion = beginDirectToDestinationCriterion;
        _wavelength = wavelength;
        _amplitude = amplitude;
        _fleeTime = fleeTime;
        _playPestStealFromFieldAnimation = playPestStealFromFieldAnimation;
    }

    // 저장되는, 해충의 고유 속성을 정의하는 값들을 설정함.
    public void SetParameters(int seed, string targetProduct, Vector2 destination, float moveSpeed)
    {
        Seed = seed;
        TargetProduct = targetProduct;
        Destination = destination;
        MoveSpeed = moveSpeed;

        OnStateChanged();
        RefreshCropLiftingGraphic();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cropWorldPosition"></param>
    /// <param name="amount"></param>
    /// <returns>먹어서 할당량 채운 후 남은 개수.</returns>
    public int Eat(Vector2 cropWorldPosition, int amount)
    {
        var edible = Math.Min(_remainingCropEatCount, amount);
        var remainder = amount - edible;

        _remainingCropEatCount -= edible;
        StartCoroutine(CoPlayStealAnimation(cropWorldPosition, edible));
        RefreshCropLiftingGraphic();
        
        if (_remainingCropEatCount <= 0)
        {
            StartCoroutine(CoFlee());
        }

        return remainder;
    }

    public void Hit()
    {
        if (State != PestState.Running)
        {
            return;
        }

        EffectPlayer.SceneGlobalInstance.PlayTapEffect(transform.position);
        _remainingClickCount -= 1;
        if (_remainingClickCount <= 0)
        {
            SoundManager.Instance.PlaySfx("SFX_T_pest_catch");
            StartCoroutine(CoDie());
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
            GetDesiredZigZagHeight(Seed, transform.position.x, _wavelength, _amplitude),
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

        OnStateChanged();

        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        // PestSize는 객체 생성 단계에서부터 알아야 하며, Init()에서 설정됨.
        State = (PestState)(json["State"]?.Value<int?>() ?? (int)PestState.Initialized);
        Seed = json["Seed"]?.Value<int?>() ?? 0;
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
        
        RefreshCropLiftingGraphic();
    }

    private void OnStateChanged()
    {
        var animationName = State switch
        {
            PestState.Running => runningAnimation,
            PestState.Dying => dyingAnimation,
            PestState.Arrived => idleLiftingAnimation,
            PestState.Fleeing => runningLiftingAnimation,
            _ => idleAnimation
        };

        var shouldFlip = State switch
        {
            PestState.Running or PestState.Dying => Destination.x < transform.position.x,
            _ => true
        };

        var sortingOrder = State switch
        {
            PestState.Arrived => Destination.y > 0.0f ? 0 : 50,
            _ => 20
        };

        _meshRenderer.sortingOrder = sortingOrder;
        _skeletonAnimation.skeleton.ScaleX = shouldFlip ? -1.0f : 1.0f;
        _skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
    }

    private void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private IEnumerator CoPlayStealAnimation(Vector2 cropWorldPosition, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            _playPestStealFromFieldAnimation(TargetProduct, cropWorldPosition, transform.position);
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private IEnumerator CoFlee()
    {
        State = PestState.Fleeing;
        
        var elapsedTime = 0.0f;
        while (elapsedTime < _fleeTime)
        {
            var x = Mathf.Clamp01(elapsedTime / _fleeTime);
            _skeletonAnimation.skeleton.A = Mathf.Pow(1.0f - x, 5);
            
            var position = transform.position;
            var yDelta = Time.deltaTime * (Destination.y > 0.0f ? 1.0f : -1.0f);
            position.y += yDelta * 10.0f;
            position.x = Destination.x + Mathf.Sin(elapsedTime * 10.0f);
            transform.position = position;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CoDie()
    {
        State = PestState.Dying;
        
        var elapsedTime = 0.0f;
        while (elapsedTime <= _dieTime)
        {
            var alpha = Mathf.Clamp01(1.0f - elapsedTime / _dieTime);
            _skeletonAnimation.skeleton.A = alpha;
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        Destroy(gameObject);
    }

    private void RefreshCropLiftingGraphic()
    {
        // 개미 스킨 적용
        if (PestSize == PestSize.Small)
        {
            if (_remainingCropEatCount == 0)
            {
                _skeletonAnimation.skeleton.SetSkin(TargetProduct.Split("_")[1]);
            }
            else
            {
                _skeletonAnimation.skeleton.SetSkin("default");
            }
        }
        else
        {
            for (int slotId = 1; slotId <= _originalCropEatCount; ++slotId)
            {
                var slotName = $"crop_{slotId}";
                _skeletonAnimation.skeleton.SetAttachment(slotName, null);
            }
            for (int slotId = 1; slotId <= _originalCropEatCount - _remainingCropEatCount; ++slotId)
            {
                var slotName = $"crop_{slotId}";
                _skeletonAnimation.skeleton.SetAttachment(slotName, $"eating/{TargetProduct.Split("_")[1]}_1");
            }
        }
    }

    private static float GetDesiredZigZagHeight(int seed, float x, float wavelength, float amplitude) =>
        amplitude * Mathf.Sin(x / wavelength + seed);
}