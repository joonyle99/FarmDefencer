using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 스테이지 (디펜스) 로 입장하는 버튼
/// </summary>
public sealed class StageButton : MonoBehaviour
{
    public event Action<int, int> OnClicked;

    public int MapIndex { get; private set; }
    public int StageIndex { get; private set; }

    private Button _button;
    private TMP_Text _mapStageText;
    private Image _lockedImage;

    private void Awake()
    {
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

    private void OnButtonPressed() => OnClicked(MapIndex, StageIndex);

    public void SetStageButtonEnabled(bool stageButtonEnabled)
    {
        _button.interactable = stageButtonEnabled;
        _mapStageText.gameObject.SetActive(stageButtonEnabled);
        _lockedImage.gameObject.SetActive(!stageButtonEnabled);
    }
}
