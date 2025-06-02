using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class TimerUI : MonoBehaviour
{
    [SerializeField] private StageTimerSprites stageTimerSprites;

    private Func<float> _getRemainingDaytimeAlpha; // 0이면 낮 시작, 1이면 낮 끝
    private Image _dayArea;
    
    public void Init(int map, int stage, Func<float> getRemainingDaytimeAlpha)
    {
        _getRemainingDaytimeAlpha = getRemainingDaytimeAlpha;
        
        // 맵 스위치
        for (int mapToDraw = 1; mapToDraw <= 3; ++mapToDraw)
        {
            var imageObject = new GameObject { name = $"Map{mapToDraw}", transform = { parent = transform.Find(mapToDraw == map ? "ActiveMapImage" : "InactiveMapImages") }};
            var imageComponent = imageObject.AddComponent<Image>();
            imageObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            var rectTransform = (RectTransform)imageObject.transform;
            
            var sprite = stageTimerSprites.GetMapSprite(mapToDraw, mapToDraw == map);
            imageComponent.sprite = sprite;
            rectTransform.sizeDelta = sprite.rect.size * 0.4f;
            
            var degrees = 360.0f / 12 * (-mapToDraw + 2.0f) + 90.0f;
            var radius = ((RectTransform)transform).rect.width / 2.0f * 1.4f;
            if (mapToDraw == 3)
            {
                radius *= 0.8f;
            }
            
            var radians = degrees / 180.0f * Mathf.PI;
            var localX = radius * Mathf.Cos(radians);
            var localY = radius * Mathf.Sin(radians);
            imageComponent.transform.localPosition = new Vector3(localX, localY, 1.0f);
        }
        
        // 스테이지 스위치
        for (int stageToDraw = 1; stageToDraw <= 10; ++stageToDraw)
        {
            var imageObject = new GameObject { name = $"Stage{stageToDraw}", transform = { parent = transform.Find("StageImages") }};
            var imageComponent = imageObject.AddComponent<Image>();
            imageObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            var rectTransform = (RectTransform)imageObject.transform;
            
            var sprite = stageTimerSprites.GetStageSprite(stageToDraw, stageToDraw <= stage);
            imageComponent.sprite = sprite;
            rectTransform.sizeDelta = sprite.rect.size * 0.5f;
            
            var degrees = 360.0f / 12 * (-stageToDraw - 0.5f) + 90.0f;
            var radius = ((RectTransform)transform).rect.width / 2.0f * 0.825f;
            
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
                radius += 1.0f;
            }
            
            var radians = degrees / 180.0f * Mathf.PI;
            var localX = radius * Mathf.Cos(radians);
            var localY = radius * Mathf.Sin(radians);
            imageComponent.transform.localPosition = new Vector3(localX, localY, 0.0f);
        }
    }

    private void Awake()
    {
        _dayArea = transform.Find("DayArea").GetComponent<Image>();
    }
    
    private void Update()
    {
        _dayArea.fillAmount = _getRemainingDaytimeAlpha();
    }
}
