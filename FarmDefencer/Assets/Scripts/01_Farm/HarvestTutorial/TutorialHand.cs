using Spine.Unity;
using TMPro;
using UnityEngine;

public class TutorialHand : MonoBehaviour
{
    private static readonly Vector2 HandZeroOffset = new(-0.5f, -0.8f);
    private static readonly Vector2 HandBeginOffset = new(-2.0f, -2.3f);
    private static readonly Vector2 DummyWateringCanOffset = new(-1.5f, 1.5f);
    
    private EffectPlayer _effectPlayer;
    private SpriteRenderer _hand;
    private SkeletonAnimation _dummyWateringCan;
    private TMP_Text _text;
    
    private float _currentActionElapsed;
    private float _remainingHideSeconds;
    private RequiredCropAction _requiredCropAction;

    public void SetTutorialHandMotion(RequiredCropAction requiredCropAction)
    {
        if (requiredCropAction == _requiredCropAction)
        {
            return;
        }
        _requiredCropAction = requiredCropAction;
        
        _hand.transform.localPosition = HandZeroOffset;
        _currentActionElapsed = 0.0f;
    }

    public void HideForSeconds(float seconds) => _remainingHideSeconds = seconds;
    
    private void LateUpdate()
    {
        _remainingHideSeconds -= Time.deltaTime;
        if (_remainingHideSeconds <= 0.0f)
        {
            _remainingHideSeconds = 0.0f;
        }

        var notInHideSeconds = _remainingHideSeconds <= 0.0f;
        _effectPlayer.gameObject.SetActive(notInHideSeconds && _requiredCropAction != RequiredCropAction.None);
        _hand.gameObject.SetActive(notInHideSeconds && _requiredCropAction != RequiredCropAction.Water && _requiredCropAction != RequiredCropAction.None);
        _dummyWateringCan.gameObject.SetActive(notInHideSeconds && _requiredCropAction == RequiredCropAction.Water);
        _text.gameObject.SetActive(notInHideSeconds);
        
        _currentActionElapsed += Time.deltaTime;
        _text.text = "";
        _effectPlayer.transform.localPosition = Vector3.zero;
        
        switch (_requiredCropAction)
        {
            case RequiredCropAction.SingleTap:
            {
                Action_SingleTap();
                break;
            }
            case RequiredCropAction.Hold_0_75:
            {
                Action_Hold(0.75f);
                break;
            }
            case RequiredCropAction.Hold_1:
            {
                Action_Hold(1.0f);
                break;
            }
            case RequiredCropAction.Hold_2:
            {
                Action_Hold(2.0f);
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
                break;
            }            
        }
    }

    private void Awake()
    {
        _effectPlayer = transform.Find("EffectPlayer").GetComponent<EffectPlayer>();
        _text = transform.Find("Text").GetComponent<TMP_Text>();
        _hand = transform.Find("Hand").GetComponent<SpriteRenderer>();
        _dummyWateringCan = transform.Find("DummyWateringCan").GetComponent<SkeletonAnimation>();
        _dummyWateringCan.transform.localPosition = DummyWateringCanOffset;
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
            _effectPlayer.PlayTapEffect(transform.position);
        }
    }

    private void Action_Hold(float holdTime)
    {
        const float handMoveTime = 0.5f;
        
        var duration = handMoveTime + holdTime;

        var currentFrame = _currentActionElapsed % duration;
        if (currentFrame <= handMoveTime)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset + HandZeroOffset, HandZeroOffset, currentFrame % handMoveTime / handMoveTime);
            return;
        }

        // 구간에 따라 숫자 표기 깔끔하게 하기 위해 수동 분기
        if (holdTime <= 0.75f)
        {
            _text.text = "0.75sec";
        }
        else if (holdTime <= 1.0f)
        {
            _text.text = "1sec";
        }
        else
        {
            _text.text = "2sec";
        }

        _effectPlayer.PlayHoldEffect(transform.position);
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
            _effectPlayer.PlayTapEffect(transform.position);
        }
        
        _text.text = "2Tap";
        
        if (currentFrame < 0.7f)
        {
            _hand.transform.localPosition = Vector2.Lerp(HandBeginOffset * 0.5f, HandZeroOffset, currentFrame % 0.2f / 0.2f);
            return;
        }
        
        if (currentFrame - Time.deltaTime < 0.7f)
        {
            _effectPlayer.PlayTapEffect(transform.position);
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
            _effectPlayer.PlayTapEffect(transform.position);
            
        }
        
        _text.text = "5Tap";
        
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
                    _effectPlayer.PlayTapEffect(transform.position);
                }
            }
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
            _effectPlayer.PlayHoldEffect(_hand.transform.position - new Vector3(HandZeroOffset.x, HandZeroOffset.y));
            return;
        }
        
        _hand.transform.localPosition = Vector2.Lerp(rightPosition, HandBeginOffset, (currentFrame % 0.5f) / 0.5f);
    }
}
