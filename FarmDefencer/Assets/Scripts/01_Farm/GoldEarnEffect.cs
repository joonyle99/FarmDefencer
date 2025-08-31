using Spine.Unity;
using TMPro;
using UnityEngine;
using System.Collections;
using System;

public sealed class GoldEarnEffect : MonoBehaviour
{
    [SerializeField] private float textPopupScale = 1.0f;
    [SerializeField] private float coinAnimationTime = 0.5f;
    [SerializeField] private float disappearSeconds = 0.5f;
    [SerializeField] [SpineAnimation] private string goldEarnAnimationName;
    
    public int GoldDisplaying { get; set; }
    private TMP_Text _text;
    private SkeletonAnimation _skeletonAnimation;
    private Action<GoldEarnEffect> _callback;
    
    public void Init(Action<GoldEarnEffect> callback) => _callback = callback;

    public void PlayEffect(int gold)
    {
        if (gameObject.activeSelf)
        {
            return;
        }
        gameObject.SetActive(true);

        GoldDisplaying = gold;
        _skeletonAnimation.AnimationState.SetAnimation(0, goldEarnAnimationName, false);
        StartCoroutine(CoPlayEffect());
    }
    
    private void Awake()
    {
        _text = transform.Find("Text").GetComponent<TMP_Text>();
        transform.Find("Text").GetComponent<MeshRenderer>().sortingLayerName = "VFX";
        _skeletonAnimation = transform.Find("Spine").GetComponent<SkeletonAnimation>();
    }

    private IEnumerator CoPlayEffect()
    {
        yield return null;
        _text.alpha = 1.0f;
        var animationDuration = _skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration;
        var deltaTime = 0.0f;
        while (deltaTime < coinAnimationTime)
        {
            _text.SetText($"+{GoldDisplaying}");
            deltaTime += Time.deltaTime;
            var textNextLocalPosition = _text.transform.localPosition;

            textNextLocalPosition.y = -textPopupScale * deltaTime * (deltaTime - animationDuration) * 4.0f / (animationDuration * animationDuration);

            _text.transform.localPosition = textNextLocalPosition;
            yield return null;
        }

        deltaTime = 0.0f;
        while (deltaTime < disappearSeconds)
        {
            deltaTime += Time.deltaTime;
            _text.alpha = 1.0f - deltaTime / disappearSeconds;
            yield return null;
        }
        
        gameObject.SetActive(false);
        _callback(this);
    }
}
