using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class PestWarningUI : MonoBehaviour
{
    [Serializable]
    public struct PoliceLineData
    {
        [NonSerialized] public Image Image;
        [SerializeField] public Vector2 beginXYRatios;
        [SerializeField] public Vector2 endXYRatios;

        [NonSerialized] public Vector2 BeginScreenPosition;
        [NonSerialized] public Vector2 EndScreenPosition;
    }
    
    public bool IsWarningShowing => gameObject.activeSelf;

    public void ShowWarning()
    {
        if (gameObject.activeSelf)
        {
            return;
        }
        gameObject.SetActive(true);
        StartCoroutine(DoWarning());
    }

    [SerializeField] private PoliceLineData[] policeLineDatas;
    [SerializeField] private Sprite policeLineSprite;

    private void Awake()
    {
        gameObject.SetActive(false);

        var policeLinesObject = transform.Find("PoliceLines").gameObject;
        for (var i = 0; i < policeLineDatas.Length; ++i)
        {
            var policeLineObject = new GameObject($"PoliceLine_{i}");
            policeLineObject.transform.parent = policeLinesObject.transform;
            policeLineObject.layer = LayerMask.NameToLayer("UI");
            var imageComponent = policeLineObject.AddComponent<Image>();
            imageComponent.sprite = policeLineSprite;
        }
    }

    private void OnEnable()
    {
        Debug.Log($"{Screen.currentResolution}");
        var policeLinesObject = transform.Find("PoliceLines").gameObject;
        for (var i = 0; i < policeLineDatas.Length; ++i)
        {
            var imageComponent = policeLinesObject.transform.Find($"PoliceLine_{i}").GetComponent<Image>();

            var rectTransform = imageComponent.transform as RectTransform;
            rectTransform.sizeDelta = policeLineSprite.rect.size;
            rectTransform.localScale = Vector3.one * 2.5f;
            rectTransform.pivot = new Vector2(1.0f, 0.5f);

            var newLocalPosition = rectTransform.localPosition;
            newLocalPosition.z = 0.0f;
            rectTransform.localPosition = newLocalPosition;
            
            var newRotation = Vector3.zero;

            policeLineDatas[i].BeginScreenPosition.x = Screen.currentResolution.width * policeLineDatas[i].beginXYRatios.x;
            policeLineDatas[i].BeginScreenPosition.y = Screen.currentResolution.height * policeLineDatas[i].beginXYRatios.y;           
            policeLineDatas[i].EndScreenPosition.x = Screen.currentResolution.width * policeLineDatas[i].endXYRatios.x;
            policeLineDatas[i].EndScreenPosition.y = Screen.currentResolution.height * policeLineDatas[i].endXYRatios.y;
            
            var positionDelta = policeLineDatas[i].EndScreenPosition - policeLineDatas[i].BeginScreenPosition;
            newRotation.z = Mathf.Atan2(positionDelta.y, positionDelta.x) * Mathf.Rad2Deg;
            rectTransform.eulerAngles = newRotation;
            
            policeLineDatas[i].Image = imageComponent;
        }
    }

    private IEnumerator DoWarning()
    {
        const float enterTimeForEachPoliceLine = 0.7f;

        var elapsedTime = 0.0f;
        while (elapsedTime < 4.0f)
        {
            var deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            
            for (var i = 0; i < 4; ++i)
            {
                var beginTime = enterTimeForEachPoliceLine * i;
                var endTime = enterTimeForEachPoliceLine * (i+1);
                var image = policeLineDatas[i].Image;
                var beginPosition = policeLineDatas[i].BeginScreenPosition;
                var endPosition = policeLineDatas[i].EndScreenPosition;

                float alpha;
                if (elapsedTime > endTime)
                {
                    alpha = 1.0f + (elapsedTime - endTime) * 0.1f;
                }
                else
                {
                    alpha = (elapsedTime - beginTime) / enterTimeForEachPoliceLine;
                }
                
                var rectTransform = (RectTransform)image.transform;
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(beginPosition, endPosition, alpha);
            }

            yield return null;
        }
        
        gameObject.SetActive(false);
        yield return null;
    }
}