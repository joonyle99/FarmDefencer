using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class HarvestAnimationObject : MonoBehaviour
{
    private Image _image;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    
    public void Play(ProductEntry productEntry, float duration, Vector2 screenFrom, Vector2 screenTo,
        Action callback)
    {
        if (gameObject.activeSelf)
        {
            return;
        }
        gameObject.SetActive(true);
        
        _image.sprite = productEntry.ProductSprite;
        var canvasRectTransform = (RectTransform)_canvas.transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenFrom, _canvas.worldCamera, out var localFrom);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenTo, _canvas.worldCamera, out var localTo);
        
        _rectTransform.anchoredPosition = localFrom;
        ((RectTransform)transform)
            .DOAnchorPos(localTo, duration)
            .SetEase(Ease.OutCirc)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                callback();
            });
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _rectTransform = (RectTransform)transform;
        _canvas = transform.parent.GetComponent<Canvas>();
    }
}