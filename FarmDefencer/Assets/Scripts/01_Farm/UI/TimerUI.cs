using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class TimerUI : MonoBehaviour
{
    [SerializeField] private StageTimerSprites stageTimerSprites;

    private Func<float> _getRemainingDaytimeAlpha; // 0이면 낮 시작, 1이면 낮 끝
    private Image _dayArea;
    private Image _clockHand;
    private GameObject _rootObject;
    private GameObject _stageImagesRootObject;
    private GameObject _activeMapImageRootObject;
    private GameObject _inactiveMapImagesRootObject;
    
    public void Init(int currentMap, int currentStage, Func<float> getRemainingDaytimeAlpha)
    {
        _getRemainingDaytimeAlpha = getRemainingDaytimeAlpha;
        
        // 맵 스위치
        var radius = ((RectTransform)_rootObject.transform).sizeDelta.x;
        for (int mapToDraw = 1; mapToDraw <= 3; ++mapToDraw)
        {
            var imageObject = new GameObject($"Map{mapToDraw}");
            imageObject.transform.parent = mapToDraw == currentMap ? _activeMapImageRootObject.transform : _inactiveMapImagesRootObject.transform;
            imageObject.transform.localScale = Vector3.one;
            imageObject.transform.localPosition = Vector3.zero;
            
            var imageComponent = imageObject.AddComponent<Image>();
            var rectTransform = (RectTransform)imageObject.transform;
            
            var sprite = stageTimerSprites.GetMapSprite(mapToDraw, mapToDraw == currentMap);
            imageComponent.sprite = sprite;
            rectTransform.sizeDelta = sprite.rect.size;
            
            var degrees = 360.0f / 12 * (-mapToDraw + 2.0f) + 90.0f;
            var adjustedMapImageRadius = radius * 1.75f;
            if (mapToDraw == 2)
            {
                adjustedMapImageRadius *= 1.25f;
            }
            
            var radians = degrees / 180.0f * Mathf.PI;
            var localX = adjustedMapImageRadius * Mathf.Cos(radians);
            var localY = adjustedMapImageRadius * Mathf.Sin(radians);
            rectTransform.anchoredPosition = new Vector2(localX, localY);
        }
        
        // 스테이지 스위치
        for (int stageToDraw = 1; stageToDraw <= 10; ++stageToDraw)
        {
            var imageObject = new GameObject($"Stage{stageToDraw}");
            imageObject.transform.parent = _stageImagesRootObject.transform;
            imageObject.transform.localScale = Vector3.one;
            imageObject.transform.localPosition = Vector3.zero;
            
            var imageComponent = imageObject.AddComponent<Image>();
            var rectTransform = (RectTransform)imageObject.transform;
            
            var sprite = stageTimerSprites.GetStageSprite(stageToDraw, stageToDraw <= currentStage);
            imageComponent.sprite = sprite;
            rectTransform.sizeDelta = sprite.rect.size;
            
            var degrees = 360.0f / 12 * (-stageToDraw - 0.5f) + 90.0f;
            var adjustedStageImageRadius = radius * 1.20f;
            
            // 이상하게 오차가 생겨서 보정
            if (stageToDraw % 3 == 0)
            {
                degrees -= 1.60f;
            }
            else if (stageToDraw % 3 == 2)
            {
                degrees += 1.60f;
            }
            else
            {
                adjustedStageImageRadius *= 1.025f;
            }
            
            var radians = degrees / 180.0f * Mathf.PI;
            var localX = adjustedStageImageRadius * Mathf.Cos(radians);
            var localY = adjustedStageImageRadius * Mathf.Sin(radians);
            rectTransform.anchoredPosition = new Vector2(localX, localY);
        }
    }

    private void Awake()
    {
        _dayArea = transform.Find("Root/DayArea").GetComponent<Image>();
        _clockHand = transform.Find("Root/ClockHand").GetComponent<Image>();
        _rootObject = transform.Find("Root").gameObject;
        _stageImagesRootObject = transform.Find("Root/StageImages").gameObject;
        _activeMapImageRootObject = transform.Find("Root/ActiveMapImage").gameObject;
        _inactiveMapImagesRootObject = transform.Find("Root/InactiveMapImages").gameObject;
    }
    
    private void Update()
    {
        var remainingDaytimeAlpha = _getRemainingDaytimeAlpha();
        _dayArea.fillAmount = remainingDaytimeAlpha;

        var clockHandEulerAngles = _clockHand.transform.eulerAngles;
        clockHandEulerAngles.z = remainingDaytimeAlpha * 360.0f;
        _clockHand.transform.eulerAngles = clockHandEulerAngles;
    }
}
