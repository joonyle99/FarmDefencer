using Spine.Unity;
using TMPro;
using UnityEngine;
using System.Collections;
using System;

public sealed class GoldEarnEffect : MonoBehaviour
{
    [SerializeField] private float textPopupScale = 1.0f;
    [SerializeField] private float textDisappearSeconds = 0.5f;
    [SerializeField] [SpineAnimation] private string goldEarnAnimationName;
    
    private TMP_Text _text;
    private SkeletonAnimation _skeletonAnimation;
    private bool _isPlayingEffect;

    public void PlayEffect(int gold,  Action<GoldEarnEffect> callback)
    {
        if (_isPlayingEffect)
        {
            return;
        }
        _isPlayingEffect = true;
        
        gameObject.SetActive(true);
        _text.SetText($"+{gold}");
        _skeletonAnimation.AnimationState.SetAnimation(0, goldEarnAnimationName, false);
        StartCoroutine(CoPlayEffect(callback));
    }
    
    private void Awake()
    {
        gameObject.SetActive(false);
        _text = transform.Find("Text").GetComponent<TMP_Text>();
        transform.Find("Text").GetComponent<MeshRenderer>().sortingLayerName = "UI";
        _skeletonAnimation = transform.Find("Spine").GetComponent<SkeletonAnimation>();
    }

    private IEnumerator CoPlayEffect(Action<GoldEarnEffect> callback)
    {
        _text.alpha = 1.0f;
        var animationDuration = _skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration;
        var deltaTime = 0.0f;
        while (!_skeletonAnimation.AnimationState.GetCurrent(0).IsComplete)
        {
            deltaTime += Time.deltaTime;
            var textNextLocalPosition = _text.transform.localPosition;

            textNextLocalPosition.y = -textPopupScale * deltaTime * (deltaTime - animationDuration) * 4.0f / (animationDuration * animationDuration);

            _text.transform.localPosition = textNextLocalPosition;
            yield return null;
        }

        deltaTime = 0.0f;
        while (deltaTime < textDisappearSeconds)
        {
            deltaTime += Time.deltaTime;
            _text.alpha = 1.0f - deltaTime / textDisappearSeconds;
            yield return null;
        }
        
        gameObject.SetActive(false);
        callback(this);
        _isPlayingEffect = false;
    }
}
