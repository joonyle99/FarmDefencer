using System;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class TutorialHand : MonoBehaviour
{
    private static readonly int Play = Animator.StringToHash("Play");
    private static readonly int Looping = Animator.StringToHash("Looping");
    private static readonly int Enter = Animator.StringToHash("Enter");
    private static readonly Vector2 HandZeroOffset = new Vector2(-0.5f, -0.8f);
    private static readonly Vector2 HandBeginOffset = new Vector2(-2.0f, -2.3f);
    
    private float _currentActionElapsed;
    
    private Animator _effectAnimator;
    private SpriteRenderer _effectSpriteRenderer;
    private SpriteRenderer _hand;
    private SkeletonAnimation _dummyWateringCan;
    private TMP_Text _text;

    private RequiredCropAction _currentAction;
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
        _text.text = "";
        _effectAnimator.transform.localPosition = Vector3.zero;
        
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
            case RequiredCropAction.Water:
            {
                Action_Water();
                break;
            }            
        }
    }

    private void OnEnable()
    {
        _currentActionElapsed = 0.0f;
        _effectAnimator.Rebind();
        _effectSpriteRenderer.sprite = null;
    }

    private void OnDisable()
    {
        _currentActionElapsed = 0.0f;
        _effectAnimator.Rebind();
        _effectSpriteRenderer.sprite = null;
    }

    private void Awake()
    {
        _text = transform.Find("Text").GetComponent<TMP_Text>();
        _effectAnimator = transform.Find("InteractEffectPlayer").GetComponent<Animator>();
        _effectSpriteRenderer = transform.Find("InteractEffectPlayer").GetComponent<SpriteRenderer>();
        
        _hand = transform.Find("Hand").GetComponent<SpriteRenderer>();
        _dummyWateringCan = transform.Find("DummyWateringCan").GetComponent<SkeletonAnimation>();
    }
    
    private void OnActionChanged()
    {
        _hand.transform.localPosition = HandZeroOffset;
        gameObject.SetActive(_currentAction != RequiredCropAction.None);
        _hand.gameObject.SetActive(_currentAction != RequiredCropAction.Water && _currentAction != RequiredCropAction.None);
        _dummyWateringCan.gameObject.SetActive(_currentAction == RequiredCropAction.Water);
        _currentActionElapsed = 0.0f;
    }

    private void Action_SingleTap()
    {
        const float duration = 2.0f;
        const float handMoveTime = 0.5f;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame <= handMoveTime)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset, HandZeroOffset, currentFrame % handMoveTime / handMoveTime);
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

        _text.text = "2sec";
        
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
        
        _text.text = "2Tab";
        
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
        const float duration = 3.0f;
        const float firstDuration = 0.5f;
        const float repeatDuration = 0.2f;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame < firstDuration)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset, HandZeroOffset, currentFrame % 0.5f / 0.5f);
            return;
        }

        if (currentFrame - Time.deltaTime < firstDuration)
        {
            _hand.transform.localPosition = HandBeginOffset;
            PlayTabEffect();

        }
        
        _text.text = "5Tab";
        
        if (currentFrame < firstDuration + repeatDuration * 6)
        {
            var repeatLocalFrame = currentFrame % repeatDuration;
            
            if (repeatLocalFrame < 0.1f)
            {
                _hand.transform.localPosition = Vector2.Lerp(HandZeroOffset, HandBeginOffset * 0.5f, repeatLocalFrame % 0.1f / 0.1f);
            }
            else
            {
                _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset * 0.5f, HandZeroOffset, repeatLocalFrame % 0.1f / 0.1f);
                if (repeatLocalFrame + Time.deltaTime > repeatDuration)
                {
                    PlayTabEffect();
                }
            }
            return;
        }
    }

    private void Action_Drag()
    {
        const float duration = 3.0f;
        const float repeatDuration = duration - 1.0f;

        var rangeOffset = new Vector2(0.5f, 0.0f);
        var leftPosition = HandZeroOffset - rangeOffset;
        var rightPosition = HandZeroOffset + rangeOffset;

        var currentFrame = _currentActionElapsed % duration;

        if (currentFrame < 0.5f)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset, HandZeroOffset, (currentFrame % 0.5f) / 0.5f);
            return;
        }
        
        if (currentFrame < duration - 0.5f)
        {
            var rad = (currentFrame - 0.5f) / repeatDuration * (2.0f + 1.5f) * Mathf.PI;
            _hand.transform.localPosition = Vector2.Lerp(leftPosition, rightPosition, Mathf.Sin(rad) * -0.5f + 0.5f);
            PlayHoldEffect();
            _effectAnimator.transform.position = _hand.transform.position - new Vector3(HandZeroOffset.x, HandZeroOffset.y);
            return;
        }
        
        _hand.transform.localPosition = Vector2.Lerp(rightPosition, HandBeginOffset, (currentFrame % 0.5f) / 0.5f);
    }   
    
    private void Action_Water()
    {

    }    
    
    private void PlayTabEffect()
    {
        _effectAnimator.Play(Enter, 0, 0.0f);
    }
    
    public void PlayHoldEffect()
    {
        if (!_isHolding)
        {
            _effectAnimator.SetTrigger(Play);
            _effectAnimator.SetBool(Looping, true);
        }
        _isHolding = true;
    }
    
    private void StopHoldEffect()
    {
        _effectAnimator.ResetTrigger(Play);
        _effectAnimator.SetBool(Looping, false);
    }
}
