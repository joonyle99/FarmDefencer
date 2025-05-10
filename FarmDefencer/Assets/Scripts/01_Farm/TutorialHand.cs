using UnityEngine;

public class TutorialHand : MonoBehaviour
{
    private static readonly int Play = Animator.StringToHash("Play");
    private static readonly int Looping = Animator.StringToHash("Looping");
    private static readonly int Enter = Animator.StringToHash("Enter");
    private static readonly Vector2 HandZeroOffset = new Vector2(-0.5f, -0.5f);
    private static readonly Vector2 HandBeginOffset = new Vector2(-2.0f, -2.0f);
    
    private float _currentActionElapsed;

    private Animator _effectAnimator;
    private SpriteRenderer _hand;

    private RequiredCropAction _currentAction = RequiredCropAction.DoubleTap;
    public RequiredCropAction CurrentAction
    {
        get => _currentAction;
        set
        {
            if (_currentAction != value)
            {
                _currentAction = value;
                OnActionChanged();
            }
        }
    }

    private bool _isHolding;
    
    private void Update()
    {
        _currentActionElapsed += Time.deltaTime;
        
        if (!_isHolding)
        {
            StopHoldEffect();
        }
        _isHolding = false;
        
        switch (_currentAction)
        {
            case RequiredCropAction.SingleTap:
            {
                Action_SingleTap();
                break;
            }
            case RequiredCropAction.Hold:
            {
                Action_Hold();
                break;
            }
            case RequiredCropAction.DoubleTap:
            {
                Action_DoubleTap();
                break;
            }
            case RequiredCropAction.FiveTap:
            {
                Action_FiveTap();
                break;
            }
            case RequiredCropAction.Drag:
            {
                Action_Drag();
                break;
            }
        }
    }

    private void Awake()
    {
        _effectAnimator = transform.Find("InteractEffectPlayer").GetComponent<Animator>();
        _hand = transform.Find("Hand").GetComponent<SpriteRenderer>();
    }

    private void OnActionChanged()
    {
        _hand.transform.localPosition = HandZeroOffset;
        gameObject.SetActive(_currentAction != RequiredCropAction.None);
        _currentActionElapsed = 0.0f;
    }

    private void Action_SingleTap()
    {
        const float duration = 2.0f;
        const float handMoveTime = 0.5f;
        var handBeginOffset = new Vector2(-3.0f, -3.0f) + HandZeroOffset;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame <= handMoveTime)
        {
            _hand.transform.localPosition = Vector2.Lerp(handBeginOffset, HandZeroOffset, currentFrame % handMoveTime / handMoveTime);
            return;
        }

        if (currentFrame - Time.deltaTime < handMoveTime)
        {
            PlayTabEffect();
        }
    }

    private void Action_Hold()
    {
        const float duration = 2.5f;
        const float handMoveTime = 0.5f;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame <= handMoveTime)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset + HandZeroOffset, HandZeroOffset, currentFrame % handMoveTime / handMoveTime);
            return;
        }

        PlayHoldEffect();
    }

    private void Action_DoubleTap()
    {
        const float duration = 2.0f;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame < 0.5f)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset, HandZeroOffset, currentFrame % 0.5f / 0.5f);
            return;
        }

        if (currentFrame - Time.deltaTime < 0.5f)
        {
            _hand.transform.localPosition = HandBeginOffset;
            PlayTabEffect();
        }
        
        if (currentFrame < 0.7f)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset * 0.5f, HandZeroOffset, currentFrame % 0.2f / 0.2f);
            return;
        }
        
        if (currentFrame - Time.deltaTime < 0.7f)
        {
            PlayTabEffect();
        }
    }

    private void Action_FiveTap()
    {
        
    }

    private void Action_Drag()
    {
        
    }
    
    private void PlayTabEffect()
    {
        _effectAnimator.Play(Enter, 0, 0.0f);
    }
    
    public void PlayHoldEffect()
    {
        _effectAnimator.Play(Enter, 0, 0.0f);
        _effectAnimator.SetBool(Looping, true);
        _isHolding = true;
    }
    
    private void StopHoldEffect()
    {
        if (_effectAnimator.GetBool(Looping))
        {
            _effectAnimator.ResetTrigger(Play);
            _effectAnimator.SetBool(Looping, false);
        }
    }
}
