using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public sealed class StageButton : MonoBehaviour
{
    public event Action<int, int> OnClicked;

    public int MapIndex { get; private set; }
    public int StageIndex { get; private set; }

    private Button _button;
    private TMP_Text _mapStageText;
    private Image _lockedImage;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        var objectName = gameObject.name;

        var parts = objectName.Split('_');
        MapIndex = int.Parse(parts[1]);
        StageIndex = int.Parse(parts[2]);

        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonPressed);
        _mapStageText = transform.Find("MapStageText").GetComponent<TMP_Text>();
        _mapStageText.text = $"{MapIndex}-{StageIndex}";
        _lockedImage = transform.Find("LockedImage").GetComponent<Image>();
    }
    private void Start()
    {
        var baseWidth = ConstantConfig.DEFAULT_RESOLUTION_WIDTH; // 2340
        var baseHeight = ConstantConfig.DEFAULT_RESOLUTION_HEIGHT; // 1080

        var curWidth = Screen.width; // 2540
        var curHeight = Screen.height; // 1600

        var ratioX = curWidth / (float)baseWidth; // 1.084
        var ratioY = curHeight / (float)baseHeight; // 1.481

        // 최종 위치 (Canvas Scaler의 Match 설정 고려해야 함)
        var match = 0.0f; // 지금은 Width 쪽 100%라 0
        float scale = ratioY; // Mathf.Lerp(ratioX, ratioY, match); // 0 ~ 1

        var originPos = _rectTransform.anchoredPosition;
        _rectTransform.anchoredPosition = new Vector2(originPos.x, originPos.y * scale);
    }

    private void OnButtonPressed() => OnClicked(MapIndex, StageIndex);

    public void SetStageButtonEnabled(bool stageButtonEnabled)
    {
        _button.interactable = stageButtonEnabled;
        _mapStageText.gameObject.SetActive(stageButtonEnabled);
        _lockedImage.gameObject.SetActive(!stageButtonEnabled);
    }
}
